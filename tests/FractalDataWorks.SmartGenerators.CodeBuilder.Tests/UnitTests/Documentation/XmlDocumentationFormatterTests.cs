using FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests.Documentation;

public class XmlDocumentationFormatterTests
{
    [Fact]
    public void FormatSummaryWithValidTextReturnsFormattedSummary()
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
    public void FormatSummaryWithEmptyStringReturnsFormattedWithEmptySummary()
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
    public void FormatParamWithValidInputsReturnsFormattedParam()
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
    public void FormatParamWithSpecialCharactersHandlesCorrectly()
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
    public void FormatReturnsWithValidDescriptionReturnsFormattedReturns()
    {
        // Arrange
        var description = "The processed result";

        // Act
        var result = XmlDocumentationFormatter.FormatReturns(description);

        // Assert
        Assert.Equal("/// <returns>The processed result</returns>", result);
    }

    [Fact]
    public void FormatReturnsWithEmptyDescriptionReturnsFormattedWithEmptyContent()
    {
        // Arrange
        var description = "";

        // Act
        var result = XmlDocumentationFormatter.FormatReturns(description);

        // Assert
        Assert.Equal("/// <returns></returns>", result);
    }

    [Fact]
    public void FormatExceptionWithValidInputsReturnsFormattedException()
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
    public void FormatExceptionWithFullyQualifiedTypeHandlesCorrectly()
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
    public void SplitPascalCaseWithPascalCaseSplitsCorrectly()
    {
        // Arrange
        var text = "PascalCaseString";

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text);

        // Assert
        Assert.Equal("Pascal Case String", result);
    }

    [Fact]
    public void SplitPascalCaseWithCamelCaseSplitsCorrectly()
    {
        // Arrange
        var text = "camelCaseString";

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text);

        // Assert
        Assert.Equal("camel Case String", result);
    }

    [Fact]
    public void SplitPascalCaseWithConsecutiveCapitalsHandlesCorrectly()
    {
        // Arrange
        var text = "XMLHttpRequest";

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text);

        // Assert
        Assert.Equal("X M L Http Request", result);
    }

    [Fact]
    public void SplitPascalCaseWithEmptyStringReturnsEmptyString()
    {
        // Arrange
        var text = "";

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void SplitPascalCaseWithNullReturnsEmptyString()
    {
        // Arrange
        string? text = null;

        // Act
        var result = XmlDocumentationFormatter.SplitPascalCase(text!);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ToLowerCaseFirstWithUpperCaseFirstConvertsCorrectly()
    {
        // Arrange
        var text = "HelloWorld";

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text);

        // Assert
        Assert.Equal("helloWorld", result);
    }

    [Fact]
    public void ToLowerCaseFirstWithLowerCaseFirstRemainsUnchanged()
    {
        // Arrange
        var text = "helloWorld";

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text);

        // Assert
        Assert.Equal("helloWorld", result);
    }

    [Fact]
    public void ToLowerCaseFirstWithSingleCharacterConvertsCorrectly()
    {
        // Arrange
        var text = "A";

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text);

        // Assert
        Assert.Equal("a", result);
    }

    [Fact]
    public void ToLowerCaseFirstWithEmptyStringReturnsEmptyString()
    {
        // Arrange
        var text = "";

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ToLowerCaseFirstWithNullReturnsEmptyString()
    {
        // Arrange
        string? text = null;

        // Act
        var result = XmlDocumentationFormatter.ToLowerCaseFirst(text!);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void CapitalizeFirstWithLowerCaseFirstConvertsCorrectly()
    {
        // Arrange
        var text = "helloWorld";

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text);

        // Assert
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void CapitalizeFirstWithUpperCaseFirstRemainsUnchanged()
    {
        // Arrange
        var text = "HelloWorld";

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text);

        // Assert
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void CapitalizeFirstWithSingleCharacterConvertsCorrectly()
    {
        // Arrange
        var text = "a";

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text);

        // Assert
        Assert.Equal("A", result);
    }

    [Fact]
    public void CapitalizeFirstWithEmptyStringReturnsEmptyString()
    {
        // Arrange
        var text = "";

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void CapitalizeFirstWithNullReturnsEmptyString()
    {
        // Arrange
        string? text = null;

        // Act
        var result = XmlDocumentationFormatter.CapitalizeFirst(text!);

        // Assert
        Assert.Equal("", result);
    }
}