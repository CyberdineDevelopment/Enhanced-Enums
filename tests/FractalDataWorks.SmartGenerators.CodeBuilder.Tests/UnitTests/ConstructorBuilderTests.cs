using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;
using FractalDataWorks.SmartGenerators.TestUtilities;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class ConstructorBuilderTests
{
    [Fact]
    public void GeneratesPublicParameterlessConstructor()
    {
        var builder = new ConstructorBuilder()
            .WithAccessModifier(AccessModifier.Public)
            .AddBody("Initialize();");
        var complete = $"namespace Test {{ public class X {{ {builder.Build()} }} }}";

        ExpectationsFactory.ExpectCode(complete)
            .HasMethod(m => m
                .HasName("X")
                .HasBody(b => b.HasStatement(0, "Initialize();")))
            .Assert();
    }

    [Fact]
    public void Constructor_DefaultConstructor_UsesDefaultClassName()
    {
        // Act
        var builder = new ConstructorBuilder();
        var result = builder.Build();

        // Assert
        Assert.Contains("X()", result);
    }

    [Fact]
    public void Constructor_WithClassName_UsesProvidedName()
    {
        // Arrange
        var className = "TestClass";

        // Act
        var builder = new ConstructorBuilder(className);
        var result = builder.Build();

        // Assert
        Assert.Contains("TestClass()", result);
    }

    [Fact]
    public void AddParameter_WithValidInputs_AddsParameter()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddParameter("string", "name")
            .Build();

        // Assert
        Assert.Contains("TestClass(string name)", result);
    }

    [Fact]
    public void AddParameter_MultipleParameters_AddsInOrder()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddParameter("string", "name")
            .AddParameter("int", "age")
            .AddParameter("bool", "isActive")
            .Build();

        // Assert
        Assert.Contains("TestClass(string name, int age, bool isActive)", result);
    }

    [Fact]
    public void AddParameter_WithDefaultValue_AddsParameter()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddParameter("int", "count", "10")
            .Build();

        // Assert
        // Note: Constructor parameters don't show default values in the signature
        Assert.Contains("TestClass(int count)", result);
    }

    [Fact]
    public void AddParameter_Generic_AddsParameterWithTypeName()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddParameter<string>("name")
            .AddParameter<int>("age")
            .Build();

        // Assert
        Assert.Contains("TestClass(String name, Int32 age)", result);
    }

    [Fact]
    public void AddParameter_NullTypeName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter(null!, "name"));
    }

    [Fact]
    public void AddParameter_EmptyTypeName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter("", "name"));
    }

    [Fact]
    public void AddParameter_WhitespaceTypeName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter("   ", "name"));
    }

    [Fact]
    public void AddParameter_NullParameterName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter("string", null!));
    }

    [Fact]
    public void AddParameter_DuplicateName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        builder.AddParameter("string", "name");

        // Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter("int", "name"));
    }

    [Fact]
    public void MakePublic_SetsPublicAccessModifier()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .MakePublic()
            .Build();

        // Assert
        Assert.Contains("public TestClass()", result);
    }

    [Fact]
    public void MakePrivate_SetsPrivateAccessModifier()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .MakePrivate()
            .Build();

        // Assert
        Assert.Contains("private TestClass()", result);
    }

    [Fact]
    public void MakeProtected_SetsProtectedAccessModifier()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .MakeProtected()
            .Build();

        // Assert
        Assert.Contains("protected TestClass()", result);
    }

    [Fact]
    public void MakeInternal_SetsInternalAccessModifier()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .MakeInternal()
            .Build();

        // Assert
        Assert.Contains("internal TestClass()", result);
    }

    [Fact]
    public void MakeStatic_SetsStaticModifier()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .MakeStatic()
            .Build();

        // Assert
        Assert.Contains("static TestClass()", result);
    }

    [Fact]
    public void WithBody_String_AddsBodyContent()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .WithBody("_field = value;")
            .Build();

        // Assert
        Assert.Contains("_field = value;", result);
        Assert.Contains("{", result);
        Assert.Contains("}", result);
    }

    [Fact]
    public void WithBody_NullString_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithBody((string)null!));
    }

    [Fact]
    public void WithBody_Action_AddsBodyContent()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .WithBody(cb => cb.AppendLine("_name = name;"))
            .Build();

        // Assert
        Assert.Contains("_name = name;", result);
    }

    [Fact]
    public void WithBody_NullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithBody((Action<ICodeBuilder>)null!));
    }

    [Fact]
    public void AddBody_String_AddsMultipleStatements()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddBody("_field1 = value1;")
            .AddBody("_field2 = value2;")
            .Build();

        // Assert
        Assert.Contains("_field1 = value1;", result);
        Assert.Contains("_field2 = value2;", result);
    }

    [Fact]
    public void AddBody_NullString_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddBody((string)null!));
    }

    [Fact]
    public void AddBody_Action_AddsBodyContent()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddBody(cb => cb.AppendLine("Initialize();"))
            .Build();

        // Assert
        Assert.Contains("Initialize();", result);
    }

    [Fact]
    public void AddBody_NullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddBody((Action<ICodeBuilder>)null!));
    }

    [Fact]
    public void WithBaseCall_AddsBaseConstructorCall()
    {
        // Arrange
        var builder = new ConstructorBuilder("DerivedClass");

        // Act
        var result = builder
            .AddParameter("string", "name")
            .WithBaseCall("name")
            .Build();

        // Assert
        Assert.Contains("DerivedClass(string name) : base(name)", result);
    }

    [Fact]
    public void WithBaseCall_MultipleArguments_AddsAllArguments()
    {
        // Arrange
        var builder = new ConstructorBuilder("DerivedClass");

        // Act
        var result = builder
            .WithBaseCall("arg1", "arg2", "arg3")
            .Build();

        // Assert
        Assert.Contains(": base(arg1, arg2, arg3)", result);
    }

    [Fact]
    public void WithBaseCall_NullArgs_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithBaseCall(null!));
    }

    [Fact]
    public void WithThisCall_AddsThisConstructorCall()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddParameter("string", "name")
            .WithThisCall("name", "0")
            .Build();

        // Assert
        Assert.Contains("TestClass(string name) : this(name, 0)", result);
    }

    [Fact]
    public void WithThisCall_NullArgs_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithThisCall(null!));
    }

    [Fact]
    public void WithBaseCall_AfterThisCall_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        builder.WithThisCall("arg");

        // Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithBaseCall("arg"));
    }

    [Fact]
    public void WithThisCall_AfterBaseCall_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        builder.WithBaseCall("arg");

        // Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithThisCall("arg"));
    }

    [Fact]
    public void AddBodyForDirective_AddsConditionalBody()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddBodyForDirective("DEBUG", cb => cb.AppendLine("Console.WriteLine(\"Debug mode\");"))
            .Build();

        // Assert
        Assert.Contains("#if DEBUG", result);
        Assert.Contains("Console.WriteLine(\"Debug mode\");", result);
        Assert.Contains("#endif", result);
    }

    [Fact]
    public void AddBodyForDirective_MultipleConditions_AddsElseIf()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddBodyForDirective("NET6_0", cb => cb.AppendLine("// .NET 6"))
            .AddBodyForDirective("NET7_0", cb => cb.AppendLine("// .NET 7"))
            .Build();

        // Assert
        Assert.Contains("#if NET6_0", result);
        Assert.Contains("#elif NET7_0", result);
        Assert.Contains("#endif", result);
    }

    [Fact]
    public void AddBodyForDirective_NullCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddBodyForDirective(null!, cb => { }));
    }

    [Fact]
    public void AddBodyForDirective_EmptyCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddBodyForDirective("", cb => { }));
    }

    [Fact]
    public void AddBodyForDirective_NullBlockBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddBodyForDirective("DEBUG", null!));
    }

    [Fact]
    public void AddElseBody_AfterDirective_AddsElseBlock()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .AddBodyForDirective("DEBUG", cb => cb.AppendLine("// Debug"))
            .AddElseBody(cb => cb.AppendLine("// Release"))
            .Build();

        // Assert
        Assert.Contains("#if DEBUG", result);
        Assert.Contains("#else", result);
        Assert.Contains("// Release", result);
        Assert.Contains("#endif", result);
    }

    [Fact]
    public void AddElseBody_WithoutDirective_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.AddElseBody(cb => { }));
    }

    [Fact]
    public void AddElseBody_NullBlockBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");
        builder.AddBodyForDirective("DEBUG", cb => { });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddElseBody(null!));
    }

    [Fact]
    public void WithXmlDocSummary_AddsDocumentation()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        var result = builder
            .WithXmlDocSummary("Initializes a new instance of the TestClass.")
            .Build();

        // Assert
        Assert.Contains("/// <summary>", result);
        Assert.Contains("/// Initializes a new instance of the TestClass.", result);
        Assert.Contains("/// </summary>", result);
    }

    [Fact]
    public void Build_ComplexConstructor_GeneratesCorrectCode()
    {
        // Arrange
        var builder = new ConstructorBuilder("Person");

        // Act
        var result = builder
            .MakePublic()
            .WithXmlDocSummary("Initializes a new instance of the Person class.")
            .AddParameter("string", "name")
            .AddParameter("int", "age")
            .WithBody(cb =>
            {
                cb.AppendLine("Name = name ?? throw new ArgumentNullException(nameof(name));");
                cb.AppendLine("Age = age;");
            })
            .Build();

        // Assert
        Assert.Contains("/// <summary>", result);
        Assert.Contains("public Person(string name, int age)", result);
        Assert.Contains("Name = name ?? throw new ArgumentNullException(nameof(name));", result);
        Assert.Contains("Age = age;", result);
    }
}
