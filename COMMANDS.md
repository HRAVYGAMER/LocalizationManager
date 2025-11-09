# Complete Command Reference

This document provides detailed information about all LRM commands, their options, and usage examples.

## Table of Contents

- [Global Options](#global-options)
- [validate](#validate) - Validate resource files
- [stats](#stats) - Display statistics
- [view](#view) - View key details
- [add](#add) - Add new keys
- [update](#update) - Update existing keys
- [delete](#delete) - Delete keys
- [export](#export) - Export to CSV
- [import](#import) - Import from CSV
- [edit](#edit) - Interactive TUI editor

---

## Global Options

All commands support these options:
- `-p, --path <PATH>` - Path to Resources folder (default: current directory)
- `-h, --help` - Show command help

---

## validate

**Description:** Validate resource files for issues including missing translations, duplicates, empty values, and extra keys.

**Arguments:** None

**Options:**
- `-p, --path <PATH>` - Resource folder path

**What it checks:**
- Missing keys in translation files
- Duplicate keys within files
- Empty values
- Extra keys not in default language

**Exit codes:**
- `0` - No issues found
- `1` - Validation issues detected

**Examples:**
```bash
# Validate current directory
lrm validate

# Validate specific path
lrm validate --path ../Resources

# Short form
lrm validate -p ./Resources
```

**Output example:**
```
✓ All validations passed!

Summary:
  ✓ No missing keys
  ✓ No duplicate keys
  ✓ No empty values
  ✓ No extra keys
```

---

## stats

**Description:** Display translation statistics and coverage with charts and tables.

**Arguments:** None

**Options:**
- `-p, --path <PATH>` - Resource folder path

**Information shown:**
- Total keys per language
- Missing keys count
- Translation coverage percentage
- Visual progress bars
- Per-language statistics table

**Examples:**
```bash
# Show stats for current directory
lrm stats

# Show stats for specific path
lrm stats --path ./Resources
```

**Output example:**
```
Translation Statistics

Default Language: English (252 keys)

Language Coverage:
Ελληνικά (el)  ████████████████████ 100% (252/252)

Summary Table:
┌────────────────┬────────┬──────────┬──────────┐
│ Language       │ Keys   │ Missing  │ Coverage │
├────────────────┼────────┼──────────┼──────────┤
│ English        │ 252    │ 0        │ 100%     │
│ Ελληνικά (el)  │ 252    │ 0        │ 100%     │
└────────────────┴────────┴──────────┴──────────┘
```

---

## view

**Description:** View details of a specific key across all languages.

**Arguments:**
- `<KEY>` - The key to view (required)

**Options:**
- `-p, --path <PATH>` - Resource folder path
- `--show-comments` - Include comments in output
- `--format <FORMAT>` - Output format: `table` (default), `json`, or `simple`

**Examples:**
```bash
# View key in table format (default)
lrm view SaveButton

# Include comments
lrm view SaveButton --show-comments

# Output as JSON
lrm view SaveButton --format json

# Simple format (one line per language)
lrm view SaveButton --format simple
```

**Output formats:**

**Table (default):**
```
Key: SaveButton

┌───────────┬────────┐
│ Language  │ Value  │
├───────────┼────────┤
│ English   │ Save   │
│ Greek     │ Σώσει  │
└───────────┴────────┘
```

**JSON:**
```json
{
  "key": "SaveButton",
  "values": {
    "en": "Save",
    "el": "Σώσει"
  }
}
```

---

## add

**Description:** Add a new key to all languages. Can be used in interactive or non-interactive mode.

**Arguments:**
- `<KEY>` - The key to add (required)

**Options:**
- `-p, --path <PATH>` - Resource folder path
- `-l, --lang <CODE:VALUE>` - Language value (e.g., `en:"Save"`) - can be used multiple times
- `-i, --interactive` - Interactive mode (prompts for all language values)
- `--comment <COMMENT>` - Add comment to the key
- `--no-backup` - Skip automatic backup creation

**Examples:**
```bash
# Interactive mode (recommended for multiple languages)
lrm add NewKey -i

# Non-interactive with all values
lrm add NewKey --lang en:"Value" --lang el:"Τιμή"

# Add with comment
lrm add SaveButton -i --comment "Button label for save action"

# Quick add without backup
lrm add TestKey -l en:"Test" -l el:"Δοκιμή" --no-backup
```

**Interactive mode workflow:**
1. Prompts for value in each language
2. Shows preview of changes
3. Creates backup (unless --no-backup)
4. Saves changes to all .resx files

---

## update

**Description:** Update values for an existing key. Can modify one or all languages.

**Arguments:**
- `<KEY>` - The key to update (required)

**Options:**
- `-p, --path <PATH>` - Resource folder path
- `-l, --lang <CODE:VALUE>` - Language value (e.g., `en:"Save Changes"`) - can be used multiple times
- `--comment <COMMENT>` - Update the comment
- `-i, --interactive` - Interactive mode (prompts for each language)
- `-y, --yes` - Skip confirmation prompt
- `--no-backup` - Skip automatic backup creation

**Examples:**
```bash
# Update specific languages
lrm update SaveButton --lang en:"Save Changes" --lang el:"Αποθήκευση Αλλαγών"

# Interactive mode
lrm update SaveButton -i

# Quick update with auto-confirm
lrm update SaveButton -y --no-backup

# Update only comment
lrm update SaveButton --comment "Updated description"
```

**Confirmation prompt:**
```
Current values:
  English: Save
  Greek: Σώσει

New values:
  English: Save Changes
  Greek: Αποθήκευση Αλλαγών

Proceed with update? [y/N]:
```

---

## delete

**Description:** Delete a key from all language files.

**Arguments:**
- `<KEY>` - The key to delete (required)

**Options:**
- `-p, --path <PATH>` - Resource folder path
- `-y, --yes` - Skip confirmation prompt
- `--no-backup` - Skip automatic backup creation

**Examples:**
```bash
# Delete with confirmation
lrm delete OldKey

# Delete without confirmation
lrm delete OldKey -y

# Delete without backup
lrm delete OldKey -y --no-backup
```

**Confirmation prompt:**
```
This will delete 'OldKey' from all languages:
  - English: Old Value
  - Greek: Παλιά Αξία

Are you sure? [y/N]:
```

---

## export

**Description:** Export all translations to CSV format for review or editing in Excel/spreadsheet applications.

**Arguments:** None

**Options:**
- `-p, --path <PATH>` - Resource folder path
- `-o, --output <FILE>` - Output CSV file path (default: `resources.csv`)
- `--include-status` - Include validation status column (shows if key has issues)

**CSV Format:**
```
Key,English,Greek,Comment
SaveButton,Save,Σώσει,"Button label"
CancelButton,Cancel,Ακύρωση,""
```

**Examples:**
```bash
# Export to default file (resources.csv)
lrm export

# Export to custom file
lrm export -o translations.csv

# Include validation status
lrm export -o translations.csv --include-status
```

**With validation status:**
```
Key,English,Greek,Status,Comment
SaveButton,Save,Σώσει,OK,"Button label"
MissingKey,Value,,"Missing in Greek",""
```

---

## import

**Description:** Import translations from a CSV file. Updates existing keys or adds new ones based on CSV content.

**Arguments:**
- `<FILE>` - CSV file to import (required)

**Options:**
- `-p, --path <PATH>` - Resource folder path
- `--overwrite` - Overwrite existing values (default: only update empty/missing values)
- `--no-backup` - Skip automatic backup creation

**CSV Format Requirements:**
- First row must be headers: `Key,Language1,Language2,...`
- Key column is required
- Language columns match your language codes
- Comments column is optional

**Examples:**
```bash
# Import (only fills missing values)
lrm import translations.csv

# Import and overwrite existing values
lrm import translations.csv --overwrite

# Import without backup
lrm import translations.csv --no-backup
```

**Import behavior:**
- **Without --overwrite:** Only updates empty or missing values
- **With --overwrite:** Replaces all values from CSV

---

## edit

**Description:** Launch the interactive Terminal UI (TUI) editor for visual editing of all translations.

**Arguments:** None

**Options:**
- `-p, --path <PATH>` - Resource folder path

**Features:**
- Side-by-side multi-language view
- Real-time search and filtering
- Visual key editing
- Automatic validation
- Unsaved changes tracking
- Keyboard-driven interface

**Keyboard Shortcuts:**
- `↑/↓` or `j/k` - Navigate keys
- `Enter` - Edit selected key
- `Ctrl+N` - Add new key
- `Del` - Delete selected key
- `Ctrl+S` - Save all changes (creates backup)
- `Ctrl+Q` - Quit editor
- `F1` - Show help panel
- `F6` - Run validation
- `/` - Search/filter keys
- `Esc` - Clear search

**Examples:**
```bash
# Launch editor for current directory
lrm edit

# Launch editor for specific path
lrm edit --path ../Resources
```

**TUI Screenshot:**
```
┌─────────────────────────────────────────────────────────────┐
│ Search: [_________]                      [Modified] [F1=Help]│
├────────────────┬──────────────┬───────────────┬─────────────┤
│ Key            │ English      │ Greek         │ Comment     │
├────────────────┼──────────────┼───────────────┼─────────────┤
│ SaveButton     │ Save         │ Σώσει         │ Button      │
│ CancelButton   │ Cancel       │ Ακύρωση       │ Button      │
│ ...            │ ...          │ ...           │ ...         │
└────────────────┴──────────────┴───────────────┴─────────────┘
```

---

## Tips and Best Practices

### Backups
- LRM creates automatic backups before modifications (`.backup` files)
- Use `--no-backup` only when absolutely sure or in automated scripts
- Backups are timestamped and stored in the same directory

### Validation
- Run `lrm validate` before commits
- Add to CI/CD pipeline to catch issues early
- Use exit codes in scripts: `lrm validate || exit 1`

### Workflow Recommendations
1. Use `edit` for bulk changes (visual, intuitive)
2. Use `add`/`update`/`delete` for scripting and automation
3. Use `export` → edit in Excel → `import` for translator workflows
4. Use `stats` to track translation progress

### CSV Workflow
```bash
# Export for translators
lrm export -o for_translation.csv

# Translators edit in Excel/Google Sheets

# Import completed translations
lrm import for_translation.csv --overwrite
```

---

For more information:
- [Installation Guide](INSTALLATION.md)
- [Usage Examples](EXAMPLES.md)
- [CI/CD Integration](CI-CD.md)
- [Contributing](CONTRIBUTING.md)
