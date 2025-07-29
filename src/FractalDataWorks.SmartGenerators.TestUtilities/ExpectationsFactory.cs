using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Factory class for creating syntax tree expectations.
/// </summary>
public static class ExpectationsFactory
{
    /// <summary>
    /// Creates expectations for a syntax tree.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to verify.</param>
    /// <returns>A new syntax tree expectations instance.</returns>
    public static SyntaxTreeExpectations Expect(SyntaxTree syntaxTree)
    {
        return new SyntaxTreeExpectations(syntaxTree);
    }

    /// <summary>
    /// Creates expectations for a generated code string.
    /// </summary>
    /// <param name="generatedCode">The generated code as a string, typically from CodeBuilder.ToString().</param>
    /// <returns>A new syntax tree expectations instance for the generated code.</returns>
    public static SyntaxTreeExpectations ExpectCode(string generatedCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
        return new SyntaxTreeExpectations(syntaxTree);
    }

    /// <summary>
    /// Expects the syntax tree to contain a namespace with the specified name.
    /// </summary>
    /// <param name="compilationUnit">The compilation unit to check.</param>
    /// <param name="namespaceName">The name of the namespace to find.</param>
    /// <param name="namespaceExpectations">A callback to add expectations for the found namespace.</param>
    /// <returns>True if the namespace is found, otherwise false.</returns>
    public static bool HasNamespace(this CompilationUnitSyntax compilationUnit, string namespaceName, Action<NamespaceExpectations> namespaceExpectations)
    {
        // Check for traditional namespace syntax (with curly braces)
        var regularNamespace = compilationUnit.DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault(n => string.Equals(n.Name.ToString(), namespaceName, StringComparison.Ordinal));

        // Check for newer file-scoped namespace syntax (without curly braces)
        var fileScopedNamespace = compilationUnit.DescendantNodes()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault(n => string.Equals(n.Name.ToString(), namespaceName, StringComparison.Ordinal));

        if (regularNamespace != null)
        {
            var regularNsExp = new NamespaceExpectations(regularNamespace);
            namespaceExpectations(regularNsExp);
            return true;
        }
        else if (fileScopedNamespace != null)
        {
            // Create a wrapper that adapts the file-scoped namespace to the NamespaceExpectations API
            var fileScopedWrapper = new NamespaceExpectations(fileScopedNamespace);
            namespaceExpectations(fileScopedWrapper);
            return true;
        }
        else
        {
            throw new ShouldAssertException($"Expected compilation unit to contain namespace '{namespaceName}'");
        }
    }

    /// <summary>
    /// Expects the syntax tree to contain a class with the specified name.
    /// </summary>
    /// <param name="compilationUnit">The compilation unit to check.</param>
    /// <param name="className">The name of the class to find.</param>
    /// <param name="classExpectations">A callback to add expectations for the found class.</param>
    /// <returns>True if the class is found, otherwise false.</returns>
    public static bool HasClass(this CompilationUnitSyntax compilationUnit, string className, Action<ClassExpectations> classExpectations)
    {
        var classDecl = compilationUnit.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => string.Equals(c.Identifier.ToString(), className, StringComparison.Ordinal));

        _ = classDecl.ShouldNotBeNull($"Expected compilation unit to contain class '{className}'");
        var classExp = new ClassExpectations(classDecl);
        classExpectations(classExp);

        return true;
    }
}
