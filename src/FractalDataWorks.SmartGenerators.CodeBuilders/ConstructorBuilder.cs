using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating constructor declarations.
/// </summary>
public class ConstructorBuilder : MemberBuilderBase<ConstructorBuilder>
{
    private readonly string _className;
    private readonly List<ConstructorParameterInfo> _parameters = [];
    private CodeBlockBuilder? _codeBlockBuilder;
    private DirectiveBuilder? _directiveBuilder;
    private readonly List<string> _baseConstructorArgs = [];
    private readonly List<string> _thisConstructorArgs = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorBuilder"/> class with default class name 'X'.
    /// </summary>
    public ConstructorBuilder()
        : this("X")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorBuilder"/> class.
    /// </summary>
    /// <param name="className">The name of the class that this constructor is for.</param>
    public ConstructorBuilder(string className)
        : base(className, string.Empty) // Constructors don't have a return type
    {
        _className = className;
    }

    /// <summary>
    /// Adds a parameter to the constructor. This is the canonical Add* method per naming guidelines.
    /// </summary>
    /// <param name="parameterTypeName">The type of the parameter.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="defaultValue">The default value for the parameter (optional).</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when parameter type or name is invalid or a duplicate.</exception>
    public ConstructorBuilder AddParameter(string parameterTypeName, string parameterName, string? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(parameterTypeName))
        {
            throw new ArgumentException("Parameter type cannot be null or whitespace.", nameof(parameterTypeName));
        }

        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or whitespace.", nameof(parameterName));
        }

        // Check for duplicate parameter names
        if (_parameters.Any(p => string.Equals(p.Name, parameterName, StringComparison.Ordinal)))
        {
            throw new ArgumentException($"A parameter with the name '{parameterName}' already exists.", nameof(parameterName));
        }

        _parameters.Add(new ConstructorParameterInfo
        {
            Name = parameterName,
            Type = parameterTypeName,
            DefaultValue = defaultValue,
        });

        return this;
    }

    /// <summary>
    /// Adds a parameter using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="defaultValue">The default value for the parameter (optional).</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder AddParameter<T>(string parameterName, string? defaultValue = null) => AddParameter(typeof(T).Name, parameterName, defaultValue);

    /// <summary>
    /// Adds a body statement to the constructor.
    /// </summary>
    /// <param name="bodyContent">The body content to add as a statement.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder AddBody(string bodyContent)
    {
        if (bodyContent == null)
        {
            throw new ArgumentNullException(nameof(bodyContent));
        }

        _codeBlockBuilder ??= new CodeBlockBuilder();
        _codeBlockBuilder.AddStatement(bodyContent);
        return this;
    }

    /// <summary>
    /// Adds a code block to the constructor using a builder action.
    /// </summary>
    /// <param name="blockBuilder">The action that builds the code block.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder AddBody(Action<ICodeBuilder> blockBuilder)
    {
        if (blockBuilder == null)
        {
            throw new ArgumentNullException(nameof(blockBuilder));
        }

        return WithBody(blockBuilder);
    }

    /// <summary>
    /// Sets the body of the constructor.
    /// </summary>
    /// <param name="bodyBuilder">An action that will build the body of the constructor.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder WithBody(Action<ICodeBuilder> bodyBuilder)
    {
        if (bodyBuilder == null)
        {
            throw new ArgumentNullException(nameof(bodyBuilder));
        }

        _codeBlockBuilder = new CodeBlockBuilder(bodyBuilder);
        return this;
    }

    /// <summary>
    /// Sets the constructor body via string content. Braces and formatting are handled internally.
    /// </summary>
    /// <param name="bodyContent">The content of the constructor body as a string.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder WithBody(string bodyContent)
    {
        if (bodyContent == null)
        {
            throw new ArgumentNullException(nameof(bodyContent));
        }

        _codeBlockBuilder = new CodeBlockBuilder(bodyContent);
        return this;
    }

    /// <summary>
    /// Adds a code block that will be wrapped in preprocessor directives (#if / #elif).
    /// </summary>
    /// <param name="condition">The preprocessor condition.</param>
    /// <param name="blockBuilder">The action that builds the code block for this condition.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder AddBodyForDirective(string condition, Action<ICodeBuilder> blockBuilder)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("Condition cannot be null or whitespace.", nameof(condition));
        }

        if (blockBuilder is null)
        {
            throw new ArgumentNullException(nameof(blockBuilder));
        }

        if (_directiveBuilder == null)
        {
            _directiveBuilder = DirectiveBuilder.Create().If(condition, blockBuilder);
        }
        else
        {
            _directiveBuilder.ElseIf(condition, blockBuilder);
        }

        return this;
    }

    /// <summary>
    /// Adds an #else body to directive sequence.
    /// </summary>
    /// <param name="blockBuilder">The action that builds the code block for the else branch.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder AddElseBody(Action<ICodeBuilder> blockBuilder)
    {
        if (blockBuilder is null)
        {
            throw new ArgumentNullException(nameof(blockBuilder));
        }

        if (_directiveBuilder == null)
        {
            throw new InvalidOperationException("Call AddBodyForDirective() before AddElseBody().");
        }

        _directiveBuilder.Else(blockBuilder);
        return this;
    }

    /// <summary>
    /// Adds arguments to pass to the base class constructor.
    /// </summary>
    /// <param name="args">The arguments to pass to the base constructor.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder WithBaseCall(params string[] args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (_thisConstructorArgs.Count > 0)
        {
            throw new InvalidOperationException("Cannot call both the base constructor and another constructor (this) from the same constructor.");
        }

        _baseConstructorArgs.AddRange(args);
        return this;
    }

    /// <summary>
    /// Adds arguments to pass to another constructor in the same class.
    /// </summary>
    /// <param name="args">The arguments to pass to the other constructor.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ConstructorBuilder WithThisCall(params string[] args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (_baseConstructorArgs.Count > 0)
        {
            throw new InvalidOperationException("Cannot call both the base constructor and another constructor (this) from the same constructor.");
        }

        _thisConstructorArgs.AddRange(args);
        return this;
    }

    /// <summary>
    /// Generates the C# code for the constructor.
    /// </summary>
    /// <returns>The generated C# code.</returns>
    public override string Build()
    {
        var sb = new System.Text.StringBuilder();
        GenerateXmlDocumentation(sb);
        GenerateAttributes(sb);

        // Build constructor declaration
        // Static constructors in C# have special syntax: they don't have access modifiers and the 'static' keyword comes before the class name
        if ((Modifiers & Modifiers.Static) == Modifiers.Static)
        {
            sb.Append("static ").Append(_className).Append('(');
        }
        else
        {
            sb.Append(GetAccessModifierString(AccessModifier))
            .Append(GetModifierString(Modifiers))
            .Append(_className)
            .Append('(');
        }

        // Build parameters
        for (var i = 0; i < _parameters.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(_parameters[i].Type)
            .Append(' ')
            .Append(_parameters[i].Name);
        }

        sb.Append(')');

        // Add base or this constructor call if needed
        if (_baseConstructorArgs.Count > 0)
        {
            sb.Append(" : base(");
            sb.Append(string.Join(", ", _baseConstructorArgs));
            sb.Append(')');
        }
        else if (_thisConstructorArgs.Count > 0)
        {
            sb.Append(" : this(");
            sb.Append(string.Join(", ", _thisConstructorArgs));
            sb.Append(')');
        }

        // Build constructor body
        sb.AppendLine("{");
        if (_directiveBuilder != null)
        {
            var body = _directiveBuilder.Build();
            var indented = string.Join("\n", body.Split('\n').Select(l => string.IsNullOrWhiteSpace(l) ? l : "    " + l));
            sb.AppendLine(indented);
        }
        else if (_codeBlockBuilder != null)
        {
            sb.Append(_codeBlockBuilder.Build());
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}
