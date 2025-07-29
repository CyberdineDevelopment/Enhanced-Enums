using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating C# attribute declarations.
/// </summary>
public class AttributeBuilder : CodeBuilderBase<AttributeBuilder>
{
    private readonly string _name;
    private readonly List<AttributeArgumentInfo> _arguments = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    public AttributeBuilder(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(name));
        }

        _name = NormalizeAttributeName(name);
    }

    /// <summary>
    /// Adds a positional argument to the attribute.
    /// </summary>
    /// <param name="value">The value of the argument.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AttributeBuilder WithArgument(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _arguments.Add(new AttributeArgumentInfo
        {
            Value = value,
            IsNamed = false,
        });

        return this;
    }

    /// <summary>
    /// Adds a named argument to the attribute.
    /// </summary>
    /// <param name="name">The name of the argument.</param>
    /// <param name="value">The value of the argument.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AttributeBuilder WithNamedArgument(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Argument name cannot be null or whitespace.", nameof(name));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _arguments.Add(new AttributeArgumentInfo
        {
            Name = name,
            Value = value,
            IsNamed = true,
        });

        return this;
    }

    /// <summary>
    /// Generates the C# code for the attribute.
    /// </summary>
    /// <returns>The generated C# code.</returns>
    public override string Build()
    {
        var sb = new StringBuilder();

        // Build XML documentation
        GenerateXmlDocumentation(sb);

        // Build the attribute declaration
        sb.Append('[');
        sb.Append(_name);

        if (_arguments.Count > 0)
        {
            sb.Append('(');

            // Build positional arguments first
            var positionalArgs = _arguments.Where(a => !a.IsNamed).ToList();
            for (var i = 0; i < positionalArgs.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(positionalArgs[i].Value);
            }

            // Build named arguments
            var namedArgs = _arguments.Where(a => a.IsNamed).ToList();
            if (namedArgs.Count > 0)
            {
                // Add a comma if there are positional arguments
                if (positionalArgs.Count > 0)
                {
                    sb.Append(", ");
                }

                for (var i = 0; i < namedArgs.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append($"{namedArgs[i].Name} = {namedArgs[i].Value}");
                }
            }

            sb.Append(')');
        }

        sb.Append(']');

        return sb.ToString();
    }

    private static string NormalizeAttributeName(string name)
    {
        // If the name doesn't end with "Attribute", add it
        if (!name.EndsWith("Attribute", StringComparison.Ordinal))
        {
            return name;
        }

        return name;
    }
}
