// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using System.ComponentModel.DataAnnotations;

namespace LocalizationManager.Models.Api;

/// <summary>
/// Request model for searching/filtering resource keys
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// The search pattern (text, wildcard, or regex depending on FilterMode)
    /// </summary>
    [StringLength(500, ErrorMessage = "Search pattern cannot exceed 500 characters")]
    public string? Pattern { get; set; }

    /// <summary>
    /// The filtering mode: "substring" (default), "wildcard", or "regex"
    /// </summary>
    [RegularExpression(@"^(substring|wildcard|regex)$", ErrorMessage = "FilterMode must be 'substring', 'wildcard', or 'regex'")]
    public string FilterMode { get; set; } = "substring";

    /// <summary>
    /// Whether the search should be case-sensitive (default: false)
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Search scope: "keys", "values", "keysAndValues" (default), "comments", or "all"
    /// </summary>
    [RegularExpression(@"^(keys|values|keysAndValues|comments|all)$", ErrorMessage = "SearchScope must be 'keys', 'values', 'keysAndValues', 'comments', or 'all'")]
    public string SearchScope { get; set; } = "keysAndValues";

    /// <summary>
    /// Optional status filters: "missing", "extra", "duplicates", "unused"
    /// Multiple can be specified (OR logic)
    /// </summary>
    public List<string>? StatusFilters { get; set; }

    /// <summary>
    /// Maximum number of results to return (for pagination)
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Limit must be between 1 and 10000")]
    public int? Limit { get; set; }

    /// <summary>
    /// Number of results to skip (for pagination)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative")]
    public int? Offset { get; set; }
}

/// <summary>
/// Response model for search results
/// </summary>
public class SearchResponse
{
    /// <summary>
    /// The filtered resource keys matching the search criteria
    /// </summary>
    public List<ResourceKeyInfo> Results { get; set; } = new();

    /// <summary>
    /// Total number of keys before filtering
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of keys matching the filter (before pagination)
    /// </summary>
    public int FilteredCount { get; set; }

    /// <summary>
    /// The filter mode that was applied
    /// </summary>
    public string AppliedFilterMode { get; set; } = "substring";
}
