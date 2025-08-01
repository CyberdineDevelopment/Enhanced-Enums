using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using System;
using Xunit;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class AttributeBuilderTests
{
    [Fact]
    public void ConstructorWithValidNameCreatesBuilder()
    {
        // Arrange & Act
        var builder = new AttributeBuilder("TestAttribute");
        var code = builder.Build();

        // Assert - using Shouldly
        code.ShouldBe("[TestAttribute]");
    }

    [Fact]
    public void ConstructorWithNullNameThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new AttributeBuilder(null!));
    }

    [Fact]
    public void ConstructorWithEmptyNameThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new AttributeBuilder(""));
    }

    [Fact]
    public void ConstructorWithWhitespaceNameThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new AttributeBuilder("   "));
    }

    [Fact]
    public void WithArgumentAddsPositionalArgument()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act
        var result = builder.WithArgument("\"value\"").Build();

        // Assert - using Shouldly
        result.ShouldBe("[Test(\"value\")]");
    }

    [Fact]
    public void WithArgumentMultipleArgumentsAddsInOrder()
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
    public void WithArgumentNullValueThrowsArgumentNullException()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.WithArgument(null!));
    }

    [Fact]
    public void WithNamedArgumentAddsNamedArgument()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act
        var result = builder.WithNamedArgument("Name", "\"value\"").Build();

        // Assert - using Shouldly
        result.ShouldBe("[Test(Name = \"value\")]");
    }

    [Fact]
    public void WithNamedArgumentMultipleNamedArguments()
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
    public void WithNamedArgumentNullNameThrowsArgumentException()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamedArgument(null!, "value"));
    }

    [Fact]
    public void WithNamedArgumentEmptyNameThrowsArgumentException()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamedArgument("", "value"));
    }

    [Fact]
    public void WithNamedArgumentNullValueThrowsArgumentNullException()
    {
        // Arrange
        var builder = new AttributeBuilder("Test");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.WithNamedArgument("Name", null!));
    }

    [Fact]
    public void MixedArgumentsPositionalThenNamed()
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
    public void ConstructorWithAttributeSuffixKeepsOriginalName()
    {
        // Arrange & Act
        var builder = new AttributeBuilder("TestAttribute");
        var code = builder.Build();

        // Assert - using Shouldly
        code.ShouldBe("[TestAttribute]");
    }
}