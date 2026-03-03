using System.Text;

namespace C_Sharp_Course_Intermediate_Task;

public sealed class CheckingAccount : Account, IReport, IExportable
{
    public double OverdraftLimit { get; private set; }
    public double OverdraftFee   { get; private set; }

    public CheckingAccount(string ownerName,
                           double overdraftLimit = 500.0,
                           double overdraftFee   = 35.0)
        : base(ownerName)
    {
        OverdraftLimit = overdraftLimit;
        OverdraftFee   = overdraftFee;
    }

    // ── Override Withdraw — allow overdraft up to limit ───────────────
    public override void Withdraw(double amount)
    {
        if (amount <= 0)
            throw new ArgumentException($"Withdrawal amount must be positive. Got: {amount}");

        if (amount > Balance + OverdraftLimit)
            throw new InsufficientFundsException(amount, Balance + OverdraftLimit, AccountNumber);

        if (amount > Balance)
        {
            // Going into overdraft
            AdjustBalance(-amount);
            Transactions.Add(new Transaction("Withdrawal", amount, "Expense", "Withdrawal"));
            Console.WriteLine($"  ⚠  Overdraft used: withdrew {amount:C} (balance was {Balance + amount:C})");

            AdjustBalance(-OverdraftFee);
            Transactions.Add(new Transaction("Overdraft Fee", OverdraftFee, "Expense", "Fee"));
            Console.WriteLine($"  ⚠  Overdraft fee: -{OverdraftFee:C}. New balance: {Balance:C}");
        }
        else
        {
            base.Withdraw(amount);
        }
    }

    public string GetOverdraftStatus()
        => Balance < 0
            ? $"⚠  Overdrawn by {Math.Abs(Balance):C}"
            : $"✓  Available overdraft: {OverdraftLimit:C}";

    // ── Abstract implementation ───────────────────────────────────────
    public override string GetAccountInfo()
        => $"Checking Account | Overdraft: {OverdraftLimit:C} | Fee: {OverdraftFee:C}";

    public override string ToString()
        => $"{base.ToString()} | Checking (overdraft {OverdraftLimit:C})";

    // ── IReport ───────────────────────────────────────────────────────
    public string GenerateReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"╔══ CHECKING ACCOUNT REPORT ═════════════════════╗");
        sb.AppendLine($"  Account : {AccountNumber}    Owner: {OwnerName}");
        sb.AppendLine($"  Balance : {Balance:C}    Overdraft Limit: {OverdraftLimit:C}");
        sb.AppendLine($"  Status  : {GetOverdraftStatus()}");
        sb.AppendLine($"  Fee     : {OverdraftFee:C}    Transactions: {Transactions.Count}");
        sb.AppendLine($"╠════════════════════════════════════════════════╣");
        foreach (var t in Transactions)
            sb.AppendLine($"  {t}");
        sb.AppendLine($"╚════════════════════════════════════════════════╝");
        return sb.ToString();
    }

    public string GetSummaryLine()
        => $"[CHKNG]  {OwnerName,-15} Balance: {Balance,12:C}  {GetOverdraftStatus()}";

    // ── IExportable ───────────────────────────────────────────────────
    public bool ExportToFile(string filePath)
    {
        try
        {
            File.WriteAllText(filePath, GenerateReport());
            Console.WriteLine($"  ✓ Exported checking report to {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Export failed: {ex.Message}");
            return false;
        }
    }

    public string GetExportFormat() => "TXT";
}
