using System;
using System.Collections.Generic;
using System.Text;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating class declarations.
/// </summary>
// DESIGN PATTERN: Builder Pattern
// This class implements the Builder pattern from Gang of Four.
// It separates the construction of a complex object (a C# class) from its representation,
// allowing the same construction process to create different representations.
// The fluent interface (method chaining) makes the code more readable and maintainable.
public class ClassBuilder : CodeBuilderBase<ClassBuilder>
{
    private string _className;
    private readonly List<IBuildable> _members = [];
    private string? _baseTypeName;
    private readonly List<string> _interfaceNames = [];
    private readonly List<string> _typeParameters = [];
    private readonly List<string> _typeConstraints = [];
    private readonly List<AttributeBuilder> _attributeBuilders = [];
    private string? _namespace;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassBuilder"/> class with default name.
    /// </summary>
    public ClassBuilder()
        : this("Class")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassBuilder"/> class.
    /// </summary>
    /// <param name="className">The name of the class to generate.</param>
    public ClassBuilder(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            throw new ArgumentException("Class name cannot be null or whitespace.", nameof(className));
        }

        _className = className;
    }

    /// <summary>
    /// Sets the name of the class.
    /// </summary>
    /// <param name="className">The name of the class.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder WithName(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            throw new ArgumentException("Class name cannot be null or whitespace.", nameof(className));
        }

        _className = className;
        return this;
    }

    /// <summary>
    /// Gets the name of the class.
    /// </summary>
    public string Name => _className;

    /// <summary>
    /// Sets the base type for this class.
    /// </summary>
    /// <param name="baseTypeName">The name of the base type.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the base type name is null or whitespace.</exception>
    public ClassBuilder WithBaseType(string baseTypeName)
    {
        if (string.IsNullOrWhiteSpace(baseTypeName))
        {
            throw new ArgumentException("Base type name cannot be null or whitespace.", nameof(baseTypeName));
        }

        _baseTypeName = baseTypeName;
        return this;
    }

    /// <summary>
    /// Sets the base type for this class.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder WithBaseType<T>() => WithBaseType(typeof(T).Name);

    /// <summary>
    /// Adds a type parameter to the class.
    /// </summary>
    /// <param name="name">The name of the type parameter.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the type parameter name is null or whitespace.</exception>
    public ClassBuilder WithTypeParameter(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Type parameter name cannot be null or whitespace.", nameof(name));
        
        if (_typeParameters.Contains(name))
            throw new ArgumentException($"Type parameter '{name}' already exists.", nameof(name));
        
        _typeParameters.Add(name);
        return this;
    }

    /// <summary>
    /// Adds multiple type parameters to the class.
    /// </summary>
    /// <param name="names">The names of the type parameters.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder WithTypeParameters(params string[] names)
    {
        foreach (var name in names)
        {
            WithTypeParameter(name);
        }
        return this;
    }

    /// <summary>
    /// Adds a type constraint to the class.
    /// </summary>
    /// <param name="constraint">The constraint clause (e.g., "where T : class").</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the constraint is null or whitespace.</exception>
    public ClassBuilder WithTypeConstraint(string constraint)
    {
        if (string.IsNullOrWhiteSpace(constraint))
            throw new ArgumentException("Type constraint cannot be null or whitespace.", nameof(constraint));
        
        _typeConstraints.Add(constraint);
        return this;
    }

    /// <summary>
    /// Adds an interface to the list of interfaces this class implements.
    /// </summary>
    /// <param name="interfaceName">The name of the interface.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the interface name is null, whitespace, or a duplicate.</exception>
    public ClassBuilder ImplementsInterface(string interfaceName)
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
    /// Adds a generic interface to the list of interfaces this class implements.
    /// </summary>
    /// <typeparam name="T">The interface type to implement.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder ImplementsInterface<T>() => ImplementsInterface(typeof(T).Name);

    /// <summary>
    /// Sets the namespace for this class.
    /// </summary>
    /// <param name="namespaceName">The namespace name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder WithNamespace(string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            throw new ArgumentException("Namespace name cannot be null or whitespace.", nameof(namespaceName));
        }

        _namespace = namespaceName;
        return this;
    }

    /// <summary>
    /// Adds a method to the class.
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
    /// Adds a method to the class and configures it.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnTypeName">The return type of the method.</param>
    /// <param name="configure">The action to configure the created <see cref="MethodBuilder"/>.</param>
    /// <returns>The <see cref="ClassBuilder"/> for chaining.</returns>
    public ClassBuilder AddMethod(string methodName, string returnTypeName, Action<MethodBuilder> configure)
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
    /// Adds a method with no implementation to the class being built.
    /// </summary>
    /// <remarks>This method creates a method definition with no implementation and adds it to the class being
    /// built. The method is marked as having no implementation, which may be useful for generating abstract or interface
    /// methods.</remarks>
    /// <param name="methodName">The name of the method to add. Cannot be null or empty.</param>
    /// <param name="returnType">The return type of the method. Cannot be null or empty.</param>
    /// <returns>The current <see cref="ClassBuilder"/> instance, allowing for method chaining.</returns>
    public ClassBuilder AddMethodWithNoImplementation(string methodName, string returnType)
    {
        var methodBuilder = new MethodBuilder(methodName, returnType);
        methodBuilder.NoImplementation(this); // Mark as no implementation
        _members.Add(methodBuilder);
        return this;
    }

    /// <summary>
    /// Adds a property to the class.
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
    /// Adds a property to the class and configures it.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyTypeName">The type of the property.</param>
    /// <param name="configure">The action to configure the property.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder AddProperty(string propertyName, string propertyTypeName, Action<PropertyBuilder> configure)
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
    /// Adds a field to this class.
    /// </summary>
    /// <param name="fieldType">The type of the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>A FieldBuilder to configure the field.</returns>
    public FieldBuilder AddField(string fieldType, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldType))
        {
            throw new ArgumentException("Field type cannot be null or whitespace.", nameof(fieldType));
        }

        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException("Field name cannot be null or whitespace.", nameof(fieldName));
        }

        var fieldBuilder = new FieldBuilder(fieldName, fieldType);
        _members.Add(fieldBuilder);
        return fieldBuilder;
    }

    /// <summary>
    /// Adds a field to this class and configures it.
    /// </summary>
    /// <param name="fieldType">The type of the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="configure">The action to configure the created <see cref="FieldBuilder"/>.</param>
    /// <returns>The <see cref="ClassBuilder"/> for chaining.</returns>
    public ClassBuilder AddField(string fieldType, string fieldName, Action<FieldBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var field = AddField(fieldType, fieldName);
        configure(field);
        return this;
    }

    /// <summary>
    /// Adds a field to this class using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>A FieldBuilder to configure the field.</returns>
    public FieldBuilder AddField<T>(string fieldName) => AddField(typeof(T).Name, fieldName);

    /// <summary>
    /// Adds a constructor to this class.
    /// </summary>
    /// <returns>A builder for configuring the constructor.</returns>
    public ConstructorBuilder AddConstructor()
    {
        var builder = CodeBuilderFactory.CreateConstructor(_className);
        _members.Add(builder);
        return builder;
    }

    /// <summary>
    /// Adds a constructor to this class and configures it.
    /// </summary>
    /// <param name="configure">The action to configure the created <see cref="ConstructorBuilder"/>.</param>
    /// <returns>The <see cref="ClassBuilder"/> for chaining.</returns>
    public ClassBuilder AddConstructor(Action<ConstructorBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var ctor = AddConstructor();
        configure(ctor);
        return this;
    }

    /// <summary>
    /// Adds a nested class to this class.
    /// </summary>
    /// <param name="configure">The action to configure the nested class.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder AddNestedClass(Action<ClassBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var nestedClass = new ClassBuilder();
        configure(nestedClass);
        _members.Add(nestedClass);
        return this;
    }

    /// <summary>
    /// Adds a nested interface to this class.
    /// </summary>
    /// <param name="configure">The action to configure the nested interface.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder AddNestedInterface(Action<InterfaceBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var nestedInterface = new InterfaceBuilder();
        configure(nestedInterface);
        _members.Add(nestedInterface);
        return this;
    }

    /// <summary>
    /// Adds a nested enum to this class.
    /// </summary>
    /// <param name="configure">The action to configure the nested enum.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder AddNestedEnum(Action<EnumBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var nestedEnum = new EnumBuilder();
        configure(nestedEnum);
        _members.Add(nestedEnum);
        return this;
    }

    /// <summary>
    /// Adds a nested record to this class.
    /// </summary>
    /// <param name="configure">The action to configure the nested record.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder AddNestedRecord(Action<RecordBuilder> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var nestedRecord = new RecordBuilder();
        configure(nestedRecord);
        _members.Add(nestedRecord);
        return this;
    }

    /// <summary>
    /// Adds a raw code block to this class using string content.
    /// </summary>
    /// <param name="blockContent">The content of the code block as a string.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder AddCodeBlock(string blockContent)
    {
        if (blockContent == null)
        {
            throw new ArgumentNullException(nameof(blockContent));
        }

        var blockBuilder = new CodeBlockBuilder(blockContent);
        _members.Add(blockBuilder);
        return this;
    }

    /// <summary>
    /// Adds a raw code block to this class using a block builder.
    /// </summary>
    /// <param name="blockBuilder">The action that will build the block.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder AddCodeBlock(Action<ICodeBuilder> blockBuilder)
    {
        if (blockBuilder == null)
        {
            throw new ArgumentNullException(nameof(blockBuilder));
        }

        var codeBlockBuilder = new CodeBlockBuilder(blockBuilder);
        _members.Add(codeBlockBuilder);
        return this;
    }

    /// <summary>
    /// Adds an attribute to this class.
    /// </summary>
    /// <param name="attributeBuilder">The attribute builder to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ClassBuilder AddAttribute(AttributeBuilder attributeBuilder)
    {
        if (attributeBuilder == null)
        {
            throw new ArgumentNullException(nameof(attributeBuilder));
        }

        _attributeBuilders.Add(attributeBuilder);
        return this;
    }

    /// <summary>
    /// Gets the XML documentation for this class.
    /// </summary>
    /// <returns>The XML documentation.</returns>
    private string GetXmlDocumentation()
    {
        if (string.IsNullOrEmpty(XmlDocSummary))
        {
            return string.Empty;
        }

        return
            "/// <summary>\r\n" +
            $"/// {XmlDocSummary}\r\n" +
            "/// </summary>\r\n";
    }

    /// <summary>
    /// Gets the attributes for this class.
    /// </summary>
    /// <returns>The attributes.</returns>
    private string GetAttributes()
    {
        var attributes = new List<string>();

        // Handle string attributes from base class
        foreach (var attribute in Attributes)
        {
            attributes.Add($"[{attribute}]");
        }

        // Handle AttributeBuilder instances
        foreach (var attributeBuilder in _attributeBuilders)
        {
            attributes.Add($"[{attributeBuilder.Build()}]");
        }

        return string.Join("\n", attributes);
    }

    /// <inheritdoc />
    public override string Build()
    {
        var sb = new StringBuilder();

        // Build namespace if specified (file-scoped)
        var hasNamespace = !string.IsNullOrEmpty(_namespace);
        if (hasNamespace)
        {
            sb.AppendLine($"namespace {_namespace};");
            sb.AppendLine();
        }

        // Build XML documentation
        var xml = GetXmlDocumentation();
        if (!string.IsNullOrEmpty(xml))
        {
            sb.Append(xml);
        }

        // Build attributes
        var attrs = GetAttributes();
        if (!string.IsNullOrEmpty(attrs))
        {
            sb.Append(attrs);
        }

        // Build the class declaration
        sb.Append(GetAccessModifierString(AccessModifier));
        sb.Append(GetModifierString(Modifiers));
        sb.Append($"class {_className}");
        if (_typeParameters.Count > 0)
        {
            sb.Append($"<{string.Join(", ", _typeParameters)}>");
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

        // Add type constraints
        foreach (var constraint in _typeConstraints)
        {
            sb.AppendLine();
            sb.Append($"    {constraint}");
        }

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

        // File-scoped namespaces don't need closing braces

        return sb.ToString();
    }
}
