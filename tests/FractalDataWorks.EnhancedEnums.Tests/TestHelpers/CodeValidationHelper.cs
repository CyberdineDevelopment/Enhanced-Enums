using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.EnhancedEnums.Tests.TestHelpers;

/// <summary>
/// Helper class for validating generated code structure.
/// This is a temporary solution until SyntaxTreeExpectations is available.
/// </summary>
internal static class CodeValidationHelper
{
    public static void ValidateGeneratedCollectionClass(
        string generatedCode,
        string expectedNamespace,
        string expectedClassName,
        string baseTypeName)
    {
        // Parse the code
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
        var root = syntaxTree.GetCompilationUnitRoot();

        // Check usings
        var usings = root.Usings.Select(u => u.Name?.ToString() ?? string.Empty).ToList();
        usings.ShouldContain("System");
        usings.ShouldContain("System.Linq");
        usings.ShouldContain("System.Collections.Generic");
        usings.ShouldContain("System.Collections.Immutable");

        // Find namespace
        var namespaceDecl = root.Members
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault(n => n.Name.ToString() == expectedNamespace);

        if (namespaceDecl == null)
        {
            // Check for file-scoped namespace
            var fileScopedNamespace = root.Members
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault(n => n.Name.ToString() == expectedNamespace);
            fileScopedNamespace.ShouldNotBeNull($"Namespace '{expectedNamespace}' not found");
        }

        // Find class
        var classDecl = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == expectedClassName);

        classDecl.ShouldNotBeNull($"Class '{expectedClassName}' not found");

        // Validate class modifiers
        classDecl.Modifiers.Any(m => m.Text == "public").ShouldBeTrue("Class should be public");
        classDecl.Modifiers.Any(m => m.Text == "static").ShouldBeTrue("Class should be static");

        // Find _all field
        var allField = classDecl.Members
            .OfType<FieldDeclarationSyntax>()
            .FirstOrDefault(f => f.Declaration.Variables.Any(v => v.Identifier.Text == "_all"));

        allField.ShouldNotBeNull("_all field not found");
        allField.Modifiers.Any(m => m.Text == "private").ShouldBeTrue("_all field should be private");
        allField.Modifiers.Any(m => m.Text == "static").ShouldBeTrue("_all field should be static");
        allField.Modifiers.Any(m => m.Text == "readonly").ShouldBeTrue("_all field should be readonly");

        // Find All property
        var allProperty = classDecl.Members
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.Text == "All");

        allProperty.ShouldNotBeNull("All property not found");
        allProperty.Modifiers.Any(m => m.Text == "public").ShouldBeTrue("All property should be public");
        allProperty.Modifiers.Any(m => m.Text == "static").ShouldBeTrue("All property should be static");

        // Find static constructor
        var staticConstructor = classDecl.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault(c => c.Modifiers.Any(m => m.Text == "static"));

        staticConstructor.ShouldNotBeNull("Static constructor not found");
    }

    public static void ValidateNamespaceAndClass(
        string generatedCode,
        string expectedNamespace,
        string expectedClassName,
        Action<ClassDeclarationSyntax>? additionalValidation = null)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
        var root = syntaxTree.GetCompilationUnitRoot();

        // Find class (might be in namespace or at root level)
        var classDecl = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == expectedClassName);

        classDecl.ShouldNotBeNull($"Class '{expectedClassName}' not found");

        if (!string.IsNullOrEmpty(expectedNamespace))
        {
            // Verify it's in the correct namespace
            var containingNamespace = classDecl.Ancestors()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .FirstOrDefault();

            containingNamespace.ShouldNotBeNull($"Class should be in a namespace");
            containingNamespace.Name.ToString().ShouldBe(expectedNamespace);
        }

        additionalValidation?.Invoke(classDecl);
    }
}
