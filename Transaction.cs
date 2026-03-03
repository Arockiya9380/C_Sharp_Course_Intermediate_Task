namespace C_Sharp_Course_Intermediate_Task;

public class Transaction
{
    // ── Static counter for auto-incrementing IDs ──────────────────────
    private static int _nextId = 1;

    // ── Properties ────────────────────────────────────────────────────
    public int    Id          { get; }
    public string Description { get; set; }
    public double Amount      { get; set; }
    public string Type        { get; set; }   // "Income" or "Expense"
    public string Category    { get; set; }
    public string Date        { get; set; }

    // ── Default constructor ───────────────────────────────────────────
    public Transaction()
    {
        Id   = _nextId++;
        Date = DateTime.Today.ToString("yyyy-MM-dd");
    }

    // ── Parameterised constructor with validation ─────────────────────
    public Transaction(string description, double amount, string type, string category)
        : this()
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidTransactionException("Description", description, "cannot be empty");

        if (amount <= 0)
            throw new InvalidTransactionException("Amount", amount, "must be greater than zero");

        if (type != "Income" && type != "Expense")
            throw new InvalidTransactionException("Type", type, "must be 'Income' or 'Expense'");

        Description = description;
        Amount      = amount;
        Type        = type;
        Category    = string.IsNullOrWhiteSpace(category) ? "Uncategorised" : category;
    }

    // ── Methods ───────────────────────────────────────────────────────
    public bool   IsIncome()           => Type == "Income";
    public string GetFormattedAmount() => IsIncome() ? $"+{Amount:C}" : $"-{Amount:C}";

    public void Display()
        => Console.WriteLine($"  [{Id,3}] {Date}  {Type,-8}  {Category,-15}  {GetFormattedAmount(),12}  {Description}");

    public override string ToString()
        => $"[{Id}] {Description} — {GetFormattedAmount()} ({Type}, {Category}, {Date})";

    public override bool Equals(object obj)
    {
        if (obj is not Transaction other) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    // ── Reset counter (used during Load to avoid duplicate IDs) ───────
    public static void ResetCounter(int nextId) => _nextId = nextId;
}
