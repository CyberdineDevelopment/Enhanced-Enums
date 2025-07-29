using FractalDataWorks.SmartGenerators.CodeBuilders;
using System;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class CodeBuilderFactoryTests
{
    [Fact]
    public void CreateCodeBuilder_DefaultIndentSize_CreatesCodeBuilder()
    {
        // Act
        var builder = CodeBuilderFactory.CreateCodeBuilder();

        // Assert
        Assert.NotNull(builder);
        Assert.IsAssignableFrom<ICodeBuilder>(builder);
    }

    [Fact]
    public void CreateCodeBuilder_CustomIndentSize_CreatesCodeBuilder()
    {
        // Act
        var builder = CodeBuilderFactory.CreateCodeBuilder(2);

        // Assert
        Assert.NotNull(builder);
        Assert.IsAssignableFrom<ICodeBuilder>(builder);

        // Test indent size works
        builder.OpenBlock();
        builder.AppendLine("test");
        builder.CloseBlock();
        var result = builder.Build();
        Assert.Contains("  test", result); // 2 spaces
    }

    [Fact]
    public void CreateClass_WithValidName_CreatesClassBuilder()
    {
        // Arrange
        var className = "TestClass";

        // Act
        var builder = CodeBuilderFactory.CreateClass(className);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<ClassBuilder>(builder);
        var result = builder.Build();
        Assert.Contains("class TestClass", result);
    }

    [Fact]
    public void CreateMethod_WithValidParameters_CreatesMethodBuilder()
    {
        // Arrange
        var methodName = "GetValue";
        var returnType = "string";

        // Act
        var builder = CodeBuilderFactory.CreateMethod(methodName, returnType);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<MethodBuilder>(builder);
        var result = builder.Build();
        Assert.Contains("string GetValue()", result);
    }

    [Fact]
    public void CreateProperty_WithValidParameters_CreatesPropertyBuilder()
    {
        // Arrange
        var propertyName = "Name";
        var propertyType = "string";

        // Act
        var builder = CodeBuilderFactory.CreateProperty(propertyName, propertyType);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<PropertyBuilder>(builder);
        var result = builder.Build();
        Assert.Contains("string Name { get; set; }", result);
    }

    [Fact]
    public void CreateInterface_WithValidName_CreatesInterfaceBuilder()
    {
        // Arrange
        var interfaceName = "ITestInterface";

        // Act
        var builder = CodeBuilderFactory.CreateInterface(interfaceName);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<InterfaceBuilder>(builder);
        var result = builder.Build();
        Assert.Contains("interface ITestInterface", result);
    }

    [Fact]
    public void CreateNamespace_WithValidName_CreatesNamespaceBuilder()
    {
        // Arrange
        var namespaceName = "Test.Namespace";

        // Act
        var builder = CodeBuilderFactory.CreateNamespace(namespaceName);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<NamespaceBuilder>(builder);
        var result = builder.Build();
        Assert.Contains("namespace Test.Namespace", result);
    }

    [Fact]
    public void CreateField_WithValidParameters_CreatesFieldBuilder()
    {
        // Arrange
        var fieldName = "_count";
        var fieldType = "int";

        // Act
        var builder = CodeBuilderFactory.CreateField(fieldName, fieldType);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<FieldBuilder>(builder);
        var result = builder.Build();
        Assert.Contains("int _count;", result);
    }

    [Fact]
    public void CreateConstructor_WithValidClassName_CreatesConstructorBuilder()
    {
        // Arrange
        var className = "TestClass";

        // Act
        var builder = CodeBuilderFactory.CreateConstructor(className);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<ConstructorBuilder>(builder);
        var result = builder.Build();
        Assert.Contains("TestClass()", result);
    }

    [Fact]
    public void CreateAttribute_WithValidName_CreatesAttributeBuilder()
    {
        // Arrange
        var attributeName = "Serializable";

        // Act
        var builder = CodeBuilderFactory.CreateAttribute(attributeName);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<AttributeBuilder>(builder);
        var result = builder.Build();
        Assert.Equal("[Serializable]", result);
    }

    [Fact]
    public void CreateEnum_WithValidName_CreatesEnumBuilder()
    {
        // Arrange
        var enumName = "TestEnum";

        // Act
        var builder = CodeBuilderFactory.CreateEnum(enumName);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<EnumBuilder>(builder);
        var result = builder.Build();
        Assert.Contains("enum TestEnum", result);
    }

    [Fact]
    public void FactoryMethods_CreateDistinctInstances()
    {
        // Act
        var builder1 = CodeBuilderFactory.CreateClass("Class1");
        var builder2 = CodeBuilderFactory.CreateClass("Class1");

        // Assert
        Assert.NotSame(builder1, builder2);
    }

    [Fact]
    public void CreateMethod_WithVoidReturnType_CreatesValidMethod()
    {
        // Act
        var builder = CodeBuilderFactory.CreateMethod("DoWork", "void");

        // Assert
        var result = builder.Build();
        Assert.Contains("void DoWork()", result);
    }

    [Fact]
    public void ComplexScenario_CombiningFactoryBuilders_GeneratesCorrectCode()
    {
        // Arrange & Act
        var namespaceBuilder = CodeBuilderFactory.CreateNamespace("MyApp.Models");

        var classBuilder = CodeBuilderFactory.CreateClass("Person");
        classBuilder.MakePublic();

        var propertyBuilder1 = CodeBuilderFactory.CreateProperty("Name", "string");
        propertyBuilder1.MakePublic();

        var propertyBuilder2 = CodeBuilderFactory.CreateProperty("Age", "int");
        propertyBuilder2.MakePublic();

        var constructorBuilder = CodeBuilderFactory.CreateConstructor("Person");
        constructorBuilder.MakePublic()
            .AddParameter("string", "name")
            .AddParameter("int", "age")
            .WithBody(cb =>
            {
                cb.AppendLine("Name = name;");
                cb.AppendLine("Age = age;");
            });

        // Manually combine (in real usage, you'd use the builder methods)
        var codeBuilder = CodeBuilderFactory.CreateCodeBuilder();
        codeBuilder.AppendLine(namespaceBuilder.Build());
        codeBuilder.OpenBlock();
        codeBuilder.AppendLine(classBuilder.Build());
        codeBuilder.OpenBlock();
        codeBuilder.AppendLine(propertyBuilder1.Build());
        codeBuilder.AppendLine(propertyBuilder2.Build());
        codeBuilder.AppendLine();
        codeBuilder.AppendLine(constructorBuilder.Build());
        codeBuilder.CloseBlock();
        codeBuilder.CloseBlock();

        var result = codeBuilder.Build();

        // Assert
        Assert.Contains("namespace MyApp.Models", result);
        Assert.Contains("public class Person", result);
        Assert.Contains("public string Name { get; set; }", result);
        Assert.Contains("public int Age { get; set; }", result);
        Assert.Contains("public Person(string name, int age)", result);
        Assert.Contains("Name = name;", result);
        Assert.Contains("Age = age;", result);
    }
}