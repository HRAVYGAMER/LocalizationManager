// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core;
using Xunit;

namespace LocalizationManager.Tests.UnitTests;

public class ResourceDiscoveryTests
{
    private readonly string _testDataPath;

    public ResourceDiscoveryTests()
    {
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
    }

    [Fact]
    public void DiscoverLanguages_ValidDirectory_FindsLanguages()
    {
        // Arrange
        var discovery = new ResourceDiscovery();

        // Act
        var languages = discovery.DiscoverLanguages(_testDataPath);

        // Assert
        Assert.NotEmpty(languages);
        Assert.Equal(3, languages.Count); // TestResource.resx, TestResource.el.resx, and TestResource.fr.resx
    }

    [Fact]
    public void DiscoverLanguages_ValidDirectory_IdentifiesDefaultLanguage()
    {
        // Arrange
        var discovery = new ResourceDiscovery();

        // Act
        var languages = discovery.DiscoverLanguages(_testDataPath);

        // Assert
        var defaultLang = languages.FirstOrDefault(l => l.IsDefault);
        Assert.NotNull(defaultLang);
        Assert.Equal("", defaultLang.Code);
        Assert.Contains("Default", defaultLang.Name);
    }

    [Fact]
    public void DiscoverLanguages_ValidDirectory_IdentifiesGreekLanguage()
    {
        // Arrange
        var discovery = new ResourceDiscovery();

        // Act
        var languages = discovery.DiscoverLanguages(_testDataPath);

        // Assert
        var greekLang = languages.FirstOrDefault(l => l.Code == "el");
        Assert.NotNull(greekLang);
        Assert.Equal("el", greekLang.Code);
        Assert.False(greekLang.IsDefault);
        Assert.Contains("TestResource.el.resx", greekLang.FilePath);
    }

    [Fact]
    public void DiscoverLanguages_EmptyDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var discovery = new ResourceDiscovery();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => discovery.DiscoverLanguages(nonExistentPath));
    }

    [Fact]
    public void DiscoverLanguages_DirectoryWithNoResxFiles_ReturnsEmptyList()
    {
        // Arrange
        var discovery = new ResourceDiscovery();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var languages = discovery.DiscoverLanguages(tempDir);

            // Assert
            Assert.Empty(languages);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void DiscoverLanguages_ValidDirectory_SetsCorrectFilePaths()
    {
        // Arrange
        var discovery = new ResourceDiscovery();

        // Act
        var languages = discovery.DiscoverLanguages(_testDataPath);

        // Assert
        foreach (var lang in languages)
        {
            Assert.NotNull(lang.FilePath);
            Assert.True(File.Exists(lang.FilePath), $"File path {lang.FilePath} does not exist");
            Assert.EndsWith(".resx", lang.FilePath);
        }
    }
}
