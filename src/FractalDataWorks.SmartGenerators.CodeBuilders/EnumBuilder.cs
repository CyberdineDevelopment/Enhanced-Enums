using System;
using System.Collections.Generic;
using System.Text;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating enum declarations.
/// </summary>
public class EnumBuilder : CodeBuilderBase<EnumBuilder>
{
    private string _enumName = "MyEnum";
    private readonly List<string> _members = [];
    private string? _baseType;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumBuilder"/> class.
    /// </summary>
    /// <remarks>This constructor creates an empty <see cref="EnumBuilder"/> instance, which can be used to
    /// define and build enumerations dynamically.</remarks>
    public EnumBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumBuilder"/> class.
    /// </summary>
    /// <param name="enumName">The name of the enum.</param>
    public EnumBuilder(string enumName)
    {
        if (string.IsNullOrWhiteSpace(enumName))
        {
            throw new ArgumentException("Enum name cannot be null or whitespace.", nameof(enumName));
        }

        _enumName = enumName;
    }

    /// <summary>
    /// Sets the name of the enum.
    /// </summary>
    /// <param name="enumName">The name of the enum.</param>
    /// <returns>The builder instance for chaining.</returns>
    public EnumBuilder WithName(string enumName)
    {
        if (string.IsNullOrWhiteSpace(enumName))
        {
            throw new ArgumentException("Enum name cannot be null or whitespace.", nameof(enumName));
        }

        _enumName = enumName;
        return this;
    }

    /// <summary>
    /// Sets the base type for the enum.
    /// </summary>
    /// <param name="baseType">The base type (e.g., "byte", "long").</param>
    /// <returns>The builder instance for chaining.</returns>
    public EnumBuilder WithBaseType(string baseType)
    {
        if (string.IsNullOrWhiteSpace(baseType))
        {
            throw new ArgumentException("Base type cannot be null or whitespace.", nameof(baseType));
        }

        _baseType = baseType;
        return this;
    }

    /// <summary>
    /// Adds an attribute to the enum (alias for AddAttribute).
    /// </summary>
    /// <param name="attributeText">The attribute text without brackets.</param>
    /// <returns>The builder instance for chaining.</returns>
    public EnumBuilder WithAttribute(string attributeText) => AddAttribute(attributeText);

    /// <summary>
    /// Adds a member to the enum.
    /// </summary>
    /// <param name="memberName">The name of the enum member.</param>
    /// <returns>The builder instance for chaining.</returns>
    public EnumBuilder AddMember(string memberName)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            throw new ArgumentException("Member name cannot be null or whitespace.", nameof(memberName));
        }

        _members.Add(memberName);
        return this;
    }

    /// <summary>
    /// Adds a member with a specific value to the enum.
    /// </summary>
    /// <param name="memberName">The name of the enum member.</param>
    /// <param name="value">The value of the enum member.</param>
    /// <returns>The builder instance for chaining.</returns>
    public EnumBuilder AddValue(string memberName, int value)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            throw new ArgumentException("Member name cannot be null or whitespace.", nameof(memberName));
        }

        _members.Add($"{memberName} = {value}");
        return this;
    }

    /// <summary>
    /// Adds a member with a specific value to the enum (long value).
    /// </summary>
    /// <param name="memberName">The name of the enum member.</param>
    /// <param name="value">The value of the enum member.</param>
    /// <returns>The builder instance for chaining.</returns>
    public EnumBuilder AddValue(string memberName, long value)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            throw new ArgumentException("Member name cannot be null or whitespace.", nameof(memberName));
        }

        _members.Add($"{memberName} = {value}");
        return this;
    }

    /// <summary>
    /// Generates the C# code for the enum.
    /// </summary>
    /// <returns>The generated C# code.</returns>
    public override string Build()
    {
        // All members are strings, so builder pattern is already followed here.
        var sb = new StringBuilder();
        GenerateXmlDocumentation(sb);
        GenerateAttributes(sb);
        sb.Append(GetAccessModifierString(AccessModifier));
        sb.Append("enum ");
        sb.Append(_enumName);

        if (!string.IsNullOrEmpty(_baseType))
        {
            sb.Append(" : ");
            sb.Append(_baseType);
        }

        sb.AppendLine();
        sb.AppendLine("{");
        for (var i = 0; i < _members.Count; i++)
        {
            sb.Append("    ");
            sb.Append(_members[i]);
            if (i < _members.Count - 1)
            {
                sb.Append(',');
            }

            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}
