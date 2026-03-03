using C_Sharp_Course_Intermediate_Task;

// ════════════════════════════════════════════════════════════════
//   PERSONAL FINANCE MANAGER v2.0
//   Intermediate OOP Exercise — Complete Implementation
//   Chapters 9–15 | Steps 1–7
// ════════════════════════════════════════════════════════════════

PrintBanner();

var manager = new FinanceManager();
Console.WriteLine();

bool running = true;
while (running)
{
    PrintMenu();
    string choice = Console.ReadLine()?.Trim() ?? "";

    switch (choice)
    {
        case "1":  AddAccount(manager);              break;
        case "2":  ListAccounts(manager);            break;
        case "3":  RemoveAccount(manager);           break;
        case "4":  DoDeposit(manager);               break;
        case "5":  DoWithdraw(manager);              break;
        case "6":  QueueTransactions(manager);       break;
        case "7":  manager.DisplaySummary();         break;
        case "8":  manager.RunAllReports();          break;
        case "9":  SearchMenu(manager);              break;
        case "10": manager.ApplyInterestToAll();     break;
        case "11": manager.ExportAll();              break;
        case "12": manager.PrintRecentLog();         break;
        case "13": manager.SaveData();               break;
        case "0":  running = false; Console.WriteLine("\n  Saving before exit..."); break;
        default:   Console.WriteLine("  ✗ Invalid option. Please try again."); break;
    }
}

// ── Auto-save in finally — always runs ───────────────────────────
try { }
finally
{
    Console.WriteLine();
    manager.SaveData();
    Console.WriteLine("\n  Goodbye!");
}

// ════════════════════════════════════════════════════════════════
//   MENU HELPERS
// ════════════════════════════════════════════════════════════════

static void PrintBanner()
{
    Console.WriteLine();
    Console.WriteLine("  ╔════════════════════════════════════════════════════════╗");
    Console.WriteLine("  ║       PERSONAL FINANCE MANAGER  v2.0                  ║");
    Console.WriteLine("  ║       Intermediate OOP Exercise — All 7 Steps         ║");
    Console.WriteLine("  ╚════════════════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine("  Loading saved data...");
}

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("  ┌─────────────────────────────────────────────────────┐");
    Console.WriteLine("  │  MAIN MENU                                          │");
    Console.WriteLine("  ├─────────────────────────────────────────────────────┤");
    Console.WriteLine("  │  ACCOUNTS                                           │");
    Console.WriteLine("  │   1. Add Account          2. List Accounts          │");
    Console.WriteLine("  │   3. Remove Account                                 │");
    Console.WriteLine("  ├─────────────────────────────────────────────────────┤");
    Console.WriteLine("  │  TRANSACTIONS                                       │");
    Console.WriteLine("  │   4. Deposit              5. Withdraw               │");
    Console.WriteLine("  │   6. Queue Transactions (batch)                     │");
    Console.WriteLine("  ├─────────────────────────────────────────────────────┤");
    Console.WriteLine("  │  REPORTS & TOOLS                                    │");
    Console.WriteLine("  │   7. Summary              8. Full Reports           │");
    Console.WriteLine("  │   9. Search/Sort         10. Apply Interest         │");
    Console.WriteLine("  │  11. Export to Files     12. View Log               │");
    Console.WriteLine("  ├─────────────────────────────────────────────────────┤");
    Console.WriteLine("  │  13. Save Now             0. Save & Exit            │");
    Console.WriteLine("  └─────────────────────────────────────────────────────┘");
    Console.Write("  Choice: ");
}

static void AddAccount(FinanceManager manager)
{
    Console.WriteLine();
    Console.WriteLine("  Account type:");
    Console.WriteLine("   1. Savings Account");
    Console.WriteLine("   2. Checking Account");
    Console.Write("  Choice: ");
    string type = Console.ReadLine()?.Trim() ?? "";

    Console.Write("  Owner name: ");
    string name = Console.ReadLine()?.Trim() ?? "";

    try
    {
        if (type == "1")
        {
            Console.Write("  Interest rate (e.g. 0.035 for 3.5%) [enter=0.035]: ");
            string rateStr = Console.ReadLine()?.Trim() ?? "";
            double rate = string.IsNullOrEmpty(rateStr) ? 0.035 : double.Parse(rateStr);

            Console.Write("  Minimum balance [enter=100]: ");
            string minStr = Console.ReadLine()?.Trim() ?? "";
            double min = string.IsNullOrEmpty(minStr) ? 100.0 : double.Parse(minStr);

            manager.AddAccount(new SavingsAccount(name, rate, min));
        }
        else if (type == "2")
        {
            Console.Write("  Overdraft limit [enter=500]: ");
            string limStr = Console.ReadLine()?.Trim() ?? "";
            double lim = string.IsNullOrEmpty(limStr) ? 500.0 : double.Parse(limStr);

            Console.Write("  Overdraft fee [enter=35]: ");
            string feeStr = Console.ReadLine()?.Trim() ?? "";
            double fee = string.IsNullOrEmpty(feeStr) ? 35.0 : double.Parse(feeStr);

            manager.AddAccount(new CheckingAccount(name, lim, fee));
        }
        else
        {
            Console.WriteLine("  ✗ Invalid account type.");
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"  ✗ {ex.Message}");
    }
}

