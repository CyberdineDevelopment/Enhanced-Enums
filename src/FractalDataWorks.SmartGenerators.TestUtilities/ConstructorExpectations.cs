using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides fluent assertions for validating constructor syntax and structure.
/// </summary>
public class ConstructorExpectations
{
    private readonly ConstructorDeclarationSyntax? _constructorSyntax;
    private readonly bool _isStatic;
    private readonly string _className;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorExpectations"/> class for instance constructors.
    /// </summary>
    /// <param name="constructorSyntax">The constructor syntax to validate.</param>
    /// <param name="className">The name of the containing class.</param>
    public ConstructorExpectations(ConstructorDeclarationSyntax constructorSyntax, string className)
    {
        _constructorSyntax = constructorSyntax ?? throw new ArgumentNullException(nameof(constructorSyntax));
        _className = className ?? throw new ArgumentNullException(nameof(className));
        _isStatic = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorExpectations"/> class.
    /// Initializes a new instance for static constructor validation.
    /// </summary>
    /// <param name="staticConstructorSyntax">The static constructor syntax.</param>
    /// <param name="className">The name of the containing class.</param>
    /// <param name="isStatic">Must be true for static constructors.</param>
    internal ConstructorExpectations(ConstructorDeclarationSyntax? staticConstructorSyntax, string className, bool isStatic)
    {
        _constructorSyntax = staticConstructorSyntax;
        _className = className ?? throw new ArgumentNullException(nameof(className));
        _isStatic = isStatic;
    }

    /// <summary>
    /// Asserts that the constructor is public.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations IsPublic()
    {
        if (_isStatic)
        {
            throw new InvalidOperationException("Static constructors cannot have access modifiers");
        }

        _constructorSyntax.ShouldNotBeNull();
        _constructorSyntax.Modifiers.Any(SyntaxKind.PublicKeyword).ShouldBeTrue(
            $"Expected constructor to be public");
        return this;
    }

    /// <summary>
    /// Asserts that the constructor is private.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations IsPrivate()
    {
        if (_isStatic)
        {
            throw new InvalidOperationException("Static constructors cannot have access modifiers");
        }

        _constructorSyntax.ShouldNotBeNull();
        _constructorSyntax.Modifiers.Any(SyntaxKind.PrivateKeyword).ShouldBeTrue(
            $"Expected constructor to be private");
        return this;
    }

    /// <summary>
    /// Asserts that the constructor is protected.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations IsProtected()
    {
        if (_isStatic)
        {
            throw new InvalidOperationException("Static constructors cannot have access modifiers");
        }

        _constructorSyntax.ShouldNotBeNull();
        _constructorSyntax.Modifiers.Any(SyntaxKind.ProtectedKeyword).ShouldBeTrue(
            $"Expected constructor to be protected");
        return this;
    }

    /// <summary>
    /// Asserts that the constructor is internal.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations IsInternal()
    {
        if (_isStatic)
        {
            throw new InvalidOperationException("Static constructors cannot have access modifiers");
        }

        _constructorSyntax.ShouldNotBeNull();
        _constructorSyntax.Modifiers.Any(SyntaxKind.InternalKeyword).ShouldBeTrue(
            $"Expected constructor to be internal");
        return this;
    }

    /// <summary>
    /// Asserts that the constructor has a specific parameter.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations HasParameter(string parameterName, string parameterType)
    {
        if (_isStatic)
        {
            throw new InvalidOperationException("Static constructors cannot have parameters");
        }

        _constructorSyntax.ShouldNotBeNull();
        var parameter = _constructorSyntax.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, parameterName, StringComparison.Ordinal));

