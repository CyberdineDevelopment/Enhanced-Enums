using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Shouldly;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class RecordBuilderTests
{
    [Fact]
    public void DefaultConstructorCreatesRecordWithDefaultName()
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
    public void ConstructorWithValidNameCreatesRecord()
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
    public void ConstructorWithNullNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RecordBuilder(null!));
    }

    [Fact]
    public void ConstructorWithEmptyNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RecordBuilder(""));
    }

    [Fact]
    public void ConstructorWithWhitespaceNameThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RecordBuilder("   "));
    }

    [Fact]
    public void WithNameSetsRecordName()
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
    public void WithNameNullNameThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithBaseTypeSetsBaseType()
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
    public void WithBaseTypeNullBaseTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithBaseType(null!));
    }

    [Fact]
    public void WithParameterAddsParameter()
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
    public void WithParameterNullTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithParameter(null!, "name"));
    }

    [Fact]
    public void WithParameterNullNameThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithParameter("string", null!));
    }

    [Fact]
    public void WithParameterWithConfigurationAddsConfiguredParameter()
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
    public void WithParameterNullConfigurationThrowsArgumentNullException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithParameter("string", "name", null!));
    }

    [Fact]
    public void MakeStructCreatesRecordStruct()
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
    public void ImplementsInterfaceAddsInterface()
    {
        // Arrange
        var builder = new RecordBuilder("Product");

        // Act
        var code = builder.ImplementsInterface("IProduct").Build();

        // Assert - just verify it builds and contains the interface
        Assert.Contains(": IProduct", code);
    }

    [Fact]
    public void ImplementsInterfaceNullInterfaceThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ImplementsInterface(null!));
    }

    [Fact]
    public void ImplementsInterfaceDuplicateInterfaceThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder()
            .ImplementsInterface("IProduct");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ImplementsInterface("IProduct"));
    }

    // Add more comprehensive tests using Shouldly

    [Fact]
    public void NamePropertyReturnsRecordName()
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
    public void WithNamespaceValidNamespaceUsesFileScopedNamespace()
    {
        // Arrange
        var builder = new RecordBuilder("Person");

        // Act
        var result = builder
            .WithNamespace("MyApp.Models")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("namespace MyApp.Models;");
        result.ShouldNotContain("namespace MyApp.Models{");
        result.ShouldNotContain("}"); // File-scoped namespaces don't have closing braces
        result.ShouldContain("record Person");
    }

    [Fact]
    public void WithNamespaceNullNamespaceThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamespace(null!));
    }

    [Fact]
    public void WithNamespaceEmptyNamespaceThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamespace(""));
    }

    [Fact]
    public void WithNamespaceWhitespaceNamespaceThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamespace("   "));
    }

    [Fact]
    public void MakePublicSetsPublicAccessModifier()
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
    public void MakePrivateSetsPrivateAccessModifier()
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
    public void MakeProtectedSetsProtectedAccessModifier()
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
    public void MakeInternalSetsInternalAccessModifier()
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
    public void MakeSealedSetsSealedModifier()
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
    public void MakeAbstractSetsAbstractModifier()
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
    public void MakePartialSetsPartialModifier()
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
    public void AddMethodCreatesMethodAndReturnsBuilder()
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
    public void AddMethodWithConfigureConfiguresMethod()
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
    public void AddMethodWithNullConfigureThrowsArgumentNullException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddMethod("Method", "void", null!));
    }

    [Fact]
    public void AddPropertyCreatesPropertyAndReturnsBuilder()
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
    public void AddPropertyWithConfigureConfiguresProperty()
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
    public void AddPropertyWithNullConfigureThrowsArgumentNullException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddProperty("Prop", "int", null!));
    }

    [Fact]
    public void WithXmlDocSummaryAddsXmlDocumentation()
    {
        // Arrange
        var builder = new RecordBuilder("PersonRecord");

        // Act
        var result = builder
            .WithXmlDocSummary("Represents a person in the system.")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Represents a person in the system.");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void BuildRecordWithNoBodyGeneratesMinimalRecord()
    {
        // Arrange
        var builder = new RecordBuilder("SimpleRecord");

        // Act
        var result = builder.Build();

        // Assert - using Shouldly
        result.ShouldContain("record SimpleRecord;");
    }

    [Fact]
    public void BuildRecordStructWithParametersGeneratesCorrectCode()
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
    public void BuildRecordWithBaseTypeAndInterfacesGeneratesCorrectCode()
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

    [Fact]
    public void BuildComplexRecordGeneratesCorrectCode()
    {
        // Arrange
        var builder = new RecordBuilder("Customer");

        // Act
        var result = builder
            .WithNamespace("MyApp.Models")
            .MakePublic()
            .MakeSealed()
            .WithXmlDocSummary("Represents a customer in the system.")
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
    public void BuildRecordWithMultipleParametersWithDefaultsGeneratesCorrectCode()
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
    public void ImplementsInterfaceMultipleInterfacesAddsAllInterfaces()
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
    public void ImplementsInterfaceEmptyInterfaceThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.ImplementsInterface(""));
    }

    [Fact]
    public void ImplementsInterfaceWhitespaceInterfaceThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.ImplementsInterface("   "));
    }

    [Fact]
    public void WithParameterEmptyTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithParameter("", "name"));
    }

    [Fact]
    public void WithParameterWhitespaceTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithParameter("   ", "name"));
    }

    [Fact]
    public void WithParameterEmptyNameThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithParameter("string", ""));
    }

    [Fact]
    public void WithParameterWhitespaceNameThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithParameter("string", "   "));
    }

    [Fact]
    public void WithBaseTypeEmptyBaseTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithBaseType(""));
    }

    [Fact]
    public void WithBaseTypeWhitespaceBaseTypeThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder("MyRecord");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithBaseType("   "));
    }

    [Fact]
    public void WithNameEmptyNameThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithName(""));
    }

    [Fact]
    public void WithNameWhitespaceNameThrowsArgumentException()
    {
        // Arrange
        var builder = new RecordBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithName("   "));
    }

    [Fact]
    public void AddAttributeStringAttributeAddsAttribute()
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
    public void FluentInterfaceChainsCorrectly()
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
            .WithXmlDocSummary("Test fluent interface chaining.")
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