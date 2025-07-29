using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using System;
using Xunit;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class AttributeBuilderTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesBuilder()
    {
        // Arrange & Act
        var builder = new AttributeBuilder("TestAttribute");
        var code = builder.Build();

        // Assert - using Shouldly
        code.ShouldBe("[TestAttribute]");
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new AttributeBuilder(null!));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new AttributeBuilder(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new AttributeBuilder("   "));
    }

    [Fact]
    public void WithArgument_AddsPositionalArgument()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act
        var result = builder.WithArgument("\"value\"").Build();

        // Assert - using Shouldly
        result.ShouldBe("[Test(\"value\")]");
    }

    [Fact]
    public void WithArgument_MultipleArguments_AddsInOrder()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act
        var result = builder
            .WithArgument("\"first\"")
            .WithArgument("\"second\"")
            .WithArgument("123")
            .Build();

        // Assert - using Shouldly
        result.ShouldBe("[Test(\"first\", \"second\", 123)]");
    }

    [Fact]
    public void WithArgument_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.WithArgument(null!));
    }

    [Fact]
    public void WithNamedArgument_AddsNamedArgument()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act
        var result = builder.WithNamedArgument("Name", "\"value\"").Build();

        // Assert - using Shouldly
        result.ShouldBe("[Test(Name = \"value\")]");
    }

    [Fact]
    public void WithNamedArgument_MultipleNamedArguments()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act
        var result = builder
            .WithNamedArgument("First", "1")
            .WithNamedArgument("Second", "true")
            .WithNamedArgument("Third", "\"text\"")
            .Build();

        // Assert - using Shouldly
        result.ShouldBe("[Test(First = 1, Second = true, Third = \"text\")]");
    }

    [Fact]
    public void WithNamedArgument_NullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamedArgument(null!, "value"));
    }

    [Fact]
    public void WithNamedArgument_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamedArgument("", "value"));
    }

    [Fact]
    public void WithNamedArgument_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.WithNamedArgument("Name", null!));
    }

    [Fact]
    public void MixedArguments_PositionalThenNamed()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act
        var result = builder
            .WithArgument("\"positional1\"")
            .WithArgument("42")
            .WithNamedArgument("Named1", "true")
            .WithNamedArgument("Named2", "\"text\"")
            .Build();

        // Assert - using Shouldly
        result.ShouldBe("[Test(\"positional1\", 42, Named1 = true, Named2 = \"text\")]");
    }

    [Fact]
    public void Constructor_WithAttributeSuffix_KeepsOriginalName()
    {
        // Arrange & Act
        var builder = new AttributeBuilder("TestAttribute");
        var code = builder.Build();

        // Assert - using Shouldly
        code.ShouldBe("[TestAttribute]");
    }
}