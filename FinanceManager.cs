using System.Text;

namespace C_Sharp_Course_Intermediate_Task;

public class FinanceManager : IReport
{
    private readonly DataManager           _dataManager;
    private          List<Account>         _accounts      = new();
    private readonly Dictionary<string, double> _categoryTotals = new();
    private readonly Queue<Transaction>    _pendingQueue  = new();

    public int AccountCount => _accounts.Count;

    public FinanceManager(string dataFile = "finance_data.json")
    {
        _dataManager = new DataManager(dataFile);
        LoadOnStartup();
    }

    // ── Startup / Shutdown ────────────────────────────────────────────
    private void LoadOnStartup()
    {
        _accounts = _dataManager.Load();
        Console.WriteLine($"  {_accounts.Count} account(s) restored.");
    }

    public void SaveData()
    {
        _dataManager.Save(_accounts);
    }

    // ── Account management ────────────────────────────────────────────
    public void AddAccount(Account account)
    {
        if (account == null) { Console.WriteLine("  ✗ Account cannot be null."); return; }
        _accounts.Add(account);
        Console.WriteLine($"  ✓ Account added: [{account.AccountNumber}] {account.OwnerName}");
    }

    public bool RemoveAccount(string accountNumber)
    {
        var account = FindAccount(accountNumber);
        if (account == null) { Console.WriteLine($"  ✗ Account not found: {accountNumber}"); return false; }
        _accounts.Remove(account);
        Console.WriteLine($"  ✓ Removed: [{accountNumber}]");
        return true;
    }

    public Account FindAccount(string accountNumber)
        => _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

    private Account FindAccountOrThrow(string accountNumber)
    {
        var account = FindAccount(accountNumber);
        if (account == null)
            throw new ArgumentException($"Account not found: {accountNumber}");
        return account;
    }

    public List<Account> GetAllAccounts() => new List<Account>(_accounts);

    // ── Safe wrappers (TryXxx pattern) ────────────────────────────────
    public bool TryDeposit(string accountNumber, double amount, string category = "Deposit")
    {
        try
        {
            var account = FindAccountOrThrow(accountNumber);
            account.Deposit(amount);
            UpdateCategoryTotal(category, amount);
            _dataManager.AppendToLog($"DEPOSIT {amount:C} → {accountNumber} ({category})");
            return true;
        }
        catch (InvalidTransactionException ex)
        {
            Console.WriteLine($"  ✗ Transaction error: {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  ✗ {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Unexpected error: {ex.Message}");
            return false;
        }
    }

    public bool TryWithdraw(string accountNumber, double amount, string category = "Withdrawal")
    {
        try
        {
            var account = FindAccountOrThrow(accountNumber);
            account.Withdraw(amount);
            UpdateCategoryTotal(category, amount);
            _dataManager.AppendToLog($"WITHDRAW {amount:C} → {accountNumber} ({category})");
            return true;
        }
        catch (InsufficientFundsException ex)
        {
            Console.WriteLine($"  ✗ Insufficient funds: {ex.Message}");
            Console.WriteLine($"    Requested: {ex.RequestedAmount:C}  Available: {ex.AvailableBalance:C}");
            return false;
        }
        catch (InvalidTransactionException ex)
        {
            Console.WriteLine($"  ✗ Invalid transaction: {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  ✗ {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Unexpected error: {ex.Message}");
            return false;
        }
    }

    // ── Balance queries ───────────────────────────────────────────────
    public double GetTotalBalance() => _accounts.Sum(a => a.Balance);

    // ── Category tracking ─────────────────────────────────────────────
    public void UpdateCategoryTotal(string category, double amount)
    {
        if (_categoryTotals.ContainsKey(category))
            _categoryTotals[category] += amount;
        else
            _categoryTotals.Add(category, amount);
    }

    public void PrintCategoryReport()
    {
        Console.WriteLine("\n  Category Spending:");
        Console.WriteLine($"  {"─",50}");
        Console.WriteLine($"  {"Category",-22} {"Total",12}");
        Console.WriteLine($"  {"─",50}");
        foreach (var kv in _categoryTotals)
            Console.WriteLine($"  {kv.Key,-22} {kv.Value,12:C}");
        Console.WriteLine($"  {"─",50}");
        Console.WriteLine($"  {"Total",-22} {_categoryTotals.Values.Sum(),12:C}");
    }

    // ── Queue ─────────────────────────────────────────────────────────
    public void EnqueueTransaction(Transaction t)
    {
        _pendingQueue.Enqueue(t);
        Console.WriteLine($"  → Queued: {t.Description} ({t.GetFormattedAmount()})");
    }

    public void ProcessPendingTransactions(string accountNumber)
    {
        int count = _pendingQueue.Count;
        Console.WriteLine($"\n  Processing {count} queued transaction(s) for {accountNumber}...");
        while (_pendingQueue.Count > 0)
        {
            var t = _pendingQueue.Dequeue();
            if (t.IsIncome())
                TryDeposit(accountNumber, t.Amount, t.Category);
            else
                TryWithdraw(accountNumber, t.Amount, t.Category);
        }
        Console.WriteLine($"  ✓ Queue empty. All {count} transaction(s) processed.");
    }

    // ── LINQ: Search & Sort ───────────────────────────────────────────
    public List<Transaction> GetAllTransactions()
    {
        var all = new List<Transaction>();
        foreach (var account in _accounts)
            all.AddRange(account.Transactions);
        return all;
    }

    public List<Transaction> SearchTransactions(string keyword)
        => GetAllTransactions()
            .Where(t => t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public List<Transaction> GetSortedTransactions(string sortBy = "amount")
    {
        var all = GetAllTransactions();
        return sortBy.ToLower() switch
        {
            "amount"   => all.OrderByDescending(t => t.Amount).ToList(),
            "date"     => all.OrderByDescending(t => t.Date).ToList(),
            "category" => all.OrderBy(t => t.Category).ToList(),
            _          => all
        };
    }

    // ── Interest ──────────────────────────────────────────────────────
    public void ApplyInterestToAll()
    {
        Console.WriteLine("\n  Applying interest to savings accounts...");
        bool any = false;
        foreach (var account in _accounts)
        {
            if (account is SavingsAccount sa)
            {
                sa.ApplyInterest();
                any = true;
            }
        }
        if (!any) Console.WriteLine("  (No savings accounts found.)");
    }

    // ── Export all via IExportable ────────────────────────────────────
    public void ExportAll()
    {
        Console.WriteLine("\n  Exporting accounts...");
        foreach (var account in _accounts)
        {
            if (account is IExportable exportable)
            {
                string path = $"{account.AccountNumber}_report.{exportable.GetExportFormat().ToLower()}";
                exportable.ExportToFile(path);
            }
        }
    }

    // ── Transaction log ───────────────────────────────────────────────
    public void PrintRecentLog(int lines = 10)
    {
        var entries = _dataManager.ReadLog(lines);
        Console.WriteLine($"\n  Recent Transaction Log (last {lines}):");
        Console.WriteLine($"  {"─",55}");
        if (entries.Count == 0) Console.WriteLine("  (No entries yet.)");
        foreach (var e in entries) Console.WriteLine($"  {e}");
    }

    // ── IReport ───────────────────────────────────────────────────────
    public string GenerateReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"╔══ FINANCE MANAGER REPORT ══════════════════════╗");
        sb.AppendLine($"  Accounts      : {AccountCount}");
        sb.AppendLine($"  Total Balance : {GetTotalBalance():C}");
        sb.AppendLine($"╠════════════════════════════════════════════════╣");
        foreach (var account in _accounts)
        {
            if (account is IReport r)
                sb.AppendLine($"  {r.GetSummaryLine()}");
        }
        sb.AppendLine($"╚════════════════════════════════════════════════╝");
        return sb.ToString();
    }

    public string GetSummaryLine()
        => $"[MANAGER] {AccountCount} accounts — Total: {GetTotalBalance():C}";

    // ── Full summary ──────────────────────────────────────────────────
    public void DisplaySummary()
    {
        Console.WriteLine();
        Console.WriteLine($"  {"═",56}");
        Console.WriteLine($"  FINANCE MANAGER — {AccountCount} account(s)");
        Console.WriteLine($"  Total Balance: {GetTotalBalance():C}");
        Console.WriteLine($"  {"═",56}");
        foreach (var account in _accounts)
            Console.WriteLine($"  {account}");
        PrintCategoryReport();
        int total = _accounts.Sum(a => a.Transactions.Count);
        Console.WriteLine($"\n  Total Transactions across all accounts: {total}");
    }

    // ── Run full reports via IReport polymorphism ─────────────────────
    public void RunAllReports()
    {
        Console.WriteLine("\n  Running all reports (polymorphic)...\n");
        var reporters = new List<IReport>();
        foreach (var account in _accounts)
            if (account is IReport r) reporters.Add(r);
        reporters.Add(this);

        foreach (var reporter in reporters)
        {
            Console.WriteLine(reporter.GenerateReport());
        }
    }
}
