using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides fluent assertions for validating enum syntax and structure.
/// </summary>
public class EnumExpectations
{
    private readonly EnumDeclarationSyntax _enumDeclaration;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumExpectations"/> class.
    /// </summary>
    /// <param name="enumDeclaration">The enum declaration to validate.</param>
    public EnumExpectations(EnumDeclarationSyntax enumDeclaration)
    {
        _enumDeclaration = enumDeclaration ?? throw new ArgumentNullException(nameof(enumDeclaration));
    }

    /// <summary>
    /// Asserts that the enum is public.
    /// </summary>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations IsPublic()
    {
        _enumDeclaration.ShouldHaveModifier(SyntaxKind.PublicKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the enum is private.
    /// </summary>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations IsPrivate()
    {
        _enumDeclaration.ShouldHaveModifier(SyntaxKind.PrivateKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the enum is protected.
    /// </summary>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations IsProtected()
    {
        _enumDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the enum is internal.
    /// </summary>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations IsInternal()
    {
        _enumDeclaration.ShouldHaveModifier(SyntaxKind.InternalKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the enum has a specific modifier.
    /// </summary>
    /// <param name="modifier">The expected modifier.</param>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasModifier(SyntaxKind modifier)
    {
        _enumDeclaration.ShouldHaveModifier(modifier);
        return this;
    }

    /// <summary>
    /// Asserts that the enum has a specific value member.
    /// </summary>
    /// <param name="valueName">The name of the enum value.</param>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasValue(string valueName)
    {
        var enumMember = _enumDeclaration.Members
            .FirstOrDefault(m => string.Equals(m.Identifier.Text, valueName, StringComparison.Ordinal));

        enumMember.ShouldNotBeNull($"Expected enum '{_enumDeclaration.Identifier}' to have value '{valueName}'");
        return this;
    }

    /// <summary>
    /// Asserts that the enum has a specific value member with an expected numeric value.
    /// </summary>
    /// <param name="valueName">The name of the enum value.</param>
    /// <param name="expectedValue">The expected numeric value.</param>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasValue(string valueName, int expectedValue)
    {
        var enumMember = _enumDeclaration.Members
            .FirstOrDefault(m => string.Equals(m.Identifier.Text, valueName, StringComparison.Ordinal));

        enumMember.ShouldNotBeNull($"Expected enum '{_enumDeclaration.Identifier}' to have value '{valueName}'");

        if (enumMember.EqualsValue != null)
        {
            var actualValue = enumMember.EqualsValue.Value.ToString();
            actualValue.ShouldBe(
                expectedValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
                $"Expected enum value '{valueName}' to have value {expectedValue} but was {actualValue}");
        }
        else
        {
            throw new ShouldAssertException($"Enum value '{valueName}' does not have an explicit value assignment");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the enum has a specific value member with additional validation.
    /// </summary>
    /// <param name="valueName">The name of the enum value.</param>
    /// <param name="valueExpectations">Additional expectations for the enum value.</param>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasValue(string valueName, Action<EnumValueExpectations> valueExpectations)
    {
        var enumMember = _enumDeclaration.Members
            .FirstOrDefault(m => string.Equals(m.Identifier.Text, valueName, StringComparison.Ordinal));

        enumMember.ShouldNotBeNull($"Expected enum '{_enumDeclaration.Identifier}' to have value '{valueName}'");

        var expectations = new EnumValueExpectations(enumMember);
        valueExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Asserts that the enum has exactly the specified number of values.
    /// </summary>
    /// <param name="count">The expected number of enum values.</param>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasValueCount(int count)
    {
        var actualCount = _enumDeclaration.Members.Count;
        actualCount.ShouldBe(
            count,
            $"Expected enum '{_enumDeclaration.Identifier}' to have {count} values but found {actualCount}");
        return this;
    }

    /// <summary>
    /// Asserts that the enum has a specific base type.
    /// </summary>
    /// <param name="baseType">The expected base type (e.g., "int", "long", "byte").</param>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasBaseType(string baseType)
    {
        if (_enumDeclaration.BaseList == null)
        {
            if (string.Equals(baseType, "int", StringComparison.Ordinal) || string.Equals(baseType, "Int32", StringComparison.Ordinal))
            {
                // Default base type is int, so this is acceptable
                return this;
            }

            throw new ShouldAssertException($"Expected enum '{_enumDeclaration.Identifier}' to have base type '{baseType}' but it has no explicit base type");
        }

        var actualBaseType = _enumDeclaration.BaseList.Types.FirstOrDefault()?.Type.ToString();
        actualBaseType.ShouldBe(
            baseType,
            $"Expected enum '{_enumDeclaration.Identifier}' to have base type '{baseType}' but was '{actualBaseType}'");

        return this;
    }

    /// <summary>
    /// Asserts that the enum has the [Flags] attribute.
    /// </summary>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasFlags()
    {
        return HasAttribute("Flags");
    }

    /// <summary>
    /// Asserts that the enum has a specific attribute.
    /// </summary>
    /// <param name="attributeName">The name of the attribute (without brackets).</param>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasAttribute(string attributeName)
    {
        var hasAttribute = _enumDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => string.Equals(a.Name.ToString(), attributeName, StringComparison.Ordinal) ||
string.Equals(a.Name.ToString(), attributeName + "Attribute", StringComparison.Ordinal));

        hasAttribute.ShouldBeTrue($"Expected enum '{_enumDeclaration.Identifier}' to have attribute [{attributeName}]");
        return this;
    }

    /// <summary>
    /// Asserts that the enum has the specified name.
    /// </summary>
    /// <param name="name">The expected name of the enum.</param>
    /// <returns>This <see cref="EnumExpectations"/> instance for method chaining.</returns>
    public EnumExpectations HasName(string name)
    {
        _enumDeclaration.Identifier.ValueText.ShouldBe(
            name,
            $"Expected enum name to be '{name}' but was '{_enumDeclaration.Identifier.ValueText}'");
        return this;
    }
}
