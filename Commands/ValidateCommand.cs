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
/// Command to validate resource files for missing keys, duplicates, and empty values.
/// </summary>
public class ValidateCommand : Command<BaseCommandSettings>
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

            AnsiConsole.MarkupLine($"[green]✓ Found {languages.Count} language(s):[/]");
            foreach (var lang in languages)
            {
                var marker = lang.IsDefault ? "[yellow](default)[/]" : "";
                AnsiConsole.MarkupLine($"  • {lang.Name} {marker}");
            }
            AnsiConsole.WriteLine();

            // Parse resource files
            var parser = new ResourceFileParser();
            var resourceFiles = new List<LocalizationManager.Core.Models.ResourceFile>();

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

            // Validate
            var validator = new ResourceValidator();
            var validationResult = validator.Validate(resourceFiles);

            // Display results
            DisplayValidationResults(validationResult);

            return validationResult.IsValid ? 0 : 1;
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

    private void DisplayValidationResults(LocalizationManager.Core.Models.ValidationResult result)
    {
        if (result.IsValid)
        {
            AnsiConsole.MarkupLine("[green bold]✓ All validations passed![/]");
            AnsiConsole.MarkupLine("[green]No issues found.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[yellow bold]⚠ Validation found {result.TotalIssues} issue(s)[/]");
        AnsiConsole.WriteLine();

        // Missing keys
        if (result.MissingKeys.Any(kv => kv.Value.Any()))
        {
            var table = new Table();
            table.Title = new TableTitle("[red]Missing Translations[/]");
            table.AddColumn("Language");
            table.AddColumn("Missing Keys");

            foreach (var kvp in result.MissingKeys.Where(kv => kv.Value.Any()))
            {
                table.AddRow(
                    kvp.Key,
                    string.Join(", ", kvp.Value.Take(10)) + (kvp.Value.Count > 10 ? $" ... ({kvp.Value.Count - 10} more)" : "")
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        // Extra keys
        if (result.ExtraKeys.Any(kv => kv.Value.Any()))
        {
            var table = new Table();
            table.Title = new TableTitle("[yellow]Extra Keys (not in default)[/]");
            table.AddColumn("Language");
            table.AddColumn("Extra Keys");

            foreach (var kvp in result.ExtraKeys.Where(kv => kv.Value.Any()))
            {
                table.AddRow(
                    kvp.Key,
                    string.Join(", ", kvp.Value.Take(10)) + (kvp.Value.Count > 10 ? $" ... ({kvp.Value.Count - 10} more)" : "")
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        // Duplicate keys
        if (result.DuplicateKeys.Any(kv => kv.Value.Any()))
        {
            var table = new Table();
            table.Title = new TableTitle("[red]Duplicate Keys[/]");
            table.AddColumn("Language");
            table.AddColumn("Duplicate Keys");

            foreach (var kvp in result.DuplicateKeys.Where(kv => kv.Value.Any()))
            {
                table.AddRow(
                    kvp.Key,
                    string.Join(", ", kvp.Value)
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        // Empty values
        if (result.EmptyValues.Any(kv => kv.Value.Any()))
        {
            var table = new Table();
            table.Title = new TableTitle("[yellow]Empty Values[/]");
            table.AddColumn("Language");
            table.AddColumn("Empty Keys");

            foreach (var kvp in result.EmptyValues.Where(kv => kv.Value.Any()))
            {
                table.AddRow(
                    kvp.Key,
                    string.Join(", ", kvp.Value.Take(10)) + (kvp.Value.Count > 10 ? $" ... ({kvp.Value.Count - 10} more)" : "")
                );
            }

            AnsiConsole.Write(table);
        }
    }
}
