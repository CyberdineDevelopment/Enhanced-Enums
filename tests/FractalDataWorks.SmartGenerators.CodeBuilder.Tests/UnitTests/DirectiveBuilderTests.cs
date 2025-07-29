using FractalDataWorks.SmartGenerators.CodeBuilders;
using System;
using Xunit;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.UnitTests;

public class DirectiveBuilderTests
{
    [Fact]
    public void Create_ReturnsNewInstance()
    {
        // Act
        var builder = DirectiveBuilder.Create();

        // Assert
        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<DirectiveBuilder>();
    }

    [Fact]
    public void If_WithValidConditionAndBody_AddsIfClause()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        var result = builder.If("NET6_0_OR_GREATER", cb => cb.AppendLine("// .NET 6+ code"));

        // Assert
        result.ShouldBeSameAs(builder); // Verify fluent API
        var code = builder.Build();
        code.ShouldContain("#if NET6_0_OR_GREATER");
        code.ShouldContain("// .NET 6+ code");
        code.ShouldContain("#endif");
    }

    [Fact]
    public void If_WithNullCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.If(null!, cb => { }));
    }

    [Fact]
    public void If_WithEmptyCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.If("", cb => { }));
    }

    [Fact]
    public void If_WithWhitespaceCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.If("   ", cb => { }));
    }

    [Fact]
    public void If_WithNullBody_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.If("CONDITION", null!));
    }

    [Fact]
    public void ElseIf_WithValidConditionAndBody_AddsElseIfClause()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("NET6_0", cb => cb.AppendLine("// .NET 6 code"))
               .ElseIf("NET7_0", cb => cb.AppendLine("// .NET 7 code"));
        var code = builder.Build();

        // Assert
        code.ShouldContain("#if NET6_0");
        code.ShouldContain("#elif NET7_0");
        code.ShouldContain("// .NET 7 code");
    }

    [Fact]
    public void ElseIf_WithNullCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.ElseIf(null!, cb => { }));
    }

    [Fact]
    public void ElseIf_WithEmptyCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.ElseIf("", cb => { }));
    }

    [Fact]
    public void ElseIf_WithNullBody_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.ElseIf("CONDITION", null!));
    }

    [Fact]
    public void Else_WithValidBody_AddsElseClause()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("DEBUG", cb => cb.AppendLine("// Debug code"))
               .Else(cb => cb.AppendLine("// Release code"));
        var code = builder.Build();

        // Assert
        code.ShouldContain("#if DEBUG");
        code.ShouldContain("#else");
        code.ShouldContain("// Release code");
        code.ShouldContain("#endif");
    }

    [Fact]
    public void Else_WithNullBody_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => builder.Else(null!));
    }

    [Fact]
    public void Build_ComplexDirective_GeneratesCorrectStructure()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("NET6_0_OR_GREATER", cb =>
            {
                cb.AppendLine("// Modern .NET");
                cb.AppendLine("using System.Diagnostics.CodeAnalysis;");
            })
            .ElseIf("NETSTANDARD2_0", cb =>
            {
                cb.AppendLine("// .NET Standard 2.0");
            })
            .Else(cb =>
            {
                cb.AppendLine("// Fallback");
            });
        var code = builder.Build();

        // Assert
        var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        lines[0].ShouldBe("#if NET6_0_OR_GREATER");
        lines[1].ShouldContain("    // Modern .NET");
        lines[2].ShouldContain("    using System.Diagnostics.CodeAnalysis;");
        lines[3].ShouldBe("#elif NETSTANDARD2_0");
        lines[4].ShouldContain("    // .NET Standard 2.0");
        lines[5].ShouldBe("#else");
        lines[6].ShouldContain("    // Fallback");
        lines[7].ShouldBe("#endif");
    }

    [Fact]
    public void Build_EmptyDirective_StillGeneratesEndif()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        var code = builder.Build();

        // Assert
        code.ShouldBe($"#endif{Environment.NewLine}");
    }

    [Fact]
    public void Build_MultipleElseIf_GeneratesCorrectStructure()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("NET6_0", cb => cb.AppendLine("// .NET 6"))
               .ElseIf("NET7_0", cb => cb.AppendLine("// .NET 7"))
               .ElseIf("NET8_0", cb => cb.AppendLine("// .NET 8"))
               .ElseIf("NET9_0", cb => cb.AppendLine("// .NET 9"))
               .Else(cb => cb.AppendLine("// Other"));
        var code = builder.Build();

        // Assert
        code.ShouldContain("#if NET6_0");
        code.ShouldContain("#elif NET7_0");
        code.ShouldContain("#elif NET8_0");
        code.ShouldContain("#elif NET9_0");
        code.ShouldContain("#else");
        code.ShouldContain("#endif");
    }

    [Fact]
    public void Build_WithoutIfClause_StillGeneratesEndif()
    {
        // Arrange & Act
        var builder = DirectiveBuilder.Create();
        builder.ElseIf("CONDITION", cb => cb.AppendLine("// Code"));
        var code = builder.Build();

        // Assert
        code.ShouldContain("#elif CONDITION");
        code.ShouldContain("#endif");
    }

    [Fact]
    public void Build_ElseWithoutIf_StillGeneratesStructure()
    {
        // Arrange & Act
        var builder = DirectiveBuilder.Create();
        builder.Else(cb => cb.AppendLine("// Code"));
        var code = builder.Build();

        // Assert
        code.ShouldContain("#else");
        code.ShouldContain("// Code");
        code.ShouldContain("#endif");
    }

    [Fact]
    public void Build_NestedDirectives_GeneratesCorrectStructure()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("OUTER_CONDITION", cb =>
        {
            cb.AppendLine("// Outer code");
            var inner = DirectiveBuilder.Create();
            inner.If("INNER_CONDITION", innerCb => innerCb.AppendLine("// Inner code"));
            cb.Append(inner.Build());
        });
        var code = builder.Build();

        // Assert
        code.ShouldContain("#if OUTER_CONDITION");
        code.ShouldContain("// Outer code");
        code.ShouldContain("#if INNER_CONDITION");
        code.ShouldContain("// Inner code");
        var endifCount = code.Split(new[] { "#endif" }, StringSplitOptions.None).Length - 1;
        endifCount.ShouldBe(2);
    }

    [Fact]
    public void Build_ComplexConditions_GeneratesCorrectCode()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("NET6_0_OR_GREATER && !NETSTANDARD", cb => cb.AppendLine("// Complex condition"));
        var code = builder.Build();

        // Assert
        code.ShouldContain("#if NET6_0_OR_GREATER && !NETSTANDARD");
    }

    [Fact]
    public void Build_EmptyBodyCallbacks_GeneratesMinimalStructure()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("CONDITION", cb => { })
               .ElseIf("OTHER", cb => { })
               .Else(cb => { });
        var code = builder.Build();

        // Assert
        code.ShouldContain("#if CONDITION");
        code.ShouldContain("#elif OTHER");
        code.ShouldContain("#else");
        code.ShouldContain("#endif");
        var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(4); // Only directive lines, no content
    }

    [Fact]
    public void ElseIf_WhitespaceCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.ElseIf("   ", cb => { }));
    }

    [Fact]
    public void Build_WithIndentedContent_MaintainsProperIndentation()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("DEBUG", cb =>
        {
            cb.AppendLine("public void DebugMethod()");
            cb.AppendLine("{");
            cb.Indent();
            cb.AppendLine("Console.WriteLine(\"Debug\");");
            cb.Outdent();
            cb.AppendLine("}");
        });
        var code = builder.Build();

        // Assert
        var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        lines[1].ShouldBe("    public void DebugMethod()");
        lines[2].ShouldBe("    {");
        lines[3].ShouldBe("        Console.WriteLine(\"Debug\");");
        lines[4].ShouldBe("    }");
    }

    [Fact]
    public void ChainingMethods_ReturnsSameInstance()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        var result1 = builder.If("A", cb => { });
        var result2 = result1.ElseIf("B", cb => { });
        var result3 = result2.Else(cb => { });

        // Assert
        result1.ShouldBeSameAs(builder);
        result2.ShouldBeSameAs(builder);
        result3.ShouldBeSameAs(builder);
    }

    [Fact]
    public void Build_DefineConstants_GeneratesCorrectDirectives()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("!FEATURE_ENABLED", cb =>
        {
            cb.AppendLine("#define FEATURE_ENABLED");
        });
        var code = builder.Build();

        // Assert
        code.ShouldContain("#if !FEATURE_ENABLED");
        code.ShouldContain("#define FEATURE_ENABLED");
    }

    [Fact]
    public void Build_PragmaWarnings_GeneratesCorrectDirectives()
    {
        // Arrange
        var builder = DirectiveBuilder.Create();

        // Act
        builder.If("SUPPRESS_WARNINGS", cb =>
        {
            cb.AppendLine("#pragma warning disable CS0168");
            cb.AppendLine("int unusedVariable;");
            cb.AppendLine("#pragma warning restore CS0168");
        });
        var code = builder.Build();

        // Assert
        code.ShouldContain("#if SUPPRESS_WARNINGS");
        code.ShouldContain("#pragma warning disable CS0168");
        code.ShouldContain("#pragma warning restore CS0168");
    }
}