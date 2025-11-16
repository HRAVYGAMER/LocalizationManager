// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using LocalizationManager.Core.Validation;
using Xunit;

namespace LocalizationManager.Tests.UnitTests.Validation;

public class PlaceholderDetectorTests
{
    [Fact]
    public void DetectPlaceholders_EmptyString_ReturnsEmptyList()
    {
        // Arrange
        var text = "";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DetectPlaceholders_NullString_ReturnsEmptyList()
    {
        // Arrange
        string? text = null;

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DetectPlaceholders_NoPlaceholders_ReturnsEmptyList()
    {
        // Arrange
        var text = "This is a plain text with no placeholders.";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Empty(result);
    }

    #region .NET Format String Tests

    [Fact]
    public void DetectPlaceholders_DotNetIndexedPlaceholder_DetectsCorrectly()
    {
        // Arrange
        var text = "Hello {0}!";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        Assert.Equal(PlaceholderType.DotNetFormat, result[0].Type);
        Assert.Equal("{0}", result[0].Original);
        Assert.Equal("0", result[0].Index);
        Assert.Null(result[0].Name);
        Assert.Null(result[0].Format);
    }

    [Fact]
    public void DetectPlaceholders_DotNetIndexedWithFormat_DetectsCorrectly()
    {
        // Arrange
        var text = "Price: {0:C2}";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        Assert.Equal(PlaceholderType.DotNetFormat, result[0].Type);
        Assert.Equal("{0:C2}", result[0].Original);
        Assert.Equal("0", result[0].Index);
        Assert.Equal("C2", result[0].Format);
    }

    [Fact]
    public void DetectPlaceholders_DotNetNamedPlaceholder_DetectsCorrectly()
    {
        // Arrange
        var text = "Hello {name}!";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        Assert.Equal(PlaceholderType.DotNetFormat, result[0].Type);
        Assert.Equal("{name}", result[0].Original);
        Assert.Equal("name", result[0].Name);
        Assert.Null(result[0].Index);
        Assert.Null(result[0].Format);
    }

    [Fact]
    public void DetectPlaceholders_DotNetNamedWithFormat_DetectsCorrectly()
    {
        // Arrange
        var text = "Date: {date:yyyy-MM-dd}";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        Assert.Equal(PlaceholderType.DotNetFormat, result[0].Type);
        Assert.Equal("{date:yyyy-MM-dd}", result[0].Original);
        Assert.Equal("date", result[0].Name);
        Assert.Equal("yyyy-MM-dd", result[0].Format);
    }

    [Fact]
    public void DetectPlaceholders_MultipleDotNetPlaceholders_DetectsAll()
    {
        // Arrange
        var text = "Hello {0}, your balance is {1:C2}!";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("{0}", result[0].Original);
        Assert.Equal("{1:C2}", result[1].Original);
    }

    #endregion

    #region Printf-Style Tests

    [Fact]
    public void DetectPlaceholders_PrintfString_DetectsCorrectly()
    {
        // Arrange
        var text = "Hello %s!";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        Assert.Equal(PlaceholderType.PrintfStyle, result[0].Type);
        Assert.Equal("%s", result[0].Original);
        Assert.Equal("s", result[0].Format);
        Assert.Null(result[0].Index);
    }

    [Fact]
    public void DetectPlaceholders_PrintfDecimal_DetectsCorrectly()
    {
        // Arrange
        var text = "Count: %d";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        Assert.Equal(PlaceholderType.PrintfStyle, result[0].Type);
        Assert.Equal("%d", result[0].Original);
        Assert.Equal("d", result[0].Format);
    }

    [Fact]
    public void DetectPlaceholders_PrintfPositional_DetectsCorrectly()
    {
        // Arrange
        var text = "Hello %1$s, you have %2$d items";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("%1$s", result[0].Original);
        Assert.Equal("1", result[0].Index);
        Assert.Equal("s", result[0].Format);
        Assert.Equal("%2$d", result[1].Original);
        Assert.Equal("2", result[1].Index);
        Assert.Equal("d", result[1].Format);
    }

    [Fact]
    public void DetectPlaceholders_PrintfWithWidth_DetectsCorrectly()
    {
        // Arrange
        var text = "Value: %10.2f";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        Assert.Equal(PlaceholderType.PrintfStyle, result[0].Type);
        Assert.Equal("%10.2f", result[0].Original);
        Assert.Equal("f", result[0].Format);
    }

    [Fact]
    public void DetectPlaceholders_EscapedPrintf_IgnoresEscaped()
    {
        // Arrange
        var text = "Use %% to escape";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region ICU MessageFormat Tests

    [Fact]
    public void DetectPlaceholders_IcuSimple_DetectedAsDotNet()
    {
        // Arrange
        var text = "Hello {user}!";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        // Simple {user} is detected as .NET format, not ICU (ICU requires comma syntax)
        Assert.Equal(PlaceholderType.DotNetFormat, result[0].Type);
        Assert.Equal("user", result[0].Name);
    }

    [Fact]
    public void DetectPlaceholders_IcuPlural_DetectsCorrectly()
    {
        // Arrange
        var text = "{count, plural, one {# item} other {# items}}";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Contains(result, p =>
            p.Type == PlaceholderType.IcuMessageFormat &&
            p.Name == "count" &&
            p.Format == "plural");
    }

    [Fact]
    public void DetectPlaceholders_IcuSelect_DetectsCorrectly()
    {
        // Arrange
        var text = "{gender, select, male {He} female {She} other {They}}";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Contains(result, p =>
            p.Type == PlaceholderType.IcuMessageFormat &&
            p.Name == "gender" &&
            p.Format == "select");
    }

    #endregion

    #region Template Literal Tests

    [Fact]
    public void DetectPlaceholders_TemplateLiteral_DetectsCorrectly()
    {
        // Arrange
        var text = "Hello ${name}!";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Single(result);
        Assert.Equal(PlaceholderType.TemplateLiteral, result[0].Type);
        Assert.Equal("${name}", result[0].Original);
        Assert.Equal("name", result[0].Name);
    }

    [Fact]
    public void DetectPlaceholders_TemplateLiteralWithDots_DetectsCorrectly()
    {
        // Arrange
        var text = "User: ${user.firstName} ${user.lastName}";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("${user.firstName}", result[0].Original);
        Assert.Equal("user.firstName", result[0].Name);
        Assert.Equal("${user.lastName}", result[1].Original);
        Assert.Equal("user.lastName", result[1].Name);
    }

    #endregion

    #region Mixed Placeholder Tests

    [Fact]
    public void DetectPlaceholders_MixedTypes_DetectsAll()
    {
        // Arrange
        var text = "Hello {0}, you have %d items in ${cart.name}";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.True(result.Count >= 3);
        Assert.Contains(result, p => p.Type == PlaceholderType.DotNetFormat && p.Original == "{0}");
        Assert.Contains(result, p => p.Type == PlaceholderType.PrintfStyle && p.Original == "%d");
        Assert.Contains(result, p => p.Type == PlaceholderType.TemplateLiteral && p.Original == "${cart.name}");
    }

    #endregion

    #region GetNormalizedIdentifier Tests

    [Fact]
    public void GetNormalizedIdentifier_DotNetIndexed_ReturnsIndex()
    {
        // Arrange
        var placeholder = new Placeholder
        {
            Type = PlaceholderType.DotNetFormat,
            Original = "{0}",
            Index = "0"
        };

        // Act
        var result = PlaceholderDetector.GetNormalizedIdentifier(placeholder);

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void GetNormalizedIdentifier_DotNetNamed_ReturnsName()
    {
        // Arrange
        var placeholder = new Placeholder
        {
            Type = PlaceholderType.DotNetFormat,
            Original = "{name}",
            Name = "name"
        };

        // Act
        var result = PlaceholderDetector.GetNormalizedIdentifier(placeholder);

        // Assert
        Assert.Equal("name", result);
    }

    [Fact]
    public void GetNormalizedIdentifier_Printf_ReturnsIndexOrFormat()
    {
        // Arrange
        var placeholder1 = new Placeholder
        {
            Type = PlaceholderType.PrintfStyle,
            Original = "%1$s",
            Index = "1",
            Format = "s"
        };

        var placeholder2 = new Placeholder
        {
            Type = PlaceholderType.PrintfStyle,
            Original = "%s",
            Format = "s"
        };

        // Act
        var result1 = PlaceholderDetector.GetNormalizedIdentifier(placeholder1);
        var result2 = PlaceholderDetector.GetNormalizedIdentifier(placeholder2);

        // Assert
        Assert.Equal("1", result1); // Uses Index when available
        Assert.Equal("s", result2); // Falls back to Format
    }

    [Fact]
    public void GetNormalizedIdentifier_TemplateLiteral_ReturnsName()
    {
        // Arrange
        var placeholder = new Placeholder
        {
            Type = PlaceholderType.TemplateLiteral,
            Original = "${user.name}",
            Name = "user.name"
        };

        // Act
        var result = PlaceholderDetector.GetNormalizedIdentifier(placeholder);

        // Assert
        Assert.Equal("user.name", result);
    }

    #endregion

    #region Position Tests

    [Fact]
    public void DetectPlaceholders_OrdersByPosition_ReturnsOrderedList()
    {
        // Arrange
        var text = "End {2}, middle {1}, start {0}";

        // Act
        var result = PlaceholderDetector.DetectPlaceholders(text);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result[0].Position < result[1].Position);
        Assert.True(result[1].Position < result[2].Position);
        Assert.Equal("{2}", result[0].Original);
        Assert.Equal("{1}", result[1].Original);
        Assert.Equal("{0}", result[2].Original);
    }

    #endregion
}
