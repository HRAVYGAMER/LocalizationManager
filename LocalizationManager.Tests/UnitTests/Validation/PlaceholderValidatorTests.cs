// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core.Validation;
using Xunit;

namespace LocalizationManager.Tests.UnitTests.Validation;

public class PlaceholderValidatorTests
{
    [Fact]
    public void Validate_BothEmpty_ReturnsValid()
    {
        // Arrange
        var source = "";
        var translation = "";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_BothNull_ReturnsValid()
    {
        // Arrange
        string? source = null;
        string? translation = null;

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_NoPlaceholders_ReturnsValid()
    {
        // Arrange
        var source = "Hello world";
        var translation = "Bonjour le monde";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MatchingDotNetPlaceholders_ReturnsValid()
    {
        // Arrange
        var source = "Hello {0}!";
        var translation = "Bonjour {0}!";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.MissingPlaceholders);
        Assert.Empty(result.ExtraPlaceholders);
    }

    [Fact]
    public void Validate_MatchingNamedPlaceholders_ReturnsValid()
    {
        // Arrange
        var source = "Hello {name}!";
        var translation = "Bonjour {name}!";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MatchingPrintfPlaceholders_ReturnsValid()
    {
        // Arrange
        var source = "You have %d items";
        var translation = "Vous avez %d articles";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MatchingTemplateLiterals_ReturnsValid()
    {
        // Arrange
        var source = "Hello ${name}!";
        var translation = "Bonjour ${name}!";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MissingPlaceholder_ReturnsInvalid()
    {
        // Arrange
        var source = "Hello {0}!";
        var translation = "Bonjour!";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Single(result.MissingPlaceholders);
        Assert.Equal("{0}", result.MissingPlaceholders[0].Original);
        Assert.Contains("Missing placeholder: {0}", result.Errors);
    }

    [Fact]
    public void Validate_ExtraPlaceholder_ReturnsInvalid()
    {
        // Arrange
        var source = "Hello!";
        var translation = "Bonjour {0}!";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Single(result.ExtraPlaceholders);
        Assert.Contains("Extra placeholder not in source", result.Errors[0]);
    }

    [Fact]
    public void Validate_MultipleMissingPlaceholders_ReturnsAllMissing()
    {
        // Arrange
        var source = "Hello {0}, you have {1} items";
        var translation = "Bonjour";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.MissingPlaceholders.Count);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Validate_DifferentPlaceholderTypes_ReturnsInvalid()
    {
        // Arrange
        var source = "Hello {0}!";
        var translation = "Bonjour %s!";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        // {0} and %s have different normalized identifiers, so treated as missing + extra
        Assert.False(result.IsValid);
        Assert.Single(result.MissingPlaceholders); // {0}
        Assert.Single(result.ExtraPlaceholders); // %s
    }

    [Fact]
    public void Validate_PlaceholderCountMismatch_ReturnsInvalid()
    {
        // Arrange
        var source = "Value: {0}";
        var translation = "Value: {0} {0}"; // Duplicate in translation

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("count mismatch", result.Errors[0].ToLower());
    }

    [Fact]
    public void Validate_DifferentOrderSamePlaceholders_ReturnsValid()
    {
        // Arrange
        var source = "First {0}, Second {1}";
        var translation = "Second {1}, First {0}";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MultipleMatchingPlaceholders_ReturnsValid()
    {
        // Arrange
        var source = "Hello {0}, your balance is {1:C2} and status is {2}";
        var translation = "Bonjour {0}, votre solde est {1:C2} et le statut est {2}";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MixedPlaceholdersMissing_ReturnsInvalid()
    {
        // Arrange
        var source = "Hello {name}, you have %d items";
        var translation = "Bonjour {name}";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.MissingPlaceholders);
        Assert.Contains(result.MissingPlaceholders, p => p.Original == "%d");
    }

    [Fact]
    public void Validate_FormatSpecifierIgnored_ValidatesByIdentifier()
    {
        // Arrange
        var source = "Date: {0:yyyy-MM-dd}";
        var translation = "Date: {0}"; // Missing format specifier

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        // Should be valid because the identifier "0" matches
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NamedPlaceholdersWithFormat_ReturnsValid()
    {
        // Arrange
        var source = "Price: {amount:C2}";
        var translation = "Prix: {amount:C2}";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_PrintfPositional_ReturnsValid()
    {
        // Arrange
        var source = "Hello %1$s, you have %2$d items";
        var translation = "Bonjour %1$s, vous avez %2$d articles";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_TemplateLiteralWithDots_ReturnsValid()
    {
        // Arrange
        var source = "User: ${user.firstName} ${user.lastName}";
        var translation = "Utilisateur: ${user.firstName} ${user.lastName}";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetSummary_Valid_ReturnsSuccessMessage()
    {
        // Arrange
        var result = new PlaceholderValidationResult
        {
            IsValid = true,
            Errors = new List<string>()
        };

        // Act
        var summary = result.GetSummary();

        // Assert
        Assert.Equal("All placeholders valid", summary);
    }

    [Fact]
    public void GetSummary_Invalid_ReturnsErrorsSummary()
    {
        // Arrange
        var result = new PlaceholderValidationResult
        {
            IsValid = false,
            Errors = new List<string> { "Missing placeholder: {0}", "Extra placeholder not in source: {1}" }
        };

        // Act
        var summary = result.GetSummary();

        // Assert
        Assert.Contains("Missing placeholder: {0}", summary);
        Assert.Contains("Extra placeholder not in source: {1}", summary);
    }

    [Fact]
    public void ValidateBatch_MultipleTranslations_ValidatesAll()
    {
        // Arrange
        var translations = new Dictionary<string, (string source, string translation)>
        {
            ["Key1"] = ("Hello {0}", "Bonjour {0}"),
            ["Key2"] = ("You have %d items", "Vous avez %d articles"),
            ["Key3"] = ("Price: ${amount}", "Prix: ${amount}")
        };

        // Act
        var results = PlaceholderValidator.ValidateBatch(translations);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.True(results["Key1"].IsValid);
        Assert.True(results["Key2"].IsValid);
        Assert.True(results["Key3"].IsValid);
    }

    [Fact]
    public void ValidateBatch_SomeInvalid_ReturnsCorrectResults()
    {
        // Arrange
        var translations = new Dictionary<string, (string source, string translation)>
        {
            ["Valid"] = ("Hello {0}", "Bonjour {0}"),
            ["Missing"] = ("Hello {0}", "Bonjour"),
            ["Extra"] = ("Hello", "Bonjour {0}")
        };

        // Act
        var results = PlaceholderValidator.ValidateBatch(translations);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.True(results["Valid"].IsValid);
        Assert.False(results["Missing"].IsValid);
        Assert.False(results["Extra"].IsValid);
        Assert.NotEmpty(results["Missing"].MissingPlaceholders);
        Assert.NotEmpty(results["Extra"].ExtraPlaceholders);
    }

    [Fact]
    public void Validate_ComplexICUPlural_ReturnsValid()
    {
        // Arrange
        var source = "{count, plural, one {# item} other {# items}}";
        var translation = "{count, plural, one {# article} other {# articles}}";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RealWorldExample_ReturnsValid()
    {
        // Arrange
        var source = "User {userName} has {itemCount:N0} items worth {totalValue:C2}";
        var translation = "L'utilisateur {userName} a {itemCount:N0} articles d'une valeur de {totalValue:C2}";

        // Act
        var result = PlaceholderValidator.Validate(source, translation);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
