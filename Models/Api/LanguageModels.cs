using System.ComponentModel.DataAnnotations;

namespace LocalizationManager.Models.Api;

// Request
public class AddLanguageRequest
{
    [Required(ErrorMessage = "Culture code is required")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Culture code must be between 2 and 20 characters")]
    [RegularExpression(@"^[a-zA-Z]{2,3}(-[a-zA-Z]{2,4})?$", ErrorMessage = "Invalid culture code format (e.g., 'en', 'en-US', 'zh-Hans')")]
    public string CultureCode { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "CopyFrom culture code cannot exceed 20 characters")]
    [RegularExpression(@"^[a-zA-Z]{2,3}(-[a-zA-Z]{2,4})?$", ErrorMessage = "Invalid culture code format")]
    public string? CopyFrom { get; set; }

    public bool Empty { get; set; }
}

// Response
public class LanguagesResponse
{
    public List<LanguageInfo> Languages { get; set; } = new();
}

public class LanguageInfo
{
    public string? Code { get; set; }
    public bool IsDefault { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public int TotalKeys { get; set; }
    public int TranslatedKeys { get; set; }
    public double Coverage { get; set; }
}

public class AddLanguageResponse
{
    public bool Success { get; set; }
    public string CultureCode { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

public class RemoveLanguageResponse
{
    public bool Success { get; set; }
    public string CultureCode { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
