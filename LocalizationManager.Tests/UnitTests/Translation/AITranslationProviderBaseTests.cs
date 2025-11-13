// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using LocalizationManager.Core.Translation;
using LocalizationManager.Core.Translation.Providers;
using Xunit;

namespace LocalizationManager.Tests.UnitTests.Translation;

public class AITranslationProviderBaseTests
{
    /// <summary>
    /// Test implementation of AITranslationProviderBase for testing protected methods.
    /// </summary>
    private class TestAIProvider : AITranslationProviderBase
    {
        public TestAIProvider(string? model = null, string? customSystemPrompt = null, int rateLimitRequestsPerMinute = 10)
            : base(model, customSystemPrompt, rateLimitRequestsPerMinute)
        {
        }

        public override string Name => "test";

        public override bool IsConfigured() => true;

        public override Task<TranslationResponse> TranslateAsync(
            TranslationRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TranslationResponse
            {
                TranslatedText = "translated",
                Provider = Name
            });
        }

        // Expose protected methods and properties for testing
        public string TestBuildUserPrompt(TranslationRequest request) => BuildUserPrompt(request);
        public string TestGetLanguageDisplayName(string languageCode) => GetLanguageDisplayName(languageCode);
        public string TestCleanTranslationResponse(string response) => CleanTranslationResponse(response);
        public string TestSystemPrompt => SystemPrompt;
    }

    #region BuildUserPrompt Tests

    [Fact]
    public void BuildUserPrompt_WithSourceAndTargetLanguageNames_UsesProvidedNames()
    {
        // Arrange
        var provider = new TestAIProvider();
        var request = new TranslationRequest
        {
            SourceText = "Hello",
            SourceLanguageName = "English",
            TargetLanguage = "fr",
            TargetLanguageName = "French"
        };

        // Act
        var prompt = provider.TestBuildUserPrompt(request);

        // Assert
        Assert.Contains("from English to French", prompt);
        Assert.Contains("Hello", prompt);
    }

    [Fact]
    public void BuildUserPrompt_WithSourceLanguageCodeOnly_UsesDisplayName()
    {
        // Arrange
        var provider = new TestAIProvider();
        var request = new TranslationRequest
        {
            SourceText = "Hello",
            SourceLanguage = "en",
            TargetLanguage = "fr"
        };

        // Act
        var prompt = provider.TestBuildUserPrompt(request);

        // Assert
        Assert.Contains("from English to French", prompt);
        Assert.Contains("Hello", prompt);
    }

    [Fact]
    public void BuildUserPrompt_WithNullSourceLanguage_UsesAutoDetected()
    {
        // Arrange
        var provider = new TestAIProvider();
        var request = new TranslationRequest
        {
            SourceText = "Hello",
            SourceLanguage = null,
            TargetLanguage = "fr"
        };

        // Act
        var prompt = provider.TestBuildUserPrompt(request);

        // Assert
        Assert.Contains("from auto-detected language to French", prompt);
        Assert.Contains("Hello", prompt);
    }

    [Fact]
    public void BuildUserPrompt_WithTargetLanguageNameOnly_UsesProvidedName()
    {
        // Arrange
        var provider = new TestAIProvider();
        var request = new TranslationRequest
        {
            SourceText = "Hello",
            SourceLanguage = "en",
            TargetLanguage = "el",
            TargetLanguageName = "Greek (el)"
        };

        // Act
        var prompt = provider.TestBuildUserPrompt(request);

        // Assert
        Assert.Contains("from English to Greek (el)", prompt);
        Assert.Contains("Hello", prompt);
    }

    [Fact]
    public void BuildUserPrompt_WithContext_IncludesContextInPrompt()
    {
        // Arrange
        var provider = new TestAIProvider();
        var request = new TranslationRequest
        {
            SourceText = "Hello",
            SourceLanguage = "en",
            TargetLanguage = "fr",
            Context = "Greeting message for users"
        };

        // Act
        var prompt = provider.TestBuildUserPrompt(request);

        // Assert
        Assert.Contains("Context: Greeting message for users", prompt);
        Assert.Contains("from English to French", prompt);
        Assert.Contains("Hello", prompt);
    }

    [Fact]
    public void BuildUserPrompt_WithoutContext_DoesNotIncludeContext()
    {
        // Arrange
        var provider = new TestAIProvider();
        var request = new TranslationRequest
        {
            SourceText = "Hello",
            SourceLanguage = "en",
            TargetLanguage = "fr"
        };

        // Act
        var prompt = provider.TestBuildUserPrompt(request);

        // Assert
        Assert.DoesNotContain("Context:", prompt);
    }

    #endregion

    #region GetLanguageDisplayName Tests

    [Theory]
    [InlineData("en", "English")]
    [InlineData("fr", "French")]
    [InlineData("de", "German")]
    [InlineData("es", "Spanish")]
    [InlineData("el", "Greek")]
    [InlineData("ja", "Japanese")]
    [InlineData("zh", "Chinese")]
    public void GetLanguageDisplayName_WithValidLanguageCode_ReturnsEnglishName(string code, string expectedName)
    {
        // Arrange
        var provider = new TestAIProvider();

        // Act
        var displayName = provider.TestGetLanguageDisplayName(code);

        // Assert
        Assert.Equal(expectedName, displayName);
    }

    [Fact]
    public void GetLanguageDisplayName_WithInvalidLanguageCode_ReturnsDisplayNameOrCode()
    {
        // Arrange
        var provider = new TestAIProvider();
        var invalidCode = "xxx";  // Use a truly invalid code that will throw

        // Act
        var displayName = provider.TestGetLanguageDisplayName(invalidCode);

        // Assert
        // CultureInfo will either return a display name or the code itself if not found
        Assert.Equal(invalidCode, displayName);
    }

    #endregion

    #region CleanTranslationResponse Tests

    [Theory]
    [InlineData("Translation: Hello", "Hello")]
    [InlineData("Translated text: Bonjour", "Bonjour")]
    [InlineData("Here is the translation: Hola", "Hola")]
    [InlineData("The translation is: Guten Tag", "Guten Tag")]
    public void CleanTranslationResponse_RemovesCommonPrefixes(string input, string expected)
    {
        // Arrange
        var provider = new TestAIProvider();

        // Act
        var cleaned = provider.TestCleanTranslationResponse(input);

        // Assert
        Assert.Equal(expected, cleaned);
    }

    [Theory]
    [InlineData("\"Bonjour\"", "Bonjour")]
    [InlineData("'Hello'", "Hello")]
    public void CleanTranslationResponse_RemovesQuotes(string input, string expected)
    {
        // Arrange
        var provider = new TestAIProvider();

        // Act
        var cleaned = provider.TestCleanTranslationResponse(input);

        // Assert
        Assert.Equal(expected, cleaned);
    }

    [Fact]
    public void CleanTranslationResponse_WithPlainText_ReturnsUnchanged()
    {
        // Arrange
        var provider = new TestAIProvider();
        var input = "Hello World";

        // Act
        var cleaned = provider.TestCleanTranslationResponse(input);

        // Assert
        Assert.Equal(input, cleaned);
    }

    [Fact]
    public void CleanTranslationResponse_WithWhitespace_TrimsCorrectly()
    {
        // Arrange
        var provider = new TestAIProvider();
        var input = "  Hello World  ";

        // Act
        var cleaned = provider.TestCleanTranslationResponse(input);

        // Assert
        Assert.Equal("Hello World", cleaned);
    }

    [Fact]
    public void CleanTranslationResponse_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var provider = new TestAIProvider();

        // Act
        var cleaned = provider.TestCleanTranslationResponse("");

        // Assert
        Assert.Equal("", cleaned);
    }

    #endregion

    #region SystemPrompt Tests

    [Fact]
    public void SystemPrompt_WithoutCustomPrompt_ReturnsDefaultPrompt()
    {
        // Arrange
        var provider = new TestAIProvider();

        // Act
        var systemPrompt = provider.TestSystemPrompt;

        // Assert
        Assert.Contains("professional translator", systemPrompt);
        Assert.Contains("ONLY the translated text", systemPrompt);
    }

    [Fact]
    public void SystemPrompt_WithCustomPrompt_ReturnsCustomPrompt()
    {
        // Arrange
        var customPrompt = "Custom translation instructions";
        var provider = new TestAIProvider(customSystemPrompt: customPrompt);

        // Act
        var systemPrompt = provider.TestSystemPrompt;

        // Assert
        Assert.Equal(customPrompt, systemPrompt);
    }

    #endregion

    #region RateLimit Tests

    [Fact]
    public void GetRateLimit_ReturnsConfiguredValue()
    {
        // Arrange
        var rateLimit = 20;
        var provider = new TestAIProvider(rateLimitRequestsPerMinute: rateLimit);

        // Act
        var result = provider.GetRateLimit();

        // Assert
        Assert.Equal(rateLimit, result);
    }

    #endregion
}
