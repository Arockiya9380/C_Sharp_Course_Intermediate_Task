using System.Text;

namespace C_Sharp_Course_Intermediate_Task;

public class SavingsAccount : Account, IReport, IExportable
{
    public double InterestRate { get; private set; }
    public double MinBalance   { get; private set; }

    public SavingsAccount(string ownerName,
                          double interestRate = 0.035,
                          double minBalance   = 100.0)
        : base(ownerName)
    {
        InterestRate = interestRate;
        MinBalance   = minBalance;
    }

    // ── Override Withdraw — enforce minimum balance ───────────────────
    public override void Withdraw(double amount)
    {
        if (amount <= 0)
            throw new ArgumentException($"Withdrawal amount must be positive. Got: {amount}");

        if (Balance - amount < MinBalance)
            throw new InsufficientFundsException(amount, Balance - MinBalance, AccountNumber);

        base.Withdraw(amount);
    }

    // ── Savings-only: apply monthly interest ─────────────────────────
    public void ApplyInterest()
    {
        double interest = Balance * InterestRate;
        AdjustBalance(interest);
        Transactions.Add(new Transaction("Interest", interest, "Income", "Interest"));
        Console.WriteLine($"  ✓ Interest applied: +{interest:C} ({InterestRate:P1}). New balance: {Balance:C}");
    }

    // ── Abstract implementation ───────────────────────────────────────
    public override string GetAccountInfo()
        => $"Savings Account  | Rate: {InterestRate:P1} | Min Balance: {MinBalance:C}";

    public override string ToString()
        => $"{base.ToString()} | Savings ({InterestRate:P1} interest)";

    // ── IReport ───────────────────────────────────────────────────────
    public string GenerateReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"╔══ SAVINGS ACCOUNT REPORT ══════════════════════╗");
        sb.AppendLine($"  Account : {AccountNumber}    Owner: {OwnerName}");
        sb.AppendLine($"  Balance : {Balance:C}    Interest Rate: {InterestRate:P1}");
        sb.AppendLine($"  Min Bal : {MinBalance:C}    Transactions: {Transactions.Count}");
        sb.AppendLine($"╠════════════════════════════════════════════════╣");
        foreach (var t in Transactions)
            sb.AppendLine($"  {t}");
        sb.AppendLine($"╚════════════════════════════════════════════════╝");
        return sb.ToString();
    }

    public string GetSummaryLine()
        => $"[SAVINGS] {OwnerName,-15} Balance: {Balance,12:C}  Rate: {InterestRate:P1}";

    // ── IExportable ───────────────────────────────────────────────────
    public bool ExportToFile(string filePath)
    {
        try
        {
            File.WriteAllText(filePath, GenerateReport());
            Console.WriteLine($"  ✓ Exported savings report to {filePath}");
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
