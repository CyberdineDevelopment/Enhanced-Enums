using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;

#pragma warning disable CA1034 // Nested types should not be visible - Acceptable in test utilities

/// <summary>
/// Helper class for creating mock objects used in testing.
/// </summary>
internal static class TestMocks
{
    /// <summary>
    /// Mock object that simulates a SourceProductionContext for testing.
    /// </summary>
    internal class MockSourceContext
    {
        private readonly List<Diagnostic> _diagnostics = [];
        private readonly Dictionary<string, SourceText> _sources = [];
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Gets the list of diagnostics that were reported.
        /// </summary>
        public IReadOnlyList<Diagnostic> ReportedDiagnostics => _diagnostics;

        /// <summary>
        /// Gets the dictionary of sources that were added.
        /// </summary>
        public IReadOnlyDictionary<string, SourceText> GeneratedSources => _sources;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockSourceContext"/> class.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public MockSourceContext(CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Reports a diagnostic.
        /// </summary>
        /// <param name="diagnostic">The diagnostic to report.</param>
        public void ReportDiagnostic(Diagnostic diagnostic)
        {
            _diagnostics.Add(diagnostic);
        }

        /// <summary>
        /// Adds a source with the specified hint name and source text.
        /// </summary>
        /// <param name="hintName">The hint name of the source.</param>
        /// <param name="source">The source text.</param>
        public void AddSource(string hintName, string source)
        {
            _sources[hintName] = SourceText.From(source);
        }

        /// <summary>
        /// Adds a source with the specified hint name and source text.
        /// </summary>
        /// <param name="hintName">The hint name of the source.</param>
        /// <param name="sourceText">The source text.</param>
        public void AddSource(string hintName, SourceText sourceText)
        {
            _sources[hintName] = sourceText;
        }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken => _cancellationToken;
    }

    /// <summary>
    /// Mock implementation of <see cref="IInputInfo"/>.
    /// </summary>
    internal class TestInputInfo : IInputInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }

        /// <inheritdoc/>
        public string InputHash => InputTracker.CalculateInputHash(this);

        /// <inheritdoc/>
        public void WriteToHash(TextWriter writer)
        {
            writer.Write(Name);
            writer.Write(Value);
        }
    }

    /// <summary>
    /// Another test implementation of <see cref="IInputInfo"/> with different properties.
    /// </summary>
    internal class ComplexInputInfo : IInputInfo
    {
        public string Id { get; set; } = string.Empty;
        public string[] Tags { get; set; } = [];
        public Dictionary<string, string> Properties { get; set; } = [];

        private string _cachedHash = string.Empty;

        /// <inheritdoc/>
        public string InputHash
        {
            get
            {
                if (string.IsNullOrEmpty(_cachedHash))
                {
                    _cachedHash = InputTracker.CalculateInputHash(this);
                }

                return _cachedHash;
            }
        }

        /// <inheritdoc/>
        public void WriteToHash(TextWriter writer)
        {
            writer.Write(Id);

            foreach (var tag in Tags)
            {
                writer.Write(tag);
            }

            foreach (var kvp in Properties.OrderBy(p => p.Key))
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }
    }
}
