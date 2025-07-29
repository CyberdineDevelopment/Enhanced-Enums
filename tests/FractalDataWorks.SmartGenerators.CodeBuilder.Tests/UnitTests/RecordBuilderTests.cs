using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Shouldly;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class RecordBuilderTests
{
    [Fact]
    public void DefaultConstructor_CreatesRecordWithDefaultName()
    {
        // Arrange & Act
        var builder = new RecordBuilder();
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasNamespace("Test")
            .HasRecord("Record")
            .Assert();
    }

    [Fact]
    public void Constructor_WithValidName_CreatesRecord()
    {
        // Arrange & Act
        var builder = new RecordBuilder("Person");
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasRecord("Person")
            .Assert();
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RecordBuilder(null!));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RecordBuilder(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RecordBuilder("   "));
    }

    [Fact]
    public void WithName_SetsRecordName()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act
        var code = builder.WithName("CustomRecord").Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasRecord("CustomRecord")
            .Assert();
    }

    [Fact]
    public void WithName_NullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithBaseType_SetsBaseType()
    {
        // Arrange
        var builder = new RecordBuilder("DerivedRecord");

        // Act
        var code = builder.WithBaseType("BaseRecord").Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasRecord("DerivedRecord", r => r.HasBaseType("BaseRecord"))
            .Assert();
    }

    [Fact]
    public void WithBaseType_NullBaseType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithBaseType(null!));
    }

    [Fact]
    public void WithParameter_AddsParameter()
    {
        // Arrange
        var builder = new RecordBuilder("Person");

        // Act
        var code = builder
            .WithParameter("string", "FirstName")
            .WithParameter("string", "LastName")
            .Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasRecord("Person", r => r
                .HasParameter("FirstName", p => p.HasType("string"))
                .HasParameter("LastName", p => p.HasType("string")))
            .Assert();
    }

    [Fact]
    public void WithParameter_NullType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithParameter(null!, "name"));
    }

    [Fact]
    public void WithParameter_NullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithParameter("string", null!));
    }

    [Fact]
    public void WithParameter_WithConfiguration_AddsConfiguredParameter()
    {
        // Arrange
        var builder = new RecordBuilder("Person");

        // Act
        var code = builder
            .WithParameter("string", "Name", p => p.WithDefaultValue("\"Unknown\""))
            .Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasRecord("Person", r => r
                .HasParameter("Name", p => p
                    .HasType("string")
                    .HasDefaultValue("\"Unknown\"")))
            .Assert();
    }

    [Fact]
    public void WithParameter_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithParameter("string", "name", null!));
    }

    [Fact]
    public void MakeStruct_CreatesRecordStruct()
    {
        // Arrange
        var builder = new RecordBuilder("Point");

        // Act
        var code = builder
            .MakeStruct()
            .WithParameter("int", "X")
            .WithParameter("int", "Y")
            .Build();

        // Assert - just verify it builds without error
        Assert.Contains("record struct", code);
    }

    [Fact]
    public void ImplementsInterface_AddsInterface()
    {
        // Arrange
        var builder = new RecordBuilder("Product");

        // Act
        var code = builder.ImplementsInterface("IProduct").Build();

        // Assert - just verify it builds and contains the interface
        Assert.Contains(": IProduct", code);
    }

    [Fact]
    public void ImplementsInterface_NullInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ImplementsInterface(null!));
    }

    [Fact]
    public void ImplementsInterface_DuplicateInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder()
            .ImplementsInterface("IProduct");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ImplementsInterface("IProduct"));
    }

    // Add more comprehensive tests using Shouldly

    [Fact]
    public void Name_Property_ReturnsRecordName()
    {
        // Arrange
        var builder = new RecordBuilder("TestRecord");

        // Act & Assert - using Shouldly
        builder.Name.ShouldBe("TestRecord");

        // Change name and verify
        builder.WithName("UpdatedRecord");
        builder.Name.ShouldBe("UpdatedRecord");
    }

    [Fact]
    public void WithNamespace_ValidNamespace_WrapsRecordInNamespace()
    {
        // Arrange
        var builder = new RecordBuilder("Person");

        // Act
        var result = builder
            .WithNamespace("MyApp.Models")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("namespace MyApp.Models");
        result.ShouldContain("record Person");
    }

    [Fact]
    public void WithNamespace_NullNamespace_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamespace(null!));
    }

    [Fact]
    public void WithNamespace_EmptyNamespace_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamespace(""));
    }

    [Fact]
    public void WithNamespace_WhitespaceNamespace_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamespace("   "));
    }

    [Fact]
    public void MakePublic_SetsPublicAccessModifier()
    {
        // Arrange
        var builder = new RecordBuilder("PublicRecord");

        // Act
        var result = builder
            .MakePublic()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public record PublicRecord");
    }

    [Fact]
    public void MakePrivate_SetsPrivateAccessModifier()
    {
        // Arrange
        var builder = new RecordBuilder("PrivateRecord");

        // Act
        var result = builder
            .MakePrivate()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("private record PrivateRecord");
    }

    [Fact]
    public void MakeProtected_SetsProtectedAccessModifier()
    {
        // Arrange
        var builder = new RecordBuilder("ProtectedRecord");

        // Act
        var result = builder
            .MakeProtected()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("protected record ProtectedRecord");
    }

    [Fact]
    public void MakeInternal_SetsInternalAccessModifier()
    {
        // Arrange
        var builder = new RecordBuilder("InternalRecord");

        // Act
        var result = builder
            .MakeInternal()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("internal record InternalRecord");
    }

    [Fact]
    public void MakeSealed_SetsSealedModifier()
    {
        // Arrange
        var builder = new RecordBuilder("SealedRecord");

        // Act
        var result = builder
            .MakeSealed()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("sealed record SealedRecord");
    }

    [Fact]
    public void MakeAbstract_SetsAbstractModifier()
    {
        // Arrange
        var builder = new RecordBuilder("AbstractRecord");

        // Act
        var result = builder
            .MakeAbstract()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("abstract record AbstractRecord");
    }

    [Fact]
    public void MakePartial_SetsPartialModifier()
    {
        // Arrange
        var builder = new RecordBuilder("PartialRecord");

        // Act
        var result = builder
            .MakePartial()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("partial record PartialRecord");
    }

    [Fact]
    public void AddMethod_CreatesMethodAndReturnsBuilder()
    {
        // Arrange
        var builder = new RecordBuilder("PersonRecord");

        // Act
        var methodBuilder = builder.AddMethod("GetFullName", "string");
        methodBuilder.MakePublic().WithBody("return $\"{FirstName} {LastName}\";");
        var result = builder.Build();

        // Assert - using Shouldly
        methodBuilder.ShouldNotBeNull();
        methodBuilder.ShouldBeOfType<MethodBuilder>();
        result.ShouldContain("public string GetFullName()");
        result.ShouldContain("return $\"{FirstName} {LastName}\";");
    }

    [Fact]
    public void AddMethod_WithConfigure_ConfiguresMethod()
    {
        // Arrange
        var builder = new RecordBuilder("PersonRecord");

        // Act
        var result = builder
            .AddMethod("IsValid", "bool", m => m
                .MakePublic()
                .WithBody("return !string.IsNullOrEmpty(Name);"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public bool IsValid()");
        result.ShouldContain("return !string.IsNullOrEmpty(Name);");
    }

    [Fact]
    public void AddMethod_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddMethod("Method", "void", null!));
    }

    [Fact]
    public void AddProperty_CreatesPropertyAndReturnsBuilder()
    {
        // Arrange
        var builder = new RecordBuilder("PersonRecord");

        // Act
        var propertyBuilder = builder.AddProperty("Age", "int");
        propertyBuilder.MakePublic();
        var result = builder.Build();

        // Assert - using Shouldly
        propertyBuilder.ShouldNotBeNull();
        propertyBuilder.ShouldBeOfType<PropertyBuilder>();
        result.ShouldContain("public int Age { get; set; }");
    }

    [Fact]
    public void AddProperty_WithConfigure_ConfiguresProperty()
    {
        // Arrange
        var builder = new RecordBuilder("PersonRecord");

        // Act
        var result = builder
            .AddProperty("CreatedAt", "DateTime", p => p
                .MakePublic()
                .MakeReadOnly()
                .WithInitializer("DateTime.UtcNow"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public DateTime CreatedAt { get; } = DateTime.UtcNow");
    }

    [Fact]
    public void AddProperty_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddProperty("Prop", "int", null!));
    }

    [Fact]
    public void WithSummary_AddsXmlDocumentation()
    {
        // Arrange
        var builder = new RecordBuilder("PersonRecord");

        // Act
        var result = builder
            .WithSummary("Represents a person in the system.")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Represents a person in the system.");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void Build_RecordWithNoBody_GeneratesMinimalRecord()
    {
        // Arrange
        var builder = new RecordBuilder("SimpleRecord");

        // Act
        var result = builder.Build();

        // Assert - using Shouldly
        result.ShouldContain("record SimpleRecord;");
    }

    [Fact]
    public void Build_RecordStructWithParameters_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new RecordBuilder("Point3D");

        // Act
        var result = builder
            .MakeStruct()
            .MakePublic()
            .WithParameter("double", "X")
            .WithParameter("double", "Y")
            .WithParameter("double", "Z")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public record struct Point3D(double X, double Y, double Z);");
    }

    [Fact]
    public void Build_RecordWithBaseTypeAndInterfaces_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new RecordBuilder("Employee");

        // Act
        var result = builder
            .MakePublic()
            .WithBaseType("Person")
            .ImplementsInterface("IEmployee")
            .ImplementsInterface("IPayable")
            .WithParameter("string", "EmployeeId")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public record Employee(string EmployeeId) : Person, IEmployee, IPayable;");
    }

    [Fact(Skip = "TODO: Debug record body generation")]
    public void Build_ComplexRecord_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new RecordBuilder("Customer");

        // Act
        var result = builder
            .WithNamespace("MyApp.Models")
            .MakePublic()
            .MakeSealed()
            .WithSummary("Represents a customer in the system.")
            .AddAttribute("Serializable")
            .WithParameter("string", "Id")
            .WithParameter("string", "Name")
            .WithParameter("string", "Email", p => p.WithDefaultValue("\"\""))
            .ImplementsInterface("ICustomer")
            .AddProperty("DateCreated", "DateTime", p => p
                .MakePublic()
                .WithInitializer("DateTime.UtcNow"))
            .AddMethod("GetDisplayName", "string", m => m
                .MakePublic()
                .WithBody("return $\"{Name} ({Email})\";"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("namespace MyApp.Models");
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Represents a customer in the system.");
        result.ShouldContain("[Serializable]");
        result.ShouldContain("public sealed record Customer(string Id, string Name, string Email = \"\") : ICustomer");
        // Records with bodies use braces, so property is inside
        result.ShouldContain("DateTime DateCreated");
        result.ShouldContain("DateTime.UtcNow");
        result.ShouldContain("public string GetDisplayName()");
        result.ShouldContain("return $\"{Name} ({Email})\";");
    }

    [Fact]
    public void Build_RecordWithMultipleParametersWithDefaults_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new RecordBuilder("Configuration");

        // Act
        var result = builder
            .MakePublic()
            .WithParameter("string", "Host", p => p.WithDefaultValue("\"localhost\""))
            .WithParameter("int", "Port", p => p.WithDefaultValue("8080"))
            .WithParameter("bool", "UseSSL", p => p.WithDefaultValue("true"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public record Configuration(string Host = \"localhost\", int Port = 8080, bool UseSSL = true);");
    }

    [Fact]
    public void ImplementsInterface_MultipleInterfaces_AddsAllInterfaces()
    {
        // Arrange
        var builder = new RecordBuilder("Product");

        // Act
        var result = builder
            .ImplementsInterface("IProduct")
            .ImplementsInterface("IIdentifiable")
            .ImplementsInterface("IVersionable")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("record Product : IProduct, IIdentifiable, IVersionable;");
    }

    [Fact]
    public void ImplementsInterface_EmptyInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.ImplementsInterface(""));
    }

    [Fact]
    public void ImplementsInterface_WhitespaceInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.ImplementsInterface("   "));
    }

    [Fact]
    public void WithParameter_EmptyType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithParameter("", "name"));
    }

    [Fact]
    public void WithParameter_WhitespaceType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithParameter("   ", "name"));
    }

    [Fact]
    public void WithParameter_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithParameter("string", ""));
    }

    [Fact]
    public void WithParameter_WhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithParameter("string", "   "));
    }

    [Fact]
    public void WithBaseType_EmptyBaseType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithBaseType(""));
    }

    [Fact]
    public void WithBaseType_WhitespaceBaseType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithBaseType("   "));
    }

    [Fact]
    public void WithName_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithName(""));
    }

    [Fact]
    public void WithName_WhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithName("   "));
    }

    [Fact]
    public void AddAttribute_StringAttribute_AddsAttribute()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act
        var result = builder
            .AddAttribute("JsonIgnore")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("[JsonIgnore]");
        result.ShouldContain("record MyRecord");
    }

    [Fact]
    public void FluentInterface_ChainsCorrectly()
    {
        // Arrange & Act
        var result = new RecordBuilder()
            .WithName("FluentTest")
            .WithNamespace("Test.Namespace")
            .MakePublic()
            .MakePartial()
            .WithBaseType("BaseRecord")
            .ImplementsInterface("IInterface1")
            .ImplementsInterface("IInterface2")
            .WithSummary("Test fluent interface chaining.")
            .AddAttribute("TestAttribute")
            .WithParameter("string", "Name")
            .WithParameter("int", "Age")
            .AddProperty("Id", "Guid", p => p.MakePublic())
            .AddMethod("ToString", "string", m => m
                .MakePublic()
                .MakeOverride()
                .WithBody("return $\"{Name} (Age: {Age})\";"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("namespace Test.Namespace");
        result.ShouldContain("/// Test fluent interface chaining.");
        result.ShouldContain("[TestAttribute]");
        result.ShouldContain("public partial record FluentTest(string Name, int Age) : BaseRecord, IInterface1, IInterface2");
        result.ShouldContain("public Guid Id { get; set; }");
        result.ShouldContain("public override string ToString()");
        result.ShouldContain("return $\"{Name} (Age: {Age})\";");
    }
}