// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using LocalizationManager.Core;

namespace LocalizationManager.Commands;

/// <summary>
/// Settings for the list-languages command.
/// </summary>
public class ListLanguagesCommandSettings : BaseCommandSettings
{
    [CommandOption("--format <FORMAT>")]
    [Description("Output format: table, simple, json")]
    [DefaultValue("table")]
    public string Format { get; set; } = "table";
}

/// <summary>
/// Command to list all available language resource files.
/// </summary>
public class ListLanguagesCommand : Command<ListLanguagesCommandSettings>
{
    public override int Execute(CommandContext context, ListLanguagesCommandSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var discovery = new ResourceDiscovery();
            var parser = new ResourceFileParser();

            // Discover languages
            var languages = discovery.DiscoverLanguages(settings.GetResourcePath());
            if (languages.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No resource files found in the specified path[/]");
                return 0;
            }

            // Group by base name
            var grouped = languages.GroupBy(l => l.BaseName).ToList();

            // Parse files and collect statistics
            var languageStats = new List<LanguageStats>();
            int totalDefaultEntries = 0;

            foreach (var group in grouped)
            {
                // Find default language (no culture code)
                var defaultLang = group.FirstOrDefault(l => string.IsNullOrEmpty(l.Code));
                if (defaultLang != null)
                {
                    var defaultFile = parser.Parse(defaultLang);
                    totalDefaultEntries = defaultFile.Entries.Count;
                }

                foreach (var lang in group)
                {
                    var resourceFile = parser.Parse(lang);
                    var entryCount = resourceFile.Entries.Count;
                    var coverage = totalDefaultEntries > 0
                        ? (int)((double)entryCount / totalDefaultEntries * 100)
                        : 100;

                    languageStats.Add(new LanguageStats
                    {
                        BaseName = lang.BaseName,
                        LanguageName = string.IsNullOrEmpty(lang.Code)
                            ? "Default"
                            : lang.Name,
                        Code = string.IsNullOrEmpty(lang.Code)
                            ? "(default)"
                            : lang.Code,
                        FileName = Path.GetFileName(lang.FilePath),
                        EntryCount = entryCount,
                        Coverage = coverage
                    });
                }
            }

            // Output based on format
            switch (settings.Format.ToLower())
            {
                case "table":
                    DisplayTable(languageStats);
                    break;
                case "simple":
                    DisplaySimple(languageStats);
                    break;
                case "json":
                    DisplayJson(languageStats);
                    break;
                default:
                    AnsiConsole.MarkupLine($"[red]Invalid format: {settings.Format}[/]");
                    AnsiConsole.MarkupLine("[grey]Valid formats: table, simple, json[/]");
                    return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            return 1;
        }
    }

    private void DisplayTable(List<LanguageStats> stats)
    {
        // Group by base name
        var grouped = stats.GroupBy(s => s.BaseName);

        foreach (var group in grouped)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Resource Files: {group.Key}[/]");
            AnsiConsole.WriteLine();

            var table = new Table();
            table.AddColumn("Language");
            table.AddColumn("Code");
            table.AddColumn("File");
            table.AddColumn(new TableColumn("Entries").RightAligned());
            table.AddColumn(new TableColumn("Coverage").RightAligned());

            foreach (var stat in group.OrderBy(s => s.Code))
            {
                var coverageDisplay = stat.Coverage == 100
                    ? $"[green]{stat.Coverage}% ✓[/]"
                    : $"[yellow]{stat.Coverage}%[/]";

                table.AddRow(
                    stat.LanguageName,
                    stat.Code,
                    stat.FileName,
                    stat.EntryCount.ToString(),
                    coverageDisplay
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]Total: {group.Count()} languages[/]");
        }
    }

    private void DisplaySimple(List<LanguageStats> stats)
    {
        var grouped = stats.GroupBy(s => s.BaseName);

        foreach (var group in grouped)
        {
            Console.WriteLine($"Resource Files: {group.Key}");
            foreach (var stat in group.OrderBy(s => s.Code))
            {
                Console.WriteLine($"  {stat.Code,-12} {stat.LanguageName,-20} {stat.EntryCount,5} entries  {stat.Coverage,3}%");
            }
            Console.WriteLine($"Total: {group.Count()} languages");
            Console.WriteLine();
        }
    }

    private void DisplayJson(List<LanguageStats> stats)
    {
        var output = stats.Select(s => new
        {
            baseName = s.BaseName,
            language = s.LanguageName,
            code = s.Code,
            fileName = s.FileName,
            entries = s.EntryCount,
            coverage = s.Coverage
        });

        var json = JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.WriteLine(json);
    }

    private class LanguageStats
    {
        public required string BaseName { get; set; }
        public required string LanguageName { get; set; }
        public required string Code { get; set; }
        public required string FileName { get; set; }
        public int EntryCount { get; set; }
        public int Coverage { get; set; }
    }
}
