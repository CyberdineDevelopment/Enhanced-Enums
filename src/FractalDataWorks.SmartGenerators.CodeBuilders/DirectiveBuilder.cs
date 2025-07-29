using System;
using System.Collections.Generic;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Builds preprocessor directive blocks.
/// </summary>
public class DirectiveBuilder : IBuildable
{
    private readonly List<DirectiveClause> _clauses = [];

    /// <summary>
    /// Creates a new instance of <see cref="DirectiveBuilder"/>.
    /// </summary>
    /// <returns>A new DirectiveBuilder instance.</returns>
    public static DirectiveBuilder Create() => new();

    /// <summary>
    /// Adds an #if clause with the specified pre-processor condition (e.g. <c>NET6_0_OR_GREATER</c>).
    /// </summary>
    /// <param name="condition">The preprocessor condition.</param>
    /// <param name="body">The action to execute for the body of the directive.</param>
    /// <returns>The DirectiveBuilder instance for chaining.</returns>
    public DirectiveBuilder If(string condition, Action<ICodeBuilder> body)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("Condition cannot be null or whitespace.", nameof(condition));
        }

        if (body == null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        _clauses.Add(new DirectiveClause("if", condition, body));
        return this;
    }

    /// <summary>
    /// Adds an #elif clause with the specified condition.
    /// </summary>
    /// <param name="condition">The preprocessor condition.</param>
    /// <param name="body">The action to execute for the body of the directive.</param>
    /// <returns>The DirectiveBuilder instance for chaining.</returns>
    public DirectiveBuilder ElseIf(string condition, Action<ICodeBuilder> body)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("Condition cannot be null or whitespace.", nameof(condition));
        }

        if (body == null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        _clauses.Add(new DirectiveClause("elif", condition, body));
        return this;
    }

    /// <summary>
    /// Adds an #else clause.
    /// </summary>
    /// <param name="body">The action to execute for the body of the directive.</param>
    /// <returns>The DirectiveBuilder instance for chaining.</returns>
    public DirectiveBuilder Else(Action<ICodeBuilder> body)
    {
        if (body == null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        _clauses.Add(new DirectiveClause("else", null, body));
        return this;
    }

    /// <inheritdoc />
    public string Build()
    {
        var cb = new CodeBuilder();
        foreach (var clause in _clauses)
        {
            cb.AppendLine(string.Equals(clause.Keyword, "else", StringComparison.Ordinal) ? "#else" : $"#{clause.Keyword} {clause.Condition}");
            using (cb.WithIndent())
            {
                clause.Body(cb);
            }
        }

        cb.AppendLine("#endif");
        return cb.ToString();
    }

    /// <summary>
    /// Represents a single directive clause (if, elif, else).
    /// </summary>
    private sealed class DirectiveClause
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveClause"/> class.
        /// </summary>
        /// <param name="keyword">The directive keyword (if, elif, else).</param>
        /// <param name="condition">The condition (null for else clauses).</param>
        /// <param name="body">The body action.</param>
        public DirectiveClause(string keyword, string? condition, Action<ICodeBuilder> body)
        {
            Keyword = keyword;
            Condition = condition;
            Body = body;
        }

        /// <summary>
        /// Gets the directive keyword.
        /// </summary>
        public string Keyword { get; }

        /// <summary>
        /// Gets the directive condition.
        /// </summary>
        public string? Condition { get; }

        /// <summary>
        /// Gets the body action.
        /// </summary>
        public Action<ICodeBuilder> Body { get; }
    }
}
