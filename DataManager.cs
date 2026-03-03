using System.Text.Json;

namespace C_Sharp_Course_Intermediate_Task;

public class DataManager
{
    private readonly string               _filePath;
    private readonly string               _logPath;
    private readonly JsonSerializerOptions _options;

    public DataManager(string filePath = "finance_data.json")
    {
        _filePath = filePath;
        _logPath  = "transaction_log.txt";
        _options  = new JsonSerializerOptions
        {
            WriteIndented        = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public bool   FileExists() => File.Exists(_filePath);
    public string GetFilePath() => _filePath;

    // ── Save ─────────────────────────────────────────────────────────
    public bool Save(List<Account> accounts)
    {
        try
        {
            var data = new FinanceDataDto
            {
                SavedAt  = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Accounts = accounts.Select(MapToDto).ToList()
            };

            string json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(_filePath, json);

            var (bytes, accts, txns) = GetStats(accounts);
            Console.WriteLine($"  ✓ Saved to {_filePath}  [{bytes} bytes | {accts} accounts | {txns} transactions]");
            return true;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"  ✗ Save failed (IO): {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Save failed: {ex.Message}");
            return false;
        }
    }

    // ── Load ─────────────────────────────────────────────────────────
    public List<Account> Load()
    {
        if (!File.Exists(_filePath))
        {
            Console.WriteLine($"  No save file found at {_filePath}. Starting fresh.");
            return new List<Account>();
        }

        try
        {
            string json  = File.ReadAllText(_filePath);
            var    data  = JsonSerializer.Deserialize<FinanceDataDto>(json, _options);

            if (data?.Accounts == null)
                return new List<Account>();

            var accounts = data.Accounts
                               .Select(MapFromDto)
                               .Where(a => a != null)
                               .ToList();

            // Restore the account counter so new accounts don't reuse numbers
            int maxNum = accounts
                .Select(a => int.TryParse(a.AccountNumber.Replace("ACC-", ""), out int n) ? n : 0)
                .DefaultIfEmpty(0).Max();
            Account.ResetCounter(maxNum + 1);

            // Restore the transaction ID counter
            int maxId = accounts.SelectMany(a => a.Transactions)
                                .Select(t => t.Id)
                                .DefaultIfEmpty(0).Max();
            Transaction.ResetCounter(maxId + 1);

            Console.WriteLine($"  ✓ Loaded {accounts.Count} account(s) from {_filePath}");
            return accounts;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"  ✗ Corrupt save file: {ex.Message}");
            BackupCorruptFile();
            return new List<Account>();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"  ✗ Load failed (IO): {ex.Message}");
            return new List<Account>();
        }
    }

    // ── Append a line to the transaction log ─────────────────────────
    public void AppendToLog(string entry)
    {
        try
        {
            using var writer = new StreamWriter(_logPath, append: true);
            writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {entry}");
        }
        catch { /* log failures are non-fatal */ }
    }

    // ── Read the last N lines from the log ───────────────────────────
    public List<string> ReadLog(int lastN = 10)
    {
        if (!File.Exists(_logPath)) return new List<string>();
        using var reader = new StreamReader(_logPath);
        var lines = new List<string>();
        string line;
        while ((line = reader.ReadLine()) != null) lines.Add(line);
        return lines.TakeLast(lastN).ToList();
    }

    // ── Stats ─────────────────────────────────────────────────────────
    public (long bytes, int accounts, int transactions) GetStats(List<Account> accounts)
    {
        long bytes = File.Exists(_filePath) ? new FileInfo(_filePath).Length : 0;
        int  txns  = accounts.Sum(a => a.Transactions.Count);
        return (bytes, accounts.Count, txns);
    }

    // ── Map domain → DTO ─────────────────────────────────────────────
    private AccountDto MapToDto(Account account)
    {
        var dto = new AccountDto
        {
            AccountNumber = account.AccountNumber,
            OwnerName     = account.OwnerName,
            Balance       = account.Balance,
            Transactions  = account.Transactions.Select(t => new TransactionDto
            {
                Id          = t.Id,
                Description = t.Description,
                Amount      = t.Amount,
                Type        = t.Type,
                Category    = t.Category,
                Date        = t.Date
            }).ToList()
        };

        if (account is SavingsAccount sa)
        {
            dto.AccountType  = "Savings";
            dto.InterestRate = sa.InterestRate;
            dto.MinBalance   = sa.MinBalance;
        }
        else if (account is CheckingAccount ca)
        {
            dto.AccountType    = "Checking";
            dto.OverdraftLimit = ca.OverdraftLimit;
            dto.OverdraftFee   = ca.OverdraftFee;
        }

        return dto;
    }

    // ── Map DTO → domain ─────────────────────────────────────────────
    private Account MapFromDto(AccountDto dto)
    {
        Account account = dto.AccountType switch
        {
            "Savings"  => new SavingsAccount(dto.OwnerName,
                              dto.InterestRate ?? 0.035,
                              dto.MinBalance   ?? 100.0),
            "Checking" => new CheckingAccount(dto.OwnerName,
                              dto.OverdraftLimit ?? 500.0,
                              dto.OverdraftFee   ?? 35.0),
            _          => throw new InvalidTransactionException($"Unknown account type: {dto.AccountType}")
        };

        account.RestoreBalance(dto.Balance);

        foreach (var tDto in dto.Transactions)
        {
            var t = new Transaction
            {
                Description = tDto.Description,
                Amount      = tDto.Amount,
                Type        = tDto.Type,
                Category    = tDto.Category,
                Date        = tDto.Date
            };
            account.Transactions.Add(t);
        }

        return account;
    }

    // ── Backup corrupt file ───────────────────────────────────────────
    private void BackupCorruptFile()
    {
        try
        {
            string backup = Path.Combine(
                Path.GetDirectoryName(_filePath) ?? ".",
                $"finance_data_corrupt_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            File.Move(_filePath, backup);
            Console.WriteLine($"  Backed up corrupt file to: {backup}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Could not back up file: {ex.Message}");
        }
    }
}
