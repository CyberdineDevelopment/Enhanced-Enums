using System;
using System.Text;
using FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating property declarations.
/// </summary>
public class PropertyBuilder : MemberBuilderBase<PropertyBuilder>
{
    private readonly bool _hasGetter = true;
    private bool _hasSetter = true;
    private bool _hasInitSetter;
    private IBuildable? _getterBlockBuilder;
    private AccessModifier _setterAccessModifier = AccessModifier.None;
    private IBuildable? _setterBlockBuilder;
    private string? _initializer;
    private string? _expressionBody;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBuilder"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyTypeName">The type name of the property.</param>
    public PropertyBuilder(string propertyName, string propertyTypeName)
        : base(propertyName, propertyTypeName)
    {
        if (propertyTypeName == null)
        {
            throw new ArgumentException("Property type name cannot be null.", nameof(propertyTypeName));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBuilder"/> class with only property name; type can be set with WithType.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    public PropertyBuilder(string propertyName)
        : this(propertyName, string.Empty)
    {
    }

    /// <summary>
    /// Sets the type of the property.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder WithType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Property type name cannot be null or whitespace.", nameof(typeName));
        }

        TypeNameInternal = typeName;
        return this;
    }

    /// <summary>
    /// Sets a custom getter builder.
    /// </summary>
    /// <param name="getterBuilder">The buildable object for the getter implementation.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder WithGetterBody(IBuildable getterBuilder)
    {
        _getterBlockBuilder = getterBuilder;
        return this;
    }

    /// <summary>
    /// Sets a custom setter builder.
    /// </summary>
    /// <param name="setterBuilder">The buildable object for the setter implementation.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder WithSetterBody(IBuildable setterBuilder)
    {
        _setterBlockBuilder = setterBuilder;
        return this;
    }

    /// <summary>
    /// Sets the access modifier for the property's setter.
    /// </summary>
    /// <param name="accessModifier">The access modifier to apply to the setter.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder WithSetterAccessModifier(AccessModifier accessModifier)
    {
        _setterAccessModifier = accessModifier;
        return this;
    }

    /// <summary>
    /// Makes the setter private.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder MakeSetterPrivate() => WithSetterAccessModifier(AccessModifier.Private);

    /// <summary>
    /// Makes the setter protected.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder MakeSetterProtected() => WithSetterAccessModifier(AccessModifier.Protected);

    /// <summary>
    /// Makes the setter internal.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder MakeSetterInternal() => WithSetterAccessModifier(AccessModifier.Internal);

    /// <summary>
    /// Makes the setter protected internal.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder MakeSetterProtectedInternal() => WithSetterAccessModifier(AccessModifier.ProtectedInternal);

    /// <summary>
    /// Sets an initializer for the property.
    /// </summary>
    /// <param name="initializer">The initialization expression.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder WithInitializer(string initializer)
    {
        _initializer = initializer;
        return this;
    }

    /// <summary>
    /// Adds an attribute to this property.
    /// </summary>
    /// <param name="attributeText">The attribute text without brackets.</param>
    /// <returns>The builder instance for chaining.</returns>
    public new PropertyBuilder AddAttribute(string attributeText)
    {
        base.AddAttribute(attributeText);
        return this;
    }

    /// <summary>
    /// Sets a getter implementation with the specified expression.
    /// </summary>
    /// <param name="expression">The expression to use in the getter.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when expression is null.</exception>
    public PropertyBuilder WithGetter(string expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        _getterBlockBuilder = new CodeBlockBuilder(expression);
        return this;
    }

    /// <summary>
    /// Sets a setter implementation with the specified expression.
    /// </summary>
    /// <param name="expression">The expression to use in the setter.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when expression is null.</exception>
    public PropertyBuilder WithSetter(string expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        _setterBlockBuilder = new CodeBlockBuilder(expression);
        return this;
    }

