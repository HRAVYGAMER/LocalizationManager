// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using LocalizationManager.Core.Configuration;
using LocalizationManager.Core.Translation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LocalizationManager.Commands.Config;

/// <summary>
/// Command to delete an API key from the secure credential store.
/// </summary>
public class DeleteApiKeyCommand : Command<DeleteApiKeyCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-p|--provider <PROVIDER>")]
        [Description("The translation provider (google, deepl, libretranslate)")]
        public string Provider { get; set; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings, System.Threading.CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate provider
            if (string.IsNullOrWhiteSpace(settings.Provider))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Provider name is required.");
                AnsiConsole.MarkupLine("Use: [cyan]lrm config delete-api-key --provider <provider>[/]");
                return 1;
            }

            if (!TranslationProviderFactory.IsProviderSupported(settings.Provider))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Unknown provider '{settings.Provider}'.");
                AnsiConsole.MarkupLine($"Supported providers: [cyan]{string.Join(", ", TranslationProviderFactory.GetSupportedProviders())}[/]");
                return 1;
            }

            // Delete the API key
            var deleted = SecureCredentialManager.DeleteApiKey(settings.Provider);

            if (deleted)
            {
                AnsiConsole.MarkupLine($"[green]✓[/] API key for '[cyan]{settings.Provider}[/]' deleted from secure credential store.");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]⚠[/] No API key for '[cyan]{settings.Provider}[/]' found in secure credential store.");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Note: This only removes the key from the secure credential store.[/]");
            AnsiConsole.MarkupLine("[dim]Environment variables and lrm.json configurations are not affected.[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}
