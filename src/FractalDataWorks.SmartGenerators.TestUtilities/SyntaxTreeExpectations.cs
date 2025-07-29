using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides a fluent API for creating and validating expectations about syntax trees.
/// </summary>
public class SyntaxTreeExpectations
{
    private readonly SyntaxTree _syntaxTree;
    private readonly List<Action<SyntaxTree>> _expectations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxTreeExpectations"/> class.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to verify.</param>
    public SyntaxTreeExpectations(SyntaxTree syntaxTree)
    {
        _syntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
    }

    /// <summary>
    /// Expects the syntax tree to contain a class with the specified name.
    /// </summary>
    /// <param name="className">The name of the class to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasClass(string className)
    {
        _expectations.Add(tree => SyntaxNodeAssertions.ShouldContainClass(tree, className));
        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain a class with the specified name and adds more expectations for that class.
    /// </summary>
    /// <param name="className">The name of the class to find.</param>
    /// <param name="classExpectations">A callback to add expectations for the found class.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasClass(string className, Action<ClassExpectations> classExpectations)
    {
        _expectations.Add(tree =>
        {
            var classDecl = SyntaxNodeAssertions.ShouldContainClass(tree, className);
            var classExp = new ClassExpectations(classDecl);
            classExpectations(classExp);
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain a namespace with the specified name.
    /// </summary>
    /// <param name="namespaceName">The name of the namespace to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasNamespace(string namespaceName)
    {
        _expectations.Add(tree =>
        {
            // Check for both namespace declaration and file-scoped namespace
            var root = tree.GetRoot();

            var fileScopedNamespace = root.DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault(n => string.Equals(n.Name.ToString(), namespaceName, StringComparison.Ordinal));

            var regularNamespace = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault(n => string.Equals(n.Name.ToString(), namespaceName, StringComparison.Ordinal));

            (fileScopedNamespace != null || regularNamespace != null)
                .ShouldBeTrue($"Expected syntax tree to contain namespace '{namespaceName}'");
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain a namespace with the specified name and adds more expectations for that namespace.
    /// </summary>
    /// <param name="namespaceName">The name of the namespace to find.</param>
    /// <param name="namespaceExpectations">A callback to add expectations for the found namespace.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasNamespace(string namespaceName, Action<NamespaceExpectations> namespaceExpectations)
    {
        _expectations.Add(tree =>
        {
            // Check for both namespace declaration and file-scoped namespace
            var root = tree.GetRoot();

            var fileScopedNamespace = root.DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault(n => string.Equals(n.Name.ToString(), namespaceName, StringComparison.Ordinal));

            var regularNamespace = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault(n => string.Equals(n.Name.ToString(), namespaceName, StringComparison.Ordinal));

            if (fileScopedNamespace != null)
            {
                var nsExp = new NamespaceExpectations(fileScopedNamespace);
                namespaceExpectations(nsExp);
            }
            else if (regularNamespace != null)
            {
                var nsExp = new NamespaceExpectations(regularNamespace);
                namespaceExpectations(nsExp);
            }
            else
            {
                throw new ShouldAssertException($"Expected syntax tree to contain namespace '{namespaceName}'");
            }
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain a using directive.
    /// </summary>
    /// <param name="usingDirective">The using directive to find (e.g., "System", "System.Linq", "using System").</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasUsing(string usingDirective)
    {
        _expectations.Add(tree =>
        {
            // Normalize the input (remove "using" if present and trim)
            var normalizedUsing = usingDirective.Trim();
            if (normalizedUsing.StartsWith("using ", StringComparison.Ordinal))
            {
                normalizedUsing = normalizedUsing.Substring(6).Trim();
            }

            // Remove semicolon if present
            normalizedUsing = normalizedUsing.TrimEnd(';');

            var root = tree.GetRoot() as CompilationUnitSyntax;
            root.ShouldNotBeNull("Expected compilation unit root");

            var usingDirectives = root.Usings
                .Select(u => u.Name?.ToString())
                .Where(u => u != null);

            usingDirectives.ShouldContain(
                normalizedUsing,
                $"Expected to find using directive '{normalizedUsing}' in the syntax tree");
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain an interface with the specified name.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasInterface(string interfaceName)
    {
        _expectations.Add(tree =>
        {
            var root = tree.GetRoot();
            var interfaceDecl = root.DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .FirstOrDefault(i => string.Equals(i.Identifier.Text, interfaceName, StringComparison.Ordinal));

            interfaceDecl.ShouldNotBeNull($"Expected syntax tree to contain interface '{interfaceName}'");
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain an interface with the specified name and adds more expectations for that interface.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to find.</param>
    /// <param name="interfaceExpectations">A callback to add expectations for the found interface.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasInterface(string interfaceName, Action<InterfaceExpectations> interfaceExpectations)
    {
        _expectations.Add(tree =>
        {
            var root = tree.GetRoot();
            var interfaceDecl = root.DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .FirstOrDefault(i => string.Equals(i.Identifier.Text, interfaceName, StringComparison.Ordinal));

            interfaceDecl.ShouldNotBeNull($"Expected syntax tree to contain interface '{interfaceName}'");

            var interfaceExp = new InterfaceExpectations(interfaceDecl);
            interfaceExpectations(interfaceExp);
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain an enum with the specified name.
    /// </summary>
    /// <param name="enumName">The name of the enum to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasEnum(string enumName)
    {
        _expectations.Add(tree =>
        {
            var root = tree.GetRoot();
            var enumDecl = root.DescendantNodes()
                .OfType<EnumDeclarationSyntax>()
                .FirstOrDefault(e => string.Equals(e.Identifier.Text, enumName, StringComparison.Ordinal));

            enumDecl.ShouldNotBeNull($"Expected syntax tree to contain enum '{enumName}'");
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain an enum with the specified name and adds more expectations for that enum.
    /// </summary>
    /// <param name="enumName">The name of the enum to find.</param>
    /// <param name="enumExpectations">A callback to add expectations for the found enum.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasEnum(string enumName, Action<EnumExpectations> enumExpectations)
    {
        _expectations.Add(tree =>
        {
            var root = tree.GetRoot();
            var enumDecl = root.DescendantNodes()
                .OfType<EnumDeclarationSyntax>()
                .FirstOrDefault(e => string.Equals(e.Identifier.Text, enumName, StringComparison.Ordinal));

            enumDecl.ShouldNotBeNull($"Expected syntax tree to contain enum '{enumName}'");

            var enumExp = new EnumExpectations(enumDecl);
            enumExpectations(enumExp);
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain a record with the specified name.
    /// </summary>
    /// <param name="recordName">The name of the record to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasRecord(string recordName)
    {
        _expectations.Add(tree =>
        {
            var root = tree.GetRoot();
            var recordDecl = root.DescendantNodes()
                .OfType<RecordDeclarationSyntax>()
                .FirstOrDefault(r => string.Equals(r.Identifier.Text, recordName, StringComparison.Ordinal));

            recordDecl.ShouldNotBeNull($"Expected syntax tree to contain record '{recordName}'");
        });

        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain a record with the specified name and adds more expectations for that record.
    /// </summary>
    /// <param name="recordName">The name of the record to find.</param>
    /// <param name="recordExpectations">A callback to add expectations for the found record.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasRecord(string recordName, Action<RecordExpectations> recordExpectations)
    {
        _expectations.Add(tree =>
        {
            var root = tree.GetRoot();
            var recordDecl = root.DescendantNodes()
                .OfType<RecordDeclarationSyntax>()
                .FirstOrDefault(r => string.Equals(r.Identifier.Text, recordName, StringComparison.Ordinal));

            recordDecl.ShouldNotBeNull($"Expected syntax tree to contain record '{recordName}'");

            var recordExp = new RecordExpectations(recordDecl);
            recordExpectations(recordExp);
        });

        return this;
    }

    /// <summary>
    /// Verifies that all expectations are met by the syntax tree.
    /// </summary>
    public void Verify()
    {
        foreach (var expectation in _expectations)
        {
            expectation(_syntaxTree);
        }
    }

    /// <summary>
    /// Alias for Verify, to end fluent expectations.
    /// </summary>
    public void Assert()
    {
        Verify();
    }

    /// <summary>
    /// Expects the syntax tree to contain a method declaration and allows expectations on it.
    /// </summary>
    /// <param name="methodExpectations">A callback to add expectations for the found method.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasMethod(Action<MethodExpectations> methodExpectations)
    {
        _expectations.Add(tree =>
        {
            var root = tree.GetRoot();
            var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDecl == null)
            {
                // Try constructor as method
                var ctor = root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
                ctor.ShouldNotBeNull("Expected method declaration.");
                methodDecl = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    ctor.Identifier)
                    .WithBody(ctor.Body!);
            }

            methodDecl.ShouldNotBeNull("Expected method declaration.");
            var me = new MethodExpectations(methodDecl!);
            methodExpectations(me);
        });
        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain a field declaration and allows expectations on it.
    /// </summary>
    /// <param name="fieldExpectations">A callback to add expectations for the found field.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasField(Action<FieldExpectations> fieldExpectations)
    {
        _expectations.Add(tree =>
        {
            var fieldDecl = tree.GetRoot().DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .FirstOrDefault();
            fieldDecl.ShouldNotBeNull("Expected field declaration.");
            var fe = new FieldExpectations(fieldDecl!);
            fieldExpectations(fe);
        });
        return this;
    }

    /// <summary>
    /// Expects the syntax tree to contain a property declaration and allows expectations on it.
    /// </summary>
    /// <param name="propertyExpectations">A callback to add expectations for the found property.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public SyntaxTreeExpectations HasProperty(Action<PropertyExpectations> propertyExpectations)
    {
        _expectations.Add(tree =>
        {
            var propDecl = tree.GetRoot().DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault();
            propDecl.ShouldNotBeNull("Expected property declaration.");
            var pe = new PropertyExpectations(propDecl!);
            propertyExpectations(pe);
        });
        return this;
    }
}
