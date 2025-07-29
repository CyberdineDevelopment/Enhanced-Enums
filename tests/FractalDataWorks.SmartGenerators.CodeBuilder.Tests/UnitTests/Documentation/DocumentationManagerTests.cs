using FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests.Documentation;

public class DocumentationManagerTests
{
    private class TestDocumentationProvider : IXmlDocumentationProvider
    {
        public string? CustomDocumentation { get; set; }
        public bool HasCustomDocumentation => !string.IsNullOrEmpty(CustomDocumentation);
        public string Name { get; }
        public string? ElementType { get; }

        public string ExpectedDocumentation { get; set; } = "";
        public int GetDocumentationCallCount { get; private set; }

        public TestDocumentationProvider(string name = "TestElement", string? elementType = null)
        {
            Name = name;
            ElementType = elementType;
        }

        public string GetDocumentation()
        {
            GetDocumentationCallCount++;
            return ExpectedDocumentation;
        }
    }

    [Fact]
    public void Constructor_WithValidProvider_CreatesInstance()
    {
        // Arrange
        var provider = new TestDocumentationProvider();

        // Act
        var manager = new DocumentationManager(provider);

        // Assert
        Assert.NotNull(manager);
        Assert.Same(provider, manager.DocumentationProvider);
    }

    [Fact]
    public void Constructor_WithNullProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DocumentationManager(null!));
    }

    [Fact]
    public void SetCustomDocumentation_WithValidSummary_SetsOnProvider()
    {
        // Arrange
        var provider = new TestDocumentationProvider();
        var manager = new DocumentationManager(provider);
        var customSummary = "This is a custom summary";

        // Act
        manager.SetCustomDocumentation(customSummary);

        // Assert
        Assert.Equal(customSummary, provider.CustomDocumentation);
    }

    [Fact]
    public void SetCustomDocumentation_WithNullSummary_ThrowsArgumentException()
    {
        // Arrange
        var provider = new TestDocumentationProvider();
        var manager = new DocumentationManager(provider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => manager.SetCustomDocumentation(null!));
    }

    [Fact]
    public void SetCustomDocumentation_WithEmptySummary_ThrowsArgumentException()
    {
        // Arrange
        var provider = new TestDocumentationProvider();
        var manager = new DocumentationManager(provider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => manager.SetCustomDocumentation(""));
    }

    [Fact]
    public void SetCustomDocumentation_WithWhitespaceSummary_ThrowsArgumentException()
    {
        // Arrange
        var provider = new TestDocumentationProvider();
        var manager = new DocumentationManager(provider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => manager.SetCustomDocumentation("   "));
    }

    [Fact]
    public void GetDocumentation_CallsProviderGetDocumentation()
    {
        // Arrange
        var provider = new TestDocumentationProvider();
        var expectedDoc = "Expected documentation";
        provider.ExpectedDocumentation = expectedDoc;
        var manager = new DocumentationManager(provider);

        // Act
        var result = manager.GetDocumentation();

        // Assert
        Assert.Equal(expectedDoc, result);
        Assert.Equal(1, provider.GetDocumentationCallCount);
    }

    [Fact]
    public void GenerateDocumentation_ReturnsFormattedSummary()
    {
        // Arrange
        var provider = new TestDocumentationProvider();
        var rawDoc = "This is the documentation";
        provider.ExpectedDocumentation = rawDoc;
        var manager = new DocumentationManager(provider);

        // Act
        var result = manager.GenerateDocumentation();

        // Assert
        var expected = $"/// <summary>{Environment.NewLine}/// {rawDoc}{Environment.NewLine}/// </summary>{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateDocumentation_WithEmptyDocumentation_ReturnsFormattedEmptySummary()
    {
        // Arrange
        var provider = new TestDocumentationProvider();
        provider.ExpectedDocumentation = "";
        var manager = new DocumentationManager(provider);

        // Act
        var result = manager.GenerateDocumentation();

        // Assert
        var expected = $"/// <summary>{Environment.NewLine}/// {Environment.NewLine}/// </summary>{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DocumentationProvider_ReturnsProvidedInstance()
    {
        // Arrange
        var provider = new TestDocumentationProvider();
        var manager = new DocumentationManager(provider);

        // Act
        var actualProvider = manager.DocumentationProvider;

        // Assert
        Assert.Same(provider, actualProvider);
    }
}