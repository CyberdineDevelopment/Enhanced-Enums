using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides fluent assertions for individual enum values.
/// </summary>
public class EnumValueExpectations
{
    private readonly EnumMemberDeclarationSyntax _enumMember;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumValueExpectations"/> class.
    /// </summary>
    /// <param name="enumMember">The enum member to validate.</param>
    public EnumValueExpectations(EnumMemberDeclarationSyntax enumMember)
    {
        _enumMember = enumMember ?? throw new ArgumentNullException(nameof(enumMember));
    }

    /// <summary>
    /// Asserts that the enum value has a specific numeric value.
    /// </summary>
    /// <param name="expectedValue">The expected numeric value.</param>
    /// <returns>This <see cref="EnumValueExpectations"/> instance for method chaining.</returns>
    public EnumValueExpectations HasValue(int expectedValue)
    {
        if (_enumMember.EqualsValue == null)
        {
            throw new ShouldAssertException($"Enum value '{_enumMember.Identifier}' does not have an explicit value assignment");
        }

        var actualValue = _enumMember.EqualsValue.Value.ToString();
        actualValue.ShouldBe(
            expectedValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            $"Expected enum value '{_enumMember.Identifier}' to have value {expectedValue} but was {actualValue}");

        return this;
    }

    /// <summary>
    /// Asserts that the enum value has no explicit value assignment.
    /// </summary>
    /// <returns>This <see cref="EnumValueExpectations"/> instance for method chaining.</returns>
    public EnumValueExpectations HasNoValue()
    {
        _enumMember.EqualsValue.ShouldBeNull(
            $"Expected enum value '{_enumMember.Identifier}' to have no explicit value assignment");
        return this;
    }

    /// <summary>
    /// Asserts that the enum value has a specific attribute.
    /// </summary>
    /// <param name="attributeName">The name of the attribute (without brackets).</param>
    /// <returns>This <see cref="EnumValueExpectations"/> instance for method chaining.</returns>
    public EnumValueExpectations HasAttribute(string attributeName)
    {
        var hasAttribute = _enumMember.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => string.Equals(a.Name.ToString(), attributeName, StringComparison.Ordinal) ||
string.Equals(a.Name.ToString(), attributeName + "Attribute", StringComparison.Ordinal));

        hasAttribute.ShouldBeTrue($"Expected enum value '{_enumMember.Identifier}' to have attribute [{attributeName}]");
        return this;
    }
}
