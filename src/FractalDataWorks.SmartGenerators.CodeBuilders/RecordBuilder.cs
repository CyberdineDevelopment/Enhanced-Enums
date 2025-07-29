using System;
using System.Collections.Generic;
using System.Text;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating record declarations.
/// </summary>
public class RecordBuilder : CodeBuilderBase<RecordBuilder>
{
    private string _recordName;
    private readonly List<RecordParameterInfo> _parameters = [];
    private readonly List<IBuildable> _members = [];
    private string? _baseTypeName;
    private readonly List<string> _interfaceNames = [];
    private readonly List<AttributeBuilder> _attributeBuilders = [];
    private string? _namespace;
    private bool _isStruct;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBuilder"/> class with default name.
    /// </summary>
    public RecordBuilder()
        : this("Record")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBuilder"/> class.
    /// </summary>
    /// <param name="recordName">The name of the record to generate.</param>
    public RecordBuilder(string recordName)
    {
        if (string.IsNullOrWhiteSpace(recordName))
        {
            throw new ArgumentException("Record name cannot be null or whitespace.", nameof(recordName));
        }

        _recordName = recordName;
    }

    /// <summary>
    /// Sets the name of the record.
    /// </summary>
    /// <param name="recordName">The name of the record.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RecordBuilder WithName(string recordName)
    {
        if (string.IsNullOrWhiteSpace(recordName))
        {
            throw new ArgumentException("Record name cannot be null or whitespace.", nameof(recordName));
        }

        _recordName = recordName;
        return this;
    }

    /// <summary>
    /// Gets the name of the record.
    /// </summary>
    public string Name => _recordName;

    /// <summary>
    /// Sets the base type for this record.
    /// </summary>
    /// <param name="baseTypeName">The name of the base type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RecordBuilder WithBaseType(string baseTypeName)
    {
        if (string.IsNullOrWhiteSpace(baseTypeName))
        {
            throw new ArgumentException("Base type name cannot be null or whitespace.", nameof(baseTypeName));
        }

        _baseTypeName = baseTypeName;
        return this;
    }

    /// <summary>
    /// Adds a parameter to the record's primary constructor.
    /// </summary>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RecordBuilder WithParameter(string parameterType, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterType))
        {
            throw new ArgumentException("Parameter type cannot be null or whitespace.", nameof(parameterType));
        }

        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or whitespace.", nameof(parameterName));
        }

        _parameters.Add(new RecordParameterInfo
        {
            Name = parameterName,
            Type = parameterType,
        });

        return this;
    }

    /// <summary>
    /// Adds a parameter to the record's primary constructor with a default value.
    /// </summary>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="configure">Configuration action for the parameter.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RecordBuilder WithParameter(string parameterType, string parameterName, Action<RecordParameterBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var paramBuilder = new RecordParameterBuilder(parameterType, parameterName);
        configure(paramBuilder);
        _parameters.Add(paramBuilder.Build());
        return this;
    }

    /// <summary>
    /// Makes this a record struct instead of a record class.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public RecordBuilder MakeStruct()
    {
        _isStruct = true;
        return this;
    }

    /// <summary>
    /// Adds an interface to the list of interfaces this record implements.
    /// </summary>
    /// <param name="interfaceName">The name of the interface.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RecordBuilder ImplementsInterface(string interfaceName)
    {
        if (string.IsNullOrWhiteSpace(interfaceName))
        {
            throw new ArgumentException("Interface name cannot be null or whitespace.", nameof(interfaceName));
        }

        if (_interfaceNames.Contains(interfaceName))
        {
            throw new ArgumentException($"Interface '{interfaceName}' is already implemented.", nameof(interfaceName));
        }

        _interfaceNames.Add(interfaceName);
        return this;
    }

    /// <summary>
    /// Adds a method to the record.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnTypeName">The return type of the method.</param>
    /// <returns>A MethodBuilder to configure the method.</returns>
    public MethodBuilder AddMethod(string methodName, string returnTypeName)
    {
        var methodBuilder = new MethodBuilder(methodName, returnTypeName);
        _members.Add(methodBuilder);
        return methodBuilder;
    }

    /// <summary>
    /// Adds a method to the record and configures it.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnTypeName">The return type of the method.</param>
    /// <param name="configure">The action to configure the created <see cref="MethodBuilder"/>.</param>
    /// <returns>The <see cref="RecordBuilder"/> for chaining.</returns>
    public RecordBuilder AddMethod(string methodName, string returnTypeName, Action<MethodBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var method = AddMethod(methodName, returnTypeName);
        configure(method);
        return this;
    }

    /// <summary>
    /// Adds a property to the record.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyTypeName">The type of the property.</param>
    /// <returns>A PropertyBuilder to configure the property.</returns>
    public PropertyBuilder AddProperty(string propertyName, string propertyTypeName)
    {
        var propertyBuilder = new PropertyBuilder(propertyName, propertyTypeName);
        _members.Add(propertyBuilder);
        return propertyBuilder;
    }

    /// <summary>
    /// Adds a property to the record and configures it.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyTypeName">The type of the property.</param>
    /// <param name="configure">The action to configure the created <see cref="PropertyBuilder"/>.</param>
    /// <returns>The <see cref="RecordBuilder"/> for chaining.</returns>
    public RecordBuilder AddProperty(string propertyName, string propertyTypeName, Action<PropertyBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var property = AddProperty(propertyName, propertyTypeName);
        configure(property);
        return this;
    }

    /// <summary>
    /// Sets the namespace for this record.
    /// </summary>
    /// <param name="namespaceName">The namespace name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RecordBuilder WithNamespace(string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            throw new ArgumentException("Namespace name cannot be null or whitespace.", nameof(namespaceName));
        }

        _namespace = namespaceName;
        return this;
    }

    /// <summary>
    /// Sets the XML documentation summary for this record.
    /// </summary>
    /// <param name="summary">The summary text.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RecordBuilder WithSummary(string summary) => WithXmlDocSummary(summary);

    /// <inheritdoc />
    public override string Build()
    {
        var sb = new StringBuilder();

        // Build namespace if specified
        var hasNamespace = !string.IsNullOrEmpty(_namespace);
        if (hasNamespace)
        {
            sb.AppendLine($"namespace {_namespace}");
            sb.AppendLine("{");
        }

        // Build XML documentation
        if (!string.IsNullOrEmpty(XmlDocSummary))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {XmlDocSummary}");
            sb.AppendLine("/// </summary>");
        }

        // Build attributes
        foreach (var attribute in Attributes)
        {
            sb.AppendLine($"[{attribute}]");
        }

        foreach (var attributeBuilder in _attributeBuilders)
        {
            sb.AppendLine($"[{attributeBuilder.Build()}]");
        }

        // Build the record declaration
        sb.Append(GetAccessModifierString(AccessModifier));
        sb.Append(GetModifierString(Modifiers));
        sb.Append($"record {(_isStruct ? "struct " : string.Empty)}{_recordName}");

        // Add parameters if any
        if (_parameters.Count > 0)
        {
            sb.Append('(');
            for (var i = 0; i < _parameters.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append($"{_parameters[i].Type} {_parameters[i].Name}");
                if (!string.IsNullOrEmpty(_parameters[i].DefaultValue))
                {
                    sb.Append($" = {_parameters[i].DefaultValue}");
                }
            }

            sb.Append(')');
        }

        // Build base type and interfaces
        var hasBaseType = !string.IsNullOrEmpty(_baseTypeName);
        var hasInterfaces = _interfaceNames.Count > 0;

        if (hasBaseType || hasInterfaces)
        {
            sb.Append(" : ");

            if (hasBaseType)
            {
                sb.Append(_baseTypeName!);
                if (hasInterfaces)
                {
                    sb.Append(", ");
                }
            }

            for (var i = 0; i < _interfaceNames.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(_interfaceNames[i]);
            }
        }

        // Build body
        if (_members.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("{");

            // Use a CodeBuilder to generate the members with proper indentation
            var memberBuilder = new CodeBuilder();
            memberBuilder.Indent();

            // Build members
            foreach (var member in _members)
            {
                var builtCode = member.Build();
                if (!string.IsNullOrWhiteSpace(builtCode))
                {
                    memberBuilder.AppendLine(builtCode);
                }
            }

            sb.Append(memberBuilder.ToString());
            sb.AppendLine("}");
        }
        else
        {
            sb.AppendLine(";");
        }

        // Close namespace if specified
        if (!string.IsNullOrEmpty(_namespace))
        {
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    internal sealed class RecordParameterInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? DefaultValue { get; set; }
    }

}
