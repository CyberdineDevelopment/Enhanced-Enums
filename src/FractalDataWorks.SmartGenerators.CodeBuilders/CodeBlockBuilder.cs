using System;
using System.Collections.Generic;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Builder for generating code blocks with proper indentation and formatting.
/// </summary>
internal sealed class CodeBlockBuilder : IBuildable
{
    private readonly string _content;
    private readonly Action<ICodeBuilder>? _blockBuilder;
    private readonly List<string> _statements;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBlockBuilder"/> class with string content.
    /// </summary>
    /// <param name="blockContent">The content of the block.</param>
    public CodeBlockBuilder(string blockContent)
    {
        _content = blockContent ?? throw new ArgumentNullException(nameof(blockContent));
        _blockBuilder = null;
        _statements = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBlockBuilder"/> class with a builder action.
    /// </summary>
    /// <param name="blockBuilder">The action that will build the block.</param>
    public CodeBlockBuilder(Action<ICodeBuilder> blockBuilder)
    {
        _blockBuilder = blockBuilder ?? throw new ArgumentNullException(nameof(blockBuilder));
        _content = string.Empty;
        _statements = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBlockBuilder"/> class for building statements.
    /// </summary>
    public CodeBlockBuilder()
    {
        _content = string.Empty;
        _blockBuilder = null;
        _statements = [];
    }

    /// <summary>
    /// Adds a statement to the code block.
    /// </summary>
    /// <param name="statement">The statement to add.</param>
    /// <returns>The current instance for method chaining.</returns>
    public CodeBlockBuilder AddStatement(string statement)
    {
        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        _statements.Add(statement);
        return this;
    }

    /// <summary>
    /// Builds and returns the code as a string.
    /// </summary>
    /// <returns>The generated code block as a string.</returns>
    public string Build()
    {
        var codeBuilder = new CodeBuilder();

        // Emit added statements if any
        if (_statements.Count > 0)
        {
            foreach (var stmt in _statements)
            {
                codeBuilder.AppendLine(stmt);
            }

            return codeBuilder.ToString();
        }

        if (_blockBuilder != null)
        {
            _blockBuilder(codeBuilder);
        }
        else if (!string.IsNullOrEmpty(_content))
        {
            var lines = _content.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
            int? minIndent = null;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var indent = 0;
                foreach (var c in line)
                {
                    if (c == ' ')
                    {
                        indent++;
                    }
                    else if (c == '\t')
                    {
                        indent += 4;
                    }
                    else
                    {
                        break;
                    }
                }

                minIndent = minIndent.HasValue ? Math.Min(minIndent.Value, indent) : indent;
            }

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    codeBuilder.AppendLine();
                    continue;
                }

                var indent = 0;
                foreach (var c in line)
                {
                    if (c == ' ')
                    {
                        indent++;
                    }
                    else if (c == '\t')
                    {
                        indent += 4;
                    }
                    else
                    {
                        break;
                    }
                }

                var adjustedLine = line;
                if (minIndent.HasValue && indent >= minIndent.Value)
                {
                    adjustedLine = line.Substring(minIndent.Value);
                }

                codeBuilder.AppendLine(adjustedLine);
            }
        }

        return codeBuilder.ToString();
    }
}
