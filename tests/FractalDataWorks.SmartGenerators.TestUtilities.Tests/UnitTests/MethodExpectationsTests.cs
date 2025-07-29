using System;
using System.Linq;
using FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.UnitTests;

/// <summary>
/// Tests for <see cref="MethodExpectations"/> class.
/// </summary>
public class MethodExpectationsTests
{
    #region HasReturnType Tests
    [Fact]
    public void HasReturnTypeWhenReturnTypeMatchesReturnsMethodExpectations()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var methodDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TestMethod");
        var expectations = new MethodExpectations(methodDecl);

        // Act
        var result = expectations.HasReturnType("string");

        // Assert
        result.ShouldBe(expectations); // Verifies fluent interface
    }

    [Fact]
    public void HasReturnTypeWhenReturnTypeDoesNotMatchThrowsException()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var methodDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TestMethod");
        var expectations = new MethodExpectations(methodDecl);

        // Act & Assert
        Should.Throw<Exception>(() => expectations.HasReturnType("int"));
    }
    #endregion

    #region HasParameter Tests
    [Fact]
    public void HasParameterWhenParameterExistsReturnsMethodExpectations()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var methodDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TestMethod");
        var expectations = new MethodExpectations(methodDecl);

        // Act
        var result = expectations.HasParameter("input");

        // Assert
        result.ShouldBe(expectations); // Verifies fluent interface
    }

    [Fact]
    public void HasParameterWithTypeWhenParameterAndTypeMatchReturnsMethodExpectations()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var methodDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TestMethod");
        var expectations = new MethodExpectations(methodDecl);

        // Act
        var result = expectations.HasParameter("input", "string");

        // Assert
        result.ShouldBe(expectations); // Verifies fluent interface
    }

    [Fact]
    public void HasParameterWhenParameterDoesNotExistThrowsException()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var methodDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TestMethod");
        var expectations = new MethodExpectations(methodDecl);

        // Act & Assert
        Should.Throw<Exception>(() => expectations.HasParameter("nonExistingParam"));
    }

    [Fact]
    public void HasParameterWithTypeWhenTypeDoesNotMatchThrowsException()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var methodDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TestMethod");
        var expectations = new MethodExpectations(methodDecl);

        // Act & Assert
        Should.Throw<Exception>(() => expectations.HasParameter("input", "int"));
    }
    #endregion

    #region Modifier Tests
    [Fact]
    public void IsPublicWhenMethodIsPublicReturnsMethodExpectations()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var methodDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TestMethod");
        var expectations = new MethodExpectations(methodDecl);

        // Act
        var result = expectations.IsPublic();

        // Assert
        result.ShouldBe(expectations); // Verifies fluent interface
    }

    [Fact]
    public void IsStaticWhenMethodIsNotStaticThrowsException()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource; // TestMethod is not static
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var methodDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TestMethod");
        var expectations = new MethodExpectations(methodDecl);

        // Act & Assert
        Should.Throw<Exception>(() => expectations.IsStatic());
    }
    #endregion
}
