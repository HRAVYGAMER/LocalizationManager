// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core.Models;
using LocalizationManager.UI;

namespace LocalizationManager.Tests.UnitTests;

public class OperationHistoryTests
{
    [Fact]
    public void OperationHistory_InitialState_IsEmpty()
    {
        // Arrange
        var history = new OperationHistory();

        // Assert
        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
        Assert.Null(history.UndoDescription);
        Assert.Null(history.RedoDescription);
    }

    [Fact]
    public void ExecuteOperation_AddsToUndoStack()
    {
        // Arrange
        var history = new OperationHistory();
        var operation = new MockOperation("Test Operation");

        // Act
        history.ExecuteOperation(operation);

        // Assert
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
        Assert.Equal("Test Operation", history.UndoDescription);
        Assert.True(operation.WasExecuted);
    }

    [Fact]
    public void Undo_ExecutesUndoOnOperation()
    {
        // Arrange
        var history = new OperationHistory();
        var operation = new MockOperation("Test");
        history.ExecuteOperation(operation);

        // Act
        history.Undo();

        // Assert
        Assert.True(operation.WasUndone);
        Assert.False(history.CanUndo);
        Assert.True(history.CanRedo);
        Assert.Equal("Test", history.RedoDescription);
    }

    [Fact]
    public void Redo_ExecutesOperationAgain()
    {
        // Arrange
        var history = new OperationHistory();
        var operation = new MockOperation("Test");
        history.ExecuteOperation(operation);
        history.Undo();
        operation.WasExecuted = false; // Reset flag

        // Act
        history.Redo();

        // Assert
        Assert.True(operation.WasExecuted);
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void ExecuteOperation_ClearsRedoStack()
    {
        // Arrange
        var history = new OperationHistory();
        var op1 = new MockOperation("Op1");
        var op2 = new MockOperation("Op2");
        history.ExecuteOperation(op1);
        history.Undo();

        // Act
        history.ExecuteOperation(op2);

        // Assert
        Assert.False(history.CanRedo);
        Assert.Equal("Op2", history.UndoDescription);
    }

    [Fact]
    public void OperationHistory_RespectsMaxHistorySize()
    {
        // Arrange
        var history = new OperationHistory(maxHistorySize: 3);

        // Act
        for (int i = 1; i <= 5; i++)
        {
            history.ExecuteOperation(new MockOperation($"Op{i}"));
        }

        // Assert
        Assert.True(history.CanUndo);
        Assert.Equal("Op5", history.UndoDescription);

        // Undo all operations
        int undoCount = 0;
        while (history.CanUndo)
        {
            history.Undo();
            undoCount++;
        }

        // Should only have 3 operations in history (max size)
        Assert.Equal(3, undoCount);
    }

    [Fact]
    public void Clear_RemovesAllHistory()
    {
        // Arrange
        var history = new OperationHistory();
        history.ExecuteOperation(new MockOperation("Op1"));
        history.ExecuteOperation(new MockOperation("Op2"));
        history.Undo();

        // Act
        history.Clear();

        // Assert
        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void EditValueOperation_UpdatesResourceFileValue()
    {
        // Arrange
        var resourceFile = new ResourceFile
        {
            Language = new LanguageInfo { Code = "en", Name = "English", FilePath = "test.resx", BaseName = "test" },
            Entries = new List<ResourceEntry>
            {
                new ResourceEntry { Key = "TestKey", Value = "OldValue", Comment = "" }
            }
        };

        var operation = new EditValueOperation(
            resourceFile,
            "TestKey",
            1,
            "OldValue",
            "NewValue",
            "en"
        );

        // Act
        operation.Execute();

        // Assert
        Assert.Equal("NewValue", resourceFile.Entries[0].Value);
    }

    [Fact]
    public void EditValueOperation_Undo_RestoresOldValue()
    {
        // Arrange
        var resourceFile = new ResourceFile
        {
            Language = new LanguageInfo { Code = "en", Name = "English", FilePath = "test.resx", BaseName = "test" },
            Entries = new List<ResourceEntry>
            {
                new ResourceEntry { Key = "TestKey", Value = "OldValue", Comment = "" }
            }
        };

        var operation = new EditValueOperation(
            resourceFile,
            "TestKey",
            1,
            "OldValue",
            "NewValue",
            "en"
        );

        operation.Execute();

        // Act
        operation.Undo();

        // Assert
        Assert.Equal("OldValue", resourceFile.Entries[0].Value);
    }

    [Fact]
    public void DeleteKeyOperation_RemovesEntryFromAllLanguages()
    {
        // Arrange
        var resourceFiles = new List<ResourceFile>
        {
            new ResourceFile
            {
                Language = new LanguageInfo { Code = "en", Name = "English", FilePath = "test.resx", BaseName = "test" },
                Entries = new List<ResourceEntry>
                {
                    new ResourceEntry { Key = "TestKey", Value = "EnValue", Comment = "" }
                }
            },
            new ResourceFile
            {
                Language = new LanguageInfo { Code = "el", Name = "Greek", FilePath = "test.el.resx", BaseName = "test" },
                Entries = new List<ResourceEntry>
                {
                    new ResourceEntry { Key = "TestKey", Value = "ElValue", Comment = "" }
                }
            }
        };

        var operation = new DeleteKeyOperation(resourceFiles, "TestKey", 1);

        // Act
        operation.Execute();

        // Assert
        Assert.Empty(resourceFiles[0].Entries);
        Assert.Empty(resourceFiles[1].Entries);
    }

    [Fact]
    public void DeleteKeyOperation_Undo_RestoresEntries()
    {
        // Arrange
        var resourceFiles = new List<ResourceFile>
        {
            new ResourceFile
            {
                Language = new LanguageInfo { Code = "en", Name = "English", FilePath = "test.resx", BaseName = "test" },
                Entries = new List<ResourceEntry>
                {
                    new ResourceEntry { Key = "TestKey", Value = "EnValue", Comment = "" }
                }
            }
        };

        var operation = new DeleteKeyOperation(resourceFiles, "TestKey", 1);
        operation.Execute();

        // Act
        operation.Undo();

        // Assert
        Assert.Single(resourceFiles[0].Entries);
        Assert.Equal("TestKey", resourceFiles[0].Entries[0].Key);
        Assert.Equal("EnValue", resourceFiles[0].Entries[0].Value);
    }

    [Fact]
    public void AddKeyOperation_AddsEntryToAllLanguages()
    {
        // Arrange
        var resourceFiles = new List<ResourceFile>
        {
            new ResourceFile
            {
                Language = new LanguageInfo { Code = "en", Name = "English", FilePath = "test.resx", BaseName = "test" },
                Entries = new List<ResourceEntry>()
            },
            new ResourceFile
            {
                Language = new LanguageInfo { Code = "el", Name = "Greek", FilePath = "test.el.resx", BaseName = "test" },
                Entries = new List<ResourceEntry>()
            }
        };

        var values = new Dictionary<string, string>
        {
            { "en", "EnValue" },
            { "el", "ElValue" }
        };

        var operation = new AddKeyOperation(resourceFiles, "NewKey", values, "Test comment");

        // Act
        operation.Execute();

        // Assert
        Assert.Single(resourceFiles[0].Entries);
        Assert.Equal("NewKey", resourceFiles[0].Entries[0].Key);
        Assert.Equal("EnValue", resourceFiles[0].Entries[0].Value);
        Assert.Equal("Test comment", resourceFiles[0].Entries[0].Comment);

        Assert.Single(resourceFiles[1].Entries);
        Assert.Equal("ElValue", resourceFiles[1].Entries[0].Value);
    }

    [Fact]
    public void AddKeyOperation_Undo_RemovesAddedEntries()
    {
        // Arrange
        var resourceFiles = new List<ResourceFile>
        {
            new ResourceFile
            {
                Language = new LanguageInfo { Code = "en", Name = "English", FilePath = "test.resx", BaseName = "test" },
                Entries = new List<ResourceEntry>()
            }
        };

        var values = new Dictionary<string, string> { { "en", "Value" } };
        var operation = new AddKeyOperation(resourceFiles, "NewKey", values);
        operation.Execute();

        // Act
        operation.Undo();

        // Assert
        Assert.Empty(resourceFiles[0].Entries);
    }

    // Mock operation for testing
    private class MockOperation : IOperation
    {
        public string Description { get; }
        public bool WasExecuted { get; set; }
        public bool WasUndone { get; set; }

        public MockOperation(string description)
        {
            Description = description;
        }

        public void Execute()
        {
            WasExecuted = true;
        }

        public void Undo()
        {
            WasUndone = true;
        }
    }
}
