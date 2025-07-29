using FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests.Documentation;

public class XmlDocumentationFormatterTests
{
    [Fact]
    public void FormatSummary_WithValidText_ReturnsFormattedSummary()
    {
        // Arrange
        var summary = "This is a test summary";

        // Act
        var result = XmlDocumentationFormatter.FormatSummary(summary);

        // Assert
        var expected = $"/// <summary>{Environment.NewLine}/// This is a test summary{Environment.NewLine}/// </summary>{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatSummary_WithEmptyString_ReturnsFormattedWithEmptySummary()
    {
        // Arrange
        var summary = "";

        // Act
        var result = XmlDocumentationFormatter.FormatSummary(summary);

        // Assert
        var expected = $"/// <summary>{Environment.NewLine}/// {Environment.NewLine}/// </summary>{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatParam_WithValidInputs_ReturnsFormattedParam()
    {
        // Arrange
        var paramName = "value";
        var description = "The value to process";

        // Act
        var result = XmlDocumentationFormatter.FormatParam(paramName, description);

        // Assert
        Assert.Equal("/// <param name=\"value\">The value to process</param>", result);
    }

    [Fact]
    public void FormatParam_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var paramName = "test&param";
        var description = "Description with <special> characters";

        // Act
        var result = XmlDocumentationFormatter.FormatParam(paramName, description);

        // Assert
        Assert.Equal("/// <param name=\"test&param\">Description with <special> characters</param>", result);
    }

    [Fact]
    public void FormatReturns_WithValidDescription_ReturnsFormattedReturns()
    {
        // Arrange
        var description = "The processed result";

        // Act
        var result = XmlDocumentationFormatter.FormatReturns(description);

        // Assert
        Assert.Equal("/// <returns>The processed result</returns>", result);
    }

    [Fact]
    public void FormatReturns_WithEmptyDescription_ReturnsFormattedWithEmptyContent()
    {
        // Arrange
        var description = "";

        // Act
        var result = XmlDocumentationFormatter.FormatReturns(description);

        // Assert
        Assert.Equal("/// <returns></returns>", result);
    }

    [Fact]
    public void FormatException_WithValidInputs_ReturnsFormattedException()
    {
        // Arrange
        var exceptionType = "ArgumentNullException";
        var description = "Thrown when value is null";

        // Act
        var result = XmlDocumentationFormatter.FormatException(exceptionType, description);

        // Assert
        Assert.Equal("/// <exception cref=\"ArgumentNullException\">Thrown when value is null</exception>", result);
    }

    [Fact]
    public void FormatException_WithFullyQualifiedType_HandlesCorrectly()
    {
        // Arrange
        var exceptionType = "System.ArgumentNullException";
        var description = "Thrown when value is null";

        // Act
        var result = XmlDocumentationFormatter.FormatException(exceptionType, description);

        // Assert
        Assert.Equal("/// <exception cref=\"System.ArgumentNullException\">Thrown when value is null</exception>", result);
    }

    [Fact]
    public void SplitPascalCase_WithPascalCase_SplitsCorrectly()
    {
        // Arrange
        var text = "PascalCaseString";

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text);

        // Assert
        Assert.Equal("Pascal Case String", result);
    }

    [Fact]
    public void SplitPascalCase_WithCamelCase_SplitsCorrectly()
    {
        // Arrange
        var text = "camelCaseString";

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text);

        // Assert
        Assert.Equal("camel Case String", result);
    }

    [Fact]
    public void SplitPascalCase_WithConsecutiveCapitals_HandlesCorrectly()
    {
        // Arrange
        var text = "XMLHttpRequest";

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text);

        // Assert
        Assert.Equal("X M L Http Request", result);
    }

    [Fact]
    public void SplitPascalCase_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var text = "";

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void SplitPascalCase_WithNull_ReturnsEmptyString()
    {
        // Arrange
        string? text = null;

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text!);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ToLowerCaseFirst_WithUpperCaseFirst_ConvertsCorrectly()
    {
        // Arrange
        var text = "HelloWorld";

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text);

        // Assert
        Assert.Equal("helloWorld", result);
    }

    [Fact]
    public void ToLowerCaseFirst_WithLowerCaseFirst_RemainsUnchanged()
    {
        // Arrange
        var text = "helloWorld";

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text);

        // Assert
        Assert.Equal("helloWorld", result);
    }

    [Fact]
    public void ToLowerCaseFirst_WithSingleCharacter_ConvertsCorrectly()
    {
        // Arrange
        var text = "A";

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text);

        // Assert
        Assert.Equal("a", result);
    }

    [Fact]
    public void ToLowerCaseFirst_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var text = "";

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ToLowerCaseFirst_WithNull_ReturnsEmptyString()
    {
        // Arrange
        string? text = null;

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text!);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void CapitalizeFirst_WithLowerCaseFirst_ConvertsCorrectly()
    {
        // Arrange
        var text = "helloWorld";

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text);

        // Assert
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void CapitalizeFirst_WithUpperCaseFirst_RemainsUnchanged()
    {
        // Arrange
        var text = "HelloWorld";

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text);

        // Assert
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void CapitalizeFirst_WithSingleCharacter_ConvertsCorrectly()
    {
        // Arrange
        var text = "a";

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text);

        // Assert
        Assert.Equal("A", result);
    }

    [Fact]
    public void CapitalizeFirst_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var text = "";

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void CapitalizeFirst_WithNull_ReturnsEmptyString()
    {
        // Arrange
        string? text = null;

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text!);

        // Assert
        Assert.Equal("", result);
    }
}