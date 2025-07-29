using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.UnitTests;

/// <summary>
/// Tests for <see cref="TestSourceProvider"/> class.
/// </summary>
public class TestSourceProviderTests
{
    #region CreateClassSource Tests
    [Fact]
    public void CreateClassSourceWithoutAttributesGeneratesBasicClass()
    {
        // Arrange
        const string className = "TestClass";
        const string namespaceName = "TestNamespace";

        // Act
        var source = TestSourceProvider.CreateClassSource(className, namespaceName);

        // Assert
        // Parse the generated source and assert class presence structurally
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
        syntaxTree.ShouldContainClass(className);

        // Namespace is a file-scoped declaration, so check for its presence as text
        syntaxTree.GetRoot(TestContext.Current.CancellationToken).ToFullString().ShouldContain($"namespace {namespaceName};");

        // No attributes should be present (string check is sufficient here)
        syntaxTree.GetRoot(TestContext.Current.CancellationToken).ToFullString().ShouldNotContain("[");
    }

    [Fact]
    public void CreateClassSourceWithAttributesIncludesAttributes()
    {
        // Arrange
        const string className = "TestClass";
        const string namespaceName = "TestNamespace";
        var attributes = new[] { "Serializable", "GenerateCode" };

        // Act
        var source = TestSourceProvider.CreateClassSource(className, namespaceName, attributes);

        // Assert
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
        syntaxTree.ShouldContainClass(className);
        syntaxTree.GetRoot(TestContext.Current.CancellationToken).ToFullString().ShouldContain($"namespace {namespaceName};");

        // Attribute presence: string-based check retained as no syntax helper is available for arbitrary attribute names
        syntaxTree.GetRoot(TestContext.Current.CancellationToken).ToFullString().ShouldContain("[Serializable]");
        syntaxTree.GetRoot(TestContext.Current.CancellationToken).ToFullString().ShouldContain("[GenerateCode]");
    }
    #endregion

    #region CreateEnumSource Tests
    [Fact]
    public void CreateEnumSourceWithValuesGeneratesEnum()
    {
        // Arrange
        const string enumName = "TestEnum";
        const string namespaceName = "TestNamespace";
        var values = new[] { "First", "Second", "Third" };

        // Act
        var source = TestSourceProvider.CreateEnumSource(enumName, namespaceName, values);

        // Assert
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
        syntaxTree.ShouldContainEnum(enumName);
        syntaxTree.GetRoot(TestContext.Current.CancellationToken).ToFullString().ShouldContain($"namespace {namespaceName};");

        // Enum values: string-based check retained as no syntax helper is available for arbitrary values
        foreach (var value in values)
        {
            syntaxTree.GetRoot(TestContext.Current.CancellationToken).ToFullString().ShouldContain(value);
        }
    }

    [Fact]
    public void CreateEnumSourceWithoutValuesGeneratesEmptyEnum()
    {
        // Arrange
        const string enumName = "TestEnum";
        const string namespaceName = "TestNamespace";

        // Act
        var source = TestSourceProvider.CreateEnumSource(enumName, namespaceName);

        // Assert
        source.ShouldContain($"namespace {namespaceName};");
        source.ShouldContain($"public enum {enumName}");

        // The enum should be empty, but still valid syntax
    }
    #endregion
}
