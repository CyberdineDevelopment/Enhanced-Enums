using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides expectations for a namespace declaration.
/// </summary>
public class NamespaceExpectations
{
    private readonly SyntaxNode _namespaceDeclaration;
    private readonly bool _isFileScoped;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamespaceExpectations"/> class.
    /// </summary>
    /// <param name="namespaceDeclaration">The namespace declaration to verify.</param>
    public NamespaceExpectations(BaseNamespaceDeclarationSyntax namespaceDeclaration)
    {
        _namespaceDeclaration = namespaceDeclaration ?? throw new ArgumentNullException(nameof(namespaceDeclaration));
        _isFileScoped = namespaceDeclaration is FileScopedNamespaceDeclarationSyntax;
    }

    // Constructor overload for file-scoped namespace

    /// <summary>
    /// Initializes a new instance of the <see cref="NamespaceExpectations"/> class.
    /// </summary>
    /// <param name="namespaceDeclaration">The file-scoped namespace declaration to verify.</param>
    public NamespaceExpectations(FileScopedNamespaceDeclarationSyntax namespaceDeclaration)
    {
        _namespaceDeclaration = namespaceDeclaration ?? throw new ArgumentNullException(nameof(namespaceDeclaration));
        _isFileScoped = true;
    }

    /// <summary>
    /// Gets the members of the namespace declaration.
    /// </summary>
    private SyntaxList<MemberDeclarationSyntax> Members
    {
        get
        {
            if (_isFileScoped && _namespaceDeclaration is FileScopedNamespaceDeclarationSyntax fileScoped)
            {
                return fileScoped.Members;
            }
            else if (_namespaceDeclaration is NamespaceDeclarationSyntax regularNamespace)
            {
                return regularNamespace.Members;
            }
            else
            {
                return default(SyntaxList<MemberDeclarationSyntax>);
            }
        }
    }

    /// <summary>
    /// Gets the name of the namespace.
    /// </summary>
    private string Name
    {
        get
        {
            if (_isFileScoped && _namespaceDeclaration is FileScopedNamespaceDeclarationSyntax fileScoped)
            {
                return fileScoped.Name.ToString();
            }
            else if (_namespaceDeclaration is NamespaceDeclarationSyntax regularNamespace)
            {
                return regularNamespace.Name.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Expects the namespace to contain a class with the specified name.
    /// </summary>
    /// <param name="className">The name of the class to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public NamespaceExpectations HasClass(string className)
    {
        var classDecl = Members
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => string.Equals(c.Identifier.ToString(), className, StringComparison.Ordinal));

        classDecl.ShouldNotBeNull($"Expected namespace '{Name}' to contain class '{className}'");
        return this;
    }

    /// <summary>
    /// Expects the namespace to contain a class with the specified name and adds more expectations for that class.
    /// </summary>
    /// <param name="className">The name of the class to find.</param>
    /// <param name="classExpectations">A callback to add expectations for the found class.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public NamespaceExpectations HasClass(string className, Action<ClassExpectations> classExpectations)
    {
        var classDecl = Members
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => string.Equals(c.Identifier.ToString(), className, StringComparison.Ordinal));

        classDecl.ShouldNotBeNull($"Expected namespace '{Name}' to contain class '{className}'");
        var exp = new ClassExpectations(classDecl);
        classExpectations(exp);
        return this;
    }

    /// <summary>
    /// Expects the namespace to contain an interface with the specified name.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public NamespaceExpectations HasInterface(string interfaceName)
    {
        var interfaceDecl = Members
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => string.Equals(i.Identifier.ToString(), interfaceName, StringComparison.Ordinal));

        interfaceDecl.ShouldNotBeNull($"Expected namespace '{Name}' to contain interface '{interfaceName}'");
        return this;
    }

    /// <summary>
    /// Expects the namespace to contain an interface with the specified name and adds more expectations for that interface.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to find.</param>
    /// <param name="interfaceExpectations">A callback to add expectations for the found interface.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public NamespaceExpectations HasInterface(string interfaceName, Action<InterfaceExpectations> interfaceExpectations)
    {
        var interfaceDecl = Members
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => string.Equals(i.Identifier.ToString(), interfaceName, StringComparison.Ordinal));

        interfaceDecl.ShouldNotBeNull($"Expected namespace '{Name}' to contain interface '{interfaceName}'");
        var exp = new InterfaceExpectations(interfaceDecl);
        interfaceExpectations(exp);
        return this;
    }
}
