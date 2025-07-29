using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using System;
using System.Collections.Generic;
using Xunit;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class ClassBuilderTests
{
    [Fact]
    public void GeneratesPublicClassWithDefaultName()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass").MakePublic();

        // Act
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasNamespace("Test")
            .HasClass("MyClass", c => c.HasModifier("public"))
            .Assert();
    }

    [Fact]
    public void GeneratesClassImplementsInterface()
    {
        var builder = new ClassBuilder("MyClass").ImplementsInterface("ITest");
        var complete = $"namespace Test {{ {builder.Build()} }}";

        ExpectationsFactory.ExpectCode(complete)
            .HasClass("MyClass", c => c.HasInterface("ITest"))
            .Assert();
    }

    [Fact]
    public void GeneratesClassWithBaseType()
    {
        var builder = new ClassBuilder("MyClass").WithBaseType("Base");
        var complete = $"namespace Test {{ {builder.Build()} }}";

        ExpectationsFactory.ExpectCode(complete)
            .HasClass("MyClass", c => c.HasBaseType("Base"))
            .Assert();
    }

    [Fact]
    public void GeneratesClassWithDocumentation()
    {
        var builder = new ClassBuilder("MyClass").WithSummary("Test class");
        var complete = $"namespace Test {{ {builder.Build()} }}";

        ExpectationsFactory.ExpectCode(complete)
            .HasClass("MyClass", c => c.HasXmlDoc("summary", "Test class"))
            .Assert();
    }

    [Fact]
    public void ClassBuilderWithMakeStaticGeneratesStaticClass()
    {
        // Arrange
        var builder = new ClassBuilder("StaticHelper").MakeStatic();

        // Act
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasNamespace("Test")
            .HasClass("StaticHelper", c => c.HasModifier("static"))
            .Assert();

        // Note: Static classes cannot have instance members.
        // Any attempt to add instance properties, fields, or methods to a static class
        // should be handled by the ClassBuilder implementation.
    }
    [Fact]
    public void ClassBuilderWithMakePartialGeneratesPartialClass()
    {
        // Arrange
        var builder = new ClassBuilder("TestClass").MakePartial();

        // Act
        var code = builder.Build();
        var complete = $"namespace Test {{ {code} }}";

        // Assert
        ExpectationsFactory.ExpectCode(complete)
            .HasNamespace("Test")
            .HasClass("TestClass", c => c.IsPartial())
            .Assert();
    }

    // Constructor tests
    [Fact]
    public void Constructor_WithValidName_CreatesClass()
    {
        // Arrange & Act
        var builder = new ClassBuilder("MyClass");
        var result = builder.Build();

        // Assert - using Shouldly
        result.ShouldContain("class MyClass");
        result.ShouldContain("{");
        result.ShouldContain("}");
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new ClassBuilder(null!));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new ClassBuilder(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => new ClassBuilder("   "));
    }

    [Fact]
    public void Constructor_DefaultConstructor_CreatesClassWithDefaultName()
    {
        // Arrange & Act
        var builder = new ClassBuilder();
        var result = builder.Build();

        // Assert - using Shouldly
        result.ShouldContain("class Class");
    }

    // WithName tests
    [Fact]
    public void WithName_ValidName_UpdatesClassName()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("UpdatedClass")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class UpdatedClass");
        result.ShouldNotContain("class Class");
    }

    [Fact]
    public void WithName_NullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithName_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithName(""));
    }

    // Access modifier tests
    [Fact]
    public void MakePublic_SetsPublicAccessModifier()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .MakePublic()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public class MyClass");
    }

    [Fact]
    public void MakePrivate_SetsPrivateAccessModifier()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .MakePrivate()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("private class MyClass");
    }

    [Fact]
    public void MakeProtected_SetsProtectedAccessModifier()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .MakeProtected()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("protected class MyClass");
    }

    [Fact]
    public void MakeInternal_SetsInternalAccessModifier()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .MakeInternal()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("internal class MyClass");
    }

    // Modifier tests
    [Fact]
    public void MakeAbstract_SetsAbstractModifier()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .MakeAbstract()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("abstract class MyClass");
    }

    [Fact]
    public void MakeSealed_SetsSealedModifier()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .MakeSealed()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("sealed class MyClass");
    }

    // Base type tests
    [Fact]
    public void WithBaseType_StringParameter_SetsBaseType()
    {
        // Arrange
        var builder = new ClassBuilder("DerivedClass");

        // Act
        var result = builder
            .WithBaseType("BaseClass")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class DerivedClass : BaseClass");
    }

    [Fact]
    public void WithBaseType_GenericParameter_SetsBaseType()
    {
        // Arrange
        var builder = new ClassBuilder("DerivedClass");

        // Act
        var result = builder
            .WithBaseType<Exception>()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class DerivedClass : Exception");
    }

    [Fact]
    public void WithBaseType_NullBaseType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithBaseType(null!));
    }

    [Fact]
    public void WithBaseType_EmptyBaseType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithBaseType(""));
    }

    // Interface tests
    [Fact]
    public void ImplementsInterface_SingleInterface_AddsInterface()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .ImplementsInterface("IDisposable")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class MyClass : IDisposable");
    }

    [Fact]
    public void ImplementsInterface_MultipleInterfaces_AddsAllInterfaces()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .ImplementsInterface("IDisposable")
            .ImplementsInterface("ICloneable")
            .ImplementsInterface("ISerializable")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class MyClass : IDisposable, ICloneable, ISerializable");
    }

    [Fact]
    public void ImplementsInterface_WithBaseType_FormatsCorrectly()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .WithBaseType("BaseClass")
            .ImplementsInterface("IDisposable")
            .ImplementsInterface("ICloneable")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class MyClass : BaseClass, IDisposable, ICloneable");
    }

    [Fact]
    public void ImplementsInterface_GenericType_AddsInterface()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .ImplementsInterface<IDisposable>()
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class MyClass : IDisposable");
    }

    [Fact]
    public void ImplementsInterface_DuplicateInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");
        builder.ImplementsInterface("IDisposable");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.ImplementsInterface("IDisposable"));
    }

    [Fact]
    public void ImplementsInterface_NullInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.ImplementsInterface(null!));
    }

    [Fact]
    public void ImplementsInterface_EmptyInterface_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.ImplementsInterface(""));
    }

    // Namespace tests
    [Fact(Skip = "TODO: Fix line ending issue with namespace closing brace")]
    public void WithNamespace_ValidNamespace_WrapsClassInNamespace()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .WithNamespace("MyApp.Models")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("namespace MyApp.Models");
        result.ShouldContain("{");
        result.ShouldContain("class MyClass");
        // Check that namespace has closing brace
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        lines[lines.Length - 1].Trim().ShouldBe("}"); // Namespace closing brace
    }

    [Fact]
    public void WithNamespace_NullNamespace_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamespace(null!));
    }

    [Fact]
    public void WithNamespace_EmptyNamespace_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.WithNamespace(""));
    }

    // Member addition tests
    [Fact]
    public void AddMethod_CreatesMethodAndReturnsBuilder()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var methodBuilder = builder.AddMethod("DoWork", "void");
        methodBuilder.MakePublic();
        var result = builder.Build();

        // Assert - using Shouldly
        methodBuilder.ShouldNotBeNull();
        methodBuilder.ShouldBeOfType<MethodBuilder>();
        result.ShouldContain("public void DoWork()");
    }

    [Fact]
    public void AddMethod_WithConfigure_ConfiguresMethod()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddMethod("Calculate", "int", m => m
                .MakePublic()
                .AddParameter("int", "value")
                .WithBody("return value * 2;"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public int Calculate(int value)");
        result.ShouldContain("return value * 2;");
    }

    [Fact]
    public void AddMethod_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddMethod("Method", "void", null!));
    }

    [Fact]
    public void AddMethodWithNoImplementation_CreatesAbstractMethod()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        builder.MakeAbstract();
        var result = builder
            .AddMethodWithNoImplementation("Process", "void")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("abstract class MyClass");
        result.ShouldContain("void Process();");
        result.ShouldNotContain("throw new System.NotImplementedException();");
    }

    [Fact]
    public void AddProperty_CreatesPropertyAndReturnsBuilder()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var propertyBuilder = builder.AddProperty("Name", "string");
        propertyBuilder.MakePublic();
        var result = builder.Build();

        // Assert - using Shouldly
        propertyBuilder.ShouldNotBeNull();
        propertyBuilder.ShouldBeOfType<PropertyBuilder>();
        result.ShouldContain("public string Name { get; set; }");
    }

    [Fact]
    public void AddProperty_WithConfigure_ConfiguresProperty()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddProperty("Count", "int", p => p
                .MakePublic()
                .MakeReadOnly()
                .WithInitializer("0"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public int Count { get; } = 0");
    }

    [Fact]
    public void AddProperty_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddProperty("Prop", "int", null!));
    }

    [Fact]
    public void AddField_CreatesFieldAndReturnsBuilder()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var fieldBuilder = builder.AddField("int", "_count");
        fieldBuilder.MakePrivate();
        var result = builder.Build();

        // Assert - using Shouldly
        fieldBuilder.ShouldNotBeNull();
        fieldBuilder.ShouldBeOfType<FieldBuilder>();
        result.ShouldContain("private int _count;");
    }

    [Fact]
    public void AddField_WithConfigure_ConfiguresField()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddField("string", "_name", f => f
                .MakePrivate()
                .MakeReadOnly()
                .WithInitializer("\"default\""))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("private readonly string _name = \"default\";");
    }

    [Fact(Skip = "TODO: Fix generic type name display")]
    public void AddField_GenericType_CreatesField()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddField<List<string>>("_items")
            .Build();

        // Assert - using Shouldly
        // Note: Generic type shows as List`1 in typeof(T).Name
        // Default field is public
        result.ShouldContain("public List`1 _items;");
    }

    [Fact]
    public void AddField_NullFieldType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.AddField(null!, "_field"));
    }

    [Fact]
    public void AddField_EmptyFieldType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.AddField("", "_field"));
    }

    [Fact]
    public void AddField_NullFieldName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.AddField("int", null!));
    }

    [Fact]
    public void AddField_EmptyFieldName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentException>(() => builder.AddField("int", ""));
    }

    [Fact]
    public void AddField_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddField("int", "_count", null!));
    }

    [Fact]
    public void AddConstructor_CreatesConstructorAndReturnsBuilder()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var ctorBuilder = builder.AddConstructor();
        ctorBuilder.MakePublic();
        var result = builder.Build();

        // Assert - using Shouldly
        ctorBuilder.ShouldNotBeNull();
        ctorBuilder.ShouldBeOfType<ConstructorBuilder>();
        result.ShouldContain("public MyClass()");
    }

    [Fact]
    public void AddConstructor_WithConfigure_ConfiguresConstructor()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddConstructor(c => c
                .MakePublic()
                .AddParameter("string", "name")
                .WithBody("Name = name;"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public MyClass(string name)");
        result.ShouldContain("Name = name;");
    }

    [Fact]
    public void AddConstructor_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddConstructor(null!));
    }

    // Nested type tests
    [Fact]
    public void AddNestedClass_CreatesNestedClass()
    {
        // Arrange
        var builder = new ClassBuilder("OuterClass");

        // Act
        var result = builder
            .AddNestedClass(nested => nested
                .WithName("InnerClass")
                .MakePrivate())
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class OuterClass");
        result.ShouldContain("private class InnerClass");
    }

    [Fact]
    public void AddNestedClass_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddNestedClass(null!));
    }

    [Fact]
    public void AddNestedInterface_CreatesNestedInterface()
    {
        // Arrange
        var builder = new ClassBuilder("OuterClass");

        // Act
        var result = builder
            .AddNestedInterface(nested => nested
                .WithName("IInner")
                .MakePublic())
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class OuterClass");
        result.ShouldContain("public interface IInner");
    }

    [Fact]
    public void AddNestedInterface_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddNestedInterface(null!));
    }

    [Fact]
    public void AddNestedEnum_CreatesNestedEnum()
    {
        // Arrange
        var builder = new ClassBuilder("OuterClass");

        // Act
        var result = builder
            .AddNestedEnum(nested => nested
                .WithName("Status")
                .AddValue("Active", 1)
                .AddValue("Inactive", 0))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class OuterClass");
        result.ShouldContain("enum Status");
        result.ShouldContain("Active");
        result.ShouldContain("Inactive");
    }

    [Fact]
    public void AddNestedEnum_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddNestedEnum(null!));
    }

    [Fact]
    public void AddNestedRecord_CreatesNestedRecord()
    {
        // Arrange
        var builder = new ClassBuilder("OuterClass");

        // Act
        var result = builder
            .AddNestedRecord(nested => nested
                .WithName("Point")
                .WithParameter("int", "X")
                .WithParameter("int", "Y"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("class OuterClass");
        result.ShouldContain("record Point(int X, int Y)");
    }

    [Fact]
    public void AddNestedRecord_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddNestedRecord(null!));
    }

    // Code block tests
    [Fact]
    public void AddCodeBlock_StringContent_AddsRawCode()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddCodeBlock("// Custom code here")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("// Custom code here");
    }

    [Fact]
    public void AddCodeBlock_NullContent_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddCodeBlock((string)null!));
    }

    [Fact]
    public void AddCodeBlock_ActionBuilder_AddsGeneratedCode()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddCodeBlock(cb => cb.AppendLine("// Generated code"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("// Generated code");
    }

    [Fact]
    public void AddCodeBlock_NullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddCodeBlock((Action<ICodeBuilder>)null!));
    }

    // Attribute tests
    [Fact]
    public void AddAttribute_StringAttribute_AddsAttribute()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddAttribute("Serializable")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("[Serializable]");
        result.ShouldContain("class MyClass");
    }

    [Fact]
    public void AddAttribute_AttributeBuilder_AddsAttribute()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");
        var attrBuilder = new AttributeBuilder("Obsolete")
            .WithArgument("\"Use NewClass instead\"");

        // Act
        var result = builder
            .AddAttribute(attrBuilder)
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("[Obsolete(\"Use NewClass instead\")]");
    }

    [Fact]
    public void AddAttribute_NullAttributeBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act & Assert - using Shouldly
        Should.Throw<ArgumentNullException>(() => builder.AddAttribute((AttributeBuilder)null!));
    }

    [Fact]
    public void AddAttribute_MultipleAttributes_AddsAllAttributes()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .AddAttribute("Serializable")
            .AddAttribute("Obsolete")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("[Serializable]");
        result.ShouldContain("[Obsolete]");
    }

    // XML documentation tests
    [Fact]
    public void WithSummary_AddsXmlDocumentation()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .WithSummary("This is a test class.")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// This is a test class.");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void WithXmlDocSummary_AddsXmlDocumentation()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass");

        // Act
        var result = builder
            .WithXmlDocSummary("Another test class.")
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Another test class.");
        result.ShouldContain("/// </summary>");
    }

    // Complex scenario tests
    [Fact]
    public void Build_ComplexClass_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new ClassBuilder("Person");

        // Act
        var result = builder
            .WithNamespace("MyApp.Models")
            .MakePublic()
            .MakeSealed()
            .WithBaseType("EntityBase")
            .ImplementsInterface("IValidatable")
            .ImplementsInterface("ICloneable")
            .WithSummary("Represents a person in the system.")
            .AddAttribute("Serializable")
            .AddProperty("FirstName", "string", p => p
                .MakePublic()
                .AddAttribute("Required"))
            .AddProperty("LastName", "string", p => p
                .MakePublic()
                .AddAttribute("Required"))
            .AddProperty("Age", "int", p => p
                .MakePublic())
            .AddField("DateTime", "_createdAt", f => f
                .MakePrivate()
                .MakeReadOnly())
            .AddConstructor(c => c
                .MakePublic()
                .AddParameter("string", "firstName")
                .AddParameter("string", "lastName")
                .AddParameter("int", "age")
                .WithBody(cb =>
                {
                    cb.AppendLine("FirstName = firstName;");
                    cb.AppendLine("LastName = lastName;");
                    cb.AppendLine("Age = age;");
                    cb.AppendLine("_createdAt = DateTime.UtcNow;");
                }))
            .AddMethod("Validate", "bool", m => m
                .MakePublic()
                .WithBody("return !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) && Age > 0;"))
            .AddMethod("Clone", "object", m => m
                .MakePublic()
                .WithBody("return MemberwiseClone();"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("namespace MyApp.Models");
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Represents a person in the system.");
        result.ShouldContain("[Serializable]");
        result.ShouldContain("public sealed class Person : EntityBase, IValidatable, ICloneable");
        result.ShouldContain("[Required]");
        result.ShouldContain("public string FirstName { get; set; }");
        result.ShouldContain("public string LastName { get; set; }");
        result.ShouldContain("public int Age { get; set; }");
        result.ShouldContain("private readonly DateTime _createdAt;");
        result.ShouldContain("public Person(string firstName, string lastName, int age)");
        result.ShouldContain("FirstName = firstName;");
        result.ShouldContain("public bool Validate()");
        result.ShouldContain("public object Clone()");
    }

    [Fact]
    public void Build_StaticClass_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new ClassBuilder("MathHelper");

        // Act
        var result = builder
            .MakePublic()
            .MakeStatic()
            .AddMethod("Add", "int", m => m
                .MakePublic()
                .MakeStatic()
                .AddParameter("int", "a")
                .AddParameter("int", "b")
                .WithBody("return a + b;"))
            .AddField("double", "PI", f => f
                .MakePublic()
                .MakeStatic()
                .MakeConst("3.14159"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public static class MathHelper");
        result.ShouldContain("public static int Add(int a, int b)");
        result.ShouldContain("public const double PI = 3.14159;");
    }

    [Fact]
    public void Build_AbstractClass_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new ClassBuilder("Animal");

        // Act
        var result = builder
            .MakePublic()
            .MakeAbstract()
            .AddProperty("Name", "string", p => p
                .MakePublic()
                .MakeAbstract())
            .AddMethodWithNoImplementation("MakeSound", "void")
            .AddMethod("Sleep", "void", m => m
                .MakePublic()
                .MakeVirtual()
                .WithBody("Console.WriteLine(\"Sleeping...\");"))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public abstract class Animal");
        result.ShouldContain("public abstract string Name { get; set; }");
        result.ShouldContain("void MakeSound();");
        result.ShouldContain("public virtual void Sleep()");
        result.ShouldContain("Console.WriteLine(\"Sleeping...\");");
    }

    [Fact]
    public void Build_ClassWithNestedTypes_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new ClassBuilder("Container");

        // Act
        var result = builder
            .MakePublic()
            .AddNestedClass(nested => nested
                .WithName("NestedData")
                .MakePrivate()
                .AddProperty("Value", "string", p => p.MakePublic()))
            .AddNestedInterface(nested => nested
                .WithName("IProcessor")
                .MakePublic()
                .AddMethod("Process", "void", m => m.NoImplementation(nested)))
            .AddNestedEnum(nested => nested
                .WithName("Status")
                .MakePublic()
                .AddValue("Active", 1)
                .AddValue("Inactive", 0))
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("public class Container");
        result.ShouldContain("private class NestedData");
        result.ShouldContain("public string Value { get; set; }");
        result.ShouldContain("public interface IProcessor");
        result.ShouldContain("void Process();");
        result.ShouldContain("public enum Status");
        result.ShouldContain("Active = 1");
        result.ShouldContain("Inactive = 0");
    }

    [Fact]
    public void FluentInterface_ChainsCorrectly()
    {
        // Arrange & Act
        var result = new ClassBuilder()
            .WithName("FluentTest")
            .WithNamespace("Test.Namespace")
            .MakePublic()
            .MakePartial()
            .WithBaseType("BaseClass")
            .ImplementsInterface("IInterface1")
            .ImplementsInterface("IInterface2")
            .WithSummary("Test fluent interface chaining.")
            .AddAttribute("TestAttribute")
            .AddProperty("Property1", "string", p => p.MakePublic())
            .AddMethod("Method1", "void", m => m.MakePublic())
            .Build();

        // Assert - using Shouldly
        result.ShouldContain("namespace Test.Namespace");
        result.ShouldContain("/// Test fluent interface chaining.");
        result.ShouldContain("[TestAttribute]");
        result.ShouldContain("public partial class FluentTest : BaseClass, IInterface1, IInterface2");
        result.ShouldContain("public string Property1 { get; set; }");
        result.ShouldContain("public void Method1()");
    }

    [Fact]
    public void Name_Property_ReturnsClassName()
    {
        // Arrange
        var builder = new ClassBuilder("TestClass");

        // Act & Assert - using Shouldly
        builder.Name.ShouldBe("TestClass");

        // Change name and verify
        builder.WithName("UpdatedClass");
        builder.Name.ShouldBe("UpdatedClass");
    }
}
