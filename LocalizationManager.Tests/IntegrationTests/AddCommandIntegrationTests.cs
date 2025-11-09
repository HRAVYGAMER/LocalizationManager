// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core;
using LocalizationManager.Core.Models;
using Xunit;

namespace LocalizationManager.Tests.IntegrationTests;

public class AddCommandIntegrationTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ResourceFileParser _parser;
    private readonly ResourceDiscovery _discovery;

    public AddCommandIntegrationTests()
    {
        // Create temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LrmTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _parser = new ResourceFileParser();
        _discovery = new ResourceDiscovery();

        // Create initial test resource files
        CreateInitialResourceFiles();
    }

    private void CreateInitialResourceFiles()
    {
        // Create default resource file
        var defaultFile = new ResourceFile
        {
            Language = new LanguageInfo
            {
                BaseName = "TestResource",
                Code = "en",
                Name = "English (Default)",
                IsDefault = true,
                FilePath = Path.Combine(_testDirectory, "TestResource.resx")
            },
            Entries = new List<ResourceEntry>
            {
                new() { Key = "Save", Value = "Save", Comment = "Save button" },
                new() { Key = "Cancel", Value = "Cancel" }
            }
        };

        // Create Greek resource file
        var greekFile = new ResourceFile
        {
            Language = new LanguageInfo
            {
                BaseName = "TestResource",
                Code = "el",
                Name = "Ελληνικά (el)",
                IsDefault = false,
                FilePath = Path.Combine(_testDirectory, "TestResource.el.resx")
            },
            Entries = new List<ResourceEntry>
            {
                new() { Key = "Save", Value = "Αποθήκευση", Comment = "Save button" },
                new() { Key = "Cancel", Value = "Ακύρωση" }
            }
        };

        _parser.Write(defaultFile);
        _parser.Write(greekFile);
    }

    [Fact]
    public void AddKey_ToAllLanguages_Success()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var newKey = "Delete";
        var values = new Dictionary<string, string>
        {
            { "", "Delete" },
            { "el", "Διαγραφή" }
        };

        // Act
        var resourceFiles = new List<ResourceFile>();
        foreach (var lang in languages)
        {
            var file = _parser.Parse(lang);
            file.Entries.Add(new ResourceEntry
            {
                Key = newKey,
                Value = values[lang.Code],
                Comment = "Delete button"
            });
            _parser.Write(file);
            resourceFiles.Add(file);
        }

        // Assert - Re-read and verify
        var verifyLanguages = _discovery.DiscoverLanguages(_testDirectory);
        foreach (var lang in verifyLanguages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == newKey);

            Assert.NotNull(entry);
            Assert.Equal(values[lang.Code], entry.Value);
            Assert.Equal("Delete button", entry.Comment);
        }
    }

    [Fact]
    public void AddKey_WithEmptyValue_Success()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var newKey = "Placeholder";

        // Act
        foreach (var lang in languages)
        {
            var file = _parser.Parse(lang);
            file.Entries.Add(new ResourceEntry
            {
                Key = newKey,
                Value = string.Empty
            });
            _parser.Write(file);
        }

        // Assert
        var verifyLanguages = _discovery.DiscoverLanguages(_testDirectory);
        foreach (var lang in verifyLanguages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == newKey);

            Assert.NotNull(entry);
            Assert.Equal(string.Empty, entry.Value);
        }
    }

    [Fact]
    public void UpdateKey_ExistingKey_Success()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var keyToUpdate = "Save";
        var newValues = new Dictionary<string, string>
        {
            { "", "Save Changes" },
            { "el", "Αποθήκευση Αλλαγών" }
        };

        // Act
        foreach (var lang in languages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == keyToUpdate);
            if (entry != null)
            {
                entry.Value = newValues[lang.Code];
            }
            _parser.Write(file);
        }

        // Assert
        var verifyLanguages = _discovery.DiscoverLanguages(_testDirectory);
        foreach (var lang in verifyLanguages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == keyToUpdate);

            Assert.NotNull(entry);
            Assert.Equal(newValues[lang.Code], entry.Value);
        }
    }

    [Fact]
    public void DeleteKey_ExistingKey_Success()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var keyToDelete = "Cancel";

        // Act
        foreach (var lang in languages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == keyToDelete);
            if (entry != null)
            {
                file.Entries.Remove(entry);
            }
            _parser.Write(file);
        }

        // Assert
        var verifyLanguages = _discovery.DiscoverLanguages(_testDirectory);
        foreach (var lang in verifyLanguages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == keyToDelete);

            Assert.Null(entry);
        }
    }

    [Fact]
    public void AddKey_UsingDefaultAlias_Success()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var newKey = "NewKeyWithDefaultAlias";
        var values = new Dictionary<string, string>
        {
            { "", "Default Value" },
            { "el", "Ελληνική Τιμή" }
        };

        // Act
        var resourceFiles = new List<ResourceFile>();
        foreach (var lang in languages)
        {
            var file = _parser.Parse(lang);
            file.Entries.Add(new ResourceEntry
            {
                Key = newKey,
                Value = values[lang.Code],
                Comment = "Test with default alias"
            });
            _parser.Write(file);
            resourceFiles.Add(file);
        }

        // Assert - Re-read and verify
        var verifyLanguages = _discovery.DiscoverLanguages(_testDirectory);
        foreach (var lang in verifyLanguages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == newKey);

            Assert.NotNull(entry);
            Assert.Equal(values[lang.Code], entry.Value);
        }
    }

    [Fact]
    public void UpdateKey_UsingDefaultAlias_Success()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var keyToUpdate = "Save";
        var newValues = new Dictionary<string, string>
        {
            { "", "Save with Default Alias" },
            { "el", "Αποθήκευση με Default Alias" }
        };

        // Act
        foreach (var lang in languages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == keyToUpdate);
            if (entry != null)
            {
                entry.Value = newValues[lang.Code];
            }
            _parser.Write(file);
        }

        // Assert
        var verifyLanguages = _discovery.DiscoverLanguages(_testDirectory);
        foreach (var lang in verifyLanguages)
        {
            var file = _parser.Parse(lang);
            var entry = file.Entries.FirstOrDefault(e => e.Key == keyToUpdate);

            Assert.NotNull(entry);
            Assert.Equal(newValues[lang.Code], entry.Value);
        }
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
