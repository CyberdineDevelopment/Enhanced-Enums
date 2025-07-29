using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Expectations;

/// <summary>
/// Provides fluent assertions for validating constructor syntax and structure.
/// </summary>
public class ConstructorExpectations
{
    private readonly ConstructorDeclarationSyntax? _constructorSyntax;
    private readonly bool _isStatic;
    private readonly string _className;
    private readonly List<string> _errors = new();

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
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations IsPublic()
    {
        if (_isStatic)
        {
            _errors.Add("Static constructors cannot have access modifiers");
            return this;
        }

        if (_constructorSyntax != null && !_constructorSyntax.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            _errors.Add($"Expected constructor to be public");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor is private.
    /// </summary>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations IsPrivate()
    {
        if (_isStatic)
        {
            _errors.Add("Static constructors cannot have access modifiers");
            return this;
        }

        if (_constructorSyntax != null && !_constructorSyntax.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            _errors.Add($"Expected constructor to be private");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor is protected.
    /// </summary>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations IsProtected()
    {
        if (_isStatic)
        {
            _errors.Add("Static constructors cannot have access modifiers");
            return this;
        }

        if (_constructorSyntax != null && !_constructorSyntax.Modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            _errors.Add($"Expected constructor to be protected");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor is internal.
    /// </summary>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations IsInternal()
    {
        if (_isStatic)
        {
            _errors.Add("Static constructors cannot have access modifiers");
            return this;
        }

        if (_constructorSyntax != null && !_constructorSyntax.Modifiers.Any(SyntaxKind.InternalKeyword))
        {
            _errors.Add($"Expected constructor to be internal");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has a specific parameter.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations HasParameter(string parameterName, string parameterType)
    {
        if (_isStatic)
        {
            _errors.Add("Static constructors cannot have parameters");
            return this;
        }

        if (_constructorSyntax == null)
        {
            _errors.Add($"No constructor found to validate parameters");
            return this;
        }

        var parameter = _constructorSyntax.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, parameterName, StringComparison.Ordinal));

        if (parameter == null)
        {
            _errors.Add($"Expected constructor to have parameter '{parameterName}'");
        }
        else if (!string.Equals(parameter.Type?.ToString(), parameterType, StringComparison.Ordinal))
        {
            _errors.Add($"Expected parameter '{parameterName}' to have type '{parameterType}' but found '{parameter.Type}'");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has a specific parameter with additional validation.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="parameterExpectations">Additional parameter expectations.</param>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations HasParameter(string parameterName, Action<ParameterExpectations> parameterExpectations)
    {
        if (_isStatic)
        {
            _errors.Add("Static constructors cannot have parameters");
            return this;
        }

        if (_constructorSyntax == null)
        {
            _errors.Add($"No constructor found to validate parameters");
            return this;
        }

        var parameter = _constructorSyntax.ParameterList.Parameters
            .FirstOrDefault(p => string.Equals(p.Identifier.Text, parameterName, StringComparison.Ordinal));

        if (parameter == null)
        {
            _errors.Add($"Expected constructor to have parameter '{parameterName}'");
        }
        else
        {
            var expectations = new ParameterExpectations(parameter);
            parameterExpectations(expectations);
            _errors.AddRange(expectations.GetErrors());
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has no parameters.
    /// </summary>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations HasNoParameters()
    {
        if (_isStatic)
        {
            // Static constructors always have no parameters
            return this;
        }

        if (_constructorSyntax != null && _constructorSyntax.ParameterList.Parameters.Count > 0)
        {
            _errors.Add($"Expected constructor to have no parameters but found {_constructorSyntax.ParameterList.Parameters.Count}");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has a specific number of parameters.
    /// </summary>
    /// <param name="count">The expected parameter count.</param>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations HasParameterCount(int count)
    {
        if (_isStatic && count > 0)
        {
            _errors.Add("Static constructors cannot have parameters");
            return this;
        }

        if (_constructorSyntax != null)
        {
            var actualCount = _constructorSyntax.ParameterList.Parameters.Count;
            if (actualCount != count)
            {
                _errors.Add($"Expected constructor to have {count} parameters but found {actualCount}");
            }
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has a body.
    /// </summary>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations HasBody()
    {
        if (_constructorSyntax?.Body == null && _constructorSyntax?.ExpressionBody == null)
        {
            _errors.Add("Expected constructor to have a body");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has an empty body.
    /// </summary>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations HasEmptyBody()
    {
        if (_constructorSyntax?.Body != null && _constructorSyntax.Body.Statements.Count > 0)
        {
            _errors.Add($"Expected constructor to have empty body but found {_constructorSyntax.Body.Statements.Count} statements");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor calls base with specific arguments.
    /// </summary>
    /// <param name="arguments">The expected base call arguments.</param>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations CallsBase(params string[] arguments)
    {
        if (_isStatic)
        {
            _errors.Add("Static constructors cannot have base calls");
            return this;
        }

        if (_constructorSyntax?.Initializer == null || _constructorSyntax.Initializer.Kind() != SyntaxKind.BaseConstructorInitializer)
        {
            _errors.Add("Expected constructor to call base constructor");
            return this;
        }

        var actualArgs = _constructorSyntax.Initializer.ArgumentList.Arguments.Select(a => a.ToString()).ToArray();
        if (!actualArgs.SequenceEqual(arguments))
        {
            _errors.Add($"Expected base call with arguments ({string.Join(", ", arguments)}) but found ({string.Join(", ", actualArgs)})");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor calls this with specific arguments.
    /// </summary>
    /// <param name="arguments">The expected this call arguments.</param>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations CallsThis(params string[] arguments)
    {
        if (_isStatic)
        {
            _errors.Add("Static constructors cannot have this calls");
            return this;
        }

        if (_constructorSyntax?.Initializer == null || _constructorSyntax.Initializer.Kind() != SyntaxKind.ThisConstructorInitializer)
        {
            _errors.Add("Expected constructor to call this constructor");
            return this;
        }

        var actualArgs = _constructorSyntax.Initializer.ArgumentList.Arguments.Select(a => a.ToString()).ToArray();
        if (!actualArgs.SequenceEqual(arguments))
        {
            _errors.Add($"Expected this call with arguments ({string.Join(", ", arguments)}) but found ({string.Join(", ", actualArgs)})");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the constructor has no initializer (neither base nor this).
    /// </summary>
    /// <returns>The current instance for method chainging.</returns>
    public ConstructorExpectations HasNoInitializer()
    {
        if (_constructorSyntax?.Initializer != null)
        {
            _errors.Add("Expected constructor to have no initializer");
        }

        return this;
    }

    /// <summary>
    /// Gets all validation errors.
    /// </summary>
    internal IReadOnlyList<string> GetErrors() => _errors.AsReadOnly();

    /// <summary>
    /// Verifies all expectations, throwing if any are not met.
    /// </summary>
    public void Verify()
    {
        if (_errors.Count > 0)
        {
            throw new ExpectationException($"Constructor expectations failed:\n{string.Join("\n", _errors)}");
        }
    }
}
