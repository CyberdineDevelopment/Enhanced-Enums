using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides fluent assertions and expectations for type parameter declarations.
/// </summary>
public class TypeParameterExpectations
{
    private readonly TypeParameterSyntax _typeParameter;
    private readonly TypeParameterConstraintClauseSyntax? _constraintClause;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeParameterExpectations"/> class.
    /// </summary>
    /// <param name="typeParameter">The type parameter to verify.</param>
    /// <param name="constraintClause">The constraint clause associated with this type parameter, if any.</param>
    public TypeParameterExpectations(TypeParameterSyntax typeParameter, TypeParameterConstraintClauseSyntax? constraintClause = null)
    {
        _typeParameter = typeParameter ?? throw new ArgumentNullException(nameof(typeParameter));
        _constraintClause = constraintClause;
    }

    /// <summary>
    /// Expects the type parameter to have the specified name.
    /// </summary>
    /// <param name="name">The expected name of the type parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public TypeParameterExpectations HasName(string name)
    {
        _typeParameter.Identifier.ValueText
            .ShouldBe(name, $"Expected type parameter name to be '{name}' but was '{_typeParameter.Identifier.ValueText}'");
        return this;
    }

    /// <summary>
    /// Expects the type parameter to have a constraint of the specified type.
    /// </summary>
    /// <param name="constraintType">The constraint type (e.g., "class", "struct", "new()", or a specific type name).</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public TypeParameterExpectations WithConstraint(string constraintType)
    {
        _constraintClause.ShouldNotBeNull($"Expected type parameter '{_typeParameter.Identifier.ValueText}' to have constraints");

        var hasConstraint = _constraintClause.Constraints
            .Any(c => string.Equals(c.ToString(), constraintType, StringComparison.Ordinal));

        hasConstraint.ShouldBeTrue($"Expected type parameter '{_typeParameter.Identifier.ValueText}' to have constraint '{constraintType}'");
        return this;
    }

    /// <summary>
    /// Expects the type parameter to have multiple constraints.
    /// </summary>
    /// <param name="constraintTypes">The constraint types.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public TypeParameterExpectations WithConstraints(params string[] constraintTypes)
    {
        foreach (var constraint in constraintTypes)
        {
            WithConstraint(constraint);
        }
        return this;
    }

    /// <summary>
    /// Expects the type parameter to have a class constraint.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public TypeParameterExpectations WithClassConstraint()
    {
        return WithConstraint("class");
    }

    /// <summary>
    /// Expects the type parameter to have a struct constraint.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public TypeParameterExpectations WithStructConstraint()
    {
        return WithConstraint("struct");
    }

    /// <summary>
    /// Expects the type parameter to have a new() constraint.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public TypeParameterExpectations WithNewConstraint()
    {
        return WithConstraint("new()");
    }

    /// <summary>
    /// Expects the type parameter to have no constraints.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public TypeParameterExpectations WithNoConstraints()
    {
        if (_constraintClause != null)
        {
            var constraintCount = _constraintClause.Constraints.Count;
            constraintCount.ShouldBe(0, $"Expected type parameter '{_typeParameter.Identifier.ValueText}' to have no constraints but found {constraintCount}");
        }
        return this;
    }
}