using System;
using System.Text;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Base class for all member builders (methods, properties, fields).
/// </summary>
/// <typeparam name="TBuilder">The builder type for fluent interface chaining.</typeparam>
public abstract class MemberBuilderBase<TBuilder> : CodeBuilderBase<TBuilder>, IMemberBuilder<TBuilder>
    where TBuilder : MemberBuilderBase<TBuilder>
{
    /// <summary>
    /// The type name of the member.
    /// </summary>
    private string _typeName;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberBuilderBase{TBuilder}"/> class.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <param name="typeName">The type of the member.</param>
    /// <exception cref="ArgumentException">Thrown when name or typeName is null or whitespace.</exception>
    protected MemberBuilderBase(string name, string typeName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        // Special case: constructors and some other special members might not have a type
        // Only validate if the type name is not null (empty string is allowed)
        Name = name;
        _typeName = typeName ?? throw new ArgumentException("Type name cannot be null.", nameof(typeName));
    }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type name of the member.
    /// </summary>
    public string TypeName => _typeName;

    /// <summary>
    /// Gets or sets the type name (for derived classes).
    /// </summary>
    protected string TypeNameInternal
    {
        get => _typeName;
        set => _typeName = value;
    }

    /// <summary>
    /// Gets the XML documentation for this member.
    /// </summary>
    /// <param name="indent">The indentation string to prepend to each line.</param>
    /// <returns>The XML documentation as a string.</returns>
    protected string GetXmlDocumentation(string indent = "")
    {
        if (string.IsNullOrEmpty(XmlDocSummary))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"{indent}/// <summary>");
        sb.AppendLine($"{indent}/// {XmlDocSummary}");
        sb.AppendLine($"{indent}/// </summary>");
        return sb.ToString();
    }

    /// <summary>
    /// Makes the method virtual.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakeVirtual()
    {
        Modifiers |= Modifiers.Virtual;
        return (TBuilder)this;
    }

    /// <summary>
    /// Gets the attribute strings for this member.
    /// </summary>
    /// <param name="indent">The indentation string to prepend to each line.</param>
    /// <returns>The attributes as a string.</returns>
    protected string GetAttributeStrings(string indent = "")
    {
        if (Attributes.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var attribute in Attributes)
        {
            sb.AppendLine($"{indent}[{attribute}]");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration for this member.
    /// </summary>
    /// <param name="indent">The indentation string to prepend to each line.</param>
    /// <returns>The declaration as a string.</returns>
    protected string GetDeclaration(string indent = "")
    {
        var sb = new StringBuilder();

        // Add XML documentation
        var xmlDocs = GetXmlDocumentation(indent);
        if (!string.IsNullOrEmpty(xmlDocs))
        {
            sb.Append(xmlDocs);
        }

        // Add attributes
        var attrs = GetAttributeStrings(indent);
        if (!string.IsNullOrEmpty(attrs))
        {
            sb.Append(attrs);
        }

        // Add access modifier
        sb.Append(indent);
        sb.Append(GetAccessModifierString(AccessModifier));

        // Add modifiers
        var modifiers = GetModifierString(Modifiers);
        if (!string.IsNullOrEmpty(modifiers))
        {
            sb.Append(modifiers);
        }

        return sb.ToString();
    }
}
