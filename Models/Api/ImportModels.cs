using System.ComponentModel.DataAnnotations;

namespace LocalizationManager.Models.Api;

// Request
public class ImportCsvRequest
{
    [Required(ErrorMessage = "CSV data is required")]
    [StringLength(10_485_760, MinimumLength = 1, ErrorMessage = "CSV data cannot exceed 10MB")]
    public string CsvData { get; set; } = string.Empty;

    public bool UpdateExisting { get; set; } = true;
    public bool DryRun { get; set; } = false;
}

// Response
public class ImportResult
{
    public bool Success { get; set; }
    public int AddedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public int TotalProcessed { get; set; }
    public bool DryRun { get; set; }
    public List<string> AddedKeys { get; set; } = new();
    public List<string> UpdatedKeys { get; set; } = new();
    public List<string>? Errors { get; set; }
}
