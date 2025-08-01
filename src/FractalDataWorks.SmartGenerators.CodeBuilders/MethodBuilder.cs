using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Fluent builder for generating method declarations.
/// </summary>
public class MethodBuilder : MemberBuilderBase<MethodBuilder>
{
    private readonly List<MethodParameterInfo> _parameters = [];
    private bool _hasNoImplementation; // Used to mark abstract methods or interface methods without bodies

    // Use builder pattern for method body
    private CodeBlockBuilder? _bodyBuilder;
    private DirectiveBuilder? _directiveBuilder;
    private readonly List<XmlDocParamInfo> _xmlDocParams = [];
    private string? _xmlDocReturns;
    private readonly List<XmlDocExceptionInfo> _xmlDocExceptions = [];
    private string? _expressionBody;

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodBuilder"/> class.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnTypeName">The return type of the method.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="methodName"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="returnTypeName"/> is null.</exception>
    public MethodBuilder(string methodName, string returnTypeName)
        : base(methodName, returnTypeName)
    {
        // Re-validate the return type name since MemberBuilderBase has been modified to accept empty types for constructors
        // For methods, we still require a non-empty return type name (void is still a valid type name)
        if (string.IsNullOrWhiteSpace(returnTypeName))
        {
            throw new ArgumentException("Return type name cannot be null or whitespace.", nameof(returnTypeName));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodBuilder"/> class with only method name.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    public MethodBuilder(string methodName)
        : this(methodName, "void")
    {
    }

    /// <summary>
    /// Adds a parameter to the method. This is the canonical Add* method per naming guidelines.
    /// </summary>
    /// <param name="parameterTypeName">The type of the parameter.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="defaultValue">The default value for the parameter (optional).</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when parameter type or name is invalid or a duplicate.</exception>
    public MethodBuilder AddParameter(string parameterTypeName, string parameterName, string? defaultValue = null)
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

        _parameters.Add(new MethodParameterInfo
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
    public MethodBuilder AddParameter<T>(string parameterName, string? defaultValue = null) => AddParameter(typeof(T).Name, parameterName, defaultValue);

    /// <summary>
    /// Adds XML documentation for a parameter.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="description">The description of the parameter.</param>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder WithXmlDocParam(string parameterName, string description)
    {
        _xmlDocParams.Add(new XmlDocParamInfo
        {
            Name = parameterName,
            Description = description,
        });

        return this;
    }

    /// <summary>
    /// Adds XML documentation for the return value.
    /// </summary>
    /// <param name="description">The description of the return value.</param>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder WithXmlDocReturns(string description)
    {
        _xmlDocReturns = description;
        return this;
    }

    /// <summary>
    /// Adds XML documentation for an exception.
    /// </summary>
    /// <param name="exceptionType">The type of the exception.</param>
    /// <param name="description">The description of the exception.</param>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder WithXmlDocException(string exceptionType, string description)
    {
        _xmlDocExceptions.Add(new XmlDocExceptionInfo
        {
            Type = exceptionType,
            Description = description,
        });

        return this;
    }

    /// <summary>
    /// Sets the return type of the method.
    /// </summary>
    /// <param name="returnTypeName">The return type name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder WithReturnType(string returnTypeName)
    {
        if (string.IsNullOrWhiteSpace(returnTypeName))
        {
            throw new ArgumentException("Return type name cannot be null or whitespace.", nameof(returnTypeName));
        }

        TypeNameInternal = returnTypeName;
        return this;
    }

    /// <summary>
    /// Sets the body of the method.
    /// </summary>
    /// <param name="bodyContent">The content to use as the body.</param>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder WithBody(string bodyContent)
    {
        if (bodyContent == null)
        {
            throw new ArgumentNullException(nameof(bodyContent));
        }

        _bodyBuilder = new CodeBlockBuilder(bodyContent);
        return this;
    }

    /// <summary>
    /// Sets the body of the method using a code block builder.
    /// </summary>
    /// <param name="blockBuilder">The action that will build the block.</param>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder WithBody(Action<ICodeBuilder> blockBuilder)
    {
        if (blockBuilder == null)
        {
            throw new ArgumentNullException(nameof(blockBuilder));
        }

        _bodyBuilder = new CodeBlockBuilder(blockBuilder);
        return this;
    }

    /// <summary>
    /// Marks this method as having no implementation (for interfaces or abstract methods).
    /// </summary>
    /// <typeparam name="TParent">The type of the parent builder.</typeparam>
    /// <param name="parent">The parent builder to return for chaining.</param>
    /// <returns>The parent builder for continued chaining.</returns>
    public TParent NoImplementation<TParent>(TParent parent)
        where TParent : class
    {
        // Mark as abstract if in a class, or just no body if in interface
        _hasNoImplementation = true;
        return parent;
    }

    /// <summary>
    /// Makes the method async.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.VisualStudio.Threading", "VSTHRD200:Avoid \"Async\" suffix in names of methods that do not return an awaitable type", Justification = "This is a builder method that sets the async modifier, not an async method itself")]
    public MethodBuilder MakeAsync()
    {
        Modifiers |= Modifiers.Async;
        return this;
    }

    /// <summary>
    /// Makes the method extern.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder MakeExtern()
    {
        Modifiers |= Modifiers.Extern;
        return this;
    }

    /// <summary>
    /// Sets an expression body for the method (=> expression;).
    /// </summary>
    /// <param name="expression">The expression for the expression-bodied method.</param>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder WithExpressionBody(string expression)
    {
        _expressionBody = expression ?? throw new ArgumentNullException(nameof(expression));
        return this;
    }

    /// <summary>
    /// Adds a code block that will be wrapped in a preprocessor directive (#if / #elif).
    /// First call becomes #if, subsequent calls become #elif.
    /// </summary>
    /// <param name="condition">The condition for the directive.</param>
    /// <param name="blockBuilder">The action that will build the block.</param>
    /// <returns>The builder instance for chaining.</returns>
    public MethodBuilder AddBodyForDirective(string condition, Action<ICodeBuilder> blockBuilder)
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
    /// Adds an "else" block to the current directive, allowing conditional logic to be extended.
    /// </summary>
    /// <remarks>This method must be called after <see cref="AddBodyForDirective"/> to ensure the directive is
    /// properly initialized. The provided <paramref name="blockBuilder"/> defines the content of the "else"
    /// block.</remarks>
    /// <param name="blockBuilder">A delegate that builds the content of the "else" block. The delegate receives an <see cref="ICodeBuilder"/>
    /// instance to construct the block.</param>
    /// <returns>The current <see cref="MethodBuilder"/> instance, allowing method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="AddBodyForDirective"/> has not been called prior to invoking this method.</exception>
    public MethodBuilder AddElseBody(Action<ICodeBuilder> blockBuilder)
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
    /// Builds code for this method.
    /// </summary>
    /// <param name="codeBuilder">The code builder to use.</param>
    public void Build(ICodeBuilder codeBuilder)
    {
        if (codeBuilder is null) throw new ArgumentNullException(nameof(codeBuilder));

        // Build XML documentation if provided
        if (!string.IsNullOrEmpty(XmlDocSummary))
        {
            codeBuilder.AppendLine("/// <summary>");
            codeBuilder.AppendLine($"/// {XmlDocSummary}");
            codeBuilder.AppendLine("/// </summary>");
        }

        // Build XML documentation for parameters
        var parameters = this.GetXmlDocParams();
        foreach (var param in parameters)
        {
            codeBuilder.AppendLine($"/// <param name=\"{param.Name}\">{param.Description}</param>");
        }

        // If expression body is set, emit expression-bodied method
        if (_expressionBody != null)
        {
            var parameterList = string.Join(", ", _parameters.Select(p => $"{p.Type} {p.Name}"));
            codeBuilder.AppendLine($"{GetAccessModifierString(AccessModifier)}{GetModifierString(Modifiers)} {TypeName} {Name}({parameterList}) => {_expressionBody};");
            return;
        }

        // Build XML documentation for returns
        var returns = this.GetXmlDocReturns();
        if (!string.IsNullOrEmpty(returns))
        {
            codeBuilder.AppendLine($"/// <returns>{returns}</returns>");
        }

        // Build XML documentation for exceptions
        var exceptions = this.GetXmlDocExceptions();
        foreach (var exception in exceptions)
        {
            codeBuilder.AppendLine($"/// <exception cref=\"{exception.Type}\">{exception.Description}</exception>");
        }

        // Build the method signature
        codeBuilder.Append(GetAccessModifierString(AccessModifier));
        codeBuilder.Append(GetModifierString(Modifiers));
        codeBuilder.Append($"{TypeName} {Name}(");

        // Build parameters in correct C# syntax: type name
        for (var i = 0; i < _parameters.Count; i++)
        {
            if (i > 0)
            {
                codeBuilder.Append(", ");
            }

            codeBuilder.Append($"{_parameters[i].Type} {_parameters[i].Name}");
        }

        codeBuilder.Append(")");

        // Check if this is an abstract or interface method
        if (_hasNoImplementation)
        {
            codeBuilder.AppendLine(";");
            return;
        }

        // Build method body
        codeBuilder.AppendLine();
        codeBuilder.OpenBlock();

        if (_directiveBuilder != null)
        {
            var body = _directiveBuilder.Build();
            var indented = string.Join("\n", body.Split('\n').Select(l => string.IsNullOrWhiteSpace(l) ? l : "    " + l));
            codeBuilder.AppendLine(indented);
        }
        else if (_bodyBuilder != null)
        {
            var body = _bodyBuilder.Build();
            var indentedBody = string.Join("\n", body.Split('\n').Select(line => string.IsNullOrWhiteSpace(line) ? line : "    " + line));
            codeBuilder.AppendLine(indentedBody);
        }
        else
        {
            codeBuilder.AppendLine("throw new System.NotImplementedException();");
        }

        codeBuilder.CloseBlock();
    }

    /// <inheritdoc />
    public override string Build()
    {
        var sb = new StringBuilder();

        // Build XML documentation
        GenerateXmlDocumentation(sb);

        // Build attributes
        GenerateAttributes(sb);

        // Build XML documentation for parameters
        var parameters = this.GetXmlDocParams();
        foreach (var param in parameters)
        {
            sb.AppendLine($"/// <param name=\"{param.Name}\">{param.Description}</param>");
        }

        // Build XML documentation for returns
        var returns = this.GetXmlDocReturns();
        if (!string.IsNullOrEmpty(returns))
        {
            sb.AppendLine($"/// <returns>{returns}</returns>");
        }

        // Build XML documentation for exceptions
        var exceptions = this.GetXmlDocExceptions();
        foreach (var exception in exceptions)
        {
            sb.AppendLine($"/// <exception cref=\"{exception.Type}\">{exception.Description}</exception>");
        }

        // If expression body is set, emit expression-bodied method
        if (_expressionBody != null)
        {
            var parameterList = string.Join(", ", _parameters.Select(p => $"{p.Type} {p.Name}"));
            sb.AppendLine($"{GetAccessModifierString(AccessModifier)}{GetModifierString(Modifiers)} {TypeName} {Name}({parameterList}) => {_expressionBody};");
            return sb.ToString();
        }

        // Build the method signature
        sb.Append(GetAccessModifierString(AccessModifier));
        sb.Append(GetModifierString(Modifiers));
        sb.Append($"{TypeName} {Name}(");

        // Build parameters in correct C# syntax: type name
        for (var i = 0; i < _parameters.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append($"{_parameters[i].Type} {_parameters[i].Name}");
        }

        sb.Append(')');

        // Check if this is an abstract or interface method
        if (_hasNoImplementation)
        {
            sb.AppendLine(";");
            return sb.ToString();
        }

        // Build method body
        sb.AppendLine();
        sb.AppendLine("{");

        if (_directiveBuilder != null)
        {
            var body = _directiveBuilder.Build();
            var indented = string.Join("\n", body.Split('\n').Select(l => string.IsNullOrWhiteSpace(l) ? l : "    " + l));
            sb.AppendLine(indented);
        }
        else if (_bodyBuilder != null)
        {
            var body = _bodyBuilder.Build();
            var indentedBody = string.Join("\n", body.Split('\n').Select(line => string.IsNullOrWhiteSpace(line) ? line : "    " + line));
            sb.AppendLine(indentedBody);
        }
        else
        {
            sb.AppendLine("    throw new System.NotImplementedException();");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private List<XmlDocParamInfo> GetXmlDocParams() => _xmlDocParams;

    private string? GetXmlDocReturns() => _xmlDocReturns;

    private List<XmlDocExceptionInfo> GetXmlDocExceptions() => _xmlDocExceptions;
}
