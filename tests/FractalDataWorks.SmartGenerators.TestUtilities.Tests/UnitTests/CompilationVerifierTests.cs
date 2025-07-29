using System;
using FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.UnitTests;

/// <summary>
/// Tests for <see cref="CompilationVerifier"/> class.
/// </summary>
public class CompilationVerifierTests
{
    #region CompileAndVerify Tests
    [Fact]
    public void CompileAndVerifyWithValidSourceCompilesSuccessfully()
    {
        // Arrange
        var sources = new[]
        {
            @"
            namespace Test;
            
            public class TestClass
            {
                public string TestMethod()
                {
                    return ""Hello World"";
                }
            }"
        };

        // Act & Assert
        Should.NotThrow(() =>
        {
            var assembly = CompilationVerifier.CompileAndVerify(sources);
            assembly.ShouldNotBeNull();
        });
    }

    [Fact]
    public void CompileAndVerifyWithInvalidSourceThrowsException()
    {
        // Arrange
        var sources = new[]
        {
            @"
            namespace Test;
            
            public class TestClass
            {
                public string TestMethod()
                {
                    return 123; // Type mismatch error
                }
            }"
        };

        // Act & Assert
        Should.Throw<Exception>(() => CompilationVerifier.CompileAndVerify(sources));
    }
    #endregion

    #region CompileWithSourceGenerator Tests
    [Fact]
    public void CompileWithSourceGeneratorWithValidSourceAndGeneratorCompilesSuccessfully()
    {
        // Arrange
        var sources = new[] { TestSources.AttributeClassSource };
        var generator = new TestGenerators.MockGenerator();

        // Act
        var (assembly, diagnostics) = CompilationVerifier.CompileWithSourceGenerator(sources, generator);

        // Assert
        assembly.ShouldNotBeNull();
        diagnostics.ShouldNotContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void CompileWithSourceGeneratorWithInvalidSourceReturnsNullAssemblyAndErrors()
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
        var (assembly, diagnostics) = CompilationVerifier.CompileWithSourceGenerator(sources, generator);

        // Assert
        assembly.ShouldBeNull();
        diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }
    #endregion

    #region InvokeMethod Tests
    [Fact]
    public void InvokeMethodWithValidTypeAndMethodReturnsExpectedResult()
    {
        // Arrange
        var sources = new[]
        {
            @"
            namespace Test;
            
            public class Calculator
            {
                public static int Add(int a, int b)
                {
                    return a + b;
                }
            }"
        };
        var assembly = CompilationVerifier.CompileAndVerify(sources);

        // Act
        var result = CompilationVerifier.InvokeMethod(
            assembly,
            "Test.Calculator",
            "Add",
            3,
            4);

        // Assert
        result.ShouldBe(7);
    }

    [Fact]
    public void InvokeMethodWithNonExistingTypeThrowsException()
    {
        // Arrange
        var sources = new[]
        {
            @"
            namespace Test;
            
            public class Calculator
            {
                public static int Add(int a, int b)
                {
                    return a + b;
                }
            }"
        };
        var assembly = CompilationVerifier.CompileAndVerify(sources);

        // Act & Assert
        Should.Throw<Exception>(() => CompilationVerifier.InvokeMethod(
            assembly,
            "Test.NonExistingClass",
            "Add",
            3,
            4));
    }

    [Fact]
    public void InvokeMethodWithNonExistingMethodThrowsException()
    {
        // Arrange
        var sources = new[]
        {
            """
            
                        namespace Test;
                        
                        public class Calculator
                        {
                            public static int Add(int a, int b)
                            {
                                return a + b;
                            }
                        }
            """
        };
        var assembly = CompilationVerifier.CompileAndVerify(sources);

        // Act & Assert
        Should.Throw<Exception>(() => CompilationVerifier.InvokeMethod(
            assembly,
            "Test.Calculator",
            "Subtract",
            7,
            4));
    }
    #endregion
}
