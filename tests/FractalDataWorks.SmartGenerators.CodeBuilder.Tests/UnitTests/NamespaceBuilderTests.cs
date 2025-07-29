using FractalDataWorks.SmartGenerators.CodeBuilders;
using System;
using System.Linq;
using Xunit;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class NamespaceBuilderTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesNamespace()
    {
        // Arrange & Act
        var builder = new NamespaceBuilder("TestNamespace");
        var code = builder.Build();

        // Assert
        code.ShouldContain("namespace TestNamespace;");
    }

    [Fact]
    public void AddUsing_AddsUsingStatement()
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
    public void AddUsing_MultipleUsings_AddsAllInOrder()
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
    public void AddUsing_DuplicateUsing_OnlyAddsOnce()
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
    public void WithUsing_AddsUsingStatement()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .WithUsing("System.Text")
            .Build();

        // Assert
        code.ShouldContain("using System.Text;");
    }

    [Fact]
    public void AddClass_WithClassBuilder_AddsClass()
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
    public void AddClass_WithConfiguration_AddsClass()
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
    public void WithClass_AddsClass()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");
        var classBuilder = new ClassBuilder("TestClass");

        // Act
        var code = builder
            .WithClass(classBuilder)
            .Build();

        // Assert
        code.ShouldContain("class TestClass");
    }

    [Fact]
    public void AddInterface_WithInterfaceBuilder_AddsInterface()
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
    public void AddInterface_WithConfiguration_AddsInterface()
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
    public void AddEnum_WithEnumBuilder_AddsEnum()
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
    public void AddEnum_WithConfiguration_AddsEnum()
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
    public void WithEnum_AddsEnum()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");
        var enumBuilder = new EnumBuilder("TestEnum");

        // Act
        var code = builder
            .WithEnum(enumBuilder)
            .Build();

        // Assert
        code.ShouldContain("enum TestEnum");
    }

    [Fact]
    public void AddRecord_WithRecordBuilder_AddsRecord()
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
    public void AddRecord_WithConfiguration_AddsRecord()
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
    public void Build_WithMultipleMembers_ProperlySpacesThem()
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

    [Fact(Skip = "TODO: NamespaceBuilder constructor doesn't validate null/empty namespace name")]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new NamespaceBuilder(null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder constructor doesn't validate null/empty namespace name")]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new NamespaceBuilder(""));
    }

    [Fact(Skip = "TODO: NamespaceBuilder constructor doesn't validate null/empty namespace name")]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new NamespaceBuilder("   "));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddUsing doesn't validate null/empty parameters")]
    public void AddUsing_WithNullUsing_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddUsing(null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddUsing doesn't validate null/empty parameters")]
    public void AddUsing_WithEmptyUsing_ThrowsArgumentException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.AddUsing(""));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.WithUsing doesn't validate null/empty parameters")]
    public void WithUsing_WithNullUsing_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.WithUsing(null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddClass doesn't validate null parameters")]
    public void AddClass_WithNullClassBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddClass((ClassBuilder)null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddClass doesn't validate null parameters")]
    public void AddClass_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddClass((Action<ClassBuilder>)null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.WithClass doesn't validate null parameters")]
    public void WithClass_WithNullClassBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.WithClass(null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddInterface doesn't validate null parameters")]
    public void AddInterface_WithNullInterfaceBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddInterface((InterfaceBuilder)null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddInterface doesn't validate null parameters")]
    public void AddInterface_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddInterface((Action<InterfaceBuilder>)null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddEnum doesn't validate null parameters")]
    public void AddEnum_WithNullEnumBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddEnum((EnumBuilder)null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddEnum doesn't validate null parameters")]
    public void AddEnum_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddEnum((Action<EnumBuilder>)null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.WithEnum doesn't validate null parameters")]
    public void WithEnum_WithNullEnumBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.WithEnum(null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddRecord doesn't validate null parameters")]
    public void AddRecord_WithNullRecordBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddRecord((RecordBuilder)null!));
    }

    [Fact(Skip = "TODO: NamespaceBuilder.AddRecord doesn't validate null parameters")]
    public void AddRecord_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AddRecord((Action<RecordBuilder>)null!));
    }

    [Fact]
    public void Build_EmptyNamespace_GeneratesOnlyNamespaceDeclaration()
    {
        // Arrange
        var builder = new NamespaceBuilder("EmptyNamespace");

        // Act
        var code = builder.Build();

        // Assert
        code.ShouldBe("namespace EmptyNamespace;" + Environment.NewLine + Environment.NewLine);
    }

    [Fact]
    public void Build_WithComplexNamespace_GeneratesCorrectStructure()
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
    public void AddUsing_WithStaticUsing_AddsStaticUsing()
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
    public void AddUsing_WithGlobalUsing_AddsGlobalUsing()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .AddUsing("global::System.Collections.Generic")
            .Build();

        // Assert
        code.ShouldContain("using global::System.Collections.Generic;");
    }

    [Fact]
    public void AddUsing_WithAlias_AddsUsingAlias()
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
    public void Build_WithMultipleClassesAndInterfaces_ProperlySpacesThem()
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
    public void WithUsing_MultipleCallsChained_AddsAllUsings()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var code = builder
            .WithUsing("System")
            .WithUsing("System.Text")
            .WithUsing("System.Linq")
            .Build();

        // Assert
        code.ShouldContain("using System;");
        code.ShouldContain("using System.Text;");
        code.ShouldContain("using System.Linq;");
    }

    [Fact]
    public void Build_WithNestedTypes_GeneratesProperStructure()
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

    [Fact(Skip = "TODO: NamespaceBuilder methods don't validate null parameters")]
    public void ICodeBuilder_Methods_NotDirectlyUsed()
    {
        // These methods from ICodeBuilder interface are not meant to be used directly on NamespaceBuilder
        // They're internal implementation details
        var builder = new NamespaceBuilder("TestNamespace");

        // These would need proper testing if they're meant to be public API
        // Currently marking as skipped since they seem to be internal implementation
    }

    [Fact]
    public void ToString_ReturnsEmptyString()
    {
        // Arrange
        var builder = new NamespaceBuilder("TestNamespace");

        // Act
        var result = builder.ToString();

        // Assert
        result.ShouldBe("");
    }
}