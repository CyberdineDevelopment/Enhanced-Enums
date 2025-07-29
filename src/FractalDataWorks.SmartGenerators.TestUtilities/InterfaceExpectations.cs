using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides fluent assertions and expectations for interface declarations.
/// </summary>
public class InterfaceExpectations
{
    private readonly InterfaceDeclarationSyntax _interfaceDeclaration;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceExpectations"/> class.
    /// </summary>
    /// <param name="interfaceDeclaration">The interface declaration to verify.</param>
    public InterfaceExpectations(InterfaceDeclarationSyntax interfaceDeclaration)
    {
        _interfaceDeclaration = interfaceDeclaration ?? throw new ArgumentNullException(nameof(interfaceDeclaration));
    }

    /// <summary>
    /// Expects the interface to be public.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations IsPublic()
    {
        _interfaceDeclaration.ShouldHaveModifier(SyntaxKind.PublicKeyword);
        return this;
    }

    /// <summary>
    /// Expects the interface to be private.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations IsPrivate()
    {
        _interfaceDeclaration.ShouldHaveModifier(SyntaxKind.PrivateKeyword);
        return this;
    }

    /// <summary>
    /// Expects the interface to be protected.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations IsProtected()
    {
        _interfaceDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        return this;
    }

    /// <summary>
    /// Expects the interface to be internal.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations IsInternal()
    {
        _interfaceDeclaration.ShouldHaveModifier(SyntaxKind.InternalKeyword);
        return this;
    }

    /// <summary>
    /// Expects the interface to be partial.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations IsPartial()
    {
        _interfaceDeclaration.ShouldHaveModifier(SyntaxKind.PartialKeyword);
        return this;
    }

    /// <summary>
    /// Expects the interface to have a method with the specified name.
    /// </summary>
    /// <param name="methodName">The name of the method to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasMethod(string methodName)
    {
        var method = _interfaceDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => string.Equals(m.Identifier.Text, methodName, StringComparison.Ordinal));

        method.ShouldNotBeNull($"Expected interface '{_interfaceDeclaration.Identifier}' to have method '{methodName}'");
        return this;
    }

    /// <summary>
    /// Expects the interface to have a method with the specified name and adds more expectations for that method.
    /// </summary>
    /// <param name="methodName">The name of the method to find.</param>
    /// <param name="methodExpectations">A callback to add expectations for the found method.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasMethod(string methodName, Action<MethodExpectations> methodExpectations)
    {
        var method = _interfaceDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => string.Equals(m.Identifier.Text, methodName, StringComparison.Ordinal));

        method.ShouldNotBeNull($"Expected interface '{_interfaceDeclaration.Identifier}' to have method '{methodName}'");

        var methodExp = new MethodExpectations(method);
        methodExpectations(methodExp);

        return this;
    }

    /// <summary>
    /// Expects the interface to have a property with the specified name.
    /// </summary>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasProperty(string propertyName)
    {
        var property = _interfaceDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, propertyName, StringComparison.Ordinal));

        property.ShouldNotBeNull($"Expected interface '{_interfaceDeclaration.Identifier}' to have property '{propertyName}'");
        return this;
    }

    /// <summary>
    /// Expects the interface to have a property with the specified name and adds more expectations for that property.
    /// </summary>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <param name="propertyExpectations">A callback to add expectations for the found property.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasProperty(string propertyName, Action<PropertyExpectations> propertyExpectations)
    {
        var property = _interfaceDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, propertyName, StringComparison.Ordinal));
        property.ShouldNotBeNull($"Expected interface '{_interfaceDeclaration.Identifier}' to have property '{propertyName}'");

        var propertyExp = new PropertyExpectations(property);
        propertyExpectations(propertyExp);

        return this;
    }

    /// <summary>
    /// Expects the interface to extend the specified interface.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to check for.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations ExtendsInterface(string interfaceName)
    {
        var baseList = _interfaceDeclaration.BaseList;
        baseList.ShouldNotBeNull($"Expected interface '{_interfaceDeclaration.Identifier}' to have a base list with interfaces.");

        var hasInterface = baseList.Types
            .Any(t => string.Equals(t.Type.ToString(), interfaceName, StringComparison.Ordinal));

        hasInterface.ShouldBeTrue($"Expected interface '{_interfaceDeclaration.Identifier}' to extend interface '{interfaceName}'.");
        return this;
    }

    /// <summary>
    /// Expects the interface to have a base interface (alias for ExtendsInterface).
    /// </summary>
    /// <param name="interfaceName">The name of the base interface.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasBaseInterface(string interfaceName)
    {
        return ExtendsInterface(interfaceName);
    }

    /// <summary>
    /// Expects the interface to implement/extend multiple interfaces.
    /// </summary>
    /// <param name="interfaceNames">The names of the interfaces to check for.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations ImplementsInterfaces(params string[] interfaceNames)
    {
        foreach (var interfaceName in interfaceNames)
        {
            ExtendsInterface(interfaceName);
        }

        return this;
    }

    /// <summary>
    /// Expects the interface to have a type parameter with the specified name.
    /// </summary>
    /// <param name="typeParameterName">The name of the type parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasTypeParameter(string typeParameterName)
    {
        var typeParameter = _interfaceDeclaration.TypeParameterList?.Parameters
            .FirstOrDefault(tp => string.Equals(tp.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        _ = typeParameter.ShouldNotBeNull($"Expected interface '{_interfaceDeclaration.Identifier}' to have type parameter '{typeParameterName}'");
        return this;
    }

    /// <summary>
    /// Expects the interface to have a type parameter with the specified name and adds more expectations for that type parameter.
    /// </summary>
    /// <param name="typeParameterName">The name of the type parameter.</param>
    /// <param name="typeParameterExpectations">A callback to add expectations for the found type parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasTypeParameter(string typeParameterName, Action<TypeParameterExpectations> typeParameterExpectations)
    {
        var typeParameter = _interfaceDeclaration.TypeParameterList?.Parameters
            .FirstOrDefault(tp => string.Equals(tp.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        typeParameter.ShouldNotBeNull($"Expected interface '{_interfaceDeclaration.Identifier}' to have type parameter '{typeParameterName}'");

        var constraintClause = _interfaceDeclaration.ConstraintClauses
            .FirstOrDefault(cc => string.Equals(cc.Name.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        var typeParamExp = new TypeParameterExpectations(typeParameter, constraintClause);
        typeParameterExpectations(typeParamExp);

        return this;
    }

    /// <summary>
    /// Expects the interface to have multiple type parameters with the specified names.
    /// </summary>
    /// <param name="typeParameterNames">The names of the type parameters.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasTypeParameters(params string[] typeParameterNames)
    {
        foreach (var name in typeParameterNames)
        {
            HasTypeParameter(name);
        }
        return this;
    }

    /// <summary>
    /// Expects the interface to be generic (have type parameters).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations IsGeneric()
    {
        var hasTypeParameters = _interfaceDeclaration.TypeParameterList?.Parameters.Count > 0;
        hasTypeParameters.ShouldBeTrue($"Expected interface '{_interfaceDeclaration.Identifier}' to be generic (have type parameters)");
        return this;
    }

    /// <summary>
    /// Expects the interface to not be generic (have no type parameters).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations IsNotGeneric()
    {
        var hasTypeParameters = _interfaceDeclaration.TypeParameterList?.Parameters.Count > 0;
        hasTypeParameters.ShouldBe(false, $"Expected interface '{_interfaceDeclaration.Identifier}' to not be generic (have no type parameters)");
        return this;
    }

    /// <summary>
    /// Expects the interface to have the specified name.
    /// </summary>
    /// <param name="name">The expected name of the interface.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasName(string name)
    {
        _interfaceDeclaration.Identifier.ValueText
            .ShouldBe(name, $"Expected interface name to be '{name}' but was '{_interfaceDeclaration.Identifier.ValueText}'");
        return this;
    }

    /// <summary>
    /// Expects the interface to have exactly the specified number of members.
    /// </summary>
    /// <param name="count">The expected number of members.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasMemberCount(int count)
    {
        var actualCount = _interfaceDeclaration.Members.Count;
        actualCount.ShouldBe(count, $"Expected interface '{_interfaceDeclaration.Identifier}' to have {count} members but found {actualCount}");
        return this;
    }

    /// <summary>
    /// Expects the interface to have no members (empty interface).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasNoMembers()
    {
        return HasMemberCount(0);
    }

    /// <summary>
    /// Expects the interface to have XML documentation containing the specified content.
    /// </summary>
    /// <param name="xmlDocContent">The expected XML documentation content.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public InterfaceExpectations HasXmlDocs(string xmlDocContent)
    {
        var leadingTrivia = _interfaceDeclaration.GetLeadingTrivia();
        var xmlComments = leadingTrivia
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .SelectMany(t => t.GetStructure()
                ?.DescendantNodes()
                .OfType<XmlTextSyntax>()
                .SelectMany(x => x.TextTokens.Select(tt => tt.ToString().Trim())))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        xmlComments.ShouldContain(
            xmlDocContent,
            $"Expected interface '{_interfaceDeclaration.Identifier}' to have XML documentation containing '{xmlDocContent}'");

        return this;
    }
}
