using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides fluent assertions for validating record syntax and structure.
/// </summary>
public class RecordExpectations
{
    private readonly RecordDeclarationSyntax _recordDeclaration;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordExpectations"/> class.
    /// </summary>
    /// <param name="recordDeclaration">The record declaration to validate.</param>
    public RecordExpectations(RecordDeclarationSyntax recordDeclaration)
    {
        _recordDeclaration = recordDeclaration ?? throw new ArgumentNullException(nameof(recordDeclaration));
    }

    /// <summary>
    /// Asserts that the record is public.
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsPublic()
    {
        _recordDeclaration.ShouldHaveModifier(SyntaxKind.PublicKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the record is private.
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsPrivate()
    {
        _recordDeclaration.ShouldHaveModifier(SyntaxKind.PrivateKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the record is protected.
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsProtected()
    {
        _recordDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the record is internal.
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsInternal()
    {
        _recordDeclaration.ShouldHaveModifier(SyntaxKind.InternalKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the record is partial.
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsPartial()
    {
        _recordDeclaration.ShouldHaveModifier(SyntaxKind.PartialKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the record is sealed.
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsSealed()
    {
        _recordDeclaration.ShouldHaveModifier(SyntaxKind.SealedKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the record is abstract.
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsAbstract()
    {
        _recordDeclaration.ShouldHaveModifier(SyntaxKind.AbstractKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the record has a primary constructor parameter with the specified name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasParameter(string parameterName)
    {
        if (_recordDeclaration.ParameterList == null)
        {
            throw new ShouldAssertException($"Expected record '{_recordDeclaration.Identifier}' to have a primary constructor parameter list");
        }

        var parameter = _recordDeclaration.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, parameterName, StringComparison.Ordinal));

        parameter.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have parameter '{parameterName}'");
        return this;
    }

    /// <summary>
    /// Asserts that the record has a primary constructor parameter with the specified name and type.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasParameter(string parameterName, string parameterType)
    {
        if (_recordDeclaration.ParameterList == null)
        {
            throw new ShouldAssertException($"Expected record '{_recordDeclaration.Identifier}' to have a primary constructor parameter list");
        }

        var parameter = _recordDeclaration.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, parameterName, StringComparison.Ordinal));

        parameter.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have parameter '{parameterName}'");
        parameter.Type?.ToString().ShouldBe(
            parameterType,
            $"Expected parameter '{parameterName}' to have type '{parameterType}' but was '{parameter.Type}'");

        return this;
    }

    /// <summary>
    /// Asserts that the record has a primary constructor parameter with the specified name and additional validation.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterExpectations">Additional expectations for the parameter.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasParameter(string parameterName, Action<ParameterExpectations> parameterExpectations)
    {
        if (_recordDeclaration.ParameterList == null)
        {
            throw new ShouldAssertException($"Expected record '{_recordDeclaration.Identifier}' to have a primary constructor parameter list");
        }

        var parameter = _recordDeclaration.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, parameterName, StringComparison.Ordinal));

        parameter.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have parameter '{parameterName}'");

        var expectations = new ParameterExpectations(parameter);
        parameterExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Asserts that the record has a primary constructor parameter with the specified name, type and additional validation.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="parameterExpectations">Additional expectations for the parameter.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasParameter(string parameterName, string parameterType, Action<ParameterExpectations> parameterExpectations)
    {
        if (_recordDeclaration.ParameterList == null)
        {
            throw new ShouldAssertException($"Expected record '{_recordDeclaration.Identifier}' to have a primary constructor parameter list");
        }

        var parameter = _recordDeclaration.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, parameterName, StringComparison.Ordinal));

        parameter.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have parameter '{parameterName}'");
        parameter.Type?.ToString().ShouldBe(
            parameterType,
            $"Expected parameter '{parameterName}' to have type '{parameterType}' but was '{parameter.Type}'");

        var expectations = new ParameterExpectations(parameter);
        parameterExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Asserts that the record has exactly the specified number of primary constructor parameters.
    /// </summary>
    /// <param name="count">The expected number of parameters.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasParameterCount(int count)
    {
        var actualCount = _recordDeclaration.ParameterList?.Parameters.Count ?? 0;
        actualCount.ShouldBe(
            count,
            $"Expected record '{_recordDeclaration.Identifier}' to have {count} parameters but found {actualCount}");
        return this;
    }

    /// <summary>
    /// Asserts that the record has a method with the specified name.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasMethod(string methodName)
    {
        var method = _recordDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => string.Equals(m.Identifier.Text, methodName, StringComparison.Ordinal));

        method.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have method '{methodName}'");
        return this;
    }

    /// <summary>
    /// Asserts that the record has a method with the specified name and additional validation.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="methodExpectations">Additional expectations for the method.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasMethod(string methodName, Action<MethodExpectations> methodExpectations)
    {
        var method = _recordDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => string.Equals(m.Identifier.Text, methodName, StringComparison.Ordinal));

        method.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have method '{methodName}'");

        var expectations = new MethodExpectations(method);
        methodExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Asserts that the record has a property with the specified name.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasProperty(string propertyName)
    {
        var property = _recordDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, propertyName, StringComparison.Ordinal));

        property.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have property '{propertyName}'");
        return this;
    }

    /// <summary>
    /// Asserts that the record has a property with the specified name and additional validation.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyExpectations">Additional expectations for the property.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasProperty(string propertyName, Action<PropertyExpectations> propertyExpectations)
    {
        var property = _recordDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, propertyName, StringComparison.Ordinal));

        property.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have property '{propertyName}'");

        var expectations = new PropertyExpectations(property);
        propertyExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Asserts that the record is a record class (not a record struct).
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsRecordClass()
    {
        var isNone = _recordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.None);
        var isClass = _recordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword);

        (isNone || isClass).ShouldBeTrue(
            $"Expected record '{_recordDeclaration.Identifier}' to be a record class");

        return this;
    }

    /// <summary>
    /// Asserts that the record is a record struct.
    /// </summary>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations IsRecordStruct()
    {
        _recordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword).ShouldBeTrue(
            $"Expected record '{_recordDeclaration.Identifier}' to be a record struct");
        return this;
    }

