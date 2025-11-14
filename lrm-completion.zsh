#compdef lrm
# Zsh completion script for lrm (Localization Resource Manager)
#
# Installation:
#   1. Copy this file to a directory in your $fpath (e.g., /usr/local/share/zsh/site-functions/_lrm)
#   2. Or add to your ~/.zshrc:
#      fpath=(~/.zsh/completions $fpath)
#      mkdir -p ~/.zsh/completions
#      cp lrm-completion.zsh ~/.zsh/completions/_lrm
#   3. Reload completions: compinit

_lrm() {
  local -a commands
  commands=(
    'validate:Validate resource files for missing translations and duplicates'
    'stats:Display translation statistics and coverage'
    'view:View details of a specific key or pattern'
    'add:Add a new key to all language files'
    'update:Update values for an existing key'
    'delete:Delete a key from all language files'
    'merge-duplicates:Merge duplicate key occurrences into single entries'
    'export:Export translations to CSV, JSON, or text'
    'import:Import translations from CSV'
    'edit:Launch interactive Terminal UI editor'
    'translate:Automatically translate keys using various providers'
    'config:Manage translation provider API keys'
    'scan:Scan source code for key usage and find unused keys'
    'check:Check for missing and unused keys in source code'
    'list-languages:List all detected language files'
    'add-language:Add a new language to the project'
    'remove-language:Remove a language from the project'
  )

  local -a global_opts
  global_opts=(
    '(-p --path)'{-p,--path}'[Path to directory containing .resx files]:directory:_directories'
    '(-h --help)'{-h,--help}'[Show help information]'
    '(-v --version)'{-v,--version}'[Show version information]'
  )

  local -a format_opts
  format_opts=(
    '--format[Output format]:format:(table json simple csv tui)'
  )

  local -a provider_opts
  provider_opts=(
    '--provider[Translation provider]:provider:(google deepl libretranslate ollama openai claude azureopenai azuretranslator)'
  )

  _arguments -C \
    '1: :->command' \
    '*:: :->args' && return 0

  case $state in
    command)
      _describe -t commands 'lrm commands' commands
      _arguments $global_opts
      ;;
    args)
      case $words[1] in
        validate)
          _arguments \
            $global_opts \
            $format_opts \
            '--missing-only[Show only missing translations]'
          ;;
        stats)
          _arguments \
            $global_opts \
            $format_opts
          ;;
        view)
          _arguments \
            $global_opts \
            $format_opts \
            '--show-comments[Display comments in output]' \
            '--regex[Treat pattern as regex]' \
            '--sort[Sort keys alphabetically]' \
            '--no-limit[Show all matching keys]' \
            '1:key pattern:'
          ;;
        add)
          _arguments \
            $global_opts \
            '(-l --lang)'{-l,--lang}'[Specific language to add to]:language:' \
            '--comment[Add comment to the key]:comment:' \
            '--no-backup[Skip creating backup files]' \
            '1:key:' \
            '*:values:'
          ;;
        update)
          _arguments \
            $global_opts \
            '(-l --lang)'{-l,--lang}'[Specific language to update]:language:' \
            '--comment[Update comment]:comment:' \
            '(-i --interactive)'{-i,--interactive}'[Interactive mode]' \
            '(-y --yes)'{-y,--yes}'[Skip confirmation prompts]' \
            '--no-backup[Skip creating backup files]' \
            '1:key:' \
            '*:values:'
          ;;
        delete)
          _arguments \
            $global_opts \
            '(-y --yes)'{-y,--yes}'[Skip confirmation prompts]' \
            '--no-backup[Skip creating backup files]' \
            '--all-duplicates[Delete all duplicate occurrences]' \
            '1:key:'
          ;;
        merge-duplicates)
          _arguments \
            $global_opts \
            '--all[Merge all duplicate keys]' \
            '--auto-first[Automatically select first occurrence]' \
            '(-y --yes)'{-y,--yes}'[Skip confirmation prompts]' \
            '--no-backup[Skip creating backup files]' \
            '1:key:'
          ;;
        export)
          _arguments \
            $global_opts \
            '(-o --output)'{-o,--output}'[Output file path]:file:_files' \
            $format_opts \
            '--include-status[Include translation status]'
          ;;
        import)
          _arguments \
            $global_opts \
            '--overwrite[Overwrite existing values]' \
            '--no-backup[Skip creating backup files]' \
            '1:csv file:_files -g "*.csv"'
          ;;
        edit)
          _arguments $global_opts
          ;;
        translate)
          _arguments \
            $global_opts \
            $provider_opts \
            '--target-languages[Target languages (comma-separated)]:languages:' \
            '--batch-size[Batch size for translations]:size:' \
            '--only-missing[Translate only missing values]' \
            '--overwrite[Overwrite existing translations]' \
            '--dry-run[Preview without saving]' \
            '--no-cache[Disable translation cache]' \
            '--source-language[Source language code]:language:' \
            $format_opts \
            '--config-file[Path to configuration file]:file:_files'
          ;;
        config)
          local -a config_subcommands
          config_subcommands=(
            'set-api-key:Set API key for a provider'
            'get-api-key:Get API key for a provider'
            'delete-api-key:Delete API key for a provider'
            'list-providers:List available translation providers'
          )
          _arguments \
            '1: :->config_subcommand' \
            '*:: :->config_args'

          case $state in
            config_subcommand)
              _describe -t config-subcommands 'config subcommands' config_subcommands
              ;;
            config_args)
              case $words[1] in
                set-api-key)
                  _arguments \
                    $provider_opts \
                    '(-k --key)'{-k,--key}'[API key value]:key:'
                  ;;
                get-api-key)
                  _arguments $provider_opts
                  ;;
                delete-api-key)
                  _arguments $provider_opts
                  ;;
                list-providers)
                  _arguments '(-h --help)'{-h,--help}'[Show help]'
                  ;;
              esac
              ;;
          esac
          ;;
        scan)
          _arguments \
            $global_opts \
            '--source-path[Path to source code]:directory:_directories' \
            '--exclude[Patterns to exclude]:pattern:' \
            '--strict[Strict mode for dynamic keys]' \
            '--show-unused[Show unused keys]' \
            '--show-missing[Show missing keys]' \
            '--show-references[Show key references]' \
            '--resource-classes[Resource class names]:classes:' \
            '--localization-methods[Localization method names]:methods:' \
            $format_opts
          ;;
        check)
          _arguments \
            $global_opts \
            '--source-path[Path to source code]:directory:_directories' \
            '--exclude[Patterns to exclude]:pattern:' \
            '--strict[Strict mode for dynamic keys]' \
            $format_opts
          ;;
        list-languages)
          _arguments \
            $global_opts \
            $format_opts
          ;;
        add-language)
          _arguments \
            $global_opts \
            '(-c --culture)'{-c,--culture}'[Culture code (e.g., fr, de)]:culture:' \
            '--base-name[Base name for resource files]:basename:' \
            '--copy-from[Copy entries from existing language]:language:' \
            '--empty[Create empty resource file]' \
            '--no-backup[Skip creating backup files]' \
            '(-y --yes)'{-y,--yes}'[Skip confirmation prompts]'
          ;;
        remove-language)
          _arguments \
            $global_opts \
            '(-c --culture)'{-c,--culture}'[Culture code to remove]:culture:' \
            '--base-name[Base name for resource files]:basename:' \
            '(-y --yes)'{-y,--yes}'[Skip confirmation prompts]' \
            '--no-backup[Skip creating backup files]'
          ;;
      esac
      ;;
  esac
}

_lrm "$@"
