using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using System;
using Xunit;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class InterfaceBuilderTests
{
    [Fact]
    public void DefaultConstructor_CreatesInterfaceWithDefaultName()
    {
        // Arrange & Act
        var builder = new InterfaceBuilder();
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasNamespace("Test")
            .HasInterface("IInterface")
            .Assert();
    }

    [Fact]
    public void Constructor_WithValidName_CreatesInterface()
    {
        // Arrange & Act
        var builder = new InterfaceBuilder("ITestInterface");
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasInterface("ITestInterface")
            .Assert();
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new InterfaceBuilder(null!));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new InterfaceBuilder(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new InterfaceBuilder("   "));
    }

    [Fact]
    public void WithName_SetsInterfaceName()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act
        var code = builder.WithName("ICustomInterface").Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasInterface("ICustomInterface")
            .Assert();
    }

    [Fact]
    public void WithName_NullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithTypeParameter_AddsGenericParameter()
    {
        // Arrange
        var builder = new InterfaceBuilder("IRepository");

        // Act
        var code = builder.WithTypeParameter("T").Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasInterface("IRepository", i => i.HasTypeParameter("T"))
            .Assert();
    }

    [Fact]
    public void WithTypeParameter_MultipleParameters()
    {
        // Arrange
        var builder = new InterfaceBuilder("IConverter");

        // Act
        var code = builder
            .WithTypeParameter("TInput")
            .WithTypeParameter("TOutput")
            .Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasInterface("IConverter", i => i
                .HasTypeParameter("TInput")
                .HasTypeParameter("TOutput"))
            .Assert();
    }

    [Fact]
    public void WithTypeParameter_NullParameter_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithTypeParameter(null!));
    }

    [Fact]
    public void WithBaseInterface_AddsBaseInterface()
    {
        // Arrange
        var builder = new InterfaceBuilder("IChildInterface");

        // Act
        var code = builder.WithBaseInterface("IParentInterface").Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasInterface("IChildInterface", i => i.HasBaseInterface("IParentInterface"))
            .Assert();
    }

    [Fact]
    public void WithBaseInterface_NullInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithBaseInterface(null!));
    }

    [Fact]
    public void WithBaseInterface_DuplicateInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder()
            .WithBaseInterface("IBase");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithBaseInterface("IBase"));
    }

    [Fact]
    public void AddMethod_AddsMethodToInterface()
    {
        // Arrange
        var builder = new InterfaceBuilder("IService");

        // Act
        var code = builder
            .AddMethod("GetData", "string", m => m.AddParameter("int", "id"))
            .Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasInterface("IService", i => i
                .HasMethod("GetData", m => m
                    .HasReturnType("string")
                    .HasParameter("id", p => p.HasType("int"))))
            .Assert();
    }

    // Additional comprehensive tests using Shouldly

    [Fact]
    public void WithName_UpdatesInterfaceName()
    {
        // Arrange
        var builder = new InterfaceBuilder("ITestInterface");

        // Act
        var result = builder
            .WithName("IUpdatedInterface")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("interface IUpdatedInterface");
    }

    [Fact]
    public void WithName_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithName(""));
    }

    [Fact]
    public void WithName_WhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithName("   "));
    }

    [Fact]
    public void WithBaseInterface_EmptyInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithBaseInterface(""));
    }

    [Fact]
    public void WithBaseInterface_WhitespaceInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithBaseInterface("   "));
    }

    [Fact]
    public void WithBaseInterface_MultipleInterfaces_AddsAllInterfaces()
    {
        // Arrange
        var builder = new InterfaceBuilder("IService");

        // Act
        var result = builder
            .WithBaseInterface("IDisposable")
            .WithBaseInterface("ICloneable")
            .WithBaseInterface("ISerializable")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("interface IService : IDisposable, ICloneable, ISerializable");
    }

    [Fact]
    public void WithTypeParameter_EmptyParameter_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithTypeParameter(""));
    }

    [Fact]
    public void WithTypeParameter_WhitespaceParameter_ThrowsArgumentException()
    {
        // Arrange
        var builder = new InterfaceBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithTypeParameter("   "));
    }

    [Fact]
    public void WithTypeParameter_AddsParameterWithoutConstraint()
    {
        // Arrange
        var builder = new InterfaceBuilder("IRepository");

        // Act
        var result = builder
            .WithTypeParameter("T")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("interface IRepository<T>");
    }

    [Fact]
    public void WithTypeParameter_MultipleParameters_AddsAllParameters()
    {
        // Arrange
        var builder = new InterfaceBuilder("IMapper");

        // Act
        var result = builder
            .WithTypeParameter("TSource")
            .WithTypeParameter("TDestination")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("interface IMapper<TSource, TDestination>");
    }

    [Fact]
    public void AddMethod_CreatesMethodAndReturnsBuilder()
    {
        // Arrange
        var builder = new InterfaceBuilder("ICalculator");

        // Act
        var methodBuilder = builder.AddMethod("Add", "int");
        methodBuilder.AddParameter("int", "a").AddParameter("int", "b").NoImplementation(builder);
        var result = builder.Build();

        // Assert - using Shouldly
        methodBuilder.ShouldNotBeNull();
        methodBuilder.ShouldBeOfType<MethodBuilder>();
        result.ShouldContain("int Add(int a, int b);");
    }

    [Fact]
    public void AddMethod_WithNullConfigure_ThrowsNullReferenceException()
    {
        // Arrange
        var builder = new InterfaceBuilder("IService");

        // Act & Assert - using Shouldly
        Should.Throw<NullReferenceException>(() => builder.AddMethod("Method", "void", null!));
    }

    [Fact]
    public void AddProperty_CreatesPropertyAndReturnsBuilder()
    {
        // Arrange
        var builder = new InterfaceBuilder("IEntity");

        // Act
        var propertyBuilder = builder.AddProperty("Id", "int");
        var result = builder.Build();

        // Assert - using Shouldly
        propertyBuilder.ShouldNotBeNull();
        propertyBuilder.ShouldBeOfType<PropertyBuilder>();
        result.ShouldContain("int Id { get; set; }");
    }

    [Fact]
    public void AddProperty_WithConfigure_ConfiguresProperty()
    {
        // Arrange
        var builder = new InterfaceBuilder("IReadOnlyEntity");

        // Act
        var result = builder
            .AddProperty("Name", "string", p => p.MakeReadOnly())
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("string Name { get; }");
    }

    [Fact]
    public void AddProperty_WithNullConfigure_ThrowsNullReferenceException()
    {
        // Arrange
        var builder = new InterfaceBuilder("IEntity");

        // Act & Assert - using Shouldly
        Should.Throw<NullReferenceException>(() => builder.AddProperty("Prop", "int", null!));
    }

    [Fact]
    public void MakePublic_SetsPublicAccessModifier()
    {
        // Arrange
        var builder = new InterfaceBuilder("IPublicInterface");

        // Act
        var result = builder
            .MakePublic()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public interface IPublicInterface");
    }

    [Fact]
    public void MakeInternal_SetsInternalAccessModifier()
    {
        // Arrange
        var builder = new InterfaceBuilder("IInternalInterface");

        // Act
        var result = builder
            .MakeInternal()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("internal interface IInternalInterface");
    }

    [Fact]
    public void MakePrivate_SetsPrivateAccessModifier()
    {
        // Arrange
        var builder = new InterfaceBuilder("IPrivateInterface");

        // Act
        var result = builder
            .MakePrivate()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("private interface IPrivateInterface");
    }

    // WithNamespace tests removed - not supported by InterfaceBuilder

    [Fact]
    public void WithSummary_AddsXmlDocumentation()
    {
        // Arrange
        var builder = new InterfaceBuilder("IService");

        // Act
        var result = builder
            .WithXmlDocSummary("Defines the contract for a service.")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Defines the contract for a service.");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void WithXmlDocSummary_AddsXmlDocumentation()
    {
        // Arrange
        var builder = new InterfaceBuilder("IRepository");

        // Act
        var result = builder
            .WithXmlDocSummary("Repository pattern interface.")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Repository pattern interface.");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void AddAttribute_StringAttribute_AddsAttribute()
    {
        // Arrange
        var builder = new InterfaceBuilder("IService");

        // Act
        var result = builder
            .AddAttribute("ServiceContract")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("[ServiceContract]");
        result.ShouldContain("interface IService");
    }

    // AddAttribute(AttributeBuilder) tests removed - not supported by InterfaceBuilder

    [Fact]
    public void AddAttribute_MultipleAttributes_AddsAllAttributes()
    {
        // Arrange
        var builder = new InterfaceBuilder("IService");

        // Act
        var result = builder
            .AddAttribute("ServiceContract")
            .AddAttribute("CLSCompliant")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("[ServiceContract]");
        result.ShouldContain("[CLSCompliant]");
    }

    [Fact]
    public void Build_EmptyInterface_GeneratesMinimalInterface()
    {
        // Arrange
        var builder = new InterfaceBuilder("IEmpty");

        // Act
        var result = builder.Build();

        // Assert - using Shouldly
        result.ShouldContain("interface IEmpty");
        result.ShouldContain("{");
        result.ShouldContain("}");
    }

    [Fact]
    public void Build_ComplexInterface_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new InterfaceBuilder("IRepository");

        // Act
        builder
            .MakePublic()
            .WithTypeParameter("T")
            .WithBaseInterface("IDisposable")
            .WithXmlDocSummary("Generic repository interface.")
            .AddAttribute("ServiceContract");

        // Add methods
        builder.AddMethod("GetById", "T", m => m
            .AddParameter("int", "id")
            .NoImplementation(builder));
        builder.AddMethod("GetAll", "IEnumerable<T>")
            .NoImplementation(builder);
        builder.AddMethod("Add", "void", m => m
            .AddParameter("T", "entity")
            .NoImplementation(builder));
        builder.AddMethod("Update", "void", m => m
            .AddParameter("T", "entity")
            .NoImplementation(builder));
        builder.AddMethod("Delete", "void", m => m
            .AddParameter("int", "id")
            .NoImplementation(builder));
        builder.AddProperty("Count", "int", p => p.MakeReadOnly());

        var result = builder.Build();

        // Assert - using Shouldly
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Generic repository interface.");
        result.ShouldContain("[ServiceContract]");
        result.ShouldContain("public interface IRepository<T> : IDisposable");
        result.ShouldContain("T GetById(int id);");
        result.ShouldContain("IEnumerable<T> GetAll();");
        result.ShouldContain("void Add(T entity);");
        result.ShouldContain("void Update(T entity);");
        result.ShouldContain("void Delete(int id);");
        result.ShouldContain("int Count { get; }");
    }

    [Fact]
    public void Build_InterfaceWithMultipleGenericTypes_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new InterfaceBuilder("IConverter");

        // Act
        builder
            .MakePublic()
            .WithTypeParameter("TInput")
            .WithTypeParameter("TOutput");

        builder.AddMethod("Convert", "TOutput", m => m
            .AddParameter("TInput", "input")
            .NoImplementation(builder));
        builder.AddMethod("ConvertBack", "TInput", m => m
            .AddParameter("TOutput", "output")
            .NoImplementation(builder));

        var result = builder.Build();

        // Assert - using Shouldly
        result.ShouldContain("public interface IConverter<TInput, TOutput>");
        result.ShouldContain("TOutput Convert(TInput input);");
        result.ShouldContain("TInput ConvertBack(TOutput output);");
    }

    [Fact]
    public void FluentInterface_ChainsCorrectly()
    {
        // Arrange & Act
        var builder = new InterfaceBuilder()
            .WithName("IFluentTest")
            .MakePublic()
            .WithTypeParameter("T")
            .WithBaseInterface("IBase")
            .WithBaseInterface("ICloneable")
            .WithXmlDocSummary("Test fluent interface chaining.")
            .AddAttribute("TestAttribute");

        builder.AddProperty("Name", "string");
        builder.AddMethod("Process", "void", m => m.AddParameter("T", "item").NoImplementation(builder));

        var result = builder.Build();

        // Assert - using Shouldly
        result.ShouldContain("/// Test fluent interface chaining.");
        result.ShouldContain("[TestAttribute]");
        result.ShouldContain("public interface IFluentTest<T> : IBase, ICloneable");
        result.ShouldContain("string Name { get; set; }");
        result.ShouldContain("void Process(T item);");
    }

    // AddCodeBlock tests removed - not supported by InterfaceBuilder
}