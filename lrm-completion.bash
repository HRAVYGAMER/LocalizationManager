#!/usr/bin/env bash
# Bash completion script for lrm (Localization Resource Manager)
#
# Installation:
#   1. Copy this file to /etc/bash_completion.d/lrm or ~/.local/share/bash-completion/completions/lrm
#   2. Or source it in your ~/.bashrc: source /path/to/lrm-completion.bash
#   3. Restart your shell or run: source ~/.bashrc

_lrm_completions() {
    local cur prev opts base
    COMPREPLY=()
    cur="${COMP_WORDS[COMP_CWORD]}"
    prev="${COMP_WORDS[COMP_CWORD-1]}"

    # Main commands
    local commands="validate stats view add update delete export import edit translate config"

    # Global options
    local global_opts="--path -p --help -h --version -v"

    # Command-specific options
    local validate_opts="--path -p --format --missing-only --help -h"
    local stats_opts="--path -p --format --help -h"
    local view_opts="--path -p --show-comments --format --regex --sort --no-limit --help -h"
    local add_opts="--path -p --lang -l --comment --no-backup --help -h"
    local update_opts="--path -p --lang -l --comment --interactive -i --yes -y --no-backup --help -h"
    local delete_opts="--path -p --yes -y --no-backup --help -h"
    local export_opts="--path -p --output -o --format --include-status --help -h"
    local import_opts="--path -p --overwrite --no-backup --help -h"
    local edit_opts="--path -p --help -h"
    local translate_opts="--path -p --provider --target-languages --batch-size --only-missing --format --config-file --help -h"
    local config_opts="--path -p --get --set --list --help -h"

    # Format options
    local format_opts="table json simple csv tui"

    # Translation providers
    local provider_opts="google deepl libretranslate"

    # Get the command (first non-option word)
    local command=""
    for ((i=1; i<COMP_CWORD; i++)); do
        if [[ "${COMP_WORDS[i]}" != -* ]]; then
            command="${COMP_WORDS[i]}"
            break
        fi
    done

    # Complete based on previous word
    case "${prev}" in
        --path|-p)
            # Complete directories
            COMPREPLY=( $(compgen -d -- "${cur}") )
            return 0
            ;;
        --format)
            # Complete format options
            COMPREPLY=( $(compgen -W "${format_opts}" -- "${cur}") )
            return 0
            ;;
        --provider)
            # Complete translation providers
            COMPREPLY=( $(compgen -W "${provider_opts}" -- "${cur}") )
            return 0
            ;;
        --output|-o|--config-file)
            # Complete files for output
            COMPREPLY=( $(compgen -f -- "${cur}") )
            return 0
            ;;
        --target-languages)
            # Suggest language codes (common ones)
            COMPREPLY=( $(compgen -W "en es fr de it pt ja zh ko ru ar" -- "${cur}") )
            return 0
            ;;
        --batch-size)
            # Suggest batch sizes
            COMPREPLY=( $(compgen -W "5 10 20 50 100" -- "${cur}") )
            return 0
            ;;
        --get|--set)
            # Configuration keys
            COMPREPLY=( $(compgen -W "DefaultLanguageCode" -- "${cur}") )
            return 0
            ;;
        lrm)
            # Complete main commands
            COMPREPLY=( $(compgen -W "${commands} ${global_opts}" -- "${cur}") )
            return 0
            ;;
    esac

    # Complete based on current command
    if [[ -z "${command}" ]]; then
        # No command yet, suggest commands
        COMPREPLY=( $(compgen -W "${commands} ${global_opts}" -- "${cur}") )
        return 0
    fi

    case "${command}" in
        validate)
            COMPREPLY=( $(compgen -W "${validate_opts}" -- "${cur}") )
            ;;
        stats)
            COMPREPLY=( $(compgen -W "${stats_opts}" -- "${cur}") )
            ;;
        view)
            if [[ "${cur}" == -* ]]; then
                COMPREPLY=( $(compgen -W "${view_opts}" -- "${cur}") )
            else
                # Key name argument
                COMPREPLY=()
            fi
            ;;
        add)
            if [[ "${cur}" == -* ]]; then
                COMPREPLY=( $(compgen -W "${add_opts}" -- "${cur}") )
            else
                # Key name argument
                COMPREPLY=()
            fi
            ;;
        update)
            if [[ "${cur}" == -* ]]; then
                COMPREPLY=( $(compgen -W "${update_opts}" -- "${cur}") )
            else
                # Key name argument
                COMPREPLY=()
            fi
            ;;
        delete)
            if [[ "${cur}" == -* ]]; then
                COMPREPLY=( $(compgen -W "${delete_opts}" -- "${cur}") )
            else
                # Key name argument
                COMPREPLY=()
            fi
            ;;
        export)
            COMPREPLY=( $(compgen -W "${export_opts}" -- "${cur}") )
            ;;
        import)
            if [[ "${cur}" == -* ]]; then
                COMPREPLY=( $(compgen -W "${import_opts}" -- "${cur}") )
            else
                # Complete CSV files
                COMPREPLY=( $(compgen -f -X '!*.csv' -- "${cur}") )
            fi
            ;;
        edit)
            COMPREPLY=( $(compgen -W "${edit_opts}" -- "${cur}") )
            ;;
        translate)
            COMPREPLY=( $(compgen -W "${translate_opts}" -- "${cur}") )
            ;;
        config)
            COMPREPLY=( $(compgen -W "${config_opts}" -- "${cur}") )
            ;;
        *)
            COMPREPLY=()
            ;;
    esac
}

# Register the completion function
complete -F _lrm_completions lrm
