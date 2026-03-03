namespace C_Sharp_Course_Intermediate_Task;

public abstract class Account
{
    // ── Static counter ────────────────────────────────────────────────
    private static int _accountCounter = 1;

    // ── Private backing fields ────────────────────────────────────────
    private double _balance;

    // ── Public properties ─────────────────────────────────────────────
    public string             AccountNumber { get; }
    public string             OwnerName     { get; set; }
    public double             Balance       => _balance;
    public List<Transaction>  Transactions  { get; } = new List<Transaction>();

    // ── Protected constructor ─────────────────────────────────────────
    protected Account(string ownerName)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
            throw new ArgumentException("Owner name cannot be empty.");

        OwnerName     = ownerName;
        AccountNumber = $"ACC-{_accountCounter++:D4}";
        _balance      = 0;
    }

    // ── Protected helpers ─────────────────────────────────────────────
    protected void AdjustBalance(double amount) => _balance += amount;

    // Used by DataManager to restore saved balance without creating transactions
    internal void RestoreBalance(double balance) => _balance = balance;

    // Used by DataManager to restore the account counter after loading
    public static void ResetCounter(int next) => _accountCounter = next;

    // ── Concrete methods ──────────────────────────────────────────────
    public void Deposit(double amount)
    {
        if (amount <= 0)
            throw new InvalidTransactionException("Amount", amount, "must be positive");

        AdjustBalance(amount);
        Transactions.Add(new Transaction("Deposit", amount, "Income", "Deposit"));
        Console.WriteLine($"  ✓ Deposited {amount:C}. New balance: {Balance:C}");
    }

    // ── Virtual — default: no overdraft ──────────────────────────────
    public virtual void Withdraw(double amount)
    {
        if (amount <= 0)
            throw new ArgumentException($"Withdrawal amount must be positive. Got: {amount}");

        if (amount > Balance)
            throw new InsufficientFundsException(amount, Balance, AccountNumber);

        AdjustBalance(-amount);
        Transactions.Add(new Transaction("Withdrawal", amount, "Expense", "Withdrawal"));
        Console.WriteLine($"  ✓ Withdrew {amount:C}. New balance: {Balance:C}");
    }

    // ── Abstract — every subclass must implement ──────────────────────
    public abstract string GetAccountInfo();

    public override string ToString()
        => $"[{AccountNumber}] {OwnerName,-15} Balance: {Balance,12:C}";
}
