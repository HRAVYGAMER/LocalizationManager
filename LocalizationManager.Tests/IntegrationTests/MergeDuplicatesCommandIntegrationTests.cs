// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core;
using LocalizationManager.Core.Models;
using Xunit;

namespace LocalizationManager.Tests.IntegrationTests;

public class MergeDuplicatesCommandIntegrationTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ResourceFileParser _parser;
    private readonly ResourceDiscovery _discovery;

    public MergeDuplicatesCommandIntegrationTests()
    {
        // Create temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LrmTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _parser = new ResourceFileParser();
        _discovery = new ResourceDiscovery();

        // Create initial test resource files with duplicates
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
                new() { Key = "SingleKey", Value = "Single value", Comment = "Single comment" },
                new() { Key = "DuplicateKey", Value = "First value", Comment = "First comment" },
                new() { Key = "DuplicateKey", Value = "Second value", Comment = "Second comment" },
                new() { Key = "TripleDuplicate", Value = "Value one" },
                new() { Key = "TripleDuplicate", Value = "Value two" },
                new() { Key = "TripleDuplicate", Value = "Value three" },
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
                new() { Key = "SingleKey", Value = "Valeur unique", Comment = "Commentaire unique" },
                new() { Key = "DuplicateKey", Value = "Première valeur", Comment = "Premier commentaire" },
                new() { Key = "DuplicateKey", Value = "Deuxième valeur", Comment = "Deuxième commentaire" },
                new() { Key = "TripleDuplicate", Value = "Valeur un" },
                new() { Key = "TripleDuplicate", Value = "Valeur deux" },
                new() { Key = "TripleDuplicate", Value = "Valeur trois" },
                new() { Key = "AnotherKey", Value = "Une autre valeur" }
            }
        };

        _parser.Write(defaultFile);
        _parser.Write(frenchFile);
    }

    [Fact]
    public void MergeAutoFirst_SingleKey_RemovesDuplicates()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        var defaultFile = resourceFiles.First(rf => rf.Language.IsDefault);

        // Verify initial state: 2 occurrences
        var initialOccurrences = defaultFile.Entries.Count(e => e.Key == "DuplicateKey");
        Assert.Equal(2, initialOccurrences);

        // Act: Merge keeping first occurrence
        MergeKeyAutoFirst(resourceFiles, "DuplicateKey");

        // Save changes
        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert: Re-read and verify only 1 occurrence remains
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();
        var reloadedDefault = reloadedFiles.First(rf => rf.Language.IsDefault);

        var finalOccurrences = reloadedDefault.Entries.Count(e => e.Key == "DuplicateKey");
        Assert.Equal(1, finalOccurrences);

        // Verify the first occurrence was kept
        var remainingEntry = reloadedDefault.Entries.First(e => e.Key == "DuplicateKey");
        Assert.Equal("First value", remainingEntry.Value);
        Assert.Equal("First comment", remainingEntry.Comment);

        // Verify same for French
        var reloadedFrench = reloadedFiles.First(rf => rf.Language.Code == "fr");
        var frenchOccurrences = reloadedFrench.Entries.Count(e => e.Key == "DuplicateKey");
        Assert.Equal(1, frenchOccurrences);

        var frenchEntry = reloadedFrench.Entries.First(e => e.Key == "DuplicateKey");
        Assert.Equal("Première valeur", frenchEntry.Value);
    }

    [Fact]
    public void MergeAutoFirst_TripleDuplicate_KeepsOnlyFirst()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        // Act
        MergeKeyAutoFirst(resourceFiles, "TripleDuplicate");

        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();

        foreach (var rf in reloadedFiles)
        {
            var occurrences = rf.Entries.Count(e => e.Key == "TripleDuplicate");
            Assert.Equal(1, occurrences);

            var entry = rf.Entries.First(e => e.Key == "TripleDuplicate");
            if (rf.Language.IsDefault)
            {
                Assert.Equal("Value one", entry.Value);
            }
            else
            {
                Assert.Equal("Valeur un", entry.Value);
            }
        }
    }

    [Fact]
    public void MergeAutoFirst_AllDuplicates_MergesAll()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();
        var defaultFile = resourceFiles.First(rf => rf.Language.IsDefault);

        // Find all keys with duplicates
        var keysToMerge = defaultFile.Entries
            .GroupBy(e => e.Key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.Equal(2, keysToMerge.Count); // DuplicateKey and TripleDuplicate

        // Act: Merge all
        foreach (var key in keysToMerge)
        {
            MergeKeyAutoFirst(resourceFiles, key);
        }

        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert: Verify no duplicates remain
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();
        var reloadedDefault = reloadedFiles.First(rf => rf.Language.IsDefault);

        var duplicateKeys = reloadedDefault.Entries
            .GroupBy(e => e.Key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.Empty(duplicateKeys);

        // Verify total entry count (4 unique keys)
        var uniqueKeys = reloadedDefault.Entries.Select(e => e.Key).Distinct().Count();
        Assert.Equal(4, uniqueKeys);
        Assert.Equal(4, reloadedDefault.Entries.Count);
    }

    [Fact]
    public void MergeCustomSelection_KeepsSelectedOccurrence()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        // Select second occurrence for both languages
        var selections = new Dictionary<string, int>
        {
            { "", 2 },    // Default: keep second occurrence
            { "fr", 2 }   // French: keep second occurrence
        };

        // Act
        ApplyMergeWithSelections(resourceFiles, "DuplicateKey", selections);

        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();

        var reloadedDefault = reloadedFiles.First(rf => rf.Language.IsDefault);
        var defaultEntry = reloadedDefault.Entries.First(e => e.Key == "DuplicateKey");
        Assert.Equal("Second value", defaultEntry.Value);
        Assert.Equal("Second comment", defaultEntry.Comment);

        var reloadedFrench = reloadedFiles.First(rf => rf.Language.Code == "fr");
        var frenchEntry = reloadedFrench.Entries.First(e => e.Key == "DuplicateKey");
        Assert.Equal("Deuxième valeur", frenchEntry.Value);
        Assert.Equal("Deuxième commentaire", frenchEntry.Comment);
    }

    [Fact]
    public void MergeCustomSelection_DifferentPerLanguage_Success()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        // Select different occurrences per language
        var selections = new Dictionary<string, int>
        {
            { "", 1 },    // Default: keep first
            { "fr", 2 }   // French: keep second
        };

        // Act
        ApplyMergeWithSelections(resourceFiles, "DuplicateKey", selections);

        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();

        var reloadedDefault = reloadedFiles.First(rf => rf.Language.IsDefault);
        var defaultEntry = reloadedDefault.Entries.First(e => e.Key == "DuplicateKey");
        Assert.Equal("First value", defaultEntry.Value);

        var reloadedFrench = reloadedFiles.First(rf => rf.Language.Code == "fr");
        var frenchEntry = reloadedFrench.Entries.First(e => e.Key == "DuplicateKey");
        Assert.Equal("Deuxième valeur", frenchEntry.Value);
    }

    [Fact]
    public void Merge_NonExistentKey_NoError()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        // Act: Try to merge a non-existent key (should not throw)
        MergeKeyAutoFirst(resourceFiles, "NonExistentKey");

        // No exception means success - just verify nothing changed
        var defaultFile = resourceFiles.First(rf => rf.Language.IsDefault);
        Assert.DoesNotContain(defaultFile.Entries, e => e.Key == "NonExistentKey");
    }

    [Fact]
    public void Merge_SingleOccurrenceKey_NoChange()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        var defaultFile = resourceFiles.First(rf => rf.Language.IsDefault);
        var initialValue = defaultFile.Entries.First(e => e.Key == "SingleKey").Value;

        // Act: Merge a key with only one occurrence
        MergeKeyAutoFirst(resourceFiles, "SingleKey");

        // Assert: Value should remain unchanged
        var finalValue = defaultFile.Entries.First(e => e.Key == "SingleKey").Value;
        Assert.Equal(initialValue, finalValue);

        var occurrences = defaultFile.Entries.Count(e => e.Key == "SingleKey");
        Assert.Equal(1, occurrences);
    }

    [Fact]
    public void Merge_PreservesOtherKeys()
    {
        // Arrange
        var languages = _discovery.DiscoverLanguages(_testDirectory);
        var resourceFiles = languages.Select(lang => _parser.Parse(lang)).ToList();

        var defaultFile = resourceFiles.First(rf => rf.Language.IsDefault);
        var initialKeyCount = defaultFile.Entries.Select(e => e.Key).Distinct().Count();

        // Act: Merge duplicates
        MergeKeyAutoFirst(resourceFiles, "DuplicateKey");

        foreach (var rf in resourceFiles)
        {
            _parser.Write(rf);
        }

        // Assert: Other keys should still exist
        var reloadedLanguages = _discovery.DiscoverLanguages(_testDirectory);
        var reloadedFiles = reloadedLanguages.Select(lang => _parser.Parse(lang)).ToList();
        var reloadedDefault = reloadedFiles.First(rf => rf.Language.IsDefault);

        Assert.Contains(reloadedDefault.Entries, e => e.Key == "SingleKey");
        Assert.Contains(reloadedDefault.Entries, e => e.Key == "AnotherKey");
        Assert.Contains(reloadedDefault.Entries, e => e.Key == "TripleDuplicate");
    }

    // Helper methods matching MergeDuplicatesCommand logic

    private void MergeKeyAutoFirst(List<ResourceFile> resourceFiles, string key)
    {
        // For each language, keep the first occurrence and remove the rest
        foreach (var rf in resourceFiles)
        {
            var occurrences = rf.Entries
                .Select((e, i) => (Entry: e, Index: i))
                .Where(x => x.Entry.Key == key)
                .ToList();

            if (occurrences.Count <= 1)
                continue;

            // Remove all occurrences except the first (in reverse order to maintain indices)
            for (int i = occurrences.Count - 1; i >= 1; i--)
            {
                rf.Entries.RemoveAt(occurrences[i].Index);
            }
        }
    }

    private void ApplyMergeWithSelections(List<ResourceFile> resourceFiles, string key, Dictionary<string, int> selections)
    {
        // Apply merge: keep selected occurrence, remove others
        foreach (var rf in resourceFiles)
        {
            if (!selections.ContainsKey(rf.Language.Code))
                continue;

            var selectedOccurrence = selections[rf.Language.Code];
            var indices = rf.Entries
                .Select((e, i) => (Entry: e, Index: i))
                .Where(x => x.Entry.Key == key)
                .Select(x => x.Index)
                .ToList();

            if (indices.Count <= 1)
                continue;

            // Remove all except the selected one (in reverse to maintain indices)
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                if (i + 1 != selectedOccurrence)
                {
                    rf.Entries.RemoveAt(indices[i]);
                }
            }
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
