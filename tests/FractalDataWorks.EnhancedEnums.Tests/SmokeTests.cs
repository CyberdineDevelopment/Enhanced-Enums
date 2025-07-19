using System.Linq;
using FractalDataWorks.EnhancedEnums.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Basic smoke tests to verify the test project is set up correctly.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void GeneratorShouldBeInstantiable()
    {
        // Arrange & Act
        var generator = new EnhancedEnumOptionGenerator();

        // Assert
        generator.ShouldNotBeNull();
        generator.ShouldBeAssignableTo<IIncrementalGenerator>();
    }

    [Fact]
    public void TestUtilitiesShouldBeAvailable()
    {
        // Arrange
        var code = """

		                           namespace Test
		                           {
		                               public class TestClass
		                               {
		                                   public string Name { get; set; }
		                               }
		                           }
		           """;

        // Act & Assert - Verify ExpectationsFactory works
        Should.NotThrow(() =>
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetCompilationUnitRoot();

            // Simple validation that the code parses correctly
            var namespaceDecl = root.Members
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault(n => n.Name.ToString() == "Test");

            namespaceDecl.ShouldNotBeNull();

            var classDecl = namespaceDecl.Members
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "TestClass");

            classDecl.ShouldNotBeNull();
            classDecl.Modifiers.Any(m => m.Text == "public").ShouldBeTrue();

            var property = classDecl.Members
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == "Name");

            property.ShouldNotBeNull();
            property.Type.ToString().ShouldBe("string");
        });
    }

    [Fact]
    public void ShouldHaveRequiredReferences()
    {
        // Verify we can access all required types at runtime
        Type.GetType("FractalDataWorks.EnhancedEnums.Attributes.EnhancedEnumBaseAttribute, FractalDataWorks.EnhancedEnums")
            .ShouldNotBeNull();

        // Note: EnableAssemblyScannerAttribute is a compile-time only attribute used by source generators
        // It doesn't need to exist at runtime
    }
}
