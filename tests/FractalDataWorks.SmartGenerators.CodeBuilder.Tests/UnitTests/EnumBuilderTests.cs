using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Microsoft.CodeAnalysis.CSharp;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class EnumBuilderTests
{
    [Fact]
    public void DefaultConstructorCreatesEnumWithDefaultName()
    {
        // Arrange & Act
        var builder = new EnumBuilder();
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasNamespace("Test")
            .HasEnum("MyEnum")
            .Assert();
    }

    [Fact]
    public void ConstructorWithValidNameCreatesEnum()
    {
        // Arrange & Act
        var builder = new EnumBuilder("TestEnum");
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasNamespace("Test")
            .HasEnum("TestEnum")
            .Assert();
    }

    [Fact]
    public void ConstructorWithNullNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EnumBuilder(null!));
    }

    [Fact]
    public void ConstructorWithEmptyNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EnumBuilder(""));
    }

    [Fact]
    public void ConstructorWithWhitespaceNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EnumBuilder("   "));
    }

    [Fact]
    public void WithNameSetsEnumName()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act
        var code = builder.WithName("CustomEnum").Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasEnum("CustomEnum")
            .Assert();
    }

    [Fact]
    public void WithNameNullNameThrowsArgumentException()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithBaseTypeSetsEnumBaseType()
    {
        // Arrange
        var builder = new EnumBuilder("Status");

        // Act
        var code = builder.WithBaseType("byte").Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasEnum("Status", e => e.HasBaseType("byte"))
            .Assert();
    }

    [Fact]
    public void WithBaseTypeNullBaseTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithBaseType(null!));
    }

    [Fact]
    public void AddMemberAddsEnumMember()
    {
        // Arrange
        var builder = new EnumBuilder("Color");

        // Act
        var code = builder
            .AddMember("Red")
            .AddMember("Green")
            .AddMember("Blue")
            .Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasEnum("Color", e => e
                .HasValue("Red")
                .HasValue("Green")
                .HasValue("Blue"))
            .Assert();
    }

    [Fact]
    public void AddMemberNullMemberThrowsArgumentException()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddMember(null!));
    }

    [Fact]
    public void AddValueAddsEnumMemberWithValue()
    {
        // Arrange
        var builder = new EnumBuilder("ErrorCode");

        // Act
        var code = builder
            .AddValue("None", 0)
            .AddValue("NotFound", 404)
            .AddValue("ServerError", 500)
            .Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasEnum("ErrorCode", e => e
                .HasValue("None", v => v.HasValue(0))
                .HasValue("NotFound", v => v.HasValue(404))
                .HasValue("ServerError", v => v.HasValue(500)))
            .Assert();
    }

    [Fact]
    public void AddValueNullMemberThrowsArgumentException()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddValue(null!, 0));
    }

    [Fact]
    public void AddAttributeAddsAttributeToEnum()
    {
        // Arrange
        var builder = new EnumBuilder("LogLevel");

        // Act
        var code = builder
            .AddAttribute("Flags")
            .AddValue("None", 0)
            .AddValue("Info", 1)
            .AddValue("Warning", 2)
            .AddValue("Error", 4)
            .Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasEnum("LogLevel", e => e.HasAttribute("Flags"))
            .Assert();
    }

    [Fact]
    public void MakePublicSetsEnumAsPublic()
    {
        // Arrange
        var builder = new EnumBuilder("PublicEnum");

        // Act
        var code = builder.MakePublic().Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasEnum("PublicEnum", e => e.HasModifier(SyntaxKind.PublicKeyword))
            .Assert();
    }
}