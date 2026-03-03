namespace C_Sharp_Course_Intermediate_Task;

public class TransactionDto
{
    public int    Id          { get; set; }
    public string Description { get; set; } = "";
    public double Amount      { get; set; }
    public string Type        { get; set; } = "";
    public string Category    { get; set; } = "";
    public string Date        { get; set; } = "";
}

public class AccountDto
{
    public string             AccountNumber  { get; set; } = "";
    public string             OwnerName      { get; set; } = "";
    public string             AccountType    { get; set; } = "";   // "Savings" | "Checking"
    public double             Balance        { get; set; }
    // Savings-specific
    public double?            InterestRate   { get; set; }
    public double?            MinBalance     { get; set; }
    // Checking-specific
    public double?            OverdraftLimit { get; set; }
    public double?            OverdraftFee   { get; set; }
    // Nested
    public List<TransactionDto> Transactions { get; set; } = new();
}

public class FinanceDataDto
{
    public string           SavedAt  { get; set; } = "";
    public string           Version  { get; set; } = "2.0";
    public List<AccountDto> Accounts { get; set; } = new();
}
