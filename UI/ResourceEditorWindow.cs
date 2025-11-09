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
        // Search bar
        var searchLabel = new Label
        {
            Text = "Search:",
            X = 1,
            Y = 1
        };

        _searchField = new TextField
        {
            X = Pos.Right(searchLabel) + 1,
            Y = 1,
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
            Text = "Enter=Edit  Ctrl+N=Add  Del=Delete  Ctrl+S=Save  F1=Help  F6=Validate",
            X = Pos.Right(_searchField) + 2,
            Y = 1
        };

        // TableView for keys and translations
        _tableView = new TableView
        {
            X = 1,
            Y = 3,
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

    private void ShowHelp()
    {
        var help = "Keyboard Shortcuts:\n\n" +
                   "Enter     - Edit selected key\n" +
                   "Ctrl+N    - Add new key\n" +
                   "Del       - Delete selected key\n" +
                   "Ctrl+S    - Save changes\n" +
                   "Ctrl+Q    - Quit editor\n" +
                   "F1        - Show this help\n" +
                   "F6        - Run validation\n\n" +
                   "Navigation:\n" +
                   "↑/↓       - Move selection\n" +
                   "PgUp/PgDn - Page up/down";

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
        var status = $"Keys: {filteredCount}/{totalCount}";
        if (_hasUnsavedChanges) status += " [MODIFIED]";
        return status;
    }
}
