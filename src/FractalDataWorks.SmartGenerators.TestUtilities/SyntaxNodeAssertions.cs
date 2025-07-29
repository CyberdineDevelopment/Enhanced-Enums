using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides assertions for syntax nodes.
/// </summary>
public static class SyntaxNodeAssertions
{
    /// <summary>
    /// Asserts that a syntax tree contains a class with the specified name.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree.</param>
    /// <param name="className">The class name.</param>
    /// <returns>The class declaration syntax.</returns>
    public static ClassDeclarationSyntax ShouldContainClass(this SyntaxTree syntaxTree, string className)
    {
        var root = syntaxTree.GetRoot();
        var classDeclaration = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => string.Equals(c.Identifier.ValueText, className, System.StringComparison.Ordinal));

        _ = classDeclaration.ShouldNotBeNull($"Class {className} was not found in the syntax tree.");

        return classDeclaration;
    }

    /// <summary>
    /// Asserts that a syntax tree contains an enum with the specified name.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to search.</param>
    /// <param name="enumName">The name of the enum to find.</param>
    /// <returns>The enum declaration syntax.</returns>
    public static EnumDeclarationSyntax ShouldContainEnum(this SyntaxTree syntaxTree, string enumName)
    {
        var root = syntaxTree.GetRoot();
        var enumDeclaration = root.DescendantNodes()
            .OfType<EnumDeclarationSyntax>()
            .FirstOrDefault(e => string.Equals(e.Identifier.ValueText, enumName, System.StringComparison.Ordinal));

        _ = enumDeclaration.ShouldNotBeNull($"Enum {enumName} was not found in the syntax tree.");
        return enumDeclaration;
    }

    /// <summary>
    /// Asserts that a class declaration has a method with the specified name.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="methodName">The method name.</param>
    /// <returns>The method declaration syntax.</returns>
    public static MethodDeclarationSyntax ShouldContainMethod(this ClassDeclarationSyntax classDeclaration, string methodName)
    {
        var methodDeclaration = classDeclaration.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => string.Equals(m.Identifier.ValueText, methodName, System.StringComparison.Ordinal));

        _ = methodDeclaration.ShouldNotBeNull($"Method {methodName} was not found in class {classDeclaration.Identifier.ValueText}.");

        return methodDeclaration;
    }

    /// <summary>
    /// Asserts that a class declaration has a property with the specified name.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property declaration syntax.</returns>
    public static PropertyDeclarationSyntax ShouldContainProperty(this ClassDeclarationSyntax classDeclaration, string propertyName)
    {
        var propertyDeclaration = classDeclaration.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => string.Equals(p.Identifier.ValueText, propertyName, System.StringComparison.Ordinal));

        _ = propertyDeclaration.ShouldNotBeNull($"Property {propertyName} was not found in class {classDeclaration.Identifier.ValueText}.");

        return propertyDeclaration;
    }

    /// <summary>
    /// Asserts that a method declaration has a parameter with the specified name.
    /// </summary>
    /// <param name="methodDeclaration">The method declaration.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The parameter syntax.</returns>
    public static ParameterSyntax ShouldContainParameter(this MethodDeclarationSyntax methodDeclaration, string parameterName)
    {
        var parameter = methodDeclaration.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.ValueText, parameterName, System.StringComparison.Ordinal));

        _ = parameter.ShouldNotBeNull($"Parameter {parameterName} was not found in method {methodDeclaration.Identifier.ValueText}.");

        return parameter;
    }

    /// <summary>
    /// Asserts that a node has a modifier of the specified kind.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="modifierKind">The modifier kind.</param>
    /// <typeparam name="T">The type of the syntax node being checked.</typeparam>
    /// <returns>The node for chaining.</returns>
    public static T ShouldHaveModifier<T>(this T node, SyntaxKind modifierKind)
        where T : MemberDeclarationSyntax
    {
        node.Modifiers.Any(m => m.IsKind(modifierKind))
            .ShouldBeTrue($"{node.GetType().Name} should have {modifierKind} modifier.");

        return node;
    }

    /// <summary>
    /// Asserts that a method declaration returns a specific type.
    /// </summary>
    /// <param name="methodDeclaration">The method declaration.</param>
    /// <param name="typeName">The type name.</param>
    /// <returns>The method declaration for chaining.</returns>
    public static MethodDeclarationSyntax ShouldReturnType(this MethodDeclarationSyntax methodDeclaration, string typeName)
    {
        if (!TypeComparer.AreEquivalent(methodDeclaration.ReturnType, typeName))
        {
            throw new ShouldAssertException(
                $"Method {methodDeclaration.Identifier.ValueText} should return {typeName} but returns {methodDeclaration.ReturnType}.");
        }

        return methodDeclaration;
    }

    /// <summary>
    /// Asserts that a parameter has a specific type.
    /// </summary>
    /// <param name="parameter">The parameter syntax.</param>
    /// <param name="typeName">The expected type name.</param>
    /// <returns>The parameter syntax for chaining.</returns>
    public static ParameterSyntax ShouldHaveType(this ParameterSyntax parameter, string typeName)
    {
        if (parameter.Type == null)
        {
            throw new ShouldAssertException($"Parameter '{parameter.Identifier.ValueText}' has no type specified.");
        }
        
        if (!TypeComparer.AreEquivalent(parameter.Type, typeName))
        {
            throw new ShouldAssertException(
                $"Parameter '{parameter.Identifier.ValueText}' has type '{parameter.Type}', but expected '{typeName}'.");
        }
        
        return parameter;
    }
}
