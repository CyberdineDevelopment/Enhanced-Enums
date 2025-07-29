using System;
using FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.UnitTests;

/// <summary>
/// Tests for <see cref="SyntaxNodeAssertions"/> extension methods.
/// </summary>
public class SyntaxNodeAssertionsTests
{
    #region ShouldContainClass Tests
    [Fact]
    public void ShouldContainClassWhenClassExistsReturnsClassDeclaration()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");

        // Assert
        classDecl.ShouldNotBeNull();
        classDecl.Identifier.ValueText.ShouldBe("SimpleClass");
    }

    [Fact]
    public void ShouldContainClassWhenClassDoesNotExistThrowsException()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);

        // Act & Assert
        Should.Throw<Exception>(() => syntaxTree.ShouldContainClass("NonExistingClass"));
    }
    #endregion

    #region ShouldContainMethod Tests
    [Fact]
    public void ShouldContainMethodWhenMethodExistsReturnsMethodDeclaration()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");

        // Act
        var methodDecl = classDecl.ShouldContainMethod("TestMethod");

        // Assert
        methodDecl.ShouldNotBeNull();
        methodDecl.Identifier.ValueText.ShouldBe("TestMethod");
    }

    [Fact]
    public void ShouldContainMethodWhenMethodDoesNotExistThrowsException()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");

        // Act & Assert
        Should.Throw<Exception>(() => classDecl.ShouldContainMethod("NonExistingMethod"));
    }
    #endregion

    #region ShouldContainProperty Tests
    [Fact]
    public void ShouldContainPropertyWhenPropertyExistsReturnsPropertyDeclaration()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");

        // Act
        var propertyDecl = classDecl.ShouldContainProperty("Name");

        // Assert
        propertyDecl.ShouldNotBeNull();
        propertyDecl.Identifier.ValueText.ShouldBe("Name");
    }

    [Fact]
    public void ShouldContainPropertyWhenPropertyDoesNotExistThrowsException()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");

        // Act & Assert
        Should.Throw<Exception>(() => classDecl.ShouldContainProperty("NonExistingProperty"));
    }
    #endregion

    #region ShouldContainParameter Tests
    [Fact]
    public void ShouldContainParameterWhenParameterExistsReturnsParameterSyntax()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");
        var methodDecl = classDecl.ShouldContainMethod("TestMethod");

        // Act
        var parameter = methodDecl.ShouldContainParameter("input");

        // Assert
        parameter.ShouldNotBeNull();
        parameter.Identifier.ValueText.ShouldBe("input");
    }

    [Fact]
    public void ShouldContainParameterWhenParameterDoesNotExistThrowsException()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");
        var methodDecl = classDecl.ShouldContainMethod("TestMethod");

        // Act & Assert
        Should.Throw<Exception>(() => methodDecl.ShouldContainParameter("nonExistingParam"));
    }
    #endregion

    #region ShouldHaveModifier Tests
    [Fact]
    public void ShouldHaveModifierWhenModifierExistsReturnsOriginalNode()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");

        // Act
        var result = classDecl.ShouldHaveModifier(SyntaxKind.PublicKeyword);

        // Assert
        result.ShouldBe(classDecl); // Verifies it returns the original node
    }

    [Fact]
    public void ShouldHaveModifierWhenModifierDoesNotExistThrowsException()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");

        // Act & Assert
        Should.Throw<Exception>(() => classDecl.ShouldHaveModifier(SyntaxKind.StaticKeyword));
    }
    #endregion

    #region ShouldReturnType Tests
    [Fact]
    public void ShouldReturnTypeWhenReturnTypeMatchesReturnsMethodDeclaration()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");
        var methodDecl = classDecl.ShouldContainMethod("TestMethod");

        // Act
        var result = methodDecl.ShouldReturnType("string");

        // Assert
        result.ShouldBe(methodDecl); // Verifies it returns the original method
    }

    [Fact]
    public void ShouldReturnTypeWhenReturnTypeDoesNotMatchThrowsException()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");
        var methodDecl = classDecl.ShouldContainMethod("TestMethod");

        // Act & Assert
        Should.Throw<Exception>(() => methodDecl.ShouldReturnType("int"));
    }
    #endregion

    #region ShouldHaveType Tests
    [Fact]
    public void ShouldHaveTypeWhenTypeMatchesReturnsParameterSyntax()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");
        var methodDecl = classDecl.ShouldContainMethod("TestMethod");
        var parameter = methodDecl.ShouldContainParameter("input");

        // Act
        var result = parameter.ShouldHaveType("string");

        // Assert
        result.ShouldBe(parameter); // Verifies it returns the original parameter
    }

    [Fact]
    public void ShouldHaveTypeWhenTypeDoesNotMatchThrowsException()
    {
        // Arrange
        var syntaxTree = CSharpSyntaxTree.ParseText(TestSources.SimpleClassSource, cancellationToken: TestContext.Current.CancellationToken);
        var classDecl = syntaxTree.ShouldContainClass("SimpleClass");
        var methodDecl = classDecl.ShouldContainMethod("TestMethod");
        var parameter = methodDecl.ShouldContainParameter("input");

        // Act & Assert
        Should.Throw<Exception>(() => parameter.ShouldHaveType("int"));
    }
    #endregion
}
