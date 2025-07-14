using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Result of running the generator.
/// </summary>
public class GeneratorRunResult
{
    public Dictionary<string, string> GeneratedSources { get; set; } = new();
    public ImmutableArray<Diagnostic> Diagnostics { get; set; } = ImmutableArray<Diagnostic>.Empty;
}
