using System;
using FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.UnitTests;

/// <summary>
/// Tests for <see cref="ExpectationsFactory"/> class.
/// </summary>
public class ExpectationsFactoryTests
{
    #region Expect Tests
    [Fact]
    public void ExpectWithValidSyntaxTreeCreatesExpectationsObject()
    {
        // Arrange
        var sourceText = TestSources.SimpleClassSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var expectations = ExpectationsFactory.Expect(syntaxTree);

        // Assert
        expectations.ShouldNotBeNull();
        expectations.ShouldBeOfType<SyntaxTreeExpectations>();
    }

    [Fact]
    public void ExpectWithNullSyntaxTreeThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => ExpectationsFactory.Expect(syntaxTree: null!));
    }
    #endregion

}
