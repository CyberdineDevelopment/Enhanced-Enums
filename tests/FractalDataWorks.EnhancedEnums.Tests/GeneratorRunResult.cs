using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Result of running the generator.
/// </summary>
public class GeneratorRunResult
{
    public GeneratedSource[] GeneratedSources { get; set; } = [];
    public ImmutableArray<Diagnostic> Diagnostics { get; set; } = ImmutableArray<Diagnostic>.Empty;
    
    /// <summary>
    /// Gets a generated source by its hint name.
    /// </summary>
    public string this[string hintName] => GeneratedSources.FirstOrDefault(s => s.HintName == hintName)?.SourceText 
        ?? throw new KeyNotFoundException($"Generated source with hint name '{hintName}' was not found.");
    
    /// <summary>
    /// Checks if a generated source exists with the given hint name.
    /// </summary>
    public bool ContainsSource(string hintName) => GeneratedSources.Any(s => s.HintName == hintName);
}

/// <summary>
/// Represents a generated source file.
/// </summary>
public class GeneratedSource
{
    public string HintName { get; set; } = string.Empty;
    public string SourceText { get; set; } = string.Empty;
}
