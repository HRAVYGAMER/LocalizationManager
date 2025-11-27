// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using LocalizationManager.Core.Models;
using LocalizationManager.Models.Api;

namespace LocalizationManager.UI.Filters;

/// <summary>
/// Service for filtering resource entries in the TUI with support for wildcards, regex, and advanced criteria
/// </summary>
public class ResourceFilterService
{
    private readonly Dictionary<string, Regex> _regexCache = new();
    private const int RegexTimeoutMs = 2000; // 2 second timeout to prevent ReDoS

    /// <summary>
    /// Filters rows in a DataTable based on the provided criteria
    /// </summary>
    /// <param name="table">The DataTable containing resource data</param>
    /// <param name="criteria">The filter criteria to apply</param>
    /// <returns>List of row indices that match the filter</returns>
    public List<int> FilterRows(DataTable table, FilterCriteria criteria)
    {
        var matchingIndices = new List<int>();

        // Empty search text = show all rows
        if (string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                matchingIndices.Add(i);
            }
            return matchingIndices;
        }

        // Compile pattern once based on mode
        Regex? regex = null;
        try
        {
            regex = GetOrCreateRegex(criteria);
        }
        catch (RegexParseException)
        {
            // Invalid regex pattern - return empty results
            return matchingIndices;
        }
        catch (ArgumentException)
        {
            // Invalid regex pattern - return empty results
            return matchingIndices;
        }

        // Check if this is double-row mode (has _RowType column)
        var isDoubleRowMode = table.Columns.Contains("_RowType");

        if (isDoubleRowMode)
        {
            // In double-row mode, process pairs of rows together (value + comment)
            for (int i = 0; i < table.Rows.Count; i += 2)
            {
                if (i + 1 >= table.Rows.Count) break; // Safety check

                var valueRow = table.Rows[i];
                var commentRow = table.Rows[i + 1];

                // Check if either value or comment row matches
                var valueMatches = MatchesFilterForDoubleRow(valueRow, criteria, regex, isValueRow: true);
                var commentMatches = MatchesFilterForDoubleRow(commentRow, criteria, regex, isValueRow: false);

                // If either matches, include both rows (value + comment)
                if (valueMatches || commentMatches)
                {
                    matchingIndices.Add(i);     // Value row
                    matchingIndices.Add(i + 1); // Comment row
                }
            }
        }
        else
        {
            // Single-row mode: use existing logic
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (MatchesFilter(table.Rows[i], criteria, regex))
                {
                    matchingIndices.Add(i);
                }
            }
        }

        return matchingIndices;
    }

    /// <summary>
    /// Checks if a row matches the filter criteria in double-row mode
    /// </summary>
    private bool MatchesFilterForDoubleRow(DataRow row, FilterCriteria criteria, Regex? regex, bool isValueRow)
    {
        if (string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            return true;
        }

        regex ??= GetOrCreateRegex(criteria);

        // Get the actual key from _LogicalKey metadata
        var key = row["_LogicalKey"]?.ToString() ?? string.Empty;

        // For value rows: check key and/or values
        if (isValueRow)
        {
            // Check key if scope includes keys
            if (criteria.Scope == SearchScope.KeysOnly ||
                criteria.Scope == SearchScope.KeysAndValues ||
                criteria.Scope == SearchScope.All)
            {
                if (MatchesPattern(key, criteria.SearchText, regex, criteria))
                {
                    return true;
                }
            }

            // Check values if scope includes values
            if (criteria.Scope == SearchScope.KeysAndValues || criteria.Scope == SearchScope.All)
            {
                var translationColumns = row.Table.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName != "Key" && !c.ColumnName.StartsWith("_"))
                    .Select(c => c.ColumnName);

                foreach (var columnName in translationColumns)
                {
                    if (criteria.TargetColumn != null && criteria.TargetColumn != columnName)
                    {
                        continue;
                    }

                    var value = row[columnName]?.ToString() ?? string.Empty;
                    if (MatchesPattern(value, criteria.SearchText, regex, criteria))
                    {
                        return true;
                    }
                }
            }
        }
        // For comment rows: check comments if scope includes comments
        else if (criteria.Scope == SearchScope.Comments || criteria.Scope == SearchScope.All)
        {
            var translationColumns = row.Table.Columns.Cast<DataColumn>()
                .Where(c => c.ColumnName != "Key" && !c.ColumnName.StartsWith("_"))
                .Select(c => c.ColumnName);

            foreach (var columnName in translationColumns)
            {
                var comment = row[columnName]?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(comment) &&
                    MatchesPattern(comment, criteria.SearchText, regex, criteria))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a specific row matches the filter criteria
    /// </summary>
    public bool MatchesFilter(DataRow row, FilterCriteria criteria, Regex? regex = null)
    {
        if (string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            return true;
        }

        regex ??= GetOrCreateRegex(criteria);

        // Check key column if searching keys
        if (criteria.Scope == SearchScope.KeysOnly || criteria.Scope == SearchScope.KeysAndValues || criteria.Scope == SearchScope.All)
        {
            var keyValue = row["Key"]?.ToString() ?? string.Empty;

            // Strip warning marker if present
            if (keyValue.StartsWith("⚠ "))
            {
                keyValue = keyValue.Substring(2);
            }

            if (MatchesPattern(keyValue, criteria.SearchText, regex, criteria))
            {
                return true;
            }
        }

        // Check translation columns if searching values
        if (criteria.Scope == SearchScope.KeysAndValues || criteria.Scope == SearchScope.All)
        {
            // Get all columns except "Key" and internal columns (starting with "_")
            var translationColumns = row.Table.Columns.Cast<DataColumn>()
                .Where(c => c.ColumnName != "Key" &&
                           !c.ColumnName.StartsWith("_"))
                .Select(c => c.ColumnName);

            foreach (var columnName in translationColumns)
            {
                // If target column specified, only check that column
                if (criteria.TargetColumn != null && criteria.TargetColumn != columnName)
                {
                    continue;
                }

                var value = row[columnName]?.ToString() ?? string.Empty;
                if (MatchesPattern(value, criteria.SearchText, regex, criteria))
                {
                    return true;
                }
            }
        }

        // Check comment columns if searching comments
        if (criteria.Scope == SearchScope.Comments || criteria.Scope == SearchScope.All)
        {
            // Get all comment columns (_Comment_*)
            var commentColumns = row.Table.Columns.Cast<DataColumn>()
                .Where(c => c.ColumnName.StartsWith("_Comment_"))
                .Select(c => c.ColumnName);

            foreach (var columnName in commentColumns)
            {
                var value = row[columnName]?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value) && MatchesPattern(value, criteria.SearchText, regex, criteria))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a value matches the search pattern
    /// </summary>
    private bool MatchesPattern(string value, string pattern, Regex regex, FilterCriteria criteria)
    {
        try
        {
            switch (criteria.Mode)
            {
                case FilterMode.Substring:
                    var comparison = criteria.CaseSensitive
                        ? StringComparison.Ordinal
                        : StringComparison.OrdinalIgnoreCase;
                    return value.Contains(pattern, comparison);

                case FilterMode.Wildcard:
                case FilterMode.Regex:
                    return regex.IsMatch(value);

                default:
                    return false;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a cached regex or creates a new one based on criteria
    /// </summary>
    private Regex GetOrCreateRegex(FilterCriteria criteria)
    {
        var cacheKey = $"{criteria.Mode}|{criteria.SearchText}|{criteria.CaseSensitive}";

        if (_regexCache.TryGetValue(cacheKey, out var cachedRegex))
        {
            return cachedRegex;
        }

        string pattern;
        if (criteria.Mode == FilterMode.Wildcard)
        {
            // Convert wildcard to regex
            pattern = ConvertWildcardToRegex(criteria.SearchText);
        }
        else
        {
            // Use pattern as-is for regex mode
            pattern = criteria.SearchText;
        }

        var options = criteria.CaseSensitive
            ? RegexOptions.None
            : RegexOptions.IgnoreCase;

        var regex = new Regex(pattern, options, TimeSpan.FromMilliseconds(RegexTimeoutMs));

        // Cache it for reuse
        if (_regexCache.Count > 100)
        {
            _regexCache.Clear(); // Prevent unbounded growth
        }
        _regexCache[cacheKey] = regex;

        return regex;
    }

    /// <summary>
    /// Filters a list of ResourceKeyInfo based on the provided criteria (for API use).
    /// This is a DataTable-independent overload for use with Web API.
    /// </summary>
    /// <param name="keys">The collection of resource keys to filter</param>
    /// <param name="criteria">The filter criteria to apply</param>
    /// <returns>List of keys that match the filter</returns>
    public List<ResourceKeyInfo> FilterKeys(IEnumerable<ResourceKeyInfo> keys, FilterCriteria criteria)
    {
        if (string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            return keys.ToList();
        }

        Regex? regex = null;
        try
        {
            regex = GetOrCreateRegex(criteria);
        }
        catch (RegexParseException)
        {
            // Invalid regex pattern - return empty results
            return new List<ResourceKeyInfo>();
        }
        catch (ArgumentException)
        {
            // Invalid regex pattern - return empty results
            return new List<ResourceKeyInfo>();
        }

        return keys.Where(key => MatchesKey(key, criteria, regex)).ToList();
    }

    /// <summary>
    /// Checks if a ResourceKeyInfo matches the filter criteria
    /// </summary>
    private bool MatchesKey(ResourceKeyInfo key, FilterCriteria criteria, Regex? regex)
    {
        regex ??= GetOrCreateRegex(criteria);

        // Check key name if scope includes keys
        if (criteria.Scope == SearchScope.KeysOnly ||
            criteria.Scope == SearchScope.KeysAndValues ||
            criteria.Scope == SearchScope.All)
        {
            if (MatchesPattern(key.Key, criteria.SearchText, regex, criteria))
            {
                return true;
            }
        }

        // Check values if scope includes values
        if (criteria.Scope == SearchScope.KeysAndValues || criteria.Scope == SearchScope.All)
        {
            foreach (var value in key.Values.Values)
            {
                if (value != null && MatchesPattern(value, criteria.SearchText, regex, criteria))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Clears the regex cache (call when memory pressure detected)
    /// </summary>
    public void ClearCache()
    {
        _regexCache.Clear();
    }

    /// <summary>
    /// Detects if a pattern contains wildcard characters (* or ?) that should be converted to regex.
    /// Handles backslash escaping for literal wildcard characters.
    /// Reused from ViewCommand.cs
    /// </summary>
    public static bool IsWildcardPattern(string pattern)
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];

            // Check if this is an escaped character
            if (c == '\\' && i + 1 < pattern.Length)
            {
                i++; // Skip next character
                continue;
            }

            // Check for unescaped wildcards
            if (c == '*' || c == '?')
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Converts a wildcard pattern to a regex pattern.
    /// Supports:
    /// - * for zero or more characters
    /// - ? for exactly one character
    /// - \* and \? for literal asterisk and question mark
    /// Reused from ViewCommand.cs
    /// </summary>
    public static string ConvertWildcardToRegex(string wildcardPattern)
    {
        var result = new StringBuilder();
        result.Append('^'); // Anchor to start

        for (int i = 0; i < wildcardPattern.Length; i++)
        {
            char c = wildcardPattern[i];

            if (c == '\\' && i + 1 < wildcardPattern.Length)
            {
                char next = wildcardPattern[i + 1];
                if (next == '*' || next == '?')
                {
                    // Escaped wildcard - treat as literal
                    result.Append('\\').Append(next);
                    i++; // Skip next character
                }
                else
                {
                    // Other escaped character - escape for regex
                    result.Append(Regex.Escape(c.ToString()));
                }
            }
            else if (c == '*')
            {
                // Wildcard: match any characters
                result.Append(".*");
            }
            else if (c == '?')
            {
                // Wildcard: match single character
                result.Append('.');
            }
            else
            {
                // Regular character - escape special regex characters
                result.Append(Regex.Escape(c.ToString()));
            }
        }

        result.Append('$'); // Anchor to end
        return result.ToString();
    }

    /// <summary>
    /// Parse and normalize comma-separated culture codes.
    /// Reused from ViewCommand.cs
    /// </summary>
    public static List<string> ParseCultureCodes(string? cultureString)
    {
        if (string.IsNullOrWhiteSpace(cultureString))
        {
            return new List<string>();
        }

        return cultureString
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToLowerInvariant())
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Detect keys that exist in filtered resource files but not in the default file.
    /// Reused from ViewCommand.cs
    /// </summary>
    public static Dictionary<string, List<string>> DetectExtraKeysInFilteredFiles(
        ResourceFile defaultFile,
        List<ResourceFile> filteredResourceFiles)
    {
        var result = new Dictionary<string, List<string>>();

        // Get all keys from default file for fast lookup
        var defaultKeys = new HashSet<string>(defaultFile.Entries.Select(e => e.Key));

        // Check each filtered resource file (excluding default)
        foreach (var resourceFile in filteredResourceFiles.Where(rf => !rf.Language.IsDefault))
        {
            var extraKeys = resourceFile.Entries
                .Select(e => e.Key)
                .Where(key => !defaultKeys.Contains(key))
                .ToList();

            if (extraKeys.Any())
            {
                result[resourceFile.Language.Name] = extraKeys;
            }
        }

        return result;
    }
}
