// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core.Models;

namespace LocalizationManager.UI;

/// <summary>
/// Represents an undoable/redoable operation in the TUI editor
/// </summary>
public interface IOperation
{
    string Description { get; }
    void Execute();
    void Undo();
}

/// <summary>
/// Manages undo/redo history for TUI operations
/// </summary>
public class OperationHistory
{
    private readonly Stack<IOperation> _undoStack = new();
    private readonly Stack<IOperation> _redoStack = new();
    private readonly int _maxHistorySize;

    public OperationHistory(int maxHistorySize = 50)
    {
        _maxHistorySize = maxHistorySize;
    }

    public bool CanUndo => _undoStack.Any();
    public bool CanRedo => _redoStack.Any();

    public string? UndoDescription => _undoStack.Any() ? _undoStack.Peek().Description : null;
    public string? RedoDescription => _redoStack.Any() ? _redoStack.Peek().Description : null;

    public void ExecuteOperation(IOperation operation)
    {
        operation.Execute();
        _undoStack.Push(operation);

        // Clear redo stack when new operation is executed
        _redoStack.Clear();

        // Maintain max history size
        if (_undoStack.Count > _maxHistorySize)
        {
            var temp = _undoStack.ToList();
            temp.RemoveAt(temp.Count - 1); // Remove oldest
            _undoStack.Clear();
            temp.Reverse();
            foreach (var op in temp)
            {
                _undoStack.Push(op);
            }
        }
    }

    public void Undo()
    {
        if (!CanUndo) return;

        var operation = _undoStack.Pop();
        operation.Undo();
        _redoStack.Push(operation);
    }

    public void Redo()
    {
        if (!CanRedo) return;

        var operation = _redoStack.Pop();
        operation.Execute();
        _undoStack.Push(operation);
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}

/// <summary>
/// Operation to edit a resource entry value
/// </summary>
public class EditValueOperation : IOperation
{
    private readonly ResourceFile _resourceFile;
    private readonly string _key;
    private readonly int _occurrenceNumber;
    private readonly string _oldValue;
    private readonly string _newValue;
    private readonly string _languageCode;

    public string Description => $"Edit '{_key}' in {_languageCode}";

    public EditValueOperation(
        ResourceFile resourceFile,
        string key,
        int occurrenceNumber,
        string oldValue,
        string newValue,
        string languageCode)
    {
        _resourceFile = resourceFile;
        _key = key;
        _occurrenceNumber = occurrenceNumber;
        _oldValue = oldValue;
        _newValue = newValue;
        _languageCode = languageCode;
    }

    public void Execute()
    {
        SetValue(_newValue);
    }

    public void Undo()
    {
        SetValue(_oldValue);
    }

    private void SetValue(string value)
    {
        var entries = _resourceFile.Entries
            .Where(e => e.Key.Equals(_key, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (_occurrenceNumber > 0 && _occurrenceNumber <= entries.Count)
        {
            entries[_occurrenceNumber - 1].Value = value;
        }
    }
}

/// <summary>
/// Operation to delete a resource entry
/// </summary>
public class DeleteKeyOperation : IOperation
{
    private readonly List<ResourceFile> _resourceFiles;
    private readonly string _key;
    private readonly int _occurrenceNumber;
    private readonly Dictionary<string, ResourceEntry> _deletedEntries = new();
    private readonly Dictionary<string, int> _deletedIndices = new();

    public string Description => $"Delete '{_key}'" +
        (_occurrenceNumber > 1 ? $" [occurrence {_occurrenceNumber}]" : "");

    public DeleteKeyOperation(
        List<ResourceFile> resourceFiles,
        string key,
        int occurrenceNumber)
    {
        _resourceFiles = resourceFiles;
        _key = key;
        _occurrenceNumber = occurrenceNumber;
    }

    public void Execute()
    {
        // Store entries and their indices before deleting
        _deletedEntries.Clear();
        _deletedIndices.Clear();

        foreach (var rf in _resourceFiles)
        {
            var entries = rf.Entries
                .Where(e => e.Key.Equals(_key, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (_occurrenceNumber > 0 && _occurrenceNumber <= entries.Count)
            {
                var entryToDelete = entries[_occurrenceNumber - 1];
                var index = rf.Entries.IndexOf(entryToDelete);

                _deletedEntries[rf.Language.Code] = new ResourceEntry
                {
                    Key = entryToDelete.Key,
                    Value = entryToDelete.Value,
                    Comment = entryToDelete.Comment
                };
                _deletedIndices[rf.Language.Code] = index;

                rf.Entries.Remove(entryToDelete);
            }
        }
    }

    public void Undo()
    {
        // Restore entries at their original indices
        foreach (var rf in _resourceFiles)
        {
            if (_deletedEntries.TryGetValue(rf.Language.Code, out var entry) &&
                _deletedIndices.TryGetValue(rf.Language.Code, out var index))
            {
                if (index >= 0 && index <= rf.Entries.Count)
                {
                    rf.Entries.Insert(index, entry);
                }
            }
        }
    }
}

/// <summary>
/// Operation to add a new resource entry
/// </summary>
public class AddKeyOperation : IOperation
{
    private readonly List<ResourceFile> _resourceFiles;
    private readonly string _key;
    private readonly Dictionary<string, string> _values;
    private readonly string? _comment;

    public string Description => $"Add '{_key}'";

    public AddKeyOperation(
        List<ResourceFile> resourceFiles,
        string key,
        Dictionary<string, string> values,
        string? comment = null)
    {
        _resourceFiles = resourceFiles;
        _key = key;
        _values = values;
        _comment = comment;
    }

    public void Execute()
    {
        foreach (var rf in _resourceFiles)
        {
            var value = _values.TryGetValue(rf.Language.Code, out var v) ? v : string.Empty;

            rf.Entries.Add(new ResourceEntry
            {
                Key = _key,
                Value = value,
                Comment = _comment ?? string.Empty
            });
        }
    }

    public void Undo()
    {
        foreach (var rf in _resourceFiles)
        {
            var entryToRemove = rf.Entries.LastOrDefault(e =>
                e.Key.Equals(_key, StringComparison.OrdinalIgnoreCase));

            if (entryToRemove != null)
            {
                rf.Entries.Remove(entryToRemove);
            }
        }
    }
}
