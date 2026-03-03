namespace C_Sharp_Course_Intermediate_Task;

// ── InsufficientFundsException ────────────────────────────────────────────
public class InsufficientFundsException : Exception
{
    public double RequestedAmount  { get; }
    public double AvailableBalance { get; }
    public string AccountNumber    { get; }

    public InsufficientFundsException(double requested, double available, string accountNumber)
        : base($"Insufficient funds in {accountNumber}: requested {requested:C}, available {available:C}.")
    {
        RequestedAmount  = requested;
        AvailableBalance = available;
        AccountNumber    = accountNumber;
    }

    public InsufficientFundsException(double requested, double available, string accountNumber,
                                      Exception innerException)
        : base($"Insufficient funds in {accountNumber}.", innerException)
    {
        RequestedAmount  = requested;
        AvailableBalance = available;
        AccountNumber    = accountNumber;
    }
}

// ── InvalidTransactionException ──────────────────────────────────────────
public class InvalidTransactionException : Exception
{
    public string TransactionField { get; }
    public object InvalidValue     { get; }

    public InvalidTransactionException(string field, object invalidValue, string reason)
        : base($"Invalid transaction — {field}: {reason} (got: '{invalidValue}')")
    {
        TransactionField = field;
        InvalidValue     = invalidValue;
    }

    public InvalidTransactionException(string message) : base(message) { }
}