static void ListAccounts(FinanceManager manager)
{
    Console.WriteLine();
    var accounts = manager.GetAllAccounts();
    if (accounts.Count == 0) { Console.WriteLine("  No accounts yet. Add one first (option 1)."); return; }

    Console.WriteLine($"  {"─",60}");
    Console.WriteLine($"  {"Account",-12} {"Owner",-16} {"Type",-10} {"Balance",12}");
    Console.WriteLine($"  {"─",60}");
    foreach (var a in accounts)
    {
        string type = a is SavingsAccount ? "Savings" : "Checking";
        Console.WriteLine($"  {a.AccountNumber,-12} {a.OwnerName,-16} {type,-10} {a.Balance,12:C}");
        Console.WriteLine($"       {a.GetAccountInfo()}");
    }
    Console.WriteLine($"  {"─",60}");
    Console.WriteLine($"  {"TOTAL",-40} {manager.GetTotalBalance(),12:C}");
}

static void RemoveAccount(FinanceManager manager)
{
    Console.Write("  Account number to remove (e.g. ACC-0001): ");
    string num = Console.ReadLine()?.Trim().ToUpper() ?? "";
    manager.RemoveAccount(num);
}

static void DoDeposit(FinanceManager manager)
{
    Console.Write("  Account number: ");
    string num = Console.ReadLine()?.Trim().ToUpper() ?? "";

    Console.Write("  Amount: $");
    if (!double.TryParse(Console.ReadLine(), out double amount))
    { Console.WriteLine("  ✗ Invalid amount."); return; }

    Console.Write("  Category (e.g. Salary, Freelance) [enter=Income]: ");
    string cat = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrEmpty(cat)) cat = "Income";

    manager.TryDeposit(num, amount, cat);
}

static void DoWithdraw(FinanceManager manager)
{
    Console.Write("  Account number: ");
    string num = Console.ReadLine()?.Trim().ToUpper() ?? "";

    Console.Write("  Amount: $");
    if (!double.TryParse(Console.ReadLine(), out double amount))
    { Console.WriteLine("  ✗ Invalid amount."); return; }

    Console.Write("  Category (e.g. Food, Rent, Transport) [enter=Expense]: ");
    string cat = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrEmpty(cat)) cat = "Expense";

    manager.TryWithdraw(num, amount, cat);
}

static void QueueTransactions(FinanceManager manager)
{
    Console.Write("  Account number to process queue on: ");
    string num = Console.ReadLine()?.Trim().ToUpper() ?? "";

    Console.WriteLine("  Enter transactions (description='done' to stop):");
    while (true)
    {
        Console.Write("  Description (or 'done'): ");
        string desc = Console.ReadLine()?.Trim() ?? "";
        if (desc.ToLower() == "done") break;

        Console.Write("  Amount: $");
        if (!double.TryParse(Console.ReadLine(), out double amt))
        { Console.WriteLine("  ✗ Invalid amount. Skipping."); continue; }

        Console.Write("  Type (Income/Expense) [enter=Expense]: ");
        string typeIn = Console.ReadLine()?.Trim() ?? "";
        string type   = string.IsNullOrEmpty(typeIn) ? "Expense" : typeIn;

        Console.Write("  Category [enter=General]: ");
        string cat = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(cat)) cat = "General";

        try
        {
            manager.EnqueueTransaction(new Transaction(desc, amt, type, cat));
        }
        catch (InvalidTransactionException ex)
        {
            Console.WriteLine($"  ✗ {ex.Message}");
        }
    }

    if (!string.IsNullOrEmpty(num))
        manager.ProcessPendingTransactions(num);
}

static void SearchMenu(FinanceManager manager)
{
    Console.WriteLine();
    Console.WriteLine("  1. Search by keyword");
    Console.WriteLine("  2. Sort by amount (largest first)");
    Console.WriteLine("  3. Sort by date (newest first)");
    Console.WriteLine("  4. Sort by category (A-Z)");
    Console.Write("  Choice: ");
    string choice = Console.ReadLine()?.Trim() ?? "";

    void PrintTransactions(List<Transaction> list)
    {
        if (list.Count == 0) { Console.WriteLine("  (No results.)"); return; }
        Console.WriteLine($"\n  {"#",-4} {"Date",-12} {"Type",-8} {"Category",-16} {"Amount",12}  Description");
        Console.WriteLine($"  {"─",66}");
        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i];
            Console.WriteLine($"  {i + 1,-4} {t.Date,-12} {t.Type,-8} {t.Category,-16} {t.GetFormattedAmount(),12}  {t.Description}");
        }
    }

    if (choice == "1")
    {
        Console.Write("  Keyword: ");
        string kw      = Console.ReadLine()?.Trim() ?? "";
        var    results = manager.SearchTransactions(kw);
        Console.WriteLine($"\n  Found {results.Count} result(s) for '{kw}':");
        PrintTransactions(results);
    }
    else
    {
        string sortBy  = choice switch { "3" => "date", "4" => "category", _ => "amount" };
        var    sorted  = manager.GetSortedTransactions(sortBy);
        Console.WriteLine($"\n  All transactions sorted by {sortBy} ({sorted.Count} total):");
        PrintTransactions(sorted);
    }
}