    /// <summary>
    /// Sets the property to use an expression-bodied implementation.
    /// </summary>
    /// <param name="expression">The expression to be used as the property body.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when expression is null.</exception>
    public PropertyBuilder WithExpressionBody(string expression)
    {
        _expressionBody = expression ?? throw new ArgumentNullException(nameof(expression));
        return this;
    }

    /// <summary>
    /// Makes the property read-only by removing the setter.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder MakeReadOnly()
    {
        _hasSetter = false;
        return this;
    }

    /// <summary>
    /// Sets the property to use an init-only setter.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PropertyBuilder WithInitSetter()
    {
        _hasSetter = true;
        _hasInitSetter = true;
        return this;
    }

    /// <summary>
    /// Builds the property declaration as a string.
    /// </summary>
    /// <returns>The generated property declaration.</returns>
    public override string Build()
    {
        var sb = new StringBuilder();

        // start on new line for namespace interpolation
        sb.AppendLine();

        // Expression-bodied property support
        if (_expressionBody != null)
        {
            // XML documentation
            var xmlDocsExpr = GetXmlDocumentation();
            if (!string.IsNullOrEmpty(xmlDocsExpr))
            {
                sb.Append(xmlDocsExpr);
            }

            // Attributes
            var attrsExpr = GetAttributeStrings();
            if (!string.IsNullOrEmpty(attrsExpr))
            {
                sb.Append(attrsExpr);
            }

            sb.Append(GetAccessModifierString(AccessModifier));
            sb.Append(GetModifierString(Modifiers));
            sb.Append($"{TypeName} {Name} => {_expressionBody};");  // Fixed: removed quotes
            sb.AppendLine();
            return sb.ToString();
        }

        // if read-only, force no setter
        if (!_hasSetter)
        {
            _setterBlockBuilder = null;
            _setterAccessModifier = AccessModifier.None;
            _hasSetter = false;
        }

        // XML documentation
        var xmlDocs = GetXmlDocumentation();
        if (!string.IsNullOrEmpty(xmlDocs))
        {
            sb.Append(xmlDocs);
        }

        // Attributes
        var attrs = GetAttributeStrings();
        if (!string.IsNullOrEmpty(attrs))
        {
            sb.Append(attrs);
        }

        // Signature
        sb.Append(GetAccessModifierString(AccessModifier));
        sb.Append(GetModifierString(Modifiers));
        sb.Append($"{TypeName} {Name}");

        var hasBodyBuilder = _getterBlockBuilder != null || _setterBlockBuilder != null;
        if (hasBodyBuilder || _initializer != null)
        {
            sb.Append(" { ");
            if (_hasGetter)
            {
                if (_getterBlockBuilder != null)
                {
                    sb.Append("get { ");
                    sb.Append(_getterBlockBuilder.Build());
                    sb.Append(" } ");
                }
                else
                {
                    sb.Append("get; ");
                }
            }

            if (_hasSetter)
            {
                if (_setterAccessModifier != AccessModifier.None)
                {
                    sb.Append($"{GetAccessModifierString(_setterAccessModifier).Trim()} ");
                }

                if (_setterBlockBuilder != null)
                {
                    sb.Append(_hasInitSetter ? "init { " : "set { ");
                    sb.Append(_setterBlockBuilder.Build());
                    sb.Append(" } ");
                }
                else
                {
                    sb.Append(_hasInitSetter ? "init; " : "set; ");
                }
            }

            sb.Append('}');
            if (_initializer != null)
            {
                sb.Append(" = ");
                sb.Append(_initializer);
            }

            sb.AppendLine();
        }
        else
        {
            // auto-property
            sb.Append(" { ");
            if (_hasGetter)
            {
                sb.Append("get; ");
            }

            if (_hasSetter)
            {
                if (_setterAccessModifier != AccessModifier.None)
                {
                    sb.Append($"{GetAccessModifierString(_setterAccessModifier).Trim()} ");
                }

                sb.Append(_hasInitSetter ? "init; " : "set; ");
            }

            sb.Append('}');
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
