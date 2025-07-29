using System;
using FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.UnitTests;

/// <summary>
/// Tests for <see cref="SyntaxTreeExpectations"/> class.
/// </summary>
public class SyntaxTreeExpectationsTests
{
    #region HasClass Tests
    [Fact]
    public void HasClassWhenClassExistsPassesVerification()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var expectations = new SyntaxTreeExpectations(syntaxTree);

        // Act & Assert
        Should.NotThrow(() =>
        {
            expectations.HasClass("SimpleClass").Verify();
        });
    }

    [Fact]
    public void HasClassWhenClassDoesNotExistFailsVerification()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var expectations = new SyntaxTreeExpectations(syntaxTree);

        // Act & Assert
        Should.Throw<Exception>(() =>
        {
            expectations.HasClass("NonExistingClass").Verify();
        });
    }

    [Fact]
    public void HasClassWithExpectationCallbackExecutesCallback()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var expectations = new SyntaxTreeExpectations(syntaxTree);
        var callbackExecuted = false;

        // Act & Assert
        Should.NotThrow(() =>
        {
            expectations.HasClass("SimpleClass", classExp =>
            {
                callbackExecuted = true;
                classExp.ShouldNotBeNull();
            }).Verify();
        });

        callbackExecuted.ShouldBeTrue();
    }
    #endregion

    #region HasNamespace Tests
    [Fact]
    public void HasNamespaceWhenNamespaceExistsPassesVerification()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var expectations = new SyntaxTreeExpectations(syntaxTree);

        // Act & Assert
        Should.NotThrow(() =>
        {
            expectations.HasNamespace("TestNamespace").Verify();
        });
    }

    [Fact]
    public void HasNamespaceWhenNamespaceDoesNotExistFailsVerification()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var expectations = new SyntaxTreeExpectations(syntaxTree);

        // Act & Assert
        Should.Throw<Exception>(() =>
        {
            expectations.HasNamespace("NonExistingNamespace").Verify();
        });
    }
    #endregion

    #region Multiple Expectations Tests
    [Fact]
    public void MultipleExpectationsAllPassPassesVerification()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var expectations = new SyntaxTreeExpectations(syntaxTree);

        // Act & Assert
        Should.NotThrow(() =>
        {
            expectations
                .HasNamespace("TestNamespace")
                .HasClass("SimpleClass", classExp => classExp
                    .HasMethod("TestMethod")
                    .HasProperty("Name"))
                .Verify();
        });
    }

    [Fact]
    public void MultipleExpectationsOneFailsFailsVerification()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);
        var expectations = new SyntaxTreeExpectations(syntaxTree);

        // Act & Assert
        Should.Throw<Exception>(() =>
        {
            expectations
                .HasNamespace("TestNamespace")
                .HasClass("SimpleClass")
                .HasClass("NonExistingClass") // This one will fail
                .Verify();
        });
    }
    #endregion
}
