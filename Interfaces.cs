namespace C_Sharp_Course_Intermediate_Task;

public interface IReport
{
    string GenerateReport();
    string GetSummaryLine();
}

public interface IExportable
{
    bool   ExportToFile(string filePath);
    string GetExportFormat();
}
