using System;
using System.Collections.Generic;
using System.Text;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Builder for generating C# namespace code.
/// </summary>
public class NamespaceBuilder : ICodeBuilder
{
    private const string _indentString = "    ";
    private readonly string _namespaceName;
    private readonly List<string> _usingStatements = [];
    private readonly List<ICodeBuilder> _members = [];
    private readonly StringBuilder _codeBuilder = new();
    private int _indentLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamespaceBuilder"/> class.
    /// </summary>
    /// <param name="namespaceName">The namespace name.</param>
    public NamespaceBuilder(string namespaceName)
    {
        _namespaceName = namespaceName;
    }

    /// <summary>
    /// Adds a using statement to the namespace.
    /// </summary>
    /// <param name="usingStatement">The using statement to add.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddUsing(string usingStatement)
    {
        if (!_usingStatements.Contains(usingStatement))
        {
            _usingStatements.Add(usingStatement);
        }

        return this;
    }

    /// <summary>
    /// Adds a using statement to the namespace (alternative naming convention).
    /// </summary>
    /// <param name="usingStatement">The using statement to add.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder WithUsing(string usingStatement) => AddUsing(usingStatement);

    /// <summary>
    /// Adds a class builder to the namespace.
    /// </summary>
    /// <param name="classBuilder">The class builder to add.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddClass(ClassBuilder classBuilder)
    {
        _members.Add(classBuilder);
        return this;
    }

    /// <summary>
    /// Adds a class to the namespace using a configuration action.
    /// </summary>
    /// <param name="classConfiguration">The action to configure the class builder.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddClass(Action<ClassBuilder> classConfiguration)
    {
        if (classConfiguration is null) throw new ArgumentNullException(nameof(classConfiguration));

        var classBuilder = new ClassBuilder();
        classConfiguration(classBuilder);
        return AddClass(classBuilder);
    }

    /// <summary>
    /// Adds a class builder to the namespace (alternative naming convention).
    /// </summary>
    /// <param name="classBuilder">The class builder to add.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder WithClass(ClassBuilder classBuilder) => AddClass(classBuilder);

    /// <summary>
    /// Adds an interface builder to the namespace.
    /// </summary>
    /// <param name="interfaceBuilder">The interface builder to add.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddInterface(InterfaceBuilder interfaceBuilder)
    {
        _members.Add(interfaceBuilder);
        return this;
    }

    /// <summary>
    /// Adds an interface to the namespace using a configuration action.
    /// </summary>
    /// <param name="interfaceConfiguration">The action to configure the interface builder.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddInterface(Action<InterfaceBuilder> interfaceConfiguration)
    {
        if (interfaceConfiguration is null) throw new ArgumentNullException(nameof(interfaceConfiguration));

        var builder = new InterfaceBuilder();
        interfaceConfiguration(builder);
        _members.Add(builder);
        return this;
    }

    /// <summary>
    /// Adds an enum builder to the namespace.
    /// </summary>
    /// <param name="enumBuilder">The enum builder to add.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddEnum(EnumBuilder enumBuilder)
    {
        _members.Add(enumBuilder);
        return this;
    }

    /// <summary>
    /// Adds an enum to the namespace using a configuration action.
    /// </summary>
    /// <param name="enumConfiguration">The action to configure the enum builder.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddEnum(Action<EnumBuilder> enumConfiguration)
    {
        if (enumConfiguration is null) throw new ArgumentNullException(nameof(enumConfiguration));

        var enumBuilder = new EnumBuilder();
        enumConfiguration(enumBuilder);
        return AddEnum(enumBuilder);
    }

    /// <summary>
    /// Adds an enum builder to the namespace (alternative naming convention).
    /// </summary>
    /// <param name="enumBuilder">The enum builder to add.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder WithEnum(EnumBuilder enumBuilder) => AddEnum(enumBuilder);

    /// <summary>
    /// Adds a record builder to the namespace.
    /// </summary>
    /// <param name="recordBuilder">The record builder to add.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddRecord(RecordBuilder recordBuilder)
    {
        _members.Add(recordBuilder);
        return this;
    }

    /// <summary>
    /// Adds a record to the namespace using a configuration action.
    /// </summary>
    /// <param name="recordConfiguration">The action to configure the record builder.</param>
    /// <returns>The namespace builder for method chaining.</returns>
    public NamespaceBuilder AddRecord(Action<RecordBuilder> recordConfiguration)
    {
        if (recordConfiguration is null) throw new ArgumentNullException(nameof(recordConfiguration));

        var recordBuilder = new RecordBuilder();
        recordConfiguration(recordBuilder);
        return AddRecord(recordBuilder);
    }

    /// <summary>
    /// Generates the C# code for the namespace.
    /// </summary>
    /// <returns>The generated C# code.</returns>
    public string Build()
    {
        var sb = new StringBuilder();

        // Add using statements
        foreach (var usingStatement in _usingStatements)
        {
            sb.AppendLine($"using {usingStatement};");
        }

        if (_usingStatements.Count > 0)
        {
            sb.AppendLine();
        }

        // Start namespace
        sb.AppendLine($"namespace {_namespaceName};");
        sb.AppendLine();

        // Add members
        for (var i = 0; i < _members.Count; i++)
        {
            sb.Append(_members[i].Build());

            if (i < _members.Count - 1)
            {
                sb.AppendLine();
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public ICodeBuilder AppendLine(string line = "")
    {
        for (var i = 0; i < _indentLevel; i++)
        {
            _codeBuilder.Append(_indentString);
        }

        _codeBuilder.AppendLine(line);
        return this;
    }

    /// <inheritdoc />
    public ICodeBuilder Append(string text)
    {
        _codeBuilder.Append(text);
        return this;
    }

    /// <inheritdoc />
    public ICodeBuilder Indent()
    {
        _indentLevel++;
        return this;
    }

    /// <inheritdoc />
    public ICodeBuilder Outdent()
    {
        if (_indentLevel > 0)
        {
            _indentLevel--;
        }

        return this;
    }

    /// <inheritdoc />
    public ICodeBuilder OpenBlock()
    {
        AppendLine("{");
        Indent();
        return this;
    }

    /// <inheritdoc />
    public ICodeBuilder CloseBlock()
    {
        Outdent();
        AppendLine("}");
        return this;
    }

    /// <inheritdoc />
    public ICodeBuilder AppendGeneratedCodeHeader()
    {
        AppendLine("// <auto-generated>");
        AppendLine("// This code was generated by a tool.");
        AppendLine("// Runtime Version: .NET");
        AppendLine("//");
        AppendLine("// Changes to this file may cause incorrect behavior and will be lost if");
        AppendLine("// the code is regenerated.");
        AppendLine("// </auto-generated>");
        AppendLine();
        return this;
    }

    /// <inheritdoc />
    public ICodeBuilder AppendNamespace(string namespaceName)
    {
        AppendLine($"namespace {namespaceName}");
        OpenBlock();
        return this;
    }

    /// <inheritdoc />
    public IDisposable WithIndent()
    {
        Indent();
        return new IndentScope(this);
    }

    /// <inheritdoc />
    public override string ToString() => _codeBuilder.ToString();

    private sealed class IndentScope : IDisposable
    {
        private readonly NamespaceBuilder _builder;

        public IndentScope(NamespaceBuilder builder)
        {
            _builder = builder;
        }

        public void Dispose() => _builder.Outdent();
    }
}
