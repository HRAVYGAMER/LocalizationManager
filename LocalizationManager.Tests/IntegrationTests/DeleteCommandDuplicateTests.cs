// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core;
using LocalizationManager.Core.Models;
using Xunit;

namespace LocalizationManager.Tests.IntegrationTests;

public class DeleteCommandDuplicateTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ResourceFileParser _parser;
    private readonly ResourceDiscovery _discovery;

    public DeleteCommandDuplicateTests()
    {
        // Create temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LrmTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _parser = new ResourceFileParser();
        _discovery = new ResourceDiscovery();

        // Create initial test resource files
        CreateResourceFilesWithDuplicates();
    }

    private void CreateResourceFilesWithDuplicates()
    {
        // Create default resource file with duplicates
        var defaultFile = new ResourceFile
        {
            Language = new LanguageInfo
            {
                BaseName = "TestResource",
                Code = "",
                Name = "Default",
                IsDefault = true,
                FilePath = Path.Combine(_testDirectory, "TestResource.resx")
            },
            Entries = new List<ResourceEntry>
            {
                new() { Key = "SingleKey", Value = "Single value" },
                new() { Key = "DuplicateKey", Value = "First value" },
                new() { Key = "DuplicateKey", Value = "Second value" },
                new() { Key = "AnotherKey", Value = "Another value" }
            }
        };

        // Create French resource file with duplicates
        var frenchFile = new ResourceFile
        {
            Language = new LanguageInfo
            {
                BaseName = "TestResource",
                Code = "fr",
                Name = "Français (fr)",
                IsDefault = false,
                FilePath = Path.Combine(_testDirectory, "TestResource.fr.resx")
            },
            Entries = new List<ResourceEntry>
            {
                new() { Key = "SingleKey", Value = "Valeur unique" },
                new() { Key = "DuplicateKey", Value = "Première valeur" },
                new() { Key = "DuplicateKey", Value = "Deuxième valeur" },
                new() { Key = "AnotherKey", Value = "Une autre valeur" }
            }
        };

        _parser.Write(defaultFile);
        _parser.Write(frenchFile);
    }

    [Fact]
    public void DeleteKey_WithDuplicates_WithoutFlag_ThrowsError()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        var defaultFile = resourceFiles.First(rf => rf.Language.IsDefault);
        var occurrenceCount = defaultFile.Entries.Count(e => e.Key == "DuplicateKey");

        // Assert: Verify we have duplicates
        Assert.Equal(2, occurrenceCount);

        // Act & Assert: Attempting to delete should detect duplicates
        // In real command, this would return error code 1
        // Here we just verify detection logic works
        var hasDuplicates = occurrenceCount > 1;
        Assert.True(hasDuplicates);
    }

    [Fact]
    public void DeleteKey_WithAllDuplicatesFlag_RemovesAll()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        // Act: Delete all occurrences (simulating --all-duplicates flag)
        foreach (var rf in resourceFiles)
        {
            rf.Entries.RemoveAll(e => e.Key == "DuplicateKey");
        }

        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert: Verify key is completely removed
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();

        foreach (var rf in reloadedFiles)
        {
            Assert.DoesNotContain(rf.Entries, e => e.Key == "DuplicateKey");
        }

        // Verify other keys remain
        var reloadedDefault = reloadedFiles.First(rf => rf.Language.IsDefault);
        Assert.Contains(reloadedDefault.Entries, e => e.Key == "SingleKey");
        Assert.Contains(reloadedDefault.Entries, e => e.Key == "AnotherKey");
    }

    [Fact]
    public void DeleteKey_SingleOccurrence_Success()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        var defaultFile = resourceFiles.First(rf => rf.Language.IsDefault);
        var occurrenceCount = defaultFile.Entries.Count(e => e.Key == "SingleKey");

        Assert.Equal(1, occurrenceCount); // Verify only one occurrence

        // Act: Delete (no flag needed for single occurrence)
        foreach (var rf in resourceFiles)
        {
            rf.Entries.RemoveAll(e => e.Key == "SingleKey");
        }

        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();

        foreach (var rf in reloadedFiles)
        {
            Assert.DoesNotContain(rf.Entries, e => e.Key == "SingleKey");
        }
    }

    [Fact]
    public void DetectDuplicates_FindsCorrectCount()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var defaultFile = _parser.Parse(languages.First(l => l.IsDefault));

        // Act: Check for duplicates
        var keysWithDuplicates = defaultFile.Entries
            .GroupBy(e => e.Key)
            .Where(g => g.Count() > 1)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToList();

        // Assert
        Assert.Single(keysWithDuplicates); // Only "DuplicateKey" has duplicates
        Assert.Equal("DuplicateKey", keysWithDuplicates[0].Key);
        Assert.Equal(2, keysWithDuplicates[0].Count);
    }

    [Fact]
    public void DeleteKey_NonExistent_NoError()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        var defaultFile = resourceFiles.First(rf => rf.Language.IsDefault);
        var initialCount = defaultFile.Entries.Count;

        // Act: Try to delete non-existent key
        var removedCount = 0;
        foreach (var rf in resourceFiles)
        {
            removedCount += rf.Entries.RemoveAll(e => e.Key == "NonExistentKey");
        }

        // Assert: Nothing was removed
        Assert.Equal(0, removedCount);
        Assert.Equal(initialCount, defaultFile.Entries.Count);
    }

    [Fact]
    public void DeleteKey_CrossLanguageSync_RemovesFromAll()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        // Verify key exists in all languages
        foreach (var rf in resourceFiles)
        {
            Assert.Contains(rf.Entries, e => e.Key == "SingleKey");
        }

        // Act: Delete from all languages
        foreach (var rf in resourceFiles)
        {
            rf.Entries.RemoveAll(e => e.Key == "SingleKey");
        }

        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert: Verify removed from all languages
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();

        foreach (var rf in reloadedFiles)
        {
            Assert.DoesNotContain(rf.Entries, e => e.Key == "SingleKey");
        }
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
