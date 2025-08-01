using System;
using System.Linq;
using Xunit;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class CodeBuilderTests
{
    [Fact]
    public void ConstructorDefaultIndentSizeUsesFourSpaces()
    {
        // Arrange & Act
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();
        builder.Indent().AppendLine("test");
        var result = builder.Build();

        // Assert
        result.ShouldBe($"    test{Environment.NewLine}");
    }

    [Fact]
    public void ConstructorCustomIndentSizeUsesSpecifiedSpaces()
    {
        // Arrange & Act
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder(2);
        builder.Indent().AppendLine("test");
        var result = builder.Build();

        // Assert
        result.ShouldBe($"  test{Environment.NewLine}");
    }

    [Fact]
    public void AppendLineWithTextAppendsWithNewline()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("Hello World");
        var result = builder.Build();

        // Assert
        result.ShouldBe($"Hello World{Environment.NewLine}");
    }

    [Fact]
    public void AppendLineEmptyStringAppendsBlankLine()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("First");
        builder.AppendLine();
        builder.AppendLine("Second");
        var result = builder.Build();

        // Assert
        result.ShouldBe($"First{Environment.NewLine}{Environment.NewLine}Second{Environment.NewLine}");
    }

    [Fact]
    public void AppendAddsTextWithoutNewline()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Append("Hello ");
        builder.Append("World");
        var result = builder.Build();

        // Assert
        result.ShouldBe("Hello World");
    }

    [Fact]
    public void IndentIncreasesIndentLevel()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("Level 0");
        builder.Indent();
        builder.AppendLine("Level 1");
        builder.Indent();
        builder.AppendLine("Level 2");
        var result = builder.Build();

        // Assert
        result.ShouldContain($"Level 0{Environment.NewLine}");
        result.ShouldContain($"    Level 1{Environment.NewLine}");
        result.ShouldContain($"        Level 2{Environment.NewLine}");
    }

    [Fact]
    public void OutdentDecreasesIndentLevel()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Indent().Indent();
        builder.AppendLine("Level 2");
        builder.Outdent();
        builder.AppendLine("Level 1");
        builder.Outdent();
        builder.AppendLine("Level 0");
        var result = builder.Build();

        // Assert
        result.ShouldContain($"        Level 2{Environment.NewLine}");
        result.ShouldContain($"    Level 1{Environment.NewLine}");
        result.ShouldContain($"Level 0{Environment.NewLine}");
    }

    [Fact]
    public void OutdentAtZeroLevelStaysAtZero()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Outdent(); // Should not go negative
        builder.AppendLine("Test");
        var result = builder.Build();

        // Assert
        result.ShouldBe($"Test{Environment.NewLine}");
    }

    [Fact]
    public void DedentAliasForOutdentWorks()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Indent();
        builder.AppendLine("Indented");
        builder.Dedent();
        builder.AppendLine("Normal");
        var result = builder.Build();

        // Assert
        result.ShouldContain($"    Indented{Environment.NewLine}");
        result.ShouldContain($"Normal{Environment.NewLine}");
    }

    [Fact]
    public void OpenBlockAddsOpeningBraceAndIndents()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("class Test");
        builder.OpenBlock();
        builder.AppendLine("// content");
        var result = builder.Build();

        // Assert
        result.ShouldContain($"class Test{Environment.NewLine}");
        result.ShouldContain($"{{{Environment.NewLine}");
        result.ShouldContain($"    // content{Environment.NewLine}");
    }

    [Fact]
    public void CloseBlockAddsClosingBraceAndOutdents()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("class Test");
        builder.OpenBlock();
        builder.AppendLine("// content");
        builder.CloseBlock();
        var result = builder.Build();

        // Assert
        result.ShouldContain($"    // content{Environment.NewLine}");
        result.ShouldContain($"}}{Environment.NewLine}");
    }

    [Fact]
    public void AppendGeneratedCodeHeaderAddsStandardHeader()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendGeneratedCodeHeader();
        var result = builder.Build();

        // Assert
        result.ShouldContain("// <auto-generated/>");
        result.ShouldContain("#nullable enable");
    }

    [Fact]
    public void AppendNamespaceAddsNamespaceDeclaration()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendNamespace("MyApp.Models");
        var result = builder.Build();

        // Assert
        result.ShouldContain($"namespace MyApp.Models;{Environment.NewLine}");
    }

    [Fact]
    public void WithIndentCreatesTemporaryIndentScope()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("Before");
        using (builder.WithIndent())
        {
            builder.AppendLine("Inside");
        }
        builder.AppendLine("After");
        var result = builder.Build();

        // Assert
        result.ShouldContain($"Before{Environment.NewLine}");
        result.ShouldContain($"    Inside{Environment.NewLine}");
        result.ShouldContain($"After{Environment.NewLine}");
    }

    [Fact]
    public void BuildReturnsBuiltCode()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();
        builder.AppendLine("Test");

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe($"Test{Environment.NewLine}");
    }

    [Fact]
    public void ToStringReturnsBuiltCode()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();
        builder.AppendLine("Test");

        // Act
        var result = builder.ToString();

        // Assert
        result.ShouldBe($"Test{Environment.NewLine}");
    }

    [Fact]
    public void AppendAtStartOfLineAddsIndentation()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Indent();
        builder.Append("First");
        builder.Append("Second");
        builder.AppendLine();
        var result = builder.Build();

        // Assert
        result.ShouldBe($"    FirstSecond{Environment.NewLine}");
    }

    [Fact]
    public void ConstructorNegativeIndentSizeThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder(-1));
    }

    [Fact]
    public void ConstructorZeroIndentSizeUsesNoIndentation()
    {
        // Arrange & Act
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder(0);
        builder.Indent().AppendLine("test");
        var result = builder.Build();

        // Assert
        result.ShouldBe($"test{Environment.NewLine}");
    }

    [Fact]
    public void AppendLineWithNullAppendsEmptyLine()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine(null!);
        var result = builder.Build();

        // Assert
        result.ShouldBe(Environment.NewLine);
    }

    [Fact]
    public void AppendWithNullTreatsAsEmptyString()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Append("before");
        builder.Append(null!);
        builder.Append("after");
        var result = builder.Build();

        // Assert
        result.ShouldBe("beforeafter");
    }

    [Fact]
    public void AppendWithEmptyStringDoesNotAddIndentation()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Indent();
        builder.Append("");
        builder.Append("text");
        var result = builder.Build();

        // Assert
        result.ShouldBe("    text");
    }

    [Fact]
    public void MultipleIndentOutdentTracksLevelCorrectly()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Indent().Indent().Indent(); // Level 3
        builder.AppendLine("Level 3");
        builder.Outdent().Outdent(); // Level 1
        builder.AppendLine("Level 1");
        builder.Outdent().Outdent().Outdent(); // Should stay at 0
        builder.AppendLine("Level 0");
        var result = builder.Build();

        // Assert
        result.ShouldContain("            Level 3");
        result.ShouldContain("    Level 1");
        result.ShouldContain("Level 0");
    }

    [Fact]
    public void ComplexBlockStructureGeneratesCorrectCode()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("namespace MyApp");
        builder.OpenBlock();
        builder.AppendLine("public class MyClass");
        builder.OpenBlock();
        builder.AppendLine("public void MyMethod()");
        builder.OpenBlock();
        builder.AppendLine("Console.WriteLine(\"Hello\");");
        builder.CloseBlock();
        builder.CloseBlock();
        builder.CloseBlock();
        var result = builder.Build();

        // Assert
        result.ShouldContain("namespace MyApp");
        result.ShouldContain("{");
        result.ShouldContain("    public class MyClass");
        result.ShouldContain("        public void MyMethod()");
        result.ShouldContain("            Console.WriteLine(\"Hello\");");
        result.ShouldContain("}");
    }

    [Fact]
    public void WithIndentNestedScopesWorkCorrectly()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("Start");
        using (builder.WithIndent())
        {
            builder.AppendLine("Level 1");
            using (builder.WithIndent())
            {
                builder.AppendLine("Level 2");
            }
            builder.AppendLine("Back to Level 1");
        }
        builder.AppendLine("Back to Start");
        var result = builder.Build();

        // Assert
        result.ShouldContain($"Start{Environment.NewLine}");
        result.ShouldContain($"    Level 1{Environment.NewLine}");
        result.ShouldContain($"        Level 2{Environment.NewLine}");
        result.ShouldContain($"    Back to Level 1{Environment.NewLine}");
        result.ShouldContain($"Back to Start{Environment.NewLine}");
    }

    [Fact]
    public void AppendGeneratedCodeHeaderWithIndentMaintainsIndentation()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Indent();
        builder.AppendGeneratedCodeHeader();
        var result = builder.Build();

        // Assert
        var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        lines.Where(l => l.StartsWith("    //")).Count().ShouldBeGreaterThan(0);
    }

    [Fact]
    public void AppendNamespaceWithNullThrowsArgumentNullException()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.AppendNamespace(null!));
    }

    [Fact]
    public void AppendNamespaceWithEmptyStringThrowsArgumentException()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.AppendNamespace(""));
    }

    [Fact]
    public void MixedAppendAndAppendLineProducesCorrectOutput()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Append("public ");
        builder.Append("class ");
        builder.AppendLine("Test");
        builder.OpenBlock();
        builder.Append("private ");
        builder.AppendLine("int _value;");
        builder.CloseBlock();
        var result = builder.Build();

        // Assert
        result.ShouldContain("public class Test");
        result.ShouldContain("    private int _value;");
    }

    [Fact]
    public void LargeIndentSizeWorksCorrectly()
    {
        // Arrange & Act
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder(8);
        builder.Indent().AppendLine("Indented");
        var result = builder.Build();

        // Assert
        result.ShouldBe($"        Indented{Environment.NewLine}");
    }

    [Fact]
    public void EmptyBuilderBuildReturnsEmptyString()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe("");
    }

    [Fact]
    public void EmptyBuilderToStringReturnsEmptyString()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        var result = builder.ToString();

        // Assert
        result.ShouldBe("");
    }

    [Fact]
    public void ChainedOperationsReturnsSameInstance()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        var result1 = builder.AppendLine("test");
        var result2 = result1.Indent();
        var result3 = result2.Append("more");
        var result4 = result3.Outdent();

        // Assert
        result1.ShouldBe(builder);
        result2.ShouldBe(builder);
        result3.ShouldBe(builder);
        result4.ShouldBe(builder);
    }

    [Fact]
    public void AppendLineAfterAppendStartsNewLineWithoutIndent()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.Indent();
        builder.Append("First");
        builder.AppendLine(" line");
        builder.AppendLine("Second line");
        var result = builder.Build();

        // Assert
        result.ShouldBe($"    First line{Environment.NewLine}    Second line{Environment.NewLine}");
    }

    [Fact]
    public void WithIndentDisposedMultipleTimesOnlyOutdentsOnce()
    {
        // Arrange
        var builder = new FractalDataWorks.SmartGenerators.CodeBuilders.CodeBuilder();

        // Act
        builder.AppendLine("Before");
        var scope = builder.WithIndent();
        builder.AppendLine("Inside");
        scope.Dispose();
        scope.Dispose(); // Second dispose should not affect indentation
        builder.AppendLine("After");
        var result = builder.Build();

        // Assert
        result.ShouldContain($"Before{Environment.NewLine}");
        result.ShouldContain($"    Inside{Environment.NewLine}");
        result.ShouldContain($"After{Environment.NewLine}");
    }

}