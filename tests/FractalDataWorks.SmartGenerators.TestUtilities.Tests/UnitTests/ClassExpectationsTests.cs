using System;
using System.Linq;
using FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.UnitTests;

/// <summary>
/// Tests for <see cref="ClassExpectations"/> class.
/// </summary>
public class ClassExpectationsTests
{
    #region HasMethod Tests
    [Fact]
    public void HasMethodWhenMethodExistsReturnsClassExpectations()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var expectations = new ClassExpectations(classDecl);

        // Act
        var result = expectations.HasMethod("TestMethod");

        // Assert
        result.ShouldBe(expectations); // Verifies fluent interface
    }

    [Fact]
    public void HasMethodWhenMethodDoesNotExistThrowsException()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var expectations = new ClassExpectations(classDecl);

        // Act & Assert
        Should.Throw<Exception>(() => expectations.HasMethod("NonExistingMethod"));
    }

    [Fact]
    public void HasMethodWithExpectationCallbackExecutesCallback()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var expectations = new ClassExpectations(classDecl);
        var callbackExecuted = false;

        // Act
        var result = expectations.HasMethod("TestMethod", methodExp =>
        {
            callbackExecuted = true;
            methodExp.ShouldNotBeNull();
        });

        // Assert
        result.ShouldBe(expectations); // Verifies fluent interface
        callbackExecuted.ShouldBeTrue();
    }
    #endregion

    #region HasProperty Tests
    [Fact]
    public void HasPropertyWhenPropertyExistsReturnsClassExpectations()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var expectations = new ClassExpectations(classDecl);

        // Act
        var result = expectations.HasProperty("Name");

        // Assert
        result.ShouldBe(expectations); // Verifies fluent interface
    }

    [Fact]
    public void HasPropertyWhenPropertyDoesNotExistThrowsException()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var expectations = new ClassExpectations(classDecl);

        // Act & Assert
        Should.Throw<Exception>(() => expectations.HasProperty("NonExistingProperty"));
    }
    #endregion

    #region Modifier Tests
    [Fact]
    public void IsPublicWhenClassIsPublicReturnsClassExpectations()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var expectations = new ClassExpectations(classDecl);

        // Act
        var result = expectations.IsPublic();

        // Assert
        result.ShouldBe(expectations); // Verifies fluent interface
    }

    [Fact]
    public void IsStaticWhenClassIsNotStaticThrowsException()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource; // SimpleClass is not static
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var expectations = new ClassExpectations(classDecl);

        // Act & Assert
        Should.Throw<Exception>(() => expectations.IsStatic());
    }

    [Fact]
    public void IsPartialWhenClassIsNotPartialThrowsException()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource; // SimpleClass is not partial
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.GetRoot(cancellationToken: TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var expectations = new ClassExpectations(classDecl);

        // Act & Assert
        Should.Throw<Exception>(() => expectations.IsPartial());
    }
    #endregion
}
