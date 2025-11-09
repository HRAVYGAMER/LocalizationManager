// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core;
using LocalizationManager.Core.Models;
using Xunit;

namespace LocalizationManager.Tests.UnitTests;

public class ResourceValidatorTests
{
    [Fact]
    public void Validate_AllKeysPresent_ReturnsNoMissingKeys()
    {
        // Arrange
        var validator = new ResourceValidator();
        var resourceFiles = new List<ResourceFile>
        {
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "en", Name = "English", IsDefault = true, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Save" },
                    new() { Key = "Cancel", Value = "Cancel" }
                }
            },
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "el", Name = "Greek", IsDefault = false, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Αποθήκευση" },
                    new() { Key = "Cancel", Value = "Ακύρωση" }
                }
            }
        };

        // Act
        var result = validator.Validate(resourceFiles);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.MissingKeys);
        Assert.Empty(result.ExtraKeys);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MissingKeys_ReturnsMissingKeysList()
    {
        // Arrange
        var validator = new ResourceValidator();
        var resourceFiles = new List<ResourceFile>
        {
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "en", Name = "English", IsDefault = true, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Save" },
                    new() { Key = "Cancel", Value = "Cancel" },
                    new() { Key = "Delete", Value = "Delete" }
                }
            },
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "el", Name = "Greek", IsDefault = false, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Αποθήκευση" }
                }
            }
        };

        // Act
        var result = validator.Validate(resourceFiles);

        // Assert
        Assert.True(result.MissingKeys.ContainsKey("el"));
        Assert.Equal(2, result.MissingKeys["el"].Count);
        Assert.Contains("Cancel", result.MissingKeys["el"]);
        Assert.Contains("Delete", result.MissingKeys["el"]);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ExtraKeys_ReturnsExtraKeysList()
    {
        // Arrange
        var validator = new ResourceValidator();
        var resourceFiles = new List<ResourceFile>
        {
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "en", Name = "English", IsDefault = true, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Save" }
                }
            },
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "el", Name = "Greek", IsDefault = false, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Αποθήκευση" },
                    new() { Key = "ExtraKey", Value = "Extra" }
                }
            }
        };

        // Act
        var result = validator.Validate(resourceFiles);

        // Assert
        Assert.True(result.ExtraKeys.ContainsKey("el"));
        Assert.Single(result.ExtraKeys["el"]);
        Assert.Contains("ExtraKey", result.ExtraKeys["el"]);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyValues_ReturnsEmptyValuesList()
    {
        // Arrange
        var validator = new ResourceValidator();
        var resourceFiles = new List<ResourceFile>
        {
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "en", Name = "English", IsDefault = true, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Save" },
                    new() { Key = "Cancel", Value = "" }
                }
            },
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "el", Name = "Greek", IsDefault = false, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Αποθήκευση" },
                    new() { Key = "Cancel", Value = "" }
                }
            }
        };

        // Act
        var result = validator.Validate(resourceFiles);

        // Assert
        Assert.True(result.EmptyValues.ContainsKey("el"));
        Assert.Single(result.EmptyValues["el"]);
        Assert.Contains("Cancel", result.EmptyValues["el"]);

        // Default language should also be checked for empty values
        Assert.True(result.EmptyValues.ContainsKey("en"));
        Assert.Single(result.EmptyValues["en"]);
        Assert.Contains("Cancel", result.EmptyValues["en"]);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_DuplicateKeys_ReturnsDuplicatesList()
    {
        // Arrange
        var validator = new ResourceValidator();
        var resourceFiles = new List<ResourceFile>
        {
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "en", Name = "English", IsDefault = true, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Save" }
                }
            },
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "el", Name = "Greek", IsDefault = false, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Αποθήκευση" },
                    new() { Key = "Save", Value = "Αποθήκευση Duplicate" }
                }
            }
        };

        // Act
        var result = validator.Validate(resourceFiles);

        // Assert
        Assert.True(result.DuplicateKeys.ContainsKey("el"));
        Assert.Single(result.DuplicateKeys["el"]);
        Assert.Contains("Save", result.DuplicateKeys["el"]);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ComplexScenario_ReturnsAllIssues()
    {
        // Arrange
        var validator = new ResourceValidator();
        var resourceFiles = new List<ResourceFile>
        {
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "en", Name = "English", IsDefault = true, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "Save" },
                    new() { Key = "Cancel", Value = "Cancel" },
                    new() { Key = "Delete", Value = "Delete" }
                }
            },
            new()
            {
                Language = new LanguageInfo { BaseName = "Test", Code = "el", Name = "Greek", IsDefault = false, FilePath = "" },
                Entries = new List<ResourceEntry>
                {
                    new() { Key = "Save", Value = "" }, // Empty value
                    new() { Key = "ExtraKey", Value = "Extra" }, // Extra key
                    new() { Key = "Cancel", Value = "Ακύρωση" },
                    new() { Key = "Cancel", Value = "Ακύρωση Duplicate" } // Duplicate
                    // Missing: Delete
                }
            }
        };

        // Act
        var result = validator.Validate(resourceFiles);

        // Assert
        Assert.True(result.MissingKeys.ContainsKey("el"));
        Assert.Single(result.MissingKeys["el"]); // Delete
        Assert.Contains("Delete", result.MissingKeys["el"]);

        Assert.True(result.ExtraKeys.ContainsKey("el"));
        Assert.Single(result.ExtraKeys["el"]); // ExtraKey
        Assert.Contains("ExtraKey", result.ExtraKeys["el"]);

        Assert.True(result.EmptyValues.ContainsKey("el"));
        Assert.Single(result.EmptyValues["el"]); // Save
        Assert.Contains("Save", result.EmptyValues["el"]);

        Assert.True(result.DuplicateKeys.ContainsKey("el"));
        Assert.Single(result.DuplicateKeys["el"]); // Cancel
        Assert.Contains("Cancel", result.DuplicateKeys["el"]);

        Assert.False(result.IsValid);
        Assert.True(result.TotalIssues >= 4);
    }
}
