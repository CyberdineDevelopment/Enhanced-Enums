using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Base class for EnhancedEnumOption tests providing common functionality and proper setup.
/// </summary>
public abstract class EnhancedEnumOptionTestBase : IDisposable
{
    private readonly CancellationTokenSource _cts;
    private bool _disposed; // To detect redundant calls

    protected CancellationToken CancellationToken => _cts.Token;

    protected EnhancedEnumOptionTestBase()
    {
        _cts = new CancellationTokenSource();
        _cts.CancelAfter(TimeSpan.FromSeconds(30)); // Prevent runaway tests
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _cts?.Cancel();
                _cts?.Dispose();
            }

            // Dispose unmanaged resources (if any)

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~EnhancedEnumOptionTestBase()
    {
        Dispose(false);
    }

    /// <summary>
    /// Runs the generator with proper assembly scanning setup.
    /// </summary>
    protected static GeneratorRunResult RunGeneratorWithAssemblyScanning(
        string[] sources,
        params MetadataReference[] additionalReferences)
    {
        var generator = new EnhancedEnumOptionGenerator();
        var allReferences = GetDefaultReferences().Concat(additionalReferences).ToArray();

        // Add assembly scanning logic here
        var scannedAssemblies = ScanAssembliesForReferences();
        allReferences = allReferences.Concat(scannedAssemblies).ToArray();

        var output = SourceGeneratorTestHelper.RunGenerator(
            generator,
            sources,
            out var diagnostics,
            allReferences);

        return new GeneratorRunResult
        {
            GeneratedSources = output,
            Diagnostics = diagnostics
        };
    }

    /// <summary>
    /// Runs the generator without assembly scanning (for testing diagnostics).
    /// </summary>
    protected static GeneratorRunResult RunGenerator(
        string[] sources,
        params MetadataReference[] additionalReferences)
    {
        var generator = new EnhancedEnumOptionGenerator();
        var allReferences = GetDefaultReferences().Concat(additionalReferences).ToArray();

        var output = SourceGeneratorTestHelper.RunGenerator(
            generator,
            sources,
            out var diagnostics,
            allReferences);

        return new GeneratorRunResult
        {
            GeneratedSources = output,
            Diagnostics = diagnostics
        };
    }

    /// <summary>
    /// Runs the generator with specific assembly references for cross-assembly testing.
    /// </summary>
    protected static GeneratorRunResult RunGeneratorWithReferences(
        string[] sources,
        params MetadataReference[] references)
    {
        var generator = new EnhancedEnumOptionGenerator();
        var allReferences = GetDefaultReferences().Concat(references).ToArray();

        var output = SourceGeneratorTestHelper.RunGenerator(
            generator,
            sources,
            out var diagnostics,
            allReferences);

        return new GeneratorRunResult
        {
            GeneratedSources = output,
            Diagnostics = diagnostics
        };
    }

    /// <summary>
    /// Creates a compilation with EnhancedTypes references.
    /// </summary>
    protected Compilation CreateCompilationWithEnhancedEnumOption(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: CancellationToken);

        return CSharpCompilation.Create(
            $"TestAssembly_{Guid.NewGuid():N}",
            new[] { syntaxTree },
            GetDefaultReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>
    /// Creates a generator driver for incremental testing.
    /// </summary>
    protected static GeneratorDriver CreateGeneratorDriver(IIncrementalGenerator generator)
    {
        return CSharpGeneratorDriver.Create(
            generators: new[] { generator.AsSourceGenerator() },
            additionalTexts: ImmutableArray<AdditionalText>.Empty,
            parseOptions: CSharpParseOptions.Default,
            optionsProvider: null);
    }

    /// <summary>
    /// Gets the default references needed for compilation.
    /// </summary>
    protected static MetadataReference[] GetDefaultReferences()
    {
        // Get the basic references
        var refs = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EnhancedEnumOptionAttribute).Assembly.Location),
        };

        // Add System.Runtime for Attribute base class
        var systemRuntimePath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!,
            "System.Runtime.dll");
        if (System.IO.File.Exists(systemRuntimePath))
        {
            refs.Add(MetadataReference.CreateFromFile(systemRuntimePath));
        }

        // Add netstandard reference
        var netstandardPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!,
            "netstandard.dll");
        if (System.IO.File.Exists(netstandardPath))
        {
            refs.Add(MetadataReference.CreateFromFile(netstandardPath));
        }

        // Try to add SmartGenerators assembly if available
        var smartGeneratorsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "FractalDataWorks.SmartGenerators");
        if (smartGeneratorsAssembly != null)
        {
            refs.Add(MetadataReference.CreateFromFile(smartGeneratorsAssembly.Location));
        }

        return refs.ToArray();
    }

    /// <summary>
    /// Compiles source code to an assembly for runtime testing.
    /// </summary>
    protected System.Reflection.Assembly CompileToAssembly(params string[] sources)
    {
        var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s, cancellationToken: CancellationToken));

        var compilation = CSharpCompilation.Create(
            $"TestAssembly_{Guid.NewGuid():N}",
            syntaxTrees,
            GetDefaultReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new System.IO.MemoryStream();
        var result = compilation.Emit(ms, cancellationToken: CancellationToken);

        if (!result.Success)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString());

            throw new InvalidOperationException(
                $"Compilation failed with errors:\n{string.Join("\n", errors)}");
        }

        ms.Seek(0, System.IO.SeekOrigin.Begin);
        return System.Reflection.Assembly.Load(ms.ToArray());
    }

    /// <summary>
    /// Logs output for debugging (only in debug mode or when test fails).
    /// </summary>
    protected static void LogGeneratedCode(string fileName, string code)
    {
        // XUnit v3 output logging not yet available
        System.Diagnostics.Debug.WriteLine($"=== Generated: {fileName} ===");
        System.Diagnostics.Debug.WriteLine(code);
        System.Diagnostics.Debug.WriteLine("=== End ===");
    }

    /// <summary>
    /// Scans assemblies for references.
    /// </summary>
    private static IEnumerable<MetadataReference> ScanAssembliesForReferences()
    {
        // Example logic for scanning assemblies
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Select(a => MetadataReference.CreateFromFile(a.Location));
    }

    /// <summary>
    /// Writes generated code to a file in the test output directory for debugging purposes.
    /// </summary>
    /// <param name="fileName">The name of the file to write.</param>
    /// <param name="content">The generated code content.</param>
    protected static void WriteGeneratedCodeToFile(string fileName, string content)
    {
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput");
        Directory.CreateDirectory(outputDir);
        var filePath = Path.Combine(outputDir, fileName);
        File.WriteAllText(filePath, content);
        // Also write a summary file with the file name and timestamp
        var summaryPath = Path.Combine(outputDir, "generated_files.log");
        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {fileName}\n";
        File.AppendAllText(summaryPath, logEntry);
    }
}

