using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides expectations for a method declaration.
/// </summary>
public class MethodExpectations
{
    private readonly MethodDeclarationSyntax _methodDeclaration;

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodExpectations"/> class.
    /// </summary>
    /// <param name="methodDeclaration">The method declaration to verify.</param>
    public MethodExpectations(MethodDeclarationSyntax methodDeclaration)
    {
        _methodDeclaration = methodDeclaration ?? throw new ArgumentNullException(nameof(methodDeclaration));
    }

    /// <summary>
    /// Expects the method to have the specified return type.
    /// </summary>
    /// <param name="returnTypeName">The name of the return type.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasReturnType(string returnTypeName)
    {
        _methodDeclaration.ShouldReturnType(returnTypeName);
        return this;
    }

    /// <summary>
    /// Expects the method to have a parameter with the specified name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasParameter(string parameterName)
    {
        _methodDeclaration.ShouldContainParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Expects the method to have a parameter with the specified name and type.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterTypeName">The type of the parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasParameter(string parameterName, string parameterTypeName)
    {
        var parameter = _methodDeclaration.ShouldContainParameter(parameterName);
        parameter.ShouldHaveType(parameterTypeName);
        return this;
    }

    /// <summary>
    /// Expects the method to have a parameter with the specified name and adds more expectations for that parameter.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterExpectations">A callback to add expectations for the found parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasParameter(string parameterName, Action<ParameterExpectations> parameterExpectations)
    {
        var parameter = _methodDeclaration.ShouldContainParameter(parameterName);
        var parameterExp = new ParameterExpectations(parameter);
        parameterExpectations(parameterExp);
        return this;
    }

    /// <summary>
    /// Expects the method to have no parameters.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasNoParameters()
    {
        if (_methodDeclaration.ParameterList.Parameters.Count > 0)
        {
            throw new InvalidOperationException($"Method '{_methodDeclaration.Identifier.Text}' has {_methodDeclaration.ParameterList.Parameters.Count} parameters, but expected none.");
        }

        return this;
    }

    /// <summary>
    /// Expects the method to be public.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations IsPublic()
    {
        _methodDeclaration.ShouldHaveModifier(SyntaxKind.PublicKeyword);
        return this;
    }

    /// <summary>
    /// Expects the method to be protected.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations IsProtected()
    {
        _methodDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        return this;
    }

    /// <summary>
    /// Expects the method to be static.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations IsStatic()
    {
        _methodDeclaration.ShouldHaveModifier(SyntaxKind.StaticKeyword);
        return this;
    }

    /// <summary>
    /// Expects the method to be marked as override.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations IsOverride()
    {
        _methodDeclaration.ShouldHaveModifier(SyntaxKind.OverrideKeyword);
        return this;
    }

    /// <summary>
    /// Expects the method to be marked as async.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations Is()
    {
        var hasAsyncModifier = _methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        hasAsyncModifier.ShouldBeTrue($"Expected method '{_methodDeclaration.Identifier.Text}' to be async");
        return this;
    }

    /// <summary>
    /// Expects the method to have no generic type parameters.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasNoGenericTypeParameters()
    {
        if (_methodDeclaration.TypeParameterList != null && _methodDeclaration.TypeParameterList.Parameters.Count > 0)
        {
            throw new Shouldly.ShouldAssertException($"Method '{_methodDeclaration.Identifier.Text}' has {_methodDeclaration.TypeParameterList.Parameters.Count} generic type parameters, but expected none.");
        }

        return this;
    }

    /// <summary>
    /// Expects the method to have a type parameter with the specified name.
    /// </summary>
    /// <param name="typeParameterName">The name of the type parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasTypeParameter(string typeParameterName)
    {
        var typeParameter = _methodDeclaration.TypeParameterList?.Parameters
            .FirstOrDefault(tp => string.Equals(tp.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        _ = typeParameter.ShouldNotBeNull($"Expected method '{_methodDeclaration.Identifier.Text}' to have type parameter '{typeParameterName}'");
        return this;
    }

    /// <summary>
    /// Expects the method to have a type parameter with the specified name and adds more expectations for that type parameter.
    /// </summary>
    /// <param name="typeParameterName">The name of the type parameter.</param>
    /// <param name="typeParameterExpectations">A callback to add expectations for the found type parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasTypeParameter(string typeParameterName, Action<TypeParameterExpectations> typeParameterExpectations)
    {
        var typeParameter = _methodDeclaration.TypeParameterList?.Parameters
            .FirstOrDefault(tp => string.Equals(tp.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        typeParameter.ShouldNotBeNull($"Expected method '{_methodDeclaration.Identifier.Text}' to have type parameter '{typeParameterName}'");

        var constraintClause = _methodDeclaration.ConstraintClauses
            .FirstOrDefault(cc => string.Equals(cc.Name.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        var typeParamExp = new TypeParameterExpectations(typeParameter, constraintClause);
        typeParameterExpectations(typeParamExp);

        return this;
    }

    /// <summary>
    /// Expects the method to have multiple type parameters with the specified names.
    /// </summary>
    /// <param name="typeParameterNames">The names of the type parameters.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasTypeParameters(params string[] typeParameterNames)
    {
        foreach (var name in typeParameterNames)
        {
            HasTypeParameter(name);
        }
        return this;
    }

    /// <summary>
    /// Expects the method to be generic (have type parameters).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations IsGeneric()
    {
        var hasTypeParameters = _methodDeclaration.TypeParameterList?.Parameters.Count > 0;
        hasTypeParameters.ShouldBeTrue($"Expected method '{_methodDeclaration.Identifier.Text}' to be generic (have type parameters)");
        return this;
    }

    /// <summary>
    /// Expects the method to not be generic (have no type parameters).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations IsNotGeneric()
    {
        var hasTypeParameters = _methodDeclaration.TypeParameterList?.Parameters.Count > 0;
        hasTypeParameters.ShouldBe(false, $"Expected method '{_methodDeclaration.Identifier.Text}' to not be generic (have no type parameters)");
        return this;
    }

    /// <summary>
    /// Expects the method to have a body.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasBody()
    {
        _methodDeclaration.Body.ShouldNotBeNull($"Method '{_methodDeclaration.Identifier.Text}' does not have a body.");
        return this;
    }

    /// <summary>
    /// Expects the method to have a body and allows further expectations on it.
    /// </summary>
    /// <param name="bodyExpectations">Callback to add expectations for the method's body.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasBody(Action<CodeBlockExpectations> bodyExpectations)
    {
        // Assert body exists
        HasBody();
        var block = _methodDeclaration.Body!;
        var blockExp = new CodeBlockExpectations(block);
        bodyExpectations(blockExp);
        return this;
    }

    /// <summary>
    /// Expects the method to have an expression-bodied member with the specified expression.
    /// </summary>
    /// <param name="expression">The expected expression body.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasExpressionBody(string expression)
    {
        var arrow = _methodDeclaration.ExpressionBody;
        arrow.ShouldNotBeNull($"Method '{_methodDeclaration.Identifier.Text}' does not have an expression body.");
        arrow!.Expression.ToString().ShouldBe(
            expression,
            $"Expected expression body '{expression}' but found '{arrow.Expression.ToString()}'.");
        return this;
    }

    /// <summary>
    /// Expects the method to have the specified name.
    /// </summary>
    /// <param name="name">The expected method name.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public MethodExpectations HasName(string name)
    {
        _methodDeclaration.Identifier.ValueText
            .ShouldBe(name, $"Expected method name to be '{name}' but was '{_methodDeclaration.Identifier.ValueText}'");
        return this;
    }
}