        parameter.ShouldNotBeNull($"Expected constructor to have parameter '{parameterName}'");
        parameter.Type?.ToString().ShouldBe(
            parameterType,
            $"Expected parameter '{parameterName}' to have type '{parameterType}' but found '{parameter.Type}'");

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has a specific parameter with additional validation.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="parameterExpectations">Additional parameter expectations.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations HasParameter(string parameterName, Action<ParameterExpectations> parameterExpectations)
    {
        if (_isStatic)
        {
            throw new InvalidOperationException("Static constructors cannot have parameters");
        }

        _constructorSyntax.ShouldNotBeNull();
        var parameter = _constructorSyntax.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, parameterName, StringComparison.Ordinal));

        parameter.ShouldNotBeNull($"Expected constructor to have parameter '{parameterName}'");

        var expectations = new ParameterExpectations(parameter);
        parameterExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has no parameters.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations HasNoParameters()
    {
        if (_isStatic)
        {
            // Static constructors always have no parameters
            return this;
        }

        _constructorSyntax.ShouldNotBeNull();
        _constructorSyntax.ParameterList.Parameters.Count.ShouldBe(
            0,
            $"Expected constructor to have no parameters but found {_constructorSyntax.ParameterList.Parameters.Count}");

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has a specific number of parameters.
    /// </summary>
    /// <param name="count">The expected parameter count.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations HasParameterCount(int count)
    {
        if (_isStatic && count > 0)
        {
            throw new InvalidOperationException("Static constructors cannot have parameters");
        }

        _constructorSyntax.ShouldNotBeNull();
        var actualCount = _constructorSyntax.ParameterList.Parameters.Count;
        actualCount.ShouldBe(
            count,
            $"Expected constructor to have {count} parameters but found {actualCount}");

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has a body.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations HasBody()
    {
        _constructorSyntax.ShouldNotBeNull();
        (_constructorSyntax.Body != null || _constructorSyntax.ExpressionBody != null).ShouldBeTrue(
            "Expected constructor to have a body");

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has an empty body.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations HasEmptyBody()
    {
        _constructorSyntax.ShouldNotBeNull();
        if (_constructorSyntax.Body != null)
        {
            _constructorSyntax.Body.Statements.Count.ShouldBe(
                0,
                $"Expected constructor to have empty body but found {_constructorSyntax.Body.Statements.Count} statements");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor calls base with specific arguments.
    /// </summary>
    /// <param name="arguments">The expected base call arguments.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations CallsBase(params string[] arguments)
    {
        if (_isStatic)
        {
            throw new InvalidOperationException("Static constructors cannot have base calls");
        }

        _constructorSyntax.ShouldNotBeNull();
        _constructorSyntax.Initializer.ShouldNotBeNull("Expected constructor to call base constructor");
        _constructorSyntax.Initializer.Kind().ShouldBe(
            SyntaxKind.BaseConstructorInitializer,
            "Expected constructor to call base constructor");

        var actualArgs = _constructorSyntax.Initializer.ArgumentList.Arguments.Select(a => a.ToString()).ToArray();
        actualArgs.ShouldBe(
            arguments,
            $"Expected base call with arguments ({string.Join(", ", arguments)}) but found ({string.Join(", ", actualArgs)})");

        return this;
    }

    /// <summary>
    /// Asserts that the constructor calls this with specific arguments.
    /// </summary>
    /// <param name="arguments">The expected this call arguments.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations CallsThis(params string[] arguments)
    {
        if (_isStatic)
        {
            throw new InvalidOperationException("Static constructors cannot have this calls");
        }

        _constructorSyntax.ShouldNotBeNull();
        _constructorSyntax.Initializer.ShouldNotBeNull("Expected constructor to call this constructor");
        _constructorSyntax.Initializer.Kind().ShouldBe(
            SyntaxKind.ThisConstructorInitializer,
            "Expected constructor to call this constructor");

        var actualArgs = _constructorSyntax.Initializer.ArgumentList.Arguments.Select(a => a.ToString()).ToArray();
        actualArgs.ShouldBe(
            arguments,
            $"Expected this call with arguments ({string.Join(", ", arguments)}) but found ({string.Join(", ", actualArgs)})");

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has no initializer (neither base nor this).
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public ConstructorExpectations HasNoInitializer()
    {
        _constructorSyntax.ShouldNotBeNull();
        _constructorSyntax.Initializer.ShouldBeNull("Expected constructor to have no initializer");

        return this;
    }
}
