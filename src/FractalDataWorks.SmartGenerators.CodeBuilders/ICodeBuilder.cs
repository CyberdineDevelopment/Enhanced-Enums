using System;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Defines methods for building code with proper formatting and indentation.
/// </summary>
public interface ICodeBuilder
{
    /// <summary>
    /// Appends a line to the code builder with proper indentation.
    /// </summary>
    /// <param name="line">The line to append.</param>
    /// <returns>The code builder instance for chaining.</returns>
    ICodeBuilder AppendLine(string line = "");

    /// <summary>
    /// Appends text to the code builder, potentially with indentation if at the start of a line.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>The code builder instance for chaining.</returns>
    ICodeBuilder Append(string text);

    /// <summary>
    /// Increases the indentation level.
    /// </summary>
    /// <returns>The code builder instance for chaining.</returns>
    ICodeBuilder Indent();

    /// <summary>
    /// Decreases the indentation level.
    /// </summary>
    /// <returns>The code builder instance for chaining.</returns>
    ICodeBuilder Outdent();

    /// <summary>
    /// Appends a block opening, increasing indentation.
    /// </summary>
    /// <returns>The code builder instance for chaining.</returns>
    ICodeBuilder OpenBlock();

    /// <summary>
    /// Appends a block closing, decreasing indentation.
    /// </summary>
    /// <returns>The code builder instance for chaining.</returns>
    ICodeBuilder CloseBlock();

    /// <summary>
    /// Appends a standard header for generated files.
    /// </summary>
    /// <returns>The code builder instance for chaining.</returns>
    ICodeBuilder AppendGeneratedCodeHeader();

    /// <summary>
    /// Appends a namespace declaration.
    /// </summary>
    /// <param name="namespaceName">The namespace name.</param>
    /// <returns>The code builder instance for chaining.</returns>
    ICodeBuilder AppendNamespace(string namespaceName);

    /// <summary>
    /// Creates a temporary indentation scope that will be automatically decreased when disposed.
    /// </summary>
    /// <returns>An IDisposable that will decrease the indentation when disposed.</returns>
    IDisposable WithIndent();

    /// <summary>
    /// Generates the code and returns it as a string.
    /// </summary>
    /// <returns>The generated code as a string.</returns>
    string Build();

    /// <summary>
    /// Returns the built code as a string.
    /// </summary>
    /// <returns>The built code as a string.</returns>
    string ToString();
}
