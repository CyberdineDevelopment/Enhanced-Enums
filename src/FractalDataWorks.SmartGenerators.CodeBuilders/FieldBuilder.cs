using System;
using System.Text;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating field declarations.
/// </summary>
public class FieldBuilder : MemberBuilderBase<FieldBuilder>
{
    private IBuildable? _initializerBlockBuilder;
    private bool _isReadOnly;
    private bool _isConst;
    private string? _initializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldBuilder"/> class.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="typeName">The type of the field.</param>
    public FieldBuilder(string fieldName, string typeName)
        : base(fieldName, typeName)
    {
        if (typeName == null)
        {
            throw new ArgumentException("Field type name cannot be null.", nameof(typeName));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldBuilder"/> class with only field name; type can be set with WithType.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    public FieldBuilder(string fieldName)
        : this(fieldName, string.Empty)
    {
    }

    /// <summary>
    /// Sets the field initializer using a custom code builder.
    /// </summary>
    /// <param name="initializerBuilder">The initializer builder.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FieldBuilder WithInitializer(IBuildable initializerBuilder)
    {
        _initializerBlockBuilder = initializerBuilder;
        return this;
    }

    /// <summary>
    /// Makes the field readonly.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public FieldBuilder MakeReadOnly()
    {
        _isReadOnly = true;
        return this;
    }

    /// <summary>
    /// Makes the field constant.
    /// </summary>
    /// <param name="value">The constant value.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FieldBuilder MakeConst(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Constant value cannot be null or empty", nameof(value));
        }

        _isConst = true;
        _initializer = value;
        return this;
    }

    /// <summary>
    /// Sets the initializer for the field.
    /// </summary>
    /// <param name="initializer">The initialization expression.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FieldBuilder WithInitializer(string initializer)
    {
        if (_isConst)
        {
            throw new InvalidOperationException("Cannot set initializer for a constant field. Use MakeConst() instead.");
        }

        _initializer = initializer;
        return this;
    }

    /// <summary>
    /// Sets the type of the field.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FieldBuilder WithType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Field type name cannot be null or whitespace.", nameof(typeName));
        }

        TypeNameInternal = typeName;
        return this;
    }

    /// <summary>
    /// Adds an attribute to this field.
    /// </summary>
    /// <param name="attributeBuilder">The attribute builder to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FieldBuilder AddAttribute(AttributeBuilder attributeBuilder)
    {
        if (attributeBuilder == null)
        {
            throw new ArgumentNullException(nameof(attributeBuilder));
        }

        AddAttributeInternal(attributeBuilder.Build());
        return this;
    }

    /// <summary>
    /// Generates the C# code for the field.
    /// </summary>
    /// <returns>The generated C# code.</returns>
    public override string Build()
    {
        var sb = new StringBuilder();

        var xml = GetXmlDocumentation();
        if (!string.IsNullOrEmpty(xml))
        {
            sb.Append(xml);
        }

        var attrs = GetAttributeStrings();
        if (!string.IsNullOrEmpty(attrs))
        {
            sb.Append(attrs);
        }

        sb.Append(GetAccessModifierString(AccessModifier));

        // Add modifiers
        if (_isConst)
        {
            sb.Append("const ");
        }
        else
        {
            sb.Append(GetModifierString(Modifiers));

            if (_isReadOnly)
            {
                sb.Append("readonly ");
            }
        }

        // Add type and name
        sb.Append($"{TypeName} {Name}");

        // Add initializer if present
        if (!string.IsNullOrEmpty(_initializer))
        {
            sb.Append($" = {_initializer}");
        }

        sb.Append(';');

        return sb.ToString();
    }
}