    /// <summary>
    /// Asserts that the record has the specified name.
    /// </summary>
    /// <param name="name">The expected name of the record.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasName(string name)
    {
        _recordDeclaration.Identifier.ValueText.ShouldBe(
            name,
            $"Expected record name to be '{name}' but was '{_recordDeclaration.Identifier.ValueText}'");
        return this;
    }

    /// <summary>
    /// Asserts that the record has the specified base type.
    /// </summary>
    /// <param name="baseTypeName">The name of the base type.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations HasBaseType(string baseTypeName)
    {
        var baseType = _recordDeclaration.BaseList?.Types
            .FirstOrDefault(t => t.Type is IdentifierNameSyntax identifierName &&
string.Equals(identifierName.Identifier.ValueText, baseTypeName, StringComparison.Ordinal));

        baseType.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have base type '{baseTypeName}'");
        return this;
    }

    /// <summary>
    /// Asserts that the record implements the specified interface.
    /// </summary>
    /// <param name="interfaceName">The name of the interface.</param>
    /// <returns>This <see cref="RecordExpectations"/> instance for method chaining.</returns>
    public RecordExpectations ImplementsInterface(string interfaceName)
    {
        var baseList = _recordDeclaration.BaseList;
        baseList.ShouldNotBeNull($"Expected record '{_recordDeclaration.Identifier}' to have a base list with interfaces.");

        var hasInterface = baseList.Types
            .Any(t => string.Equals(t.Type.ToString(), interfaceName, StringComparison.Ordinal));

        hasInterface.ShouldBeTrue($"Expected record '{_recordDeclaration.Identifier}' to implement interface '{interfaceName}'");
        return this;
    }
}
