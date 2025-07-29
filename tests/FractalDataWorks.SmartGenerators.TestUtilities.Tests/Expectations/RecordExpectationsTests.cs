using FractalDataWorks.SmartGenerators.CodeBuilders;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.Expectations;

/// <summary>
/// Tests for the RecordExpectations class and HasRecord methods in SyntaxTreeExpectations.
/// </summary>
public class RecordExpectationsTests
{
    [Fact]
    public void SyntaxTreeExpectations_HasRecord_FindsSimpleRecord()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("Person")
                .MakePublic()
                .WithParameter("string", "Name")
                .WithParameter("int", "Age"))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasNamespace("Test")
            .HasRecord("Person")
            .Assert();
    }

    [Fact]
    public void SyntaxTreeExpectations_HasRecord_WithExpectations_ValidatesRecordStructure()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("Person")
                .MakePublic()
                .WithParameter("string", "Name")
                .WithParameter("int", "Age")
                .AddMethod("GetDisplayName", "string", m => m
                    .WithBody("return Name.ToUpper();")))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasNamespace("Test")
            .HasRecord("Person", record => record
                .IsPublic()
                .HasParameter("Name", "string")
                .HasParameter("Age", "int")
                .HasParameterCount(2)
                .HasMethod("GetDisplayName"))
            .Assert();
    }

    [Fact]
    public void RecordExpectations_IsPublic_ValidatesPublicModifier()
    {
        // Arrange
        var namespaceBuilder = new NamespaceBuilder("Test");

        // Add public record
        namespaceBuilder.AddRecord(r => r
            .WithName("PublicRecord")
            .MakePublic()
            .WithParameter("string", "Value"));

        // Add internal record
        namespaceBuilder.AddRecord(r => r
            .WithName("InternalRecord")
            .MakeInternal()
            .WithParameter("string", "Value"));

        var code = namespaceBuilder.Build();
        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert - Public record should pass
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("PublicRecord", record => record.IsPublic())
            .Assert();

        // Act & Assert - Internal record should fail
        Should.Throw<ShouldAssertException>(() =>
            ExpectationsFactory.Expect(syntaxTree)
                .HasRecord("InternalRecord", record => record.IsPublic())
                .Assert());
    }

    [Fact]
    public void RecordExpectations_IsPartial_ValidatesPartialModifier()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("PartialRecord")
                .MakePublic()
                .MakePartial()
                .WithParameter("string", "Value"))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("PartialRecord", record => record
                .IsPublic()
                .IsPartial())
            .Assert();
    }

    [Fact]
    public void RecordExpectations_IsSealed_ValidatesSealedModifier()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("SealedRecord")
                .MakePublic()
                .MakeSealed()
                .WithParameter("string", "Value"))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("SealedRecord", record => record
                .IsPublic()
                .IsSealed())
            .Assert();
    }

    [Fact]
    public void RecordExpectations_IsAbstract_ValidatesAbstractModifier()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("AbstractRecord")
                .MakePublic()
                .MakeAbstract()
                .WithParameter("string", "Value"))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("AbstractRecord", record => record
                .IsPublic()
                .IsAbstract())
            .Assert();
    }

    [Fact]
    public void RecordExpectations_HasParameter_ValidatesParameters()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("Customer")
                .MakePublic()
                .WithParameter("string", "FirstName")
                .WithParameter("string", "LastName")
                .WithParameter("int", "Age", p => p.WithDefaultValue("18")))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("Customer", record => record
                .HasParameter("FirstName")
                .HasParameter("LastName", "string")
                .HasParameter("Age", "int")
                .HasParameterCount(3))
            .Assert();
    }

    [Fact]
    public void RecordExpectations_HasParameter_WithExpectations_ValidatesParameterDetails()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("Options")
                .MakePublic()
                .WithParameter("string", "Name")
                .WithParameter("int", "Count", p => p.WithDefaultValue("10"))
                .WithParameter("params string[]", "Tags"))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("Options", record => record
                .HasParameter("Name", param => param
                    .HasType("string")
                    .HasNoDefaultValue())
                .HasParameter("Count", param => param
                    .HasType("int")
                    .HasDefaultValue("10"))
                .HasParameter("Tags", param => param
                    .HasType("string[]")
                    .IsParams()))
            .Assert();
    }

    [Fact]
    public void RecordExpectations_IsRecordClass_ValidatesRecordType()
    {
        // Arrange
        var namespaceBuilder = new NamespaceBuilder("Test");

        // Add implicit record class
        namespaceBuilder.AddRecord(r => r
            .WithName("Customer")
            .MakePublic()
            .WithParameter("string", "Name"));

        // Add explicit record class
        namespaceBuilder.AddRecord(r => r
            .WithName("ExplicitClassRecord")
            .MakePublic()
            .WithParameter("string", "Value"));

        var code = namespaceBuilder.Build();
        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("Customer", record => record.IsRecordClass())
            .Assert();

        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("ExplicitClassRecord", record => record.IsRecordClass())
            .Assert();
    }

    [Fact]
    public void RecordExpectations_IsRecordStruct_ValidatesRecordStructType()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("Point")
                .MakePublic()
                .MakeStruct()
                .WithParameter("int", "X")
                .WithParameter("int", "Y"))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("Point", record => record
                .IsPublic()
                .IsRecordStruct()
                .HasParameter("X", "int")
                .HasParameter("Y", "int"))
            .Assert();
    }

    [Fact]
    public void RecordExpectations_HasMethod_ValidatesRecordMethods()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("Product")
                .MakePublic()
                .WithParameter("string", "Name")
                .WithParameter("decimal", "Price")
                .AddMethod("GetDiscountedPrice", "decimal", m => m
                    .AddParameter("decimal", "discount")
                    .WithBody("return Price * (1 - discount);"))
                .AddMethod("IsExpensive", "bool", m => m
                    .WithBody("return Price > 100;")))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("Product", record => record
                .HasMethod("GetDiscountedPrice")
                .HasMethod("IsExpensive", method => method
                    .IsPublic()
                    .HasReturnType("bool")
                    .HasNoParameters()))
            .Assert();
    }

    [Fact]
    public void RecordExpectations_HasProperty_ValidatesAdditionalProperties()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("Employee")
                .MakePublic()
                .WithParameter("string", "Name")
                .WithParameter("int", "Id")
                .AddProperty("Department", "string", p => p
                    .WithInitializer("\"Unassigned\""))  // Use WithInitializer instead of WithDefaultValue
                .AddProperty("IsManager", "bool", p => p
                    .WithInitSetter()))  // Remove WithGetter() call, it's automatic
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("Employee", record => record
                .HasParameter("Name", "string")
                .HasParameter("Id", "int")
                .HasProperty("Department", prop => prop
                    .IsPublic()
                    .HasType("string")
                    .HasGetter()
                    .HasSetter())
                .HasProperty("IsManager", prop => prop
                    .IsPublic()
                    .HasType("bool")
                    .HasGetter()
                    .HasInitSetter()))
            .Assert();
    }

    [Fact]
    public void RecordExpectations_HasBaseType_ValidatesInheritance()
    {
        // Arrange
        var namespaceBuilder = new NamespaceBuilder("Test");

        // Add base record
        namespaceBuilder.AddRecord(r => r
            .WithName("Animal")
            .MakePublic()
            .MakeAbstract()
            .WithParameter("string", "Name"));

        // Add derived record with base type call
        namespaceBuilder.AddRecord(r => r
            .WithName("Dog")
            .MakePublic()
            .WithParameter("string", "Name")
            .WithParameter("string", "Breed")
            .WithBaseType("Animal(Name)"));

        var code = namespaceBuilder.Build();
        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("Dog", record => record
                .IsPublic()
                .HasBaseType("Animal")
                .HasParameter("Name", "string")
                .HasParameter("Breed", "string"))
            .Assert();
    }

    [Fact]
    public void RecordExpectations_ImplementsInterface_ValidatesInterfaces()
    {
        // Arrange
        var namespaceBuilder = new NamespaceBuilder("Test");

        // Add interface
        namespaceBuilder.AddInterface(i => i
            .WithName("IIdentifiable")
            .MakePublic()
            .AddProperty("Id", "int"));  // Remove WithGetter() call

        // Add record implementing interface
        namespaceBuilder.AddRecord(r => r
            .WithName("User")
            .MakePublic()
            .WithParameter("string", "Name")
            .WithParameter("int", "Id")
            .ImplementsInterface("IIdentifiable"));

        var code = namespaceBuilder.Build();
        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasInterface("IIdentifiable")
            .HasRecord("User", record => record
                .IsPublic()
                .ImplementsInterface("IIdentifiable")
                .HasParameter("Name", "string")
                .HasParameter("Id", "int"))
            .Assert();
    }

    [Fact]
    public void RecordExpectations_HasName_ValidatesRecordName()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddRecord(r => r
                .WithName("CustomerRecord")
                .MakePublic()
                .WithParameter("string", "Name"))
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasRecord("CustomerRecord", record => record
                .HasName("CustomerRecord"))
            .Assert();

        // Should fail with wrong name
        Should.Throw<ShouldAssertException>(() =>
            ExpectationsFactory.Expect(syntaxTree)
                .HasRecord("CustomerRecord", record => record
                    .HasName("WrongName"))
                .Assert());
    }

    [Fact]
    public void SyntaxTreeExpectations_HasRecord_ThrowsWhenRecordNotFound()
    {
        // Arrange
        var code = new NamespaceBuilder("Test")
            .AddClass(c => c
                .WithName("NotARecord")
                .MakePublic())
            .Build();

        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        Should.Throw<ShouldAssertException>(() =>
            ExpectationsFactory.Expect(syntaxTree)
                .HasRecord("NotARecord")
                .Assert());
    }

    [Fact]
    public void RecordExpectations_ComplexRecord_ValidatesAllAspects()
    {
        // Arrange
        var namespaceBuilder = new NamespaceBuilder("Test");

        // Add interface
        namespaceBuilder.AddInterface(i => i
            .WithName("IEntity")
            .MakePublic()
            .AddProperty("Id", "Guid"));  // Remove WithGetter() call

        // Add base record
        namespaceBuilder.AddRecord(r => r
            .WithName("BaseEntity")
            .MakePublic()
            .MakeAbstract()
            .WithParameter("Guid", "Id")
            .ImplementsInterface("IEntity"));

        // Add complex derived record
        namespaceBuilder.AddRecord(r => r
            .WithName("Customer")
            .MakePublic()
            .MakeSealed()
            .MakePartial()
            .WithParameter("Guid", "Id")
            .WithParameter("string", "FirstName")
            .WithParameter("string", "LastName")
            .WithParameter("DateTime", "DateOfBirth")
            .WithParameter("string?", "Email", p => p.WithDefaultValue("null"))
            .WithBaseType("BaseEntity(Id)")
            .AddProperty("FullName", "string", p => p
                .WithExpressionBody("$\"{FirstName} {LastName}\""))
            .AddProperty("Age", "int", p => p
                .WithExpressionBody("(DateTime.Now - DateOfBirth).Days / 365"))
            .AddMethod("IsAdult", "bool", m => m
                .WithBody("return Age >= 18;"))
            .AddMethod("UpdateEmail", "void", m => m
                .AddParameter("string", "newEmail")
                .WithBody("// Implementation details")));

        var code = namespaceBuilder.Build();
        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        ExpectationsFactory.Expect(syntaxTree)
            .HasInterface("IEntity")
            .HasRecord("BaseEntity", record => record
                .IsPublic()
                .IsAbstract()
                .ImplementsInterface("IEntity")
                .HasParameter("Id", "Guid"))
            .HasRecord("Customer", record => record
                .IsPublic()
                .IsSealed()
                .IsPartial()
                .HasBaseType("BaseEntity")
                .HasParameterCount(5)
                .HasParameter("Id", "Guid")
                .HasParameter("FirstName", "string")
                .HasParameter("LastName", "string")
                .HasParameter("DateOfBirth", "DateTime")
                .HasParameter("Email", param => param
                    .HasType("string?")
                    .HasDefaultValue("null"))
                .HasProperty("FullName", prop => prop
                    .IsPublic()
                    .HasType("string")
                    .HasGetter())
                .HasProperty("Age", prop => prop
                    .IsPublic()
                    .HasType("int")
                    .HasGetter())
                .HasMethod("IsAdult", method => method
                    .IsPublic()
                    .HasReturnType("bool")
                    .HasNoParameters())
                .HasMethod("UpdateEmail", method => method
                    .IsPublic()
                    .HasReturnType("void")
                    .HasParameter("newEmail", param => param.HasType("string"))))
            .Assert();
    }
}
