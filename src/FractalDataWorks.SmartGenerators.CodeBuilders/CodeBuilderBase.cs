using System;
using System.Collections.Generic;
using System.Text;
using FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Base class for all code builders that provides common functionality.
/// </summary>
/// <typeparam name="TBuilder">The builder type for fluent interface chaining.</typeparam>
public abstract class CodeBuilderBase<TBuilder> : ICodeBuilder, IDocumentationManagerProvider, IBuildable
    where TBuilder : CodeBuilderBase<TBuilder>
{
    private const string _indentString = "    ";

    /// <summary>
    /// The code builder for this instance.
    /// </summary>
    private readonly StringBuilder _codeBuilder = new();

    /// <summary>
    /// The current indentation level.
    /// </summary>
    private int _indentLevel;

    /// <summary>
    /// The access modifier for this element.
    /// </summary>
    private AccessModifier _accessModifier = AccessModifier.Public;

    /// <summary>
    /// The modifiers for this element.
    /// </summary>
    private Modifiers _modifiers = Modifiers.None;

    /// <summary>
    /// The XML documentation summary for this element.
    /// </summary>
    private string? _xmlDocSummary;

    /// <summary>
    /// The documentation manager for this element.
    /// </summary>
    private DocumentationManager? _documentationManager;

    /// <summary>
    /// The attributes for this element.
    /// </summary>
    private readonly List<string> _attributes = [];

    /// <summary>
    /// Gets the code builder for this instance.
    /// </summary>
    protected StringBuilder CodeBuilder => _codeBuilder;

    /// <summary>
    /// Gets or sets the current indentation level.
    /// </summary>
    protected int IndentLevel
    {
        get => _indentLevel;
        set => _indentLevel = value;
    }

    /// <summary>
    /// Gets or sets the access modifier for this element.
    /// </summary>
    protected AccessModifier AccessModifier
    {
        get => _accessModifier;
        set => _accessModifier = value;
    }

    /// <summary>
    /// Gets or sets the modifiers for this element.
    /// </summary>
    protected Modifiers Modifiers
    {
        get => _modifiers;
        set => _modifiers = value;
    }

    /// <summary>
    /// Gets or sets the XML documentation summary for this element.
    /// </summary>
    protected string? XmlDocSummary
    {
        get => _xmlDocSummary;
        set => _xmlDocSummary = value;
    }

    /// <summary>
    /// Gets the attributes for this element.
    /// </summary>
    protected IReadOnlyList<string> Attributes => _attributes.AsReadOnly();

    /// <summary>
    /// Gets the documentation manager for this element.
    /// </summary>
    public DocumentationManager? DocumentationManager => _documentationManager;

    /// <summary>
    /// Sets the documentation manager for this element.
    /// </summary>
    /// <param name="documentationManager">The documentation manager to use.</param>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder WithDocumentationManager(Documentation.DocumentationManager documentationManager)
    {
        _documentationManager = documentationManager ?? throw new ArgumentNullException(nameof(documentationManager));
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the access modifier for this element.
    /// </summary>
    /// <param name="accessModifier">The access modifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder WithAccessModifier(AccessModifier accessModifier)
    {
        _accessModifier = accessModifier;
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a modifier to this element.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder WithModifier(Modifiers modifier)
    {
        _modifiers |= modifier;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the XML documentation summary for this element.
    /// </summary>
    /// <param name="summary">The documentation summary.</param>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder WithXmlDocSummary(string summary)
    {
        _xmlDocSummary = summary;

        // If we have a documentation manager, update it too
        _documentationManager?.SetCustomDocumentation(summary);

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an attribute to this element.
    /// </summary>
    /// <param name="attributeText">The attribute text without square brackets.</param>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder AddAttribute(string attributeText)
    {
        _attributes.Add(attributeText);
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an attribute to the internal list.
    /// </summary>
    /// <param name="attributeText">The attribute text.</param>
    protected void AddAttributeInternal(string attributeText) => _attributes.Add(attributeText);

    /// <summary>
    /// Makes the element static.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakeStatic()
    {
        _modifiers |= Modifiers.Static;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element partial.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakePartial()
    {
        _modifiers |= Modifiers.Partial;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element sealed.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakeSealed()
    {
        _modifiers |= Modifiers.Sealed;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element abstract.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakeAbstract()
    {
        _modifiers |= Modifiers.Abstract;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element override its base implementation.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakeOverride()
    {
        _modifiers |= Modifiers.Override;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element public.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakePublic()
    {
        _accessModifier = AccessModifier.Public;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element private.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakePrivate()
    {
        _accessModifier = AccessModifier.Private;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element protected.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakeProtected()
    {
        _accessModifier = AccessModifier.Protected;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element internal.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakeInternal()
    {
        _accessModifier = AccessModifier.Internal;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element protected internal.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakeProtectedInternal()
    {
        _accessModifier = AccessModifier.ProtectedInternal;
        return (TBuilder)this;
    }

    /// <summary>
    /// Makes the element private protected.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public TBuilder MakePrivateProtected()
    {
        _accessModifier = AccessModifier.PrivateProtected;
        return (TBuilder)this;
    }

    /// <summary>
    /// Generates the code for this element.
    /// </summary>
    /// <returns>The generated code.</returns>
    public abstract string Build();

    /// <inheritdoc />
    public virtual ICodeBuilder AppendLine(string line = "")
    {
        for (var i = 0; i < _indentLevel; i++)
        {
            _codeBuilder.Append(_indentString);
        }

        _codeBuilder.AppendLine(line);
        return this;
    }

    /// <inheritdoc />
    public virtual ICodeBuilder Append(string text)
    {
        _codeBuilder.Append(text);
        return this;
    }

    /// <inheritdoc />
    public virtual ICodeBuilder Indent()
    {
        _indentLevel++;
        return this;
    }

    /// <inheritdoc />
    public virtual ICodeBuilder Outdent()
    {
        if (_indentLevel > 0)
        {
            _indentLevel--;
        }

        return this;
    }

    /// <inheritdoc />
    public virtual ICodeBuilder OpenBlock()
    {
        AppendLine("{");
        Indent();
        return this;
    }

    /// <inheritdoc />
    public virtual ICodeBuilder CloseBlock()
    {
        Outdent();
        AppendLine("}");
        return this;
    }

    /// <inheritdoc />
    public virtual ICodeBuilder AppendGeneratedCodeHeader()
    {
        AppendLine("// <auto-generated/>");
        AppendLine();
        return this;
    }

    /// <inheritdoc />
    public virtual ICodeBuilder AppendNamespace(string namespaceName)
    {
        AppendLine($"namespace {namespaceName}");
        OpenBlock();
        return this;
    }

    /// <inheritdoc />
    public virtual IDisposable WithIndent()
    {
        Indent();
        return new IndentScope(this);
    }

    /// <inheritdoc />
    public override string ToString() => _codeBuilder.ToString();

    /// <summary>
    /// Generates XML documentation for this element.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    protected void GenerateXmlDocumentation(StringBuilder sb)
    {
        if (sb == null)
        {
            throw new ArgumentNullException(nameof(sb));
        }

        if (!string.IsNullOrEmpty(_xmlDocSummary))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {_xmlDocSummary}");
            sb.AppendLine("/// </summary>");
        }
    }

    /// <summary>
    /// Generates attributes for this element.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    protected void GenerateAttributes(StringBuilder sb)
    {
        if (sb is null) throw new ArgumentNullException(nameof(sb));

        foreach (var attr in _attributes)
        {
            sb.AppendLine($"[{attr}]");
        }
    }

    /// <summary>
    /// Gets the string representation of a modifier.
    /// </summary>
    /// <param name="modifier">The modifier to convert.</param>
    /// <returns>The string representation.</returns>
    protected static string GetModifierString(Modifiers modifier)
    {
        var result = string.Empty;

        if ((modifier & Modifiers.Static) == Modifiers.Static)
        {
            result += "static ";
        }

        if ((modifier & Modifiers.Partial) == Modifiers.Partial)
        {
            result += "partial ";
        }

        if ((modifier & Modifiers.Sealed) == Modifiers.Sealed)
        {
            result += "sealed ";
        }

        if ((modifier & Modifiers.Override) == Modifiers.Override)
        {
            result += "override ";
        }

        if ((modifier & Modifiers.Virtual) == Modifiers.Virtual)
        {
            result += "virtual ";
        }

        if ((modifier & Modifiers.Abstract) == Modifiers.Abstract)
        {
            result += "abstract ";
        }

        if ((modifier & Modifiers.Async) == Modifiers.Async)
        {
            result += "async ";
        }

        if ((modifier & Modifiers.ReadOnly) == Modifiers.ReadOnly)
        {
            result += "readonly ";
        }

        if ((modifier & Modifiers.Required) == Modifiers.Required)
        {
            result += "required ";
        }

        return result;
    }

    /// <summary>
    /// Gets the string representation of an access modifier.
    /// </summary>
    /// <param name="accessModifier">The access modifier to convert.</param>
    /// <returns>The string representation.</returns>
    protected static string GetAccessModifierString(AccessModifier accessModifier)
    {
        return accessModifier switch
        {
            AccessModifier.Public => "public ",
            AccessModifier.Private => "private ",
            AccessModifier.Protected => "protected ",
            AccessModifier.Internal => "internal ",
            AccessModifier.ProtectedInternal => "protected internal ",
            AccessModifier.PrivateProtected => "private protected ",
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Disposable scope for automatic indentation management.
    /// </summary>
    private sealed class IndentScope : IDisposable
    {
        private readonly ICodeBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentScope"/> class.
        /// </summary>
        /// <param name="builder">The code builder.</param>
        public IndentScope(ICodeBuilder builder)
        {
            _builder = builder;
        }

        /// <inheritdoc />
        public void Dispose() => _builder.Outdent();
    }
}
