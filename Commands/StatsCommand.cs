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
using Spectre.Console;
using Spectre.Console.Cli;

namespace LocalizationManager.Commands;

/// <summary>
/// Command to display statistics about resource files and translation coverage.
/// </summary>
public class StatsCommand : Command<BaseCommandSettings>
{
    public override int Execute(CommandContext context, BaseCommandSettings settings, CancellationToken cancellationToken = default)
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
            var resourceFiles = new List<LocalizationManager.Core.Models.ResourceFile>();

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

            // Display statistics
            DisplayStatistics(resourceFiles);

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

    private void DisplayStatistics(List<LocalizationManager.Core.Models.ResourceFile> resourceFiles)
    {
        // Create statistics table
        var table = new Table();
        table.Title = new TableTitle("[bold]Localization Statistics[/]");
        table.AddColumn("Language");
        table.AddColumn("Total Keys");
        table.AddColumn("Completed");
        table.AddColumn("Empty");
        table.AddColumn("Coverage");
        table.AddColumn("File Size");

        foreach (var rf in resourceFiles)
        {
            var fileInfo = new FileInfo(rf.Language.FilePath);
            var fileSizeKb = (fileInfo.Length / 1024.0).ToString("F1");
            var emptyCount = rf.Count - rf.CompletedCount;

            // Color code coverage
            var coverageColor = rf.CompletionPercentage >= 100 ? "green" :
                               rf.CompletionPercentage >= 80 ? "yellow" :
                               "red";

            var languageDisplay = rf.Language.IsDefault
                ? $"[yellow]{rf.Language.Name}[/]"
                : rf.Language.Name;

            table.AddRow(
                languageDisplay,
                rf.Count.ToString(),
                rf.CompletedCount.ToString(),
                emptyCount.ToString(),
                $"[{coverageColor}]{rf.CompletionPercentage:F1}%[/]",
                $"{fileSizeKb} KB"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        // Display coverage chart
        if (resourceFiles.Count > 1)
        {
            AnsiConsole.MarkupLine("[bold]Translation Coverage:[/]");
            foreach (var rf in resourceFiles.Where(r => !r.Language.IsDefault))
            {
                var barChart = new BarChart()
                    .Width(60)
                    .Label($"[bold]{rf.Language.Name}[/]")
                    .AddItem("Completed", rf.CompletedCount, Color.Green)
                    .AddItem("Empty", rf.Count - rf.CompletedCount, Color.Red);

                AnsiConsole.Write(barChart);
            }
        }
    }
}
