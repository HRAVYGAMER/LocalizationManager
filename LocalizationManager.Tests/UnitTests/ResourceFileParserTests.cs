// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core;
using LocalizationManager.Core.Models;
using Xunit;

namespace LocalizationManager.Tests.UnitTests;

public class ResourceFileParserTests
{
    private readonly string _testDataPath;

    public ResourceFileParserTests()
    {
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
    }

    [Fact]
    public void Parse_ValidResxFile_ReturnsResourceFile()
    {
        // Arrange
        var parser = new ResourceFileParser();
        var languageInfo = new LanguageInfo
        {
            BaseName = "TestResource",
            Code = "en",
            Name = "English (Default)",
            IsDefault = true,
            FilePath = Path.Combine(_testDataPath, "TestResource.resx")
        };

        // Act
        var result = parser.Parse(languageInfo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(languageInfo, result.Language);
        Assert.NotEmpty(result.Entries);
        Assert.Equal(4, result.Entries.Count); // Save, Cancel, Welcome, EmptyValue
    }

    [Fact]
    public void Parse_ValidResxFile_ParsesEntriesCorrectly()
    {
        // Arrange
        var parser = new ResourceFileParser();
        var languageInfo = new LanguageInfo
        {
            BaseName = "TestResource",
            Code = "en",
            Name = "English (Default)",
            IsDefault = true,
            FilePath = Path.Combine(_testDataPath, "TestResource.resx")
        };

        // Act
        var result = parser.Parse(languageInfo);

        // Assert
        var saveEntry = result.Entries.FirstOrDefault(e => e.Key == "Save");
        Assert.NotNull(saveEntry);
        Assert.Equal("Save", saveEntry.Value);
        Assert.Equal("Save button label", saveEntry.Comment);
    }

    [Fact]
    public void Parse_ValidResxFile_ParsesEmptyValues()
    {
        // Arrange
        var parser = new ResourceFileParser();
        var languageInfo = new LanguageInfo
        {
            BaseName = "TestResource",
            Code = "en",
            Name = "English (Default)",
            IsDefault = true,
            FilePath = Path.Combine(_testDataPath, "TestResource.resx")
        };

        // Act
        var result = parser.Parse(languageInfo);

        // Assert
        var emptyEntry = result.Entries.FirstOrDefault(e => e.Key == "EmptyValue");
        Assert.NotNull(emptyEntry);
        Assert.Equal(string.Empty, emptyEntry.Value);
    }

    [Fact]
    public void Parse_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var parser = new ResourceFileParser();
        var languageInfo = new LanguageInfo
        {
            BaseName = "NonExistent",
            Code = "en",
            Name = "English",
            IsDefault = true,
            FilePath = Path.Combine(_testDataPath, "NonExistent.resx")
        };

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => parser.Parse(languageInfo));
    }

    [Fact]
    public void Write_ValidResourceFile_CreatesFile()
    {
        // Arrange
        var parser = new ResourceFileParser();
        var tempFile = Path.Combine(Path.GetTempPath(), "TempTest.resx");
        var resourceFile = new ResourceFile
        {
            Language = new LanguageInfo
            {
                BaseName = "TempTest",
                Code = "en",
                Name = "English",
                IsDefault = true,
                FilePath = tempFile
            },
            Entries = new List<ResourceEntry>
            {
                new() { Key = "TestKey", Value = "Test Value", Comment = "Test Comment" }
            }
        };

        try
        {
            // Act
            parser.Write(resourceFile);

            // Assert
            Assert.True(File.Exists(tempFile));

            // Verify we can read it back
            var readBack = parser.Parse(resourceFile.Language);
            Assert.Single(readBack.Entries);
            Assert.Equal("TestKey", readBack.Entries[0].Key);
            Assert.Equal("Test Value", readBack.Entries[0].Value);
            Assert.Equal("Test Comment", readBack.Entries[0].Comment);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
