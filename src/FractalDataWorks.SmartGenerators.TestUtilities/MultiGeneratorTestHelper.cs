using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace TestUtilities
{
    /// <summary>
    /// Helper for running multiple source generators in sequence with proper state management.
    /// Supports testing generator pipelines and dependencies.
    /// </summary>
    public static class MultiGeneratorTestHelper
    {
        /// <summary>
        /// Runs multiple generators in sequence on the same compilation.
        /// </summary>
        public static MultiGeneratorResult RunGenerators(
            Compilation compilation,
            params IIncrementalGenerator[] generators)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));
            if (generators == null || generators.Length == 0)
                throw new ArgumentException("At least one generator must be provided", nameof(generators));

            var result = new MultiGeneratorResult
            {
                InputCompilation = compilation,
                GeneratorResults = new Dictionary<string, GeneratorRunResult>(StringComparer.Ordinal)
            };

            var currentCompilation = compilation;
            var allDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            // Run each generator in sequence
            foreach (var generator in generators)
            {
                var generatorName = generator.GetType().Name;

                // Create a new compilation with the current state
                GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

                // Run the generator
                driver = driver.RunGeneratorsAndUpdateCompilation(
                    currentCompilation,
                    out var outputCompilation,
                    out var diagnostics);

                // Collect diagnostics
                allDiagnostics.AddRange(diagnostics);

                // Get the run result for this specific generator
                var runResult = driver.GetRunResult();
                var generatorResult = runResult.Results.FirstOrDefault();

                if (generatorResult.Generator != null)
                {
                    result.GeneratorResults[generatorName] = generatorResult;
                }

                // Update compilation for next generator
                currentCompilation = outputCompilation;
            }

            result.OutputCompilation = currentCompilation;
            result.AllDiagnostics = allDiagnostics.ToImmutable();

            return result;
        }

        /// <summary>
        /// Runs multiple generators with cancellation support.
        /// </summary>
        public static MultiGeneratorResult RunGenerators(
            Compilation compilation,
            CancellationToken cancellationToken,
            params IIncrementalGenerator[] generators)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));
            if (generators == null || generators.Length == 0)
                throw new ArgumentException("At least one generator must be provided", nameof(generators));

            var result = new MultiGeneratorResult
            {
                InputCompilation = compilation,
                GeneratorResults = new Dictionary<string, GeneratorRunResult>(StringComparer.Ordinal)
            };

            var currentCompilation = compilation;
            var allDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            foreach (var generator in generators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var generatorName = generator.GetType().Name;

                // Create driver with cancellation token
                GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

                // Run with cancellation support
                driver = driver.RunGeneratorsAndUpdateCompilation(
                    currentCompilation,
                    out var outputCompilation,
                    out var diagnostics,
                    cancellationToken);

                allDiagnostics.AddRange(diagnostics);

                var runResult = driver.GetRunResult();
                var generatorResult = runResult.Results.FirstOrDefault();

                if (generatorResult.Generator != null)
                {
                    result.GeneratorResults[generatorName] = generatorResult;
                }

                currentCompilation = outputCompilation;
            }

            result.OutputCompilation = currentCompilation;
            result.AllDiagnostics = allDiagnostics.ToImmutable();

            return result;
        }

        /// <summary>
        /// Runs generators and verifies the output compilation has no errors.
        /// </summary>
        public static MultiGeneratorResult RunGeneratorsAndVerify(
            Compilation compilation,
            params IIncrementalGenerator[] generators)
        {
            var result = RunGenerators(compilation, generators);

            var errors = result.OutputCompilation
                .GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            if (errors.Any())
            {
                var errorMessages = string.Join("\n", errors);
                throw new InvalidOperationException($"Output compilation has errors:\n{errorMessages}");
            }

            return result;
        }

        /// <summary>
        /// Result of running multiple generators.
        /// </summary>
        public class MultiGeneratorResult
        {
            /// <summary>
            /// The original input compilation.
            /// </summary>
            public Compilation InputCompilation { get; set; } = null!;

            /// <summary>
            /// The final compilation after all generators have run.
            /// </summary>
            public Compilation OutputCompilation { get; set; } = null!;

            /// <summary>
            /// Individual results for each generator, keyed by generator type name.
            /// </summary>
            public Dictionary<string, GeneratorRunResult> GeneratorResults { get; set; } = new(StringComparer.Ordinal);

            /// <summary>
            /// All diagnostics produced by all generators.
            /// </summary>
            public ImmutableArray<Diagnostic> AllDiagnostics { get; set; }

            /// <summary>
            /// Gets all generated sources from all generators.
            /// </summary>
            public IEnumerable<GeneratedSourceResult> GetAllGeneratedSources()
            {
                return GeneratorResults.Values
                    .SelectMany(r => r.GeneratedSources);
            }

            /// <summary>
            /// Gets generated sources for a specific generator.
            /// </summary>
            public IEnumerable<GeneratedSourceResult> GetGeneratedSources(string generatorName)
            {
                if (GeneratorResults.TryGetValue(generatorName, out var result))
                {
                    return result.GeneratedSources;
                }
                return Enumerable.Empty<GeneratedSourceResult>();
            }

            /// <summary>
            /// Checks if any generator reported diagnostics with the specified severity.
            /// </summary>
            public bool HasDiagnostics(DiagnosticSeverity severity)
            {
                return AllDiagnostics.Any(d => d.Severity == severity);
            }

            /// <summary>
            /// Gets the syntax trees added by all generators.
            /// </summary>
            public IEnumerable<SyntaxTree> GetAddedSyntaxTrees()
            {
                var originalTrees = new HashSet<SyntaxTree>(InputCompilation.SyntaxTrees);
                return OutputCompilation.SyntaxTrees.Where(t => !originalTrees.Contains(t));
            }
        }
    }

    /// <summary>
    /// Extension methods for MultiGeneratorResult.
    /// </summary>
    public static class MultiGeneratorResultExtensions
    {
        /// <summary>
        /// Asserts that no errors were produced during generation.
        /// </summary>
        public static MultiGeneratorTestHelper.MultiGeneratorResult AssertNoErrors(
            this MultiGeneratorTestHelper.MultiGeneratorResult result)
        {
            var errors = result.AllDiagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            if (errors.Any())
            {
                var errorMessages = string.Join("\n", errors);
                throw new InvalidOperationException($"Generator produced errors:\n{errorMessages}");
            }

            return result;
        }

        /// <summary>
        /// Asserts that the specified number of sources were generated.
        /// </summary>
        public static MultiGeneratorTestHelper.MultiGeneratorResult AssertGeneratedSourceCount(
            this MultiGeneratorTestHelper.MultiGeneratorResult result,
            int expectedCount)
        {
            var actualCount = result.GetAllGeneratedSources().Count();
            if (actualCount != expectedCount)
            {
                throw new InvalidOperationException(
                    $"Expected {expectedCount} generated sources but found {actualCount}");
            }

            return result;
        }

        /// <summary>
        /// Gets a generated source by hint name.
        /// </summary>
        public static string GetGeneratedSource(
            this MultiGeneratorTestHelper.MultiGeneratorResult result,
            string hintName)
        {
            var source = result.GetAllGeneratedSources()
                .FirstOrDefault(s => string.Equals(s.HintName, hintName, StringComparison.Ordinal));

            if (source.HintName == null)
            {
                var availableHints = string.Join(", ",
                    result.GetAllGeneratedSources().Select(s => s.HintName));
                throw new InvalidOperationException(
                    $"No generated source found with hint name '{hintName}'. " +
                    $"Available: {availableHints}");
            }

            return source.SourceText.ToString();
        }
    }
}