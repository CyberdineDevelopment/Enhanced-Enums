using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class PropertyBuilderTests
{
    [Fact]
    public void GeneratesAutoPropertyWithGetterAndSetter()
    {
        var builder = new PropertyBuilder("MyProp").WithType("int");
        var complete = $"namespace Test {{ {builder.Build()} }}";

        ExpectationsFactory.ExpectCode(complete)
            .HasProperty(p => p
                .HasName("MyProp")
                .HasType("int")
                .HasGetter("get;")
                .HasSetter("set;"))
            .Assert();
    }

    [Fact]
    public void GeneratesExpressionBodiedProperty()
    {
        var builder = new PropertyBuilder("Val").WithType("string").WithExpressionBody("""Hello""");
        var complete = $"namespace Test {{ {builder.Build()} }}";

        ExpectationsFactory.ExpectCode(complete)
            .HasProperty(p => p
                .HasName("Val")
                .HasExpressionBody("Hello"))  // Fixed: expect just the literal value
            .Assert();
    }

    // Constructor tests
    [Fact]
    public void ConstructorWithNameAndTypeCreatesProperty()
    {
        // Arrange & Act
        var builder = new PropertyBuilder("Count", "int");
        var result = builder.Build();

        // Assert
        Assert.Contains("int Count { get; set; }", result);
    }

    [Fact]
    public void ConstructorWithNameOnlyCreatesPropertyWithEmptyType()
    {
        // Arrange & Act
        var builder = new PropertyBuilder("MyProperty");
        var result = builder.Build();

        // Assert
        Assert.Contains(" MyProperty { get; set; }", result);
    }

    [Fact]
    public void ConstructorWithNullTypeNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PropertyBuilder("MyProperty", null!));
    }

    [Fact]
    public void ConstructorWithNullNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PropertyBuilder(null!));
    }

    [Fact]
    public void ConstructorWithEmptyNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PropertyBuilder(""));
    }

    // WithType tests
    [Fact]
    public void WithTypeSetsPropertyType()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty");

        // Act
        var result = builder
            .WithType("string")
            .Build();

        // Assert
        Assert.Contains("string MyProperty { get; set; }", result);
    }

    [Fact]
    public void WithTypeNullTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithType(null!));
    }

    [Fact]
    public void WithTypeEmptyTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithType(""));
    }

    [Fact]
    public void WithTypeWhitespaceTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithType("   "));
    }

    // Access modifier tests
    [Fact]
    public void MakePublicSetsPublicAccessModifier()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakePublic()
            .Build();

        // Assert
        Assert.Contains("public int MyProperty { get; set; }", result);
    }

    [Fact]
    public void MakePrivateSetsPrivateAccessModifier()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakePrivate()
            .Build();

        // Assert
        Assert.Contains("private int MyProperty { get; set; }", result);
    }

    [Fact]
    public void MakeProtectedSetsProtectedAccessModifier()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakeProtected()
            .Build();

        // Assert
        Assert.Contains("protected int MyProperty { get; set; }", result);
    }

    [Fact]
    public void MakeInternalSetsInternalAccessModifier()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakeInternal()
            .Build();

        // Assert
        Assert.Contains("internal int MyProperty { get; set; }", result);
    }

    // Modifier tests
    [Fact]
    public void MakeStaticSetsStaticModifier()
    {
        // Arrange
        var builder = new PropertyBuilder("Instance", "string");

        // Act
        var result = builder
            .MakeStatic()
            .Build();

        // Assert
        Assert.Contains("static string Instance { get; set; }", result);
    }

    [Fact]
    public void MakeVirtualSetsVirtualModifier()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "string");

        // Act
        var result = builder
            .MakeVirtual()
            .Build();

        // Assert
        Assert.Contains("virtual string MyProperty { get; set; }", result);
    }

    [Fact]
    public void MakeOverrideSetsOverrideModifier()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "string");

        // Act
        var result = builder
            .MakeOverride()
            .Build();

        // Assert
        Assert.Contains("override string MyProperty { get; set; }", result);
    }

    [Fact]
    public void MakeAbstractSetsAbstractModifier()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "string");

        // Act
        var result = builder
            .MakeAbstract()
            .Build();

        // Assert
        Assert.Contains("abstract string MyProperty { get; set; }", result);
    }

    // Read-only property tests
    [Fact]
    public void MakeReadOnlyRemovesSetter()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakeReadOnly()
            .Build();

        // Assert
        Assert.Contains("int MyProperty { get; }", result);
        Assert.DoesNotContain("set;", result);
    }

    // Init setter tests
    [Fact]
    public void WithInitSetterCreatesInitOnlyProperty()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "string");

        // Act
        var result = builder
            .WithInitSetter()
            .Build();

        // Assert
        Assert.Contains("string MyProperty { get; init; }", result);
    }

    // Setter access modifier tests
    [Fact]
    public void MakeSetterPrivateSetsPrivateSetter()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakePublic()
            .MakeSetterPrivate()
            .Build();

        // Assert
        Assert.Contains("public int MyProperty { get; private set; }", result);
    }

    [Fact]
    public void MakeSetterProtectedSetsProtectedSetter()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakePublic()
            .MakeSetterProtected()
            .Build();

        // Assert
        Assert.Contains("public int MyProperty { get; protected set; }", result);
    }

    [Fact]
    public void MakeSetterInternalSetsInternalSetter()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakePublic()
            .MakeSetterInternal()
            .Build();

        // Assert
        Assert.Contains("public int MyProperty { get; internal set; }", result);
    }

    [Fact]
    public void MakeSetterProtectedInternalSetsProtectedInternalSetter()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act
        var result = builder
            .MakePublic()
            .MakeSetterProtectedInternal()
            .Build();

        // Assert
        Assert.Contains("public int MyProperty { get; protected internal set; }", result);
    }

    // Initializer tests
    [Fact]
    public void WithInitializerAddsPropertyInitializer()
    {
        // Arrange
        var builder = new PropertyBuilder("Items", "List<string>");

        // Act
        var result = builder
            .WithInitializer("new List<string>()")
            .Build();

        // Assert
        Assert.Contains("List<string> Items { get; set; } = new List<string>()", result);
    }

    [Fact]
    public void WithInitializerWithReadOnlyPropertyAddsInitializer()
    {
        // Arrange
        var builder = new PropertyBuilder("Items", "List<string>");

        // Act
        var result = builder
            .MakeReadOnly()
            .WithInitializer("new List<string>()")
            .Build();

        // Assert
        Assert.Contains("List<string> Items { get; } = new List<string>()", result);
    }

    // Expression body tests
    [Fact]
    public void WithExpressionBodyCreatesExpressionBodiedProperty()
    {
        // Arrange
        var builder = new PropertyBuilder("FullName", "string");

        // Act
        var result = builder
            .WithExpressionBody("$\"{FirstName} {LastName}\"")
            .Build();

        // Assert
        Assert.Contains("string FullName => $\"{FirstName} {LastName}\";", result);
    }

    [Fact]
    public void WithExpressionBodyNullExpressionThrowsArgumentNullException()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithExpressionBody(null!));
    }

    [Fact]
    public void WithExpressionBodyWithModifiersAppliesModifiers()
    {
        // Arrange
        var builder = new PropertyBuilder("Pi", "double");

        // Act
        var result = builder
            .MakePublic()
            .MakeStatic()
            .WithExpressionBody("3.14159")
            .Build();

        // Assert
        Assert.Contains("public static double Pi => 3.14159;", result);
    }

    // Custom getter/setter tests

    [Fact]
    public void WithGetterNullExpressionThrowsArgumentNullException()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithGetter(null!));
    }


    [Fact]
    public void WithSetterNullExpressionThrowsArgumentNullException()
    {
        // Arrange
        var builder = new PropertyBuilder("MyProperty", "int");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithSetter(null!));
    }


    // Attribute tests
    [Fact]
    public void AddAttributeAddsAttributeToProperty()
    {
        // Arrange
        var builder = new PropertyBuilder("Id", "int");

        // Act
        var result = builder
            .AddAttribute("Required")
            .Build();

        // Assert
        Assert.Contains("[Required]", result);
        Assert.Contains("int Id { get; set; }", result);
    }

    [Fact]
    public void AddAttributeMultipleAttributesAddsAllAttributes()
    {
        // Arrange
        var builder = new PropertyBuilder("Email", "string");

        // Act
        var result = builder
            .AddAttribute("Required")
            .AddAttribute("EmailAddress")
            .Build();

        // Assert
        Assert.Contains("[Required]", result);
        Assert.Contains("[EmailAddress]", result);
    }

    // XML documentation tests
    [Fact]
    public void WithXmlDocSummaryAddsDocumentation()
    {
        // Arrange
        var builder = new PropertyBuilder("Name", "string");

        // Act
        var result = builder
            .WithXmlDocSummary("Gets or sets the name.")
            .Build();

        // Assert
        Assert.Contains("/// <summary>", result);
        Assert.Contains("/// Gets or sets the name.", result);
        Assert.Contains("/// </summary>", result);
    }

    // Complex scenario tests
    [Fact]
    public void BuildComplexPropertyGeneratesCorrectCode()
    {
        // Arrange
        var builder = new PropertyBuilder("Items", "IReadOnlyList<string>");

        // Act
        var result = builder
            .MakePublic()
            .MakeVirtual()
            .WithXmlDocSummary("Gets the collection of items.")
            .AddAttribute("JsonProperty(\"items\")")
            .MakeReadOnly()
            .WithInitializer("new List<string>()")
            .Build();

        // Assert
        Assert.Contains("/// <summary>", result);
        Assert.Contains("/// Gets the collection of items.", result);
        Assert.Contains("[JsonProperty(\"items\")]", result);
        Assert.Contains("public virtual IReadOnlyList<string> Items { get; } = new List<string>()", result);
    }


    [Fact]
    public void BuildAbstractPropertyGeneratesCorrectCode()
    {
        // Arrange
        var builder = new PropertyBuilder("Id", "Guid");

        // Act
        var result = builder
            .MakePublic()
            .MakeAbstract()
            .Build();

        // Assert
        Assert.Contains("public abstract Guid Id { get; set; }", result);
    }

    [Fact]
    public void BuildPropertyWithPrivateSetAndInitializerGeneratesCorrectCode()
    {
        // Arrange
        var builder = new PropertyBuilder("CreatedAt", "DateTime");

        // Act
        var result = builder
            .MakePublic()
            .MakeSetterPrivate()
            .WithInitializer("DateTime.UtcNow")
            .Build();

        // Assert
        Assert.Contains("public DateTime CreatedAt { get; private set; } = DateTime.UtcNow", result);
    }

    [Fact]
    public void FluentInterfaceChainsCorrectly()
    {
        // Arrange & Act
        var result = new PropertyBuilder("IsEnabled")
            .WithType("bool")
            .MakePublic()
            .MakeVirtual()
            .WithXmlDocSummary("Gets or sets whether the feature is enabled.")
            .WithInitializer("true")
            .Build();

        // Assert
        Assert.Contains("/// Gets or sets whether the feature is enabled.", result);
        Assert.Contains("public virtual bool IsEnabled { get; set; } = true", result);
    }
}
