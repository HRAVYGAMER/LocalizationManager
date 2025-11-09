// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using LocalizationManager.Core;
using LocalizationManager.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text;

namespace LocalizationManager.Commands;

/// <summary>
/// Command to export resource files to CSV format.
/// </summary>
public class ExportCommandSettings : BaseCommandSettings
{
    [CommandOption("-o|--output <FILE>")]
    [Description("Output CSV file path (default: resources.csv)")]
    public string OutputFile { get; set; } = "resources.csv";

    [CommandOption("--include-status")]
    [Description("Include validation status in export")]
    public bool IncludeStatus { get; set; }
}

public class ExportCommand : Command<ExportCommandSettings>
{
    public override int Execute(CommandContext context, ExportCommandSettings settings, CancellationToken cancellationToken = default)
    {
        var resourcePath = settings.GetResourcePath();

        AnsiConsole.MarkupLine($"[blue]Scanning:[/] {resourcePath}");
        AnsiConsole.WriteLine();

        try
        {
            // Discover languages
            var discovery = new ResourceDiscovery();
            var languages = discovery.DiscoverLanguages(resourcePath);

            if (!languages.Any())
            {
                AnsiConsole.MarkupLine("[red]✗ No .resx files found![/]");
                return 1;
            }

            // Parse resource files
            var parser = new ResourceFileParser();
            var resourceFiles = new List<ResourceFile>();

            foreach (var lang in languages)
            {
                try
                {
                    var resourceFile = parser.Parse(lang);
                    resourceFiles.Add(resourceFile);
                    AnsiConsole.MarkupLine($"[dim]Parsed {lang.Name}: {resourceFile.Count} entries[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]✗ Error parsing {lang.Name}: {ex.Message}[/]");
                    return 1;
                }
            }

            AnsiConsole.WriteLine();

            // Validate if status requested
            Core.Models.ValidationResult? validationResult = null;
            if (settings.IncludeStatus)
            {
                var validator = new ResourceValidator();
                validationResult = validator.Validate(resourceFiles);
            }

            // Export to CSV
            ExportToCsv(resourceFiles, settings.OutputFile, validationResult);

            AnsiConsole.MarkupLine($"[green]✓ Exported {resourceFiles.First().Count} keys to:[/]");
            AnsiConsole.MarkupLine($"  [cyan]{Path.GetFullPath(settings.OutputFile)}[/]");

            return 0;
        }
        catch (DirectoryNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ {ex.Message}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Unexpected error: {ex.Message}[/]");
            return 1;
        }
    }

    private void ExportToCsv(List<ResourceFile> resourceFiles, string outputFile, Core.Models.ValidationResult? validationResult)
    {
        var sb = new StringBuilder();

        // Build header
        var header = new List<string> { "Key" };
        header.AddRange(resourceFiles.Select(rf => rf.Language.Name));
        if (validationResult != null)
        {
            header.Add("Status");
        }
        header.Add("Comment");

        sb.AppendLine(EscapeCsvRow(header));

        // Get all unique keys from default language
        var defaultFile = resourceFiles.FirstOrDefault(rf => rf.Language.IsDefault);
        if (defaultFile == null)
        {
            throw new InvalidOperationException("No default language found");
        }

        var allKeys = defaultFile.Entries.Select(e => e.Key).OrderBy(k => k).ToList();

        // Build rows
        foreach (var key in allKeys)
        {
            var row = new List<string> { key };

            // Add values for each language
            foreach (var resourceFile in resourceFiles)
            {
                var entry = resourceFile.Entries.FirstOrDefault(e => e.Key == key);
                row.Add(entry?.Value ?? string.Empty);
            }

            // Add status if requested
            if (validationResult != null)
            {
                var status = GetKeyStatus(key, resourceFiles, validationResult);
                row.Add(status);
            }

            // Add comment (from default language)
            var defaultEntry = defaultFile.Entries.FirstOrDefault(e => e.Key == key);
            row.Add(defaultEntry?.Comment ?? string.Empty);

            sb.AppendLine(EscapeCsvRow(row));
        }

        File.WriteAllText(outputFile, sb.ToString(), Encoding.UTF8);
    }

    private string GetKeyStatus(string key, List<ResourceFile> resourceFiles, Core.Models.ValidationResult validationResult)
    {
        var statuses = new List<string>();

        foreach (var rf in resourceFiles.Where(r => !r.Language.IsDefault))
        {
            var langCode = rf.Language.Code;

            if (validationResult.MissingKeys.ContainsKey(langCode) &&
                validationResult.MissingKeys[langCode].Contains(key))
            {
                statuses.Add($"Missing in {langCode}");
            }

            if (validationResult.EmptyValues.ContainsKey(langCode) &&
                validationResult.EmptyValues[langCode].Contains(key))
            {
                statuses.Add($"Empty in {langCode}");
            }

            if (validationResult.DuplicateKeys.ContainsKey(langCode) &&
                validationResult.DuplicateKeys[langCode].Contains(key))
            {
                statuses.Add($"Duplicate in {langCode}");
            }
        }

        return statuses.Any() ? string.Join("; ", statuses) : "OK";
    }

    private string EscapeCsvRow(List<string> fields)
    {
        var escapedFields = fields.Select(f =>
        {
            if (string.IsNullOrEmpty(f))
            {
                return string.Empty;
            }

            // Escape quotes and wrap in quotes if contains comma, quote, or newline
            if (f.Contains(',') || f.Contains('"') || f.Contains('\n') || f.Contains('\r'))
            {
                return $"\"{f.Replace("\"", "\"\"")}\"";
            }

            return f;
        });

        return string.Join(",", escapedFields);
    }
}
