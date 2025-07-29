using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// A mock implementation of SourceProductionContext for testing source generators.
/// This class provides a simplified interface for capturing generated source code
/// and diagnostics during testing.
/// </summary>
public class TestGeneratorContext
{
    /// <summary>
    /// Gets the dictionary of generated source code files.
    /// </summary>
    /// <remarks>
    /// Keys are hint names for the source files, values are the source code content.
    /// </remarks>
    public Dictionary<string, string> GeneratedSources { get; } = [];

    /// <summary>
    /// Gets the list of reported diagnostics.
    /// </summary>
#pragma warning disable CA1002
    public List<Diagnostic> ReportedDiagnostics { get; } = [];
#pragma warning restore CA1002

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public static CancellationToken CancellationToken => CancellationToken.None;

    /// <summary>
    /// Adds a source file to the context.
    /// </summary>
    /// <param name="hintName">The hint name for the source file.</param>
    /// <param name="source">The source code.</param>
    public void AddSource(string hintName, string source)
    {
        GeneratedSources[hintName] = source;
    }

    /// <summary>
    /// Reports a diagnostic.
    /// </summary>
    /// <param name="diagnostic">The diagnostic to report.</param>
    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        ReportedDiagnostics.Add(diagnostic);
    }
}
