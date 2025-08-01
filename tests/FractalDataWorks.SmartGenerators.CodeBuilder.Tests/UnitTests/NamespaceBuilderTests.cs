using FractalDataWorks.SmartGenerators.CodeBuilders;
using System;
using System.Linq;
using Xunit;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class NamespaceBuilderTests
{
    [Fact]
    public void ConstructorWithValidNameCreatesNamespace()
    {
        // Arrange & Act
        var builder = new NamespaceBuilder("TestNamespace");
        var code = builder.Build();

        // Assert
        code.ShouldContain("namespace TestNamespace;");
    }

    [Fact]
    public void AddUsingAddsUsingStatement()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("System")
            .Build();

        // Assert
        code.ShouldContain("using System;");
        code.ShouldContain("namespace TestNamespace;");
    }

    [Fact]
    public void AddUsingMultipleUsingsAddsAllInOrder()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("System")
            .AddUsing("System.Linq")
            .AddUsing("System.Collections.Generic")
            .Build();

        // Assert
        var lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        lines[0].ShouldContain("using System;");
        lines[1].ShouldContain("using System.Linq;");
        lines[2].ShouldContain("using System.Collections.Generic;");
    }

    [Fact]
    public void AddUsingDuplicateUsingOnlyAddsOnce()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("System")
            .AddUsing("System")
            .Build();

        // Assert
        var systemCount = 0;
        foreach (var line in code.Split('\n'))
        {
            if (line.Contains("using System;"))
                systemCount++;
        }
        systemCount.ShouldBe(1);
    }

    [Fact]
    public void AddUsingAddsUsingStatementAlternativeTest()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("System.Text")
            .Build();

        // Assert
        code.ShouldContain("using System.Text;");
    }

    [Fact]
    public void AddClassWithClassBuilderAddsClass()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");
        var classBuilder = new ClassBuilder("TestClass");

        // Act
        var code = builder
            .AddClass(classBuilder)
            .Build();

        // Assert
        code.ShouldContain("namespace TestNamespace;");
        code.ShouldContain("class TestClass");
    }

    [Fact]
    public void AddClassWithConfigurationAddsClass()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddClass(c => c.WithName("ConfiguredClass").MakePublic())
            .Build();

        // Assert
        code.ShouldContain("public class ConfiguredClass");
    }

    [Fact]
    public void AddClassAddsClassAlternativeTest()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");
        var classBuilder = new ClassBuilder("TestClass");

        // Act
        var code = builder
            .AddClass(classBuilder)
            .Build();

        // Assert
        code.ShouldContain("class TestClass");
    }

    [Fact]
    public void AddInterfaceWithInterfaceBuilderAddsInterface()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");
        var interfaceBuilder = new InterfaceBuilder("ITestInterface");

        // Act
        var code = builder
            .AddInterface(interfaceBuilder)
            .Build();

        // Assert
        code.ShouldContain("interface ITestInterface");
    }

    [Fact]
    public void AddInterfaceWithConfigurationAddsInterface()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddInterface(i => i.WithName("IConfigured").MakePublic())
            .Build();

        // Assert
        code.ShouldContain("public interface IConfigured");
    }

    [Fact]
    public void AddEnumWithEnumBuilderAddsEnum()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");
        var enumBuilder = new EnumBuilder("TestEnum");

        // Act
        var code = builder
            .AddEnum(enumBuilder)
            .Build();

        // Assert
        code.ShouldContain("enum TestEnum");
    }

    [Fact]
    public void AddEnumWithConfigurationAddsEnum()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddEnum(e => e.WithName("Status").AddMember("Active").AddMember("Inactive"))
            .Build();

        // Assert
        code.ShouldContain("enum Status");
        code.ShouldContain("Active");
        code.ShouldContain("Inactive");
    }

    [Fact]
    public void AddEnumAddsEnumAlternativeTest()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");
        var enumBuilder = new EnumBuilder("TestEnum");

        // Act
        var code = builder
            .AddEnum(enumBuilder)
            .Build();

        // Assert
        code.ShouldContain("enum TestEnum");
    }

    [Fact]
    public void AddRecordWithRecordBuilderAddsRecord()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");
        var recordBuilder = new RecordBuilder("TestRecord");

        // Act
        var code = builder
            .AddRecord(recordBuilder)
            .Build();

        // Assert
        code.ShouldContain("record TestRecord");
    }

    [Fact]
    public void AddRecordWithConfigurationAddsRecord()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddRecord(r => r.WithName("Person").WithParameter("string", "Name"))
            .Build();

        // Assert
        code.ShouldContain("record Person");
    }

    [Fact]
    public void BuildWithMultipleMembersProperlySpacesThem()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("System")
            .AddClass(c => c.WithName("FirstClass"))
            .AddInterface(i => i.WithName("IInterface"))
            .AddEnum(e => e.WithName("TestEnum"))
            .Build();

        // Assert
        code.ShouldContain("using System;");
        code.ShouldContain("namespace TestNamespace;");
        code.ShouldContain("class FirstClass");
        code.ShouldContain("interface IInterface");
        code.ShouldContain("enum TestEnum");

        // Verify spacing between members
        var parts = code.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);
        parts.Length.ShouldBeGreaterThanOrEqualTo(4); // Using section, namespace declaration, and members with spacing
    }

    [Fact]
    public void ConstructorWithNullNameThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new NamespaceBuilder(null!));
    }

    [Fact]
    public void ConstructorWithEmptyNameThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new NamespaceBuilder(""));
    }

    [Fact]
    public void ConstructorWithWhitespaceNameThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new NamespaceBuilder("   "));
    }

    [Fact]
    public void AddUsingWithNullUsingThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddUsing(null!));
    }

    [Fact]
    public void AddUsingWithEmptyUsingThrowsArgumentException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.AddUsing(""));
    }


    [Fact]
    public void AddClassWithNullClassBuilderThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddClass((ClassBuilder)null!));
    }

    [Fact]
    public void AddClassWithNullConfigurationThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddClass((Action<ClassBuilder>)null!));
    }


    [Fact]
    public void AddInterfaceWithNullInterfaceBuilderThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddInterface((InterfaceBuilder)null!));
    }

    [Fact]
    public void AddInterfaceWithNullConfigurationThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddInterface((Action<InterfaceBuilder>)null!));
    }

    [Fact]
    public void AddEnumWithNullEnumBuilderThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddEnum((EnumBuilder)null!));
    }

    [Fact]
    public void AddEnumWithNullConfigurationThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddEnum((Action<EnumBuilder>)null!));
    }


    [Fact]
    public void AddRecordWithNullRecordBuilderThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddRecord((RecordBuilder)null!));
    }

    [Fact]
    public void AddRecordWithNullConfigurationThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddRecord((Action<RecordBuilder>)null!));
    }

    [Fact]
    public void BuildEmptyNamespaceGeneratesOnlyNamespaceDeclaration()
    {
        // Arrange
        var builder = new NamespaceBuilder("EmptyNamespace");

        // Act
        var code = builder.Build();

        // Assert
        code.ShouldBe("namespace EmptyNamespace;" + Environment.NewLine + Environment.NewLine);
    }

    [Fact]
    public void BuildWithComplexNamespaceGeneratesCorrectStructure()
    {
        // Arrange
        var builder = new NamespaceBuilder("Company.Product.Feature");

        // Act
        var code = builder
            .AddUsing("System")
            .AddUsing("System.Linq")
            .AddClass(c => c.WithName("Service").MakePublic()
                .AddMethod("Execute", "void", m => m.MakePublic()))
            .Build();

        // Assert
        code.ShouldContain("namespace Company.Product.Feature;");
        code.ShouldContain("using System;");
        code.ShouldContain("using System.Linq;");
        code.ShouldContain("public class Service");
        code.ShouldContain("public void Execute()");
    }

    [Fact]
    public void AddUsingWithStaticUsingAddsStaticUsing()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("static System.Math")
            .Build();

        // Assert
        code.ShouldContain("using static System.Math;");
    }


    [Fact]
    public void AddUsingWithAliasAddsUsingAlias()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("StringList = System.Collections.Generic.List<string>")
            .Build();

        // Assert
        code.ShouldContain("using StringList = System.Collections.Generic.List<string>;");
    }

    [Fact]
    public void BuildWithMultipleClassesAndInterfacesProperlySpacesThem()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddClass(c => c.WithName("FirstClass"))
            .AddInterface(i => i.WithName("IFirstInterface"))
            .AddClass(c => c.WithName("SecondClass"))
            .AddInterface(i => i.WithName("ISecondInterface"))
            .Build();

        // Assert
        code.ShouldContain("class FirstClass");
        code.ShouldContain("interface IFirstInterface");
        code.ShouldContain("class SecondClass");
        code.ShouldContain("interface ISecondInterface");

        // Verify proper spacing
        var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        lines.Where(l => l == "").Count().ShouldBeGreaterThan(3); // Should have empty lines between members
    }

    [Fact]
    public void AddUsingMultipleCallsChainedAddsAllUsings()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("System")
            .AddUsing("System.Text")
            .AddUsing("System.Linq")
            .Build();

        // Assert
        code.ShouldContain("using System;");
        code.ShouldContain("using System.Text;");
        code.ShouldContain("using System.Linq;");
    }

    [Fact]
    public void BuildWithNestedTypesGeneratesProperStructure()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddClass(c => c.WithName("OuterClass").MakePublic()
                .AddNestedClass(nested => nested.WithName("NestedClass").MakePrivate()))
            .Build();

        // Assert
        code.ShouldContain("public class OuterClass");
        code.ShouldContain("private class NestedClass");
    }

    [Fact]
    public void ICodeBuilderMethodsAreImplemented()
    {
        // These methods from ICodeBuilder interface are implementation details
        // but we can verify they exist and don't throw exceptions
        var builder = new NamespaceBuilder("TestNamespace");

        // These methods should not throw exceptions when called
        builder.Append("test");
        builder.AppendLine("test");
        builder.Indent();
        builder.Outdent();
        builder.OpenBlock();
        builder.CloseBlock();
        
        // ToString returns internal builder state, not the namespace code
        var result = builder.ToString();
        result.ShouldNotBeNull();
    }

    [Fact]
    public void ToStringReturnsEmptyString()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var result = builder.ToString();

        // Assert
        result.ShouldBe("");
    }
}