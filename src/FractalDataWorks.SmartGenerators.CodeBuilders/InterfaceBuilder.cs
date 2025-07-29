using System;
using System.Collections.Generic;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating interface declarations.
/// </summary>
public class InterfaceBuilder : CodeBuilderBase<InterfaceBuilder>
{
    private string _interfaceName = "IInterface";
    private readonly List<IBuildable> _members = [];
    private readonly List<string> _baseInterfaceNames = [];
    private readonly List<string> _typeParameters = [];
    private readonly List<string> _typeConstraints = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceBuilder"/> class.
    /// </summary>
    /// <remarks>Use this constructor to create a new instance of the <see cref="InterfaceBuilder"/> for
    /// building user interfaces programmatically.</remarks>
    public InterfaceBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceBuilder"/> class.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to generate.</param>
    public InterfaceBuilder(string interfaceName)
    {
        if (string.IsNullOrWhiteSpace(interfaceName))
        {
            throw new ArgumentException("Interface name cannot be null or whitespace.", nameof(interfaceName));
        }

        _interfaceName = interfaceName;
    }

    /// <summary>
    /// Sets the name of the interface.
    /// </summary>
    /// <param name="interfaceName">The name of the interface.</param>
    /// <returns>The builder instance for chaining.</returns>
    public InterfaceBuilder WithName(string interfaceName)
    {
        if (string.IsNullOrWhiteSpace(interfaceName))
        {
            throw new ArgumentException("Interface name cannot be null or whitespace.", nameof(interfaceName));
        }

        _interfaceName = interfaceName;
        return this;
    }

    /// <summary>
    /// Adds a type parameter to the interface.
    /// </summary>
    /// <param name="typeParameterName">The name of the type parameter.</param>
    /// <returns>The builder instance for chaining.</returns>
    public InterfaceBuilder WithTypeParameter(string typeParameterName)
    {
        if (string.IsNullOrWhiteSpace(typeParameterName))
        {
            throw new ArgumentException("Type parameter name cannot be null or whitespace.", nameof(typeParameterName));
        }

        if (!_typeParameters.Contains(typeParameterName))
        {
            _typeParameters.Add(typeParameterName);
        }

        return this;
    }

    /// <summary>
    /// Sets the base interface for this interface.
    /// </summary>
    /// <param name="baseInterfaceName">The name of the base interface.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the base interface name is null or whitespace.</exception>
    public InterfaceBuilder WithBaseInterface(string baseInterfaceName)
    {
        if (string.IsNullOrWhiteSpace(baseInterfaceName))
        {
            throw new ArgumentException("Base interface name cannot be null or whitespace.", nameof(baseInterfaceName));
        }

        if (_baseInterfaceNames.Contains(baseInterfaceName))
        {
            throw new ArgumentException($"Interface '{baseInterfaceName}' is already a base interface.", nameof(baseInterfaceName));
        }

        _baseInterfaceNames.Add(baseInterfaceName);
        return this;
    }

    /// <summary>
    /// Sets the base interface for this interface.
    /// </summary>
    /// <typeparam name="T">The base interface type.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public InterfaceBuilder WithBaseInterface<T>() => WithBaseInterface(typeof(T).Name);

    /// <summary>
    /// Adds multiple type parameters to the interface.
    /// </summary>
    /// <param name="names">The names of the type parameters.</param>
    /// <returns>The builder instance for chaining.</returns>
    public InterfaceBuilder WithTypeParameters(params string[] names)
    {
        foreach (var name in names)
        {
            WithTypeParameter(name);
        }
        return this;
    }

    /// <summary>
    /// Adds a type constraint to the interface.
    /// </summary>
    /// <param name="constraint">The constraint clause (e.g., "where T : class").</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the constraint is null or whitespace.</exception>
    public InterfaceBuilder WithTypeConstraint(string constraint)
    {
        if (string.IsNullOrWhiteSpace(constraint))
            throw new ArgumentException("Type constraint cannot be null or whitespace.", nameof(constraint));
        
        _typeConstraints.Add(constraint);
        return this;
    }

    /// <summary>
    /// Adds a method to the interface.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnTypeName">The return type of the method.</param>
    /// <returns>A method builder for configuring the method.</returns>
    public MethodBuilder AddMethod(string methodName, string returnTypeName)
    {
        var methodBuilder = new MethodBuilder(methodName, returnTypeName);
        _members.Add(methodBuilder);
        return methodBuilder;
    }

    /// <summary>
    /// Adds a method to the interface with configuration.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnTypeName">The return type of the method.</param>
    /// <param name="configure">Action to configure the method.</param>
    /// <returns>The interface builder for chaining.</returns>
    public InterfaceBuilder AddMethod(string methodName, string returnTypeName, Action<MethodBuilder> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var methodBuilder = AddMethod(methodName, returnTypeName);
        configure(methodBuilder);
        return this;
    }

    /// <summary>
    /// Adds a method to the interface definition with no implementation.
    /// </summary>
    /// <remarks>This method creates a method definition with no implementation and adds it to the interface being
    /// built. Use this when defining an interface where the method is intended to be implemented by a concrete
    /// class.</remarks>
    /// <param name="methodName">The name of the method to add. Must be a valid identifier.</param>
    /// <param name="returnType">The return type of the method. Specify the type as a string, such as "void" or "int".</param>
    /// <returns>The current <see cref="InterfaceBuilder"/> instance, allowing for method chaining.</returns>
    public InterfaceBuilder AddMethodWithNoImplementation(string methodName, string returnType)
    {
        var methodBuilder = new MethodBuilder(methodName, returnType);
        methodBuilder.NoImplementation(this); // Mark as no implementation
        _members.Add(methodBuilder);
        return this;
    }

    /// <summary>
    /// Adds a property to the interface.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyTypeName">The type of the property.</param>
    /// <returns>A property builder for configuring the property.</returns>
    public PropertyBuilder AddProperty(string propertyName, string propertyTypeName)
    {
        var propertyBuilder = new PropertyBuilder(propertyName, propertyTypeName);
        _members.Add(propertyBuilder);
        return propertyBuilder;
    }

    /// <summary>
    /// Adds a property to the interface with configuration.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyTypeName">The type of the property.</param>
    /// <param name="configure">Action to configure the property.</param>
    /// <returns>The interface builder for chaining.</returns>
    public InterfaceBuilder AddProperty(string propertyName, string propertyTypeName, Action<PropertyBuilder> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var propertyBuilder = AddProperty(propertyName, propertyTypeName);
        configure(propertyBuilder);
        return this;
    }

    /// <summary>
    /// Generates the code for the interface and returns it as a string.
    /// </summary>
    /// <returns>The generated code as a string.</returns>
    public override string Build()
    {
        var builder = new CodeBuilder();

        // Build XML documentation if provided
        if (!string.IsNullOrEmpty(XmlDocSummary))
        {
            builder.AppendLine("/// <summary>");
            builder.AppendLine($"/// {XmlDocSummary}");
            builder.AppendLine("/// </summary>");
        }

        // Build attributes
        foreach (var attribute in Attributes)
        {
            builder.AppendLine($"[{attribute}]");
        }

        // Start interface declaration
        builder.Append(GetAccessModifierString(AccessModifier));
        builder.Append("interface ");
        builder.Append(_interfaceName);

        // Add type parameters
        if (_typeParameters.Count > 0)
        {
            builder.Append("<");
            builder.Append(string.Join(", ", _typeParameters));
            builder.Append(">");
        }

        // Add base interfaces
        if (_baseInterfaceNames.Count > 0)
        {
            builder.Append(" : ");
            builder.Append(string.Join(", ", _baseInterfaceNames));
        }

        // Add type constraints
        foreach (var constraint in _typeConstraints)
        {
            builder.AppendLine();
            builder.Append($"    {constraint}");
        }

        builder.AppendLine();
        builder.AppendLine("{");
        builder.Indent();

        // Add members
        foreach (var member in _members)
        {
            var built = member.Build();
            if (!string.IsNullOrWhiteSpace(built))
            {
                builder.AppendLine(built);
            }
        }

        builder.Outdent();
        builder.AppendLine("}");
        return builder.ToString();
    }
}
