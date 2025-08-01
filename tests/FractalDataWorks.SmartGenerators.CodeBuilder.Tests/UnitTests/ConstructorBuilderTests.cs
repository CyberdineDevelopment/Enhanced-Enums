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
    public void ConstructorDefaultConstructorUsesDefaultClassName()
    {
        // Act
        var builder = new ConstructorBuilder();
        var result = builder.Build();

        // Assert
        Assert.Contains("X()", result);
    }

    [Fact]
    public void ConstructorWithClassNameUsesProvidedName()
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
    public void AddParameterWithValidInputsAddsParameter()
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
    public void AddParameterMultipleParametersAddsInOrder()
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
    public void AddParameterWithDefaultValueAddsParameter()
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
    public void AddParameterGenericAddsParameterWithTypeName()
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
    public void AddParameterNullTypeNameThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter(null!, "name"));
    }

    [Fact]
    public void AddParameterEmptyTypeNameThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter("", "name"));
    }

    [Fact]
    public void AddParameterWhitespaceTypeNameThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter("   ", "name"));
    }

    [Fact]
    public void AddParameterNullParameterNameThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter("string", null!));
    }

    [Fact]
    public void AddParameterDuplicateNameThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        builder.AddParameter("string", "name");

        // Assert
        Assert.Throws<ArgumentException>(() => builder.AddParameter("int", "name"));
    }

    [Fact]
    public void MakePublicSetsPublicAccessModifier()
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
    public void MakePrivateSetsPrivateAccessModifier()
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
    public void MakeProtectedSetsProtectedAccessModifier()
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
    public void MakeInternalSetsInternalAccessModifier()
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
    public void MakeStaticSetsStaticModifier()
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
    public void WithBodyStringAddsBodyContent()
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
    public void WithBodyNullStringThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithBody((string)null!));
    }

    [Fact]
    public void WithBodyActionAddsBodyContent()
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
    public void WithBodyNullActionThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithBody((Action<ICodeBuilder>)null!));
    }

    [Fact]
    public void AddBodyStringAddsMultipleStatements()
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
    public void AddBodyNullStringThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddBody((string)null!));
    }

    [Fact]
    public void AddBodyActionAddsBodyContent()
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
    public void AddBodyNullActionThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddBody((Action<ICodeBuilder>)null!));
    }

    [Fact]
    public void WithBaseCallAddsBaseConstructorCall()
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
    public void WithBaseCallMultipleArgumentsAddsAllArguments()
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
    public void WithBaseCallNullArgsThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithBaseCall(null!));
    }

    [Fact]
    public void WithThisCallAddsThisConstructorCall()
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
    public void WithThisCallNullArgsThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithThisCall(null!));
    }

    [Fact]
    public void WithBaseCallAfterThisCallThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        builder.WithThisCall("arg");

        // Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithBaseCall("arg"));
    }

    [Fact]
    public void WithThisCallAfterBaseCallThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act
        builder.WithBaseCall("arg");

        // Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithThisCall("arg"));
    }

    [Fact]
    public void AddBodyForDirectiveAddsConditionalBody()
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
    public void AddBodyForDirectiveMultipleConditionsAddsElseIf()
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
    public void AddBodyForDirectiveNullConditionThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddBodyForDirective(null!, cb => { }));
    }

    [Fact]
    public void AddBodyForDirectiveEmptyConditionThrowsArgumentException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddBodyForDirective("", cb => { }));
    }

    [Fact]
    public void AddBodyForDirectiveNullBlockBuilderThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddBodyForDirective("DEBUG", null!));
    }

    [Fact]
    public void AddElseBodyAfterDirectiveAddsElseBlock()
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
    public void AddElseBodyWithoutDirectiveThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.AddElseBody(cb => { }));
    }

    [Fact]
    public void AddElseBodyNullBlockBuilderThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConstructorBuilder("TestClass");
        builder.AddBodyForDirective("DEBUG", cb => { });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddElseBody(null!));
    }

    [Fact]
    public void WithXmlDocSummaryAddsDocumentation()
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
    public void BuildComplexConstructorGeneratesCorrectCode()
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
