using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Helper class for running source generators and examining their output.
/// </summary>
public static class SourceGeneratorTestHelper
{
    /// <summary>
    /// Runs a source generator with the specified sources and returns the generated output.
    /// </summary>
    /// <param name="generator">The source generator to run.</param>
    /// <param name="sources">The source code to compile.</param>
    /// <param name="diagnostics">The diagnostics produced during compilation.</param>
    /// <param name="additionalReferences">Additional assembly references to include.</param>
    /// <returns>A dictionary of generated sources with hintName as the key and content as the value.</returns>
    public static Dictionary<string, string> RunGenerator(
        IIncrementalGenerator generator,
        string[] sources,
        out ImmutableArray<Diagnostic> diagnostics,
        params MetadataReference[] additionalReferences)
    {
        var syntaxTrees = sources.Select(source =>
            CSharpSyntaxTree.ParseText(source)).ToArray();

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.IEnumerable<>).Assembly.Location),
        };

        if (additionalReferences.Length > 0)
        {
            references.AddRange(additionalReferences);
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Store the compilation diagnostics but still proceed with generation even if there are errors
        var compilationDiagnostics = compilation.GetDiagnostics();

        var driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [],
            parseOptions: (CSharpParseOptions)syntaxTrees[0].Options);

        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);

        var runResult = driver.GetRunResult();
        var generatedSources = runResult.Results[0].GeneratedSources;
        var output = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var generatedSource in generatedSources)
        {
            var content = generatedSource.SourceText.ToString();
            var hintName = generatedSource.HintName;
            output.Add(hintName, content);
        }

        // Return all diagnostics including ones from the generator
        diagnostics = runResult.Diagnostics.IsEmpty
            ? compilationDiagnostics
            : [.. compilationDiagnostics.Concat(runResult.Diagnostics)];
        return output;
    }

    /// <summary>
    /// Gets a syntax tree from the generated output.
    /// </summary>
    /// <param name="generatedOutput">The generated output.</param>
    /// <param name="hintName">The hint name of the generated file to get.</param>
    /// <returns>The syntax tree of the generated file.</returns>
    public static SyntaxTree GetSyntaxTree(Dictionary<string, string> generatedOutput, string hintName)
    {
        if (!generatedOutput.TryGetValue(hintName, out var source))
        {
            throw new InvalidOperationException($"Generated file {hintName} was not found.");
        }

        return CSharpSyntaxTree.ParseText(source);
    }

    /// <summary>
    /// Gets a compilation with generator output included.
    /// </summary>
    /// <param name="generator">The source generator to run.</param>
    /// <param name="sources">The source code to compile.</param>
    /// <param name="additionalReferences">Additional assembly references to include.</param>
    /// <returns>A tuple containing the output compilation and the run result.</returns>
    public static (Compilation OutputCompilation, GeneratorDriverRunResult RunResult) RunGeneratorAndCompile(
        IIncrementalGenerator generator,
        string[] sources,
        params MetadataReference[] additionalReferences)
    {
        // Create the initial compilation
        var runGenerator = RunGenerator(generator, sources, out _, additionalReferences);
        if (runGenerator.Count == 0)
        {
            throw new InvalidOperationException("No sources were generated.");
        }

        // Create a new compilation with the generated sources
        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source)).ToList();
        foreach (var generatedSource in runGenerator)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(generatedSource.Value, path: generatedSource.Key));
        }

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.IEnumerable<>).Assembly.Location),
        };

        if (additionalReferences.Length > 0)
        {
            references.AddRange(additionalReferences);
        }

        var outputCompilation = CSharpCompilation.Create(
            assemblyName: "TestOutput",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Run the generator to get the run result
        var driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [],
            parseOptions: (CSharpParseOptions)syntaxTrees[0].Options);

        driver = (CSharpGeneratorDriver)driver.RunGenerators(outputCompilation);

        return (outputCompilation, driver.GetRunResult());
    }

    /// <summary>
    /// Compiles the provided code and returns the resulting compilation.
    /// </summary>
    /// <param name="sources">The source code to compile.</param>
    /// <param name="diagnostics">The diagnostics produced during compilation.</param>
    /// <param name="additionalReferences">Additional assembly references to include.</param>
    /// <returns>The compilation output or null if compilation failed.</returns>
    public static Compilation? CompileCode(
        string[] sources,
        out ImmutableArray<Diagnostic> diagnostics,
        params MetadataReference[] additionalReferences)
    {
        var syntaxTrees = sources.Select(source =>
            CSharpSyntaxTree.ParseText(source)).ToArray();

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.IEnumerable<>).Assembly.Location),
        };

        if (additionalReferences.Length > 0)
        {
            references.AddRange(additionalReferences);
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestCompilation",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        diagnostics = compilation.GetDiagnostics();

        // Check if there are any errors
        var hasErrors = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

        return hasErrors ? null : compilation;
    }
}
