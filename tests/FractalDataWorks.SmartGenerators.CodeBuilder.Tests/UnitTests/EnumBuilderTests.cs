using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class EnumBuilderTests
{
    [Fact]
    public void DefaultConstructor_CreatesEnumWithDefaultName()
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
    public void Constructor_WithValidName_CreatesEnum()
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
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EnumBuilder(null!));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EnumBuilder(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EnumBuilder("   "));
    }

    [Fact]
    public void WithName_SetsEnumName()
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
    public void WithName_NullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithBaseType_SetsEnumBaseType()
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
    public void WithBaseType_NullBaseType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithBaseType(null!));
    }

    [Fact]
    public void AddMember_AddsEnumMember()
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
    public void AddMember_NullMember_ThrowsArgumentException()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddMember(null!));
    }

    [Fact]
    public void AddValue_AddsEnumMemberWithValue()
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
    public void AddValue_NullMember_ThrowsArgumentException()
    {
        // Arrange
        var builder = new EnumBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddValue(null!, 0));
    }

    [Fact]
    public void WithAttribute_AddsAttributeToEnum()
    {
        // Arrange
        var builder = new EnumBuilder("LogLevel");

        // Act
        var code = builder
            .WithAttribute("Flags")
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

    [Fact(Skip = "TODO: EnumExpectations doesn't have HasModifier method")]
    public void MakePublic_SetsEnumAsPublic()
    {
        // Arrange
        var builder = new EnumBuilder("PublicEnum");

        // Act
        var code = builder.MakePublic().Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        // TODO: Fix when EnumExpectations supports HasModifier
        Assert.NotNull(code);
    }
}