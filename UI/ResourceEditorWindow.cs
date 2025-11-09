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

using System.Data;
using LocalizationManager.Core;
using LocalizationManager.Core.Models;
using LocalizationManager.Utils;
using Terminal.Gui;

namespace LocalizationManager.UI;

/// <summary>
/// Interactive TUI window for editing resource files.
/// </summary>
public class ResourceEditorWindow : Window
{
    private readonly List<ResourceFile> _resourceFiles;
    private readonly ResourceFileParser _parser;
    private readonly ResourceValidator _validator;
    private TableView? _tableView;
    private TextField? _searchField;
    private Label? _statusLabel;
    private bool _hasUnsavedChanges = false;
    private string _searchText = string.Empty;
    private DataTable _dataTable;
    private List<string> _allKeys = new();

    public ResourceEditorWindow(List<ResourceFile> resourceFiles, ResourceFileParser parser)
    {
        _resourceFiles = resourceFiles;
        _parser = parser;
        _validator = new ResourceValidator();

        Title = $"Localization Resource Manager - Interactive Editor ({Application.QuitKey} to quit)";

        // Load keys
        var defaultFile = resourceFiles.FirstOrDefault(rf => rf.Language.IsDefault);
        if (defaultFile != null)
        {
            _allKeys = defaultFile.Entries.Select(e => e.Key).OrderBy(k => k).ToList();
        }

        // Build DataTable
        _dataTable = BuildDataTable();

        InitializeComponents();
    }

    private DataTable BuildDataTable()
    {
        var dt = new DataTable();

        // Add Key column
        dt.Columns.Add("Key", typeof(string));

        // Add column for each language
        foreach (var rf in _resourceFiles)
        {
            dt.Columns.Add(rf.Language.Name, typeof(string));
        }

        // Populate rows
        foreach (var key in _allKeys)
        {
            var row = dt.NewRow();
            row["Key"] = key;

            foreach (var rf in _resourceFiles)
            {
                var entry = rf.Entries.FirstOrDefault(e => e.Key == key);
                row[rf.Language.Name] = entry?.Value ?? "";
            }

            dt.Rows.Add(row);
        }

        return dt;
    }

    private void InitializeComponents()
    {
        // Menu bar
        var menu = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_File", new MenuItem[]
            {
                new MenuItem("_Save", "Save all changes", () => SaveChanges(), null, null, Key.S | Key.CtrlMask),
                new MenuItem("_Validate", "Run validation", () => ShowValidation(), null, null, Key.F6),
                null!, // separator
                new MenuItem("_Quit", "Exit editor", () => { if (ConfirmQuit()) Application.RequestStop(); }, null, null, Key.Q | Key.CtrlMask)
            }),
            new MenuBarItem("_Edit", new MenuItem[]
            {
                new MenuItem("_Add Key", "Add new translation key", () => AddNewKey(), null, null, Key.N | Key.CtrlMask),
                new MenuItem("_Edit Key", "Edit selected key", () => { if (_tableView?.SelectedRow >= 0) EditKey(_dataTable.Rows[_tableView.SelectedRow]["Key"].ToString()!); }, null, null, Key.Enter),
                new MenuItem("_Delete Key", "Delete selected key", () => DeleteSelectedKey(), null, null, Key.DeleteChar)
            }),
            new MenuBarItem("_Languages", new MenuItem[]
            {
                new MenuItem("_List", "Show all languages", () => ShowLanguageList(), null, null, Key.L | Key.CtrlMask),
                new MenuItem("_Add New", "Add new language", () => AddLanguage(), null, null, Key.F2),
                new MenuItem("_Remove", "Remove language", () => RemoveLanguage(), null, null, Key.F3)
            }),
            new MenuBarItem("_Help", new MenuItem[]
            {
                new MenuItem("_Shortcuts", "Show keyboard shortcuts", () => ShowHelp(), null, null, Key.F1)
            })
        });

        Add(menu);

        // Search bar (adjusted Y position for menu)
        var searchLabel = new Label
        {
            Text = "Search:",
            X = 1,
            Y = 2
        };

        _searchField = new TextField
        {
            X = Pos.Right(searchLabel) + 1,
            Y = 2,
            Width = 40,
            Text = ""
        };

        _searchField.TextChanged += (oldValue) =>
        {
            _searchText = _searchField.Text.ToString() ?? string.Empty;
            FilterKeys();
        };

        // Help label
        var helpLabel = new Label
        {
            Text = "Enter=Edit  Ctrl+N=Add  Del=Delete  Ctrl+S=Save  F1=Help  F2=Add Lang  F3=Remove Lang",
            X = Pos.Right(_searchField) + 2,
            Y = 2
        };

        // TableView for keys and translations (adjusted Y position for menu)
        _tableView = new TableView
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 2,
            FullRowSelect = true,
            MultiSelect = false,
            Table = _dataTable
        };

        _tableView.CellActivated += (args) =>
        {
            if (args.Row >= 0 && args.Row < _dataTable.Rows.Count)
            {
                var key = _dataTable.Rows[args.Row]["Key"].ToString();
                if (!string.IsNullOrEmpty(key))
                {
                    EditKey(key);
                }
            }
        };

        // Status bar
        _statusLabel = new Label
        {
            Text = GetStatusText(),
            X = 1,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        Add(searchLabel, _searchField, helpLabel, _tableView, _statusLabel);

        // Keyboard shortcuts
        KeyPress += OnKeyPress;
    }

    private void OnKeyPress(KeyEventEventArgs e)
    {
        if (e.KeyEvent.Key == (Key.N | Key.CtrlMask))
        {
            AddNewKey();
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == Key.DeleteChar || e.KeyEvent.Key == Key.Backspace)
        {
            DeleteSelectedKey();
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == (Key.S | Key.CtrlMask))
        {
            SaveChanges();
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == Key.F1)
        {
            ShowHelp();
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == Key.F6)
        {
            ShowValidation();
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == (Key.Q | Key.CtrlMask))
        {
            if (ConfirmQuit())
            {
                Application.RequestStop();
            }
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == (Key.L | Key.CtrlMask))
        {
            ShowLanguageList();
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == Key.F2)
        {
            AddLanguage();
            e.Handled = true;
        }
        else if (e.KeyEvent.Key == Key.F3)
        {
            RemoveLanguage();
            e.Handled = true;
        }
    }

    private void FilterKeys()
    {
        if (_tableView == null) return;

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            // Show all rows
            _dataTable.DefaultView.RowFilter = string.Empty;
        }
        else
        {
            // Filter by key name or any translation value
            var conditions = new List<string>();

            // Add Key column filter
            conditions.Add($"Key LIKE '%{_searchText.Replace("'", "''")}%'");

            // Add filter for each language column
            foreach (var rf in _resourceFiles)
            {
                conditions.Add($"[{rf.Language.Name}] LIKE '%{_searchText.Replace("'", "''")}%'");
            }

            _dataTable.DefaultView.RowFilter = string.Join(" OR ", conditions);
        }

        UpdateStatus();
    }

    private void EditKey(string key)
    {
        var dialog = new Dialog
        {
            Title = $"Edit: {key}",
            Width = Dim.Percent(80),
            Height = Dim.Percent(70)
        };

        var fields = new Dictionary<string, TextField>();
        var yPos = 1;

        foreach (var rf in _resourceFiles)
        {
            var entry = rf.Entries.FirstOrDefault(e => e.Key == key);
            var currentValue = entry?.Value ?? string.Empty;

            var label = new Label
            {
                Text = $"{rf.Language.Name}:",
                X = 1,
                Y = yPos
            };

            var textField = new TextField
            {
                Text = currentValue,
                X = 1,
                Y = yPos + 1,
                Width = Dim.Fill() - 1
            };

            fields[rf.Language.Code] = textField;
            dialog.Add(label, textField);
            yPos += 3;
        }

        var btnSave = new Button
        {
            Text = "Save",
            X = Pos.Center() - 10,
            Y = Pos.AnchorEnd(2),
            IsDefault = true
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            X = Pos.Center() + 5,
            Y = Pos.AnchorEnd(2)
        };

        btnSave.Clicked += () =>
        {
            foreach (var kvp in fields)
            {
                var rf = _resourceFiles.FirstOrDefault(r => r.Language.Code == kvp.Key);
                if (rf != null)
                {
                    var entry = rf.Entries.FirstOrDefault(ent => ent.Key == key);
                    if (entry != null)
                    {
                        entry.Value = kvp.Value.Text.ToString();
                        _hasUnsavedChanges = true;

                        // Update DataTable
                        var row = _dataTable.Rows.Cast<DataRow>().FirstOrDefault(r => r["Key"].ToString() == key);
                        if (row != null)
                        {
                            row[rf.Language.Name] = kvp.Value.Text.ToString();
                        }
                    }
                }
            }

            UpdateStatus();
            Application.RequestStop();
        };

        btnCancel.Clicked += () =>
        {
            Application.RequestStop();
        };

        dialog.Add(btnSave, btnCancel);
        Application.Run(dialog);
        dialog.Dispose();
    }

    private void AddNewKey()
    {
        var dialog = new Dialog
        {
            Title = "Add New Key",
            Width = 60,
            Height = 10 + _resourceFiles.Count * 3
        };

        var keyLabel = new Label
        {
            Text = "Key name:",
            X = 1,
            Y = 1
        };

        var keyField = new TextField
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill() - 1,
            Text = ""
        };

        dialog.Add(keyLabel, keyField);

        var valueFields = new Dictionary<string, TextField>();
        var yPos = 4;

        foreach (var rf in _resourceFiles)
        {
            var label = new Label
            {
                Text = $"{rf.Language.Name}:",
                X = 1,
                Y = yPos
            };

            var textField = new TextField
            {
                X = 1,
                Y = yPos + 1,
                Width = Dim.Fill() - 1,
                Text = ""
            };

            valueFields[rf.Language.Code] = textField;
            dialog.Add(label, textField);
            yPos += 3;
        }

        var btnAdd = new Button
        {
            Text = "Add",
            X = Pos.Center() - 10,
            Y = Pos.AnchorEnd(2),
            IsDefault = true
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            X = Pos.Center() + 5,
            Y = Pos.AnchorEnd(2)
        };

        btnAdd.Clicked += () =>
        {
            var key = keyField.Text.ToString();
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.ErrorQuery("Error", "Key name is required", "OK");
                return;
            }

            if (_allKeys.Contains(key))
            {
                MessageBox.ErrorQuery("Error", "Key already exists", "OK");
                return;
            }

            foreach (var kvp in valueFields)
            {
                var rf = _resourceFiles.FirstOrDefault(r => r.Language.Code == kvp.Key);
                if (rf != null)
                {
                    rf.Entries.Add(new ResourceEntry
                    {
                        Key = key,
                        Value = kvp.Value.Text.ToString()
                    });
                }
            }

            _allKeys.Add(key);
            _allKeys = _allKeys.OrderBy(k => k).ToList();

            // Add to DataTable
            var newRow = _dataTable.NewRow();
            newRow["Key"] = key;
            foreach (var kvp in valueFields)
            {
                var rf = _resourceFiles.FirstOrDefault(r => r.Language.Code == kvp.Key);
                if (rf != null)
                {
                    newRow[rf.Language.Name] = kvp.Value.Text.ToString();
                }
            }
            _dataTable.Rows.Add(newRow);

            // Re-sort DataTable
            _dataTable.DefaultView.Sort = "Key ASC";
            _dataTable = _dataTable.DefaultView.ToTable();
            if (_tableView != null)
            {
                _tableView.Table = _dataTable;
            }

            _hasUnsavedChanges = true;
            FilterKeys();
            Application.RequestStop();
        };

        btnCancel.Clicked += () =>
        {
            Application.RequestStop();
        };

        dialog.Add(btnAdd, btnCancel);
        Application.Run(dialog);
        dialog.Dispose();
    }

    private void DeleteSelectedKey()
    {
        if (_tableView == null || _tableView.SelectedRow < 0) return;

        var filteredView = _dataTable.DefaultView;
        if (_tableView.SelectedRow >= filteredView.Count) return;

        var key = filteredView[_tableView.SelectedRow]["Key"].ToString();
        if (string.IsNullOrEmpty(key)) return;

        var result = MessageBox.Query("Confirm Delete",
            $"Delete key '{key}' from all languages?", "Yes", "No");

        if (result == 0)
        {
            foreach (var rf in _resourceFiles)
            {
                var entry = rf.Entries.FirstOrDefault(e => e.Key == key);
                if (entry != null)
                {
                    rf.Entries.Remove(entry);
                }
            }

            _allKeys.Remove(key);

            // Remove from DataTable
            var rowToDelete = _dataTable.Rows.Cast<DataRow>().FirstOrDefault(r => r["Key"].ToString() == key);
            if (rowToDelete != null)
            {
                _dataTable.Rows.Remove(rowToDelete);
            }

            _hasUnsavedChanges = true;
            FilterKeys();
        }
    }

    private void SaveChanges()
    {
        if (!_hasUnsavedChanges)
        {
            MessageBox.Query("Save", "No changes to save", "OK");
            return;
        }

        var result = MessageBox.Query("Save Changes",
            "Create backups before saving?", "Yes", "No", "Cancel");

        if (result == 2) return;

        try
        {
            if (result == 0)
            {
                var backupManager = new BackupManager();
                backupManager.CreateBackups(_resourceFiles.Select(rf => rf.Language.FilePath));
            }

            foreach (var rf in _resourceFiles)
            {
                _parser.Write(rf);
            }

            _hasUnsavedChanges = false;
            UpdateStatus();
            MessageBox.Query("Success", "Changes saved successfully", "OK");
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to save: {ex.Message}", "OK");
        }
    }

    private void ShowValidation()
    {
        var result = _validator.Validate(_resourceFiles);

        var message = result.IsValid
            ? "All validations passed!"
            : $"Found {result.TotalIssues} issue(s):\n\n" +
              $"Missing: {result.MissingKeys.Sum(kv => kv.Value.Count)}\n" +
              $"Extra: {result.ExtraKeys.Sum(kv => kv.Value.Count)}\n" +
              $"Duplicates: {result.DuplicateKeys.Sum(kv => kv.Value.Count)}\n" +
              $"Empty: {result.EmptyValues.Sum(kv => kv.Value.Count)}";

        MessageBox.Query("Validation", message, "OK");
    }

    private void ShowLanguageList()
    {
        var dialog = new Dialog
        {
            Title = "Manage Languages",
            Width = Dim.Percent(70),
            Height = Dim.Percent(60)
        };

        var languageList = _resourceFiles.Select(rf =>
        {
            var code = string.IsNullOrEmpty(rf.Language.Code) ? "(default)" : rf.Language.Code;
            var isDefault = rf.Language.IsDefault ? " [DEFAULT]" : "";
            return $"{code,-12} {rf.Language.Name,-25} ({rf.Entries.Count,4} entries){isDefault}";
        }).ToList();

        var listView = new ListView(languageList)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 4
        };

        var btnAdd = new Button("Add New (F2)")
        {
            X = 1,
            Y = Pos.AnchorEnd(2)
        };
        btnAdd.Clicked += () => { Application.RequestStop(); AddLanguage(); };

        var btnRemove = new Button("Remove (F3)")
        {
            X = Pos.Right(btnAdd) + 2,
            Y = Pos.AnchorEnd(2)
        };
        btnRemove.Clicked += () => { Application.RequestStop(); RemoveLanguage(); };

        var btnClose = new Button("Close")
        {
            X = Pos.Right(btnRemove) + 2,
            Y = Pos.AnchorEnd(2)
        };
        btnClose.Clicked += () => Application.RequestStop();

        dialog.Add(listView, btnAdd, btnRemove, btnClose);
        Application.Run(dialog);
    }

    private void AddLanguage()
    {
        var dialog = new Dialog
        {
            Title = "Add New Language",
            Width = 65,
            Height = 16
        };

        var cultureLabel = new Label("Culture code (e.g., fr, de-DE, ja):") { X = 1, Y = 1 };
        var cultureField = new TextField { X = 1, Y = 2, Width = 20 };
        var statusLabel = new Label { X = 22, Y = 2, Width = Dim.Fill() - 1, ColorScheme = Colors.Base };

        var copyFromLabel = new Label("Copy entries from:") { X = 1, Y = 4 };
        var languageOptions = _resourceFiles.Select(rf =>
            string.IsNullOrEmpty(rf.Language.Code) ? "Default" : $"{rf.Language.Code} ({rf.Language.Name})"
        ).ToList();

        var copyFromCombo = new ComboBox
        {
            X = 1,
            Y = 5,
            Width = 40,
            Height = 5
        };
        copyFromCombo.SetSource(languageOptions);

        var emptyCheckbox = new CheckBox("Create empty (no entries)")
        {
            X = 1,
            Y = 7
        };

        var manager = new LanguageFileManager();
        var discovery = new ResourceDiscovery();

        cultureField.TextChanged += (oldValue) =>
        {
            var code = cultureField.Text.ToString();
            if (string.IsNullOrWhiteSpace(code))
            {
                statusLabel.Text = "";
                return;
            }

            if (manager.IsValidCultureCode(code, out var culture))
            {
                statusLabel.Text = $"✓ {culture!.DisplayName}";
                statusLabel.ColorScheme = Colors.Dialog;
            }
            else
            {
                statusLabel.Text = "✗ Invalid code";
                statusLabel.ColorScheme = Colors.Error;
            }
        };

        var btnCreate = new Button("Create")
        {
            X = 1,
            Y = Pos.AnchorEnd(2)
        };

        btnCreate.Clicked += () =>
        {
            var code = cultureField.Text.ToString();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.ErrorQuery("Error", "Culture code is required.", "OK");
                return;
            }

            if (!manager.IsValidCultureCode(code, out var culture))
            {
                MessageBox.ErrorQuery("Error", $"Invalid culture code: {code}", "OK");
                return;
            }

            var baseName = _resourceFiles[0].Language.BaseName;
            var resourcePath = Path.GetDirectoryName(_resourceFiles[0].Language.FilePath) ?? "";

            if (manager.LanguageFileExists(baseName, code, resourcePath))
            {
                MessageBox.ErrorQuery("Error", $"Language '{code}' already exists.", "OK");
                return;
            }

            try
            {
                // Get source file
                ResourceFile? sourceFile = null;
                if (!emptyCheckbox.Checked)
                {
                    var selectedIdx = copyFromCombo.SelectedItem;
                    sourceFile = _resourceFiles[selectedIdx];
                }

                // Create new language file
                var newFile = manager.CreateLanguageFile(
                    baseName,
                    code,
                    resourcePath,
                    sourceFile,
                    copyEntries: !emptyCheckbox.Checked
                );

                // Add to resource files list
                _resourceFiles.Add(newFile);

                // Rebuild DataTable with new column
                var newDataTable = BuildDataTable();
                _dataTable = newDataTable;
                if (_tableView != null)
                {
                    _tableView.Table = _dataTable;
                }

                UpdateStatus();

                MessageBox.Query("Success",
                    $"Added {culture!.DisplayName} ({code}) language\n" +
                    $"File: {Path.GetFileName(newFile.Language.FilePath)}",
                    "OK");

                Application.RequestStop();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to create language:\n{ex.Message}", "OK");
            }
        };

        var btnCancel = new Button("Cancel")
        {
            X = Pos.Right(btnCreate) + 2,
            Y = Pos.AnchorEnd(2)
        };
        btnCancel.Clicked += () => Application.RequestStop();

        dialog.Add(cultureLabel, cultureField, statusLabel, copyFromLabel, copyFromCombo, emptyCheckbox, btnCreate, btnCancel);
        Application.Run(dialog);
    }

    private void RemoveLanguage()
    {
        var removableLanguages = _resourceFiles
            .Where(rf => !rf.Language.IsDefault)
            .ToList();

        if (!removableLanguages.Any())
        {
            MessageBox.ErrorQuery("Error", "No languages to remove.\nCannot delete the default language.", "OK");
            return;
        }

        var dialog = new Dialog
        {
            Title = "Remove Language",
            Width = 65,
            Height = 18
        };

        var label = new Label("Select language to remove:") { X = 1, Y = 1 };

        var languageList = removableLanguages.Select(rf =>
            $"{rf.Language.Code,-12} {rf.Language.Name,-25} ({rf.Entries.Count,4} entries)"
        ).ToList();

        var listView = new ListView(languageList)
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 5
        };

        var noBackupCheckbox = new CheckBox("Skip backup (not recommended)")
        {
            X = 1,
            Y = Pos.AnchorEnd(3)
        };

        var btnRemove = new Button("Remove")
        {
            X = 1,
            Y = Pos.AnchorEnd(2)
        };

        btnRemove.Clicked += () =>
        {
            var selectedIdx = listView.SelectedItem;
            if (selectedIdx < 0 || selectedIdx >= removableLanguages.Count)
            {
                MessageBox.ErrorQuery("Error", "Please select a language to remove.", "OK");
                return;
            }

            var rf = removableLanguages[selectedIdx];
            var result = MessageBox.Query("Confirm Delete",
                $"Delete {rf.Language.Name} ({rf.Language.Code})?\n\n" +
                $"{rf.Entries.Count} entries will be lost.\n" +
                $"File: {Path.GetFileName(rf.Language.FilePath)}",
                "Delete", "Cancel");

            if (result == 0)
            {
                try
                {
                    // Create backup if requested
                    if (!noBackupCheckbox.Checked)
                    {
                        var backup = new BackupManager();
                        backup.CreateBackup(rf.Language.FilePath);
                    }

                    // Delete the file
                    var manager = new LanguageFileManager();
                    manager.DeleteLanguageFile(rf.Language);

                    // Remove from list
                    _resourceFiles.Remove(rf);

                    // Rebuild DataTable without this column
                    var newDataTable = BuildDataTable();
                    _dataTable = newDataTable;
                    if (_tableView != null)
                    {
                        _tableView.Table = _dataTable;
                    }

                    UpdateStatus();

                    MessageBox.Query("Success",
                        $"Removed {rf.Language.Name} ({rf.Language.Code})",
                        "OK");

                    Application.RequestStop();
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("Error", $"Failed to remove language:\n{ex.Message}", "OK");
                }
            }
        };

        var btnCancel = new Button("Cancel")
        {
            X = Pos.Right(btnRemove) + 2,
            Y = Pos.AnchorEnd(2)
        };
        btnCancel.Clicked += () => Application.RequestStop();

        dialog.Add(label, listView, noBackupCheckbox, btnRemove, btnCancel);
        Application.Run(dialog);
    }

    private void ShowHelp()
    {
        var help = "Keyboard Shortcuts:\n\n" +
                   "Key Management:\n" +
                   "Enter     - Edit selected key\n" +
                   "Ctrl+N    - Add new key\n" +
                   "Del       - Delete selected key\n\n" +
                   "Language Management:\n" +
                   "Ctrl+L    - List languages\n" +
                   "F2        - Add new language\n" +
                   "F3        - Remove language\n\n" +
                   "File Operations:\n" +
                   "Ctrl+S    - Save changes\n" +
                   "F6        - Run validation\n" +
                   "Ctrl+Q    - Quit editor\n\n" +
                   "Navigation:\n" +
                   "↑/↓       - Move selection\n" +
                   "PgUp/PgDn - Page up/down\n" +
                   "F1        - Show this help";

        MessageBox.Query("Help", help, "OK");
    }

    private bool ConfirmQuit()
    {
        if (!_hasUnsavedChanges) return true;

        var result = MessageBox.Query("Unsaved Changes",
            "Save before quitting?", "Save", "Discard", "Cancel");

        if (result == 0)
        {
            SaveChanges();
            return !_hasUnsavedChanges;
        }

        return result == 1;
    }

    private void UpdateStatus()
    {
        if (_statusLabel == null) return;
        _statusLabel.Text = GetStatusText();
    }

    private string GetStatusText()
    {
        var filteredCount = _dataTable.DefaultView.Count;
        var totalCount = _dataTable.Rows.Count;
        var langCount = _resourceFiles.Count;
        var status = $"Keys: {filteredCount}/{totalCount} | Languages: {langCount}";
        if (_hasUnsavedChanges) status += " [MODIFIED]";
        return status;
    }
}
