using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Helper class for verifying that generated code compiles and runs correctly.
/// </summary>
public static class CompilationVerifier
{
    /// <summary>
    /// Compiles the specified source code and verifies that it compiles without errors.
    /// </summary>
    /// <param name="sources">The source code to compile.</param>
    /// <param name="additionalReferences">Additional assembly references to include.</param>
    /// <returns>The compiled assembly.</returns>
    public static Assembly CompileAndVerify(
        string[] sources,
        params MetadataReference[] additionalReferences)
    {
        var syntaxTrees = sources.Select(source =>
            CSharpSyntaxTree.ParseText(source)).ToArray();

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.IEnumerable<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).Assembly.Location),
        };

        if (additionalReferences.Length > 0)
        {
            references.AddRange(additionalReferences);
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: Guid.NewGuid().ToString(),
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
            .ShouldBeEmpty($"Compilation failed with errors: {string.Join(SyntaxFactory.ElasticCarriageReturnLineFeed.ToString(), result.Diagnostics.Select(d => d.ToString()))}");

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

        return assembly!; // Added null-forgiving operator
    }

    /// <summary>
    /// Compiles source code with a source generator and returns the resulting assembly.
    /// </summary>
    /// <param name="sources">The source code to compile.</param>
    /// <param name="generator">The source generator to run.</param>
    /// <param name="additionalReferences">Additional assembly references to include.</param>
    /// <returns>A tuple containing the compiled assembly and any diagnostics.</returns>
    public static (Assembly? Assembly, IEnumerable<Diagnostic> Diagnostics) CompileWithSourceGenerator(
        string[] sources,
        IIncrementalGenerator generator,
        params MetadataReference[] additionalReferences)
    {
        // Create syntax trees from source
        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source)).ToArray();

        // Collect references
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.IEnumerable<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
        };

        if (additionalReferences.Length > 0)
        {
            references.AddRange(additionalReferences);
        }

        // Create compilation
        var compilation = CSharpCompilation.Create(
            assemblyName: Guid.NewGuid().ToString(),
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Debug));

        // Run the generator
        var driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [],
            parseOptions: (CSharpParseOptions)syntaxTrees[0].Options);

        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var runResult = driver.GetRunResult();

        // Get generated sources
        var generatedSources = runResult.Results[0].GeneratedSources;
        var generatedTrees = generatedSources.Select(s => CSharpSyntaxTree.ParseText(s.SourceText)).ToList();
        var updatedCompilation = compilation.AddSyntaxTrees(generatedTrees);

        // Compile to in-memory assembly
        using var ms = new MemoryStream();
        var emitResult = updatedCompilation.Emit(ms);

        if (!emitResult.Success)
        {
            return (null, emitResult.Diagnostics);
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

        return (assembly!, emitResult.Diagnostics);
    }

    /// <summary>
    /// Dynamically invokes a method in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="typeName">The type name.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="parameters">The method parameters.</param>
    /// <returns>The result of the method invocation.</returns>
    public static object? InvokeMethod(
        Assembly assembly,
        string typeName,
        string methodName,
        params object[] parameters)
    {
        var type = assembly.GetType(typeName);
        type.ShouldNotBeNull($"Type {typeName} was not found in the assembly.");

        var method = type.GetMethod(methodName);
        method.ShouldNotBeNull($"Method {methodName} was not found in type {typeName}.");

        // If the method is static, invoke it directly
        if (method.IsStatic)
        {
            return method.Invoke(null, parameters);
        }

        // If the method is instance, create an instance of the type
        var instance = Activator.CreateInstance(type);
        return method.Invoke(instance, parameters);
    }
}
