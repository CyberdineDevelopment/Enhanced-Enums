using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides expectations for code blocks.
/// </summary>
public class CodeBlockExpectations
{
    private readonly BlockSyntax _blockSyntax;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBlockExpectations"/> class.
    /// </summary>
    /// <param name="blockSyntax">The syntax node being evaluated.</param>
    public CodeBlockExpectations(BlockSyntax blockSyntax)
    {
        _blockSyntax = blockSyntax;
    }

    /// <summary>
    /// Expects the block to contain at least one statement of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of statement to look for.</typeparam>
    /// <returns>The current expectations instance for chaining.</returns>
    public CodeBlockExpectations HasStatementOfType<T>()
        where T : StatementSyntax
    {
        var match = _blockSyntax.Statements.OfType<T>().Any();
        match.ShouldBeTrue($"Expected block to contain a statement of type {typeof(T).Name}.");
        return this;
    }

    /// <summary>
    /// Expects the block to contain an invocation of the specified method name.
    /// </summary>
    /// <param name="methodName">The name of the method to find in invocations.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public CodeBlockExpectations HasInvocation(string methodName)
    {
        var found = _blockSyntax.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .Any(inv => inv.Expression.ToString().Contains(methodName));
        found.ShouldBeTrue($"Expected block to contain invocation of method '{methodName}'.");
        return this;
    }

    /// <summary>
    /// Expects the block to contain a return statement.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public CodeBlockExpectations HasReturnStatement()
    {
        return HasStatementOfType<Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax>();
    }

    /// <summary>
    /// Expects the block to contain the specified number of statements.
    /// </summary>
    /// <param name="count">The expected number of statements.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public CodeBlockExpectations HasStatementCount(int count)
    {
        _blockSyntax.Statements.Count.ShouldBe(count, $"Expected {count} statements but found {_blockSyntax.Statements.Count}.");
        return this;
    }

    /// <summary>
    /// Expects the block's statement at the specified index to match the given text.
    /// </summary>
    /// <param name="index">The zero-based index of the statement to check.</param>
    /// <param name="expected">The expected text representation of the statement.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public CodeBlockExpectations HasStatement(int index, string expected)
    {
        var stmt = _blockSyntax.Statements[index];
        stmt.ToString().Trim().ShouldBe(expected, $"Expected statement at index {index} to be '{expected}' but was '{stmt.ToString().Trim()}'.");
        return this;
    }
}
