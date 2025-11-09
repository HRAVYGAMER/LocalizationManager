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
using LocalizationManager.Utils;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace LocalizationManager.Commands;

/// <summary>
/// Command to import translations from CSV format.
/// </summary>
public class ImportCommandSettings : BaseCommandSettings
{
    [CommandArgument(0, "<FILE>")]
    [Description("Input CSV file path")]
    public required string InputFile { get; set; }

    [CommandOption("--overwrite")]
    [Description("Overwrite existing values (default: skip conflicts)")]
    public bool Overwrite { get; set; }

    [CommandOption("--no-backup")]
    [Description("Skip creating backups before modifying files")]
    public bool NoBackup { get; set; }
}

public class ImportCommand : Command<ImportCommandSettings>
{
    public override int Execute(CommandContext context, ImportCommandSettings settings, CancellationToken cancellationToken = default)
    {
        var resourcePath = settings.GetResourcePath();

        if (!File.Exists(settings.InputFile))
        {
            AnsiConsole.MarkupLine($"[red]✗ File not found: {settings.InputFile}[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[blue]Reading CSV:[/] {settings.InputFile}");
        AnsiConsole.MarkupLine($"[blue]Target:[/] {resourcePath}");
        AnsiConsole.WriteLine();

        try
        {
            // Parse CSV
            var csvData = ParseCsv(settings.InputFile);

            if (!csvData.Any())
            {
                AnsiConsole.MarkupLine("[red]✗ CSV file is empty or invalid![/]");
                return 1;
            }

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
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]✗ Error parsing {lang.Name}: {ex.Message}[/]");
                    return 1;
                }
            }

            // Create backups
            if (!settings.NoBackup)
            {
                AnsiConsole.MarkupLine("[dim]Creating backups...[/]");
                var backupManager = new BackupManager();
                var filePaths = resourceFiles.Select(rf => rf.Language.FilePath).ToList();

                try
                {
                    backupManager.CreateBackups(filePaths);
                    AnsiConsole.MarkupLine("[dim green]✓ Backups created[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[yellow]⚠ Backup failed: {ex.Message}[/]");
                    if (!AnsiConsole.Confirm("Continue without backup?"))
                    {
                        return 1;
                    }
                }
                AnsiConsole.WriteLine();
            }

            // Import data
            var stats = ImportData(csvData, resourceFiles, settings.Overwrite);

            // Save files
            foreach (var resourceFile in resourceFiles)
            {
                try
                {
                    parser.Write(resourceFile);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]✗ Failed to write {resourceFile.Language.Name}: {ex.Message}[/]");
                    return 1;
                }
            }

            // Display summary
            AnsiConsole.WriteLine();
            var table = new Table();
            table.AddColumn("Statistic");
            table.AddColumn("Count");
            table.AddRow("Total rows", stats.TotalRows.ToString());
            table.AddRow("[green]Added[/]", $"[green]{stats.Added}[/]");
            table.AddRow("[yellow]Updated[/]", $"[yellow]{stats.Updated}[/]");
            table.AddRow("[dim]Skipped[/]", $"[dim]{stats.Skipped}[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green bold]✓ Import completed successfully[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Unexpected error: {ex.Message}[/]");
            return 1;
        }
    }

    private Dictionary<string, Dictionary<string, string>> ParseCsv(string filePath)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        var lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
        {
            return result;
        }

        // Parse header
        var header = ParseCsvLine(lines[0]);
        var keyIndex = Array.IndexOf(header, "Key");

        if (keyIndex == -1)
        {
            throw new InvalidOperationException("CSV must have a 'Key' column");
        }

        // Parse rows
        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);

            if (fields.Length <= keyIndex || string.IsNullOrWhiteSpace(fields[keyIndex]))
            {
                continue;
            }

            var key = fields[keyIndex];
            var values = new Dictionary<string, string>();

            for (int j = 0; j < Math.Min(header.Length, fields.Length); j++)
            {
                if (j != keyIndex && !string.IsNullOrEmpty(header[j]))
                {
                    values[header[j]] = fields[j];
                }
            }

            result[key] = values;
        }

        return result;
    }

    private string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        fields.Add(currentField.ToString());
        return fields.ToArray();
    }

    private ImportStats ImportData(
        Dictionary<string, Dictionary<string, string>> csvData,
        List<ResourceFile> resourceFiles,
        bool overwrite)
    {
        var stats = new ImportStats();

        foreach (var kvp in csvData)
        {
            var key = kvp.Key;
            var values = kvp.Value;

            stats.TotalRows++;

            foreach (var resourceFile in resourceFiles)
            {
                var langName = resourceFile.Language.Name;

                if (!values.ContainsKey(langName))
                {
                    continue; // No value for this language in CSV
                }

                var newValue = values[langName];
                var existingEntry = resourceFile.Entries.FirstOrDefault(e => e.Key == key);

                if (existingEntry != null)
                {
                    if (overwrite)
                    {
                        existingEntry.Value = newValue;
                        stats.Updated++;
                    }
                    else
                    {
                        stats.Skipped++;
                    }
                }
                else
                {
                    // Add new entry
                    resourceFile.Entries.Add(new ResourceEntry
                    {
                        Key = key,
                        Value = newValue,
                        Comment = values.ContainsKey("Comment") ? values["Comment"] : null
                    });
                    stats.Added++;
                }
            }
        }

        return stats;
    }

    private class ImportStats
    {
        public int TotalRows { get; set; }
        public int Added { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
    }
}
