using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.UnitTests;

/// <summary>
/// Tests for <see cref="SourceGeneratorTestHelper"/> class.
/// </summary>
public class SourceGeneratorTestHelperTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGeneratorTestHelperTests"/> class.
    /// </summary>
    /// <param name="testOutputHelper"></param>
    public SourceGeneratorTestHelperTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    #region RunGenerator Tests
    [Fact]
    public void RunGeneratorWithValidSourceAndGeneratorGeneratesExpectedOutput()
    {
        // Arrange
        var sources = new[]
        {
            TestSources.GenerateCodeAttributeSource, // Include the attribute definition
            TestSources.AttributeClassSource
        };
        var generator = new TestGenerators.MockGenerator();

        // Debug - print the exact source content for inspection
        _testOutputHelper.WriteLine("DEBUG - Input sources:");
        for (var i = 0; i < sources.Length; i++)
        {
            _testOutputHelper.WriteLine($"Source {i}:");
            _testOutputHelper.WriteLine(sources[i]);
        }

        // Act
        var output = SourceGeneratorTestHelper.RunGenerator(
            generator, sources, out var diagnostics);

        // Assert
        // Debug output to inspect the keys and help diagnose the issue
        _testOutputHelper.WriteLine("TEST DEBUG OUTPUT:");
        _testOutputHelper.WriteLine($"Generated files count: {output.Count}");
        _testOutputHelper.WriteLine("Generated output keys:");
        foreach (var key in output.Keys)
        {
            _testOutputHelper.WriteLine($"- {key}");
            _testOutputHelper.WriteLine($"  Content preview: {output[key].Substring(0, Math.Min(100, output[key].Length))}...");
        }

        diagnostics.ShouldBeEmpty();
        output.Count.ShouldBe(2); // Attribute + extension class

        output.ShouldContainKey("GenerateCodeAttribute.g.cs");
        output.ShouldContainKey("AttributeClassGenerated.g.cs");

        // Verify the attribute contents
        output["GenerateCodeAttribute.g.cs"].ShouldContain("namespace TestNamespace");
        output["AttributeClassGenerated.g.cs"].ShouldContain("namespace TestNamespace");
    }

    [Fact]
    public void RunGeneratorWithInvalidSourceProducesDiagnostics()
    {
        // Arrange
        var sources = new[]
        {
            @"
            namespace Test;
            
            [GenerateCode]
            public class TestClass
            {
                public string TestMethod()
                {
                    return 123; // Type mismatch error
                }
            }"
        };
        var generator = new TestGenerators.MockGenerator();

        // Act
        var output = SourceGeneratorTestHelper.RunGenerator(
            generator, sources, out var diagnostics);

        // Assert
        diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);

        // We still expect the attribute to be generated even if there are errors
        output.Count.ShouldBe(2);
        output.ShouldContainKey("GenerateCodeAttribute.g.cs");
    }
    #endregion

    #region GetSyntaxTree Tests
    [Fact]
    public void GetSyntaxTreeWithExistingFileReturnsSyntaxTree()
    {
        // Arrange
        var generatedOutput = new Dictionary<string, string>
        {
            { "TestClass.g.cs", "public class TestClass {}" }
        };

        // Act
        var syntaxTree = SourceGeneratorTestHelper.GetSyntaxTree(generatedOutput, "TestClass.g.cs");

        // Assert
        syntaxTree.ShouldNotBeNull();
    }

    [Fact]
    public void GetSyntaxTreeWithNonExistingFileThrowsException()
    {
        // Arrange
        var generatedOutput = new Dictionary<string, string>
        {
            { "TestClass.g.cs", "public class TestClass {}" }
        };

        // Act & Assert
        Should.Throw<Exception>(() =>
            SourceGeneratorTestHelper.GetSyntaxTree(generatedOutput, "NonExistingFile.g.cs"));
    }
    #endregion

    #region RunGeneratorAndCompile Tests
    [Fact]
    public void RunGeneratorAndCompileWithValidSourceAndGeneratorCompilesSuccessfully()
    {
        // Arrange
        var sources = new[]
        {
            TestSources.GenerateCodeAttributeSource, // Include the attribute definition
            TestSources.AttributeClassSource
        };
        var generator = new TestGenerators.MockGenerator();

        // Act
        var (outputCompilation, runResult) = SourceGeneratorTestHelper.RunGeneratorAndCompile(
            generator, sources);

        // Assert
        outputCompilation.ShouldNotBeNull();
        runResult.Results.SelectMany(r => r.GeneratedSources).Count().ShouldBe(2); // Attribute + extension class
        runResult.Diagnostics.ShouldBeEmpty();
    }
    #endregion
}
