using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Extension methods for syntax nodes to aid in testing code generation.
/// </summary>
public static class SyntaxNodeHelpers
{
    /// <summary>
    /// Checks if a syntax node has a specific modifier.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="modifier">The modifier to look for.</param>
    public static void ShouldHaveModifier(this MemberDeclarationSyntax node, SyntaxKind modifier)
    {
        node.Modifiers.Any(m => m.IsKind(modifier)).ShouldBeTrue(
            $"Expected '{node.GetType().Name}' to have modifier '{modifier}'");
    }

    /// <summary>
    /// Finds a method in a class declaration and returns it.
    /// </summary>
    /// <param name="classDeclaration">The class declaration to search in.</param>
    /// <param name="methodName">The name of the method to find.</param>
    /// <returns>The method declaration if found.</returns>
    public static MethodDeclarationSyntax FindMethod(this ClassDeclarationSyntax classDeclaration, string methodName)
    {
        var method = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => string.Equals(m.Identifier.ToString(), methodName, System.StringComparison.Ordinal));

        method.ShouldNotBeNull($"Expected class '{classDeclaration.Identifier}' to contain method '{methodName}'");
        return method;
    }

    /// <summary>
    /// Finds a property in a class declaration and returns it.
    /// </summary>
    /// <param name="classDeclaration">The class declaration to search in.</param>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <returns>The property declaration if found.</returns>
    public static PropertyDeclarationSyntax FindProperty(this ClassDeclarationSyntax classDeclaration, string propertyName)
    {
        var property = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => string.Equals(p.Identifier.ToString(), propertyName, System.StringComparison.Ordinal));

        property.ShouldNotBeNull($"Expected class '{classDeclaration.Identifier}' to contain property '{propertyName}'");
        return property;
    }

    /// <summary>
    /// Finds a field in a class declaration and returns it.
    /// </summary>
    /// <param name="classDeclaration">The class declaration to search in.</param>
    /// <param name="fieldName">The name of the field to find.</param>
    /// <returns>The field declaration if found.</returns>
    public static FieldDeclarationSyntax FindField(this ClassDeclarationSyntax classDeclaration, string fieldName)
    {
        var field = classDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .FirstOrDefault(f => f.Declaration.Variables.Any(v => string.Equals(v.Identifier.ToString(), fieldName, System.StringComparison.Ordinal)));

        field.ShouldNotBeNull($"Expected class '{classDeclaration.Identifier}' to contain field '{fieldName}'");
        return field;
    }

    /// <summary>
    /// Finds a class in a syntax tree and returns it.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to search in.</param>
    /// <param name="className">The name of the class to find.</param>
    /// <returns>The class declaration if found.</returns>
    public static ClassDeclarationSyntax FindClass(this SyntaxTree syntaxTree, string className)
    {
        var classDecl = syntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => string.Equals(c.Identifier.ToString(), className, System.StringComparison.Ordinal));

        classDecl.ShouldNotBeNull($"Expected syntax tree to contain class '{className}'");
        return classDecl;
    }

    /// <summary>
    /// Finds an enum member in an enum declaration and returns it.
    /// </summary>
    /// <param name="enumDeclaration">The enum declaration to search in.</param>
    /// <param name="memberName">The name of the enum member to find.</param>
    /// <returns>The enum member declaration if found.</returns>
    public static EnumMemberDeclarationSyntax FindEnumMember(this EnumDeclarationSyntax enumDeclaration, string memberName)
    {
        var member = enumDeclaration.Members
            .FirstOrDefault(m => string.Equals(m.Identifier.ToString(), memberName, System.StringComparison.Ordinal));

        member.ShouldNotBeNull($"Expected enum '{enumDeclaration.Identifier}' to contain member '{memberName}'");
        return member;
    }

    /// <summary>
    /// Finds an interface in a syntax tree and returns it.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to search in.</param>
    /// <param name="interfaceName">The name of the interface to find.</param>
    /// <returns>The interface declaration if found.</returns>
    public static InterfaceDeclarationSyntax FindInterface(this SyntaxTree syntaxTree, string interfaceName)
    {
        var interfaceDecl = syntaxTree.GetRoot().DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => string.Equals(i.Identifier.ToString(), interfaceName, System.StringComparison.Ordinal));

        interfaceDecl.ShouldNotBeNull($"Expected syntax tree to contain interface '{interfaceName}'");
        return interfaceDecl;
    }
}
