using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides expectations for a parameter declaration.
/// </summary>
public class ParameterExpectations
{
    private readonly ParameterSyntax _parameterSyntax;
    private readonly List<string> _errors = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterExpectations"/> class.
    /// </summary>
    /// <param name="parameterSyntax">The parameter syntax to verify.</param>
    public ParameterExpectations(ParameterSyntax parameterSyntax)
    {
        _parameterSyntax = parameterSyntax ?? throw new ArgumentNullException(nameof(parameterSyntax));
    }

    /// <summary>
    /// Expects the parameter to have the specified type.
    /// </summary>
    /// <param name="typeName">The name of the type.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ParameterExpectations HasType(string typeName)
    {
        if (_parameterSyntax.Type == null)
        {
            _errors.Add($"Parameter '{_parameterSyntax.Identifier.Text}' has no type specified");
        }
        else if (!TypeComparer.AreEquivalent(_parameterSyntax.Type, typeName))
        {
            _errors.Add($"Expected parameter '{_parameterSyntax.Identifier.Text}' to have type '{typeName}' but found '{_parameterSyntax.Type}'");
        }

        return this;
    }

    /// <summary>
    /// Expects the parameter to have the specified default value.
    /// </summary>
    /// <param name="defaultValue">The expected default value.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ParameterExpectations HasDefaultValue(string defaultValue)
    {
        if (_parameterSyntax.Default == null)
        {
            _errors.Add($"Parameter '{_parameterSyntax.Identifier.Text}' does not have a default value");
        }
        else
        {
            var actual = _parameterSyntax.Default.Value.ToString();
            if (!string.Equals(actual, defaultValue, StringComparison.Ordinal))
            {
                _errors.Add($"Expected parameter '{_parameterSyntax.Identifier.Text}' to have default value '{defaultValue}' but found '{actual}'");
            }
        }

        return this;
    }

    /// <summary>
    /// Expects the parameter to have a default value.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ParameterExpectations HasDefaultValue()
    {
        if (_parameterSyntax.Default == null)
        {
            _errors.Add($"Expected parameter '{_parameterSyntax.Identifier.Text}' to have a default value");
        }

        return this;
    }

    /// <summary>
    /// Expects the parameter to have no default value.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ParameterExpectations HasNoDefaultValue()
    {
        if (_parameterSyntax.Default != null)
        {
            _errors.Add($"Expected parameter '{_parameterSyntax.Identifier.Text}' to have no default value but found '{_parameterSyntax.Default.Value}'");
        }

        return this;
    }

    /// <summary>
    /// Expects the parameter to have the ref modifier.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ParameterExpectations IsRef()
    {
        if (!_parameterSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)))
        {
            _errors.Add($"Expected parameter '{_parameterSyntax.Identifier.Text}' to be ref");
        }

        return this;
    }

    /// <summary>
    /// Expects the parameter to have the out modifier.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ParameterExpectations IsOut()
    {
        if (!_parameterSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword)))
        {
            _errors.Add($"Expected parameter '{_parameterSyntax.Identifier.Text}' to be out");
        }

        return this;
    }

    /// <summary>
    /// Expects the parameter to have the in modifier.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ParameterExpectations IsIn()
    {
        if (!_parameterSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.InKeyword)))
        {
            _errors.Add($"Expected parameter '{_parameterSyntax.Identifier.Text}' to be in");
        }

        return this;
    }

    /// <summary>
    /// Expects the parameter to have the params modifier.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ParameterExpectations IsParams()
    {
        if (!_parameterSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamsKeyword)))
        {
            _errors.Add($"Expected parameter '{_parameterSyntax.Identifier.Text}' to be params");
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
            throw new ExpectationException($"Parameter expectations failed:\n{string.Join("\n", _errors)}");
        }
    }
}
