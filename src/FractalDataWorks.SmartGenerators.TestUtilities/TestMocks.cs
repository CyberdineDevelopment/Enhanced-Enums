using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

#pragma warning disable CA1034 // Nested types should not be visible - Acceptable in test utilities

/// <summary>
/// Helper class for creating mock objects used in testing.
/// </summary>
public static class TestMocks
{
    /// <summary>
    /// Result of a source production context for testing diagnostic reporting.
    /// </summary>
    public class GeneratorContextResult
    {
        private readonly List<Diagnostic> _reportedDiagnostics = [];
        private readonly Dictionary<string, string> _generatedSources = [];

        /// <summary>
        /// Gets list of reported diagnostics.
        /// </summary>
        public IReadOnlyList<Diagnostic> ReportedDiagnostics => _reportedDiagnostics.AsReadOnly();

        /// <summary>
        /// Gets dictionary of generated sources.
        /// </summary>
        public IReadOnlyDictionary<string, string> GeneratedSources => _generatedSources;

        /// <summary>
        /// Adds a diagnostic to the result.
        /// </summary>
        /// <param name="diagnostic">The diagnostic to add.</param>
        internal void AddDiagnostic(Diagnostic diagnostic)
        {
            _reportedDiagnostics.Add(diagnostic);
        }

        /// <summary>
        /// Adds a generated source to the result.
        /// </summary>
        /// <param name="hintName">The hint name for the source.</param>
        /// <param name="source">The source content.</param>
        internal void AddGeneratedSource(string hintName, string source)
        {
            _generatedSources[hintName] = source;
        }
    }

    /// <summary>
    /// Test generator that reports diagnostics for testing the DiagnosticReporter.
    /// </summary>
    public class TestDiagnosticGenerator : IIncrementalGenerator
    {
        private readonly DiagnosticDescriptor _descriptor;
        private readonly Location? _location;
        private readonly object?[] _args;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDiagnosticGenerator"/> class.
        /// </summary>
        /// <param name="descriptor">The diagnostic descriptor.</param>
        /// <param name="location">Optional location for the diagnostic.</param>
        /// <param name="args">The diagnostic message arguments.</param>
        public TestDiagnosticGenerator(
            DiagnosticDescriptor descriptor,
            Location? location = null,
            params object?[] args)
        {
            _descriptor = descriptor;
            _location = location;
            _args = args;
        }

        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                // Report the diagnostic
                var diagnostic = Diagnostic.Create(_descriptor, _location ?? Location.None, _args);
                spc.ReportDiagnostic(diagnostic);
            });
        }
    }

    /// <summary>
    /// Mock implementation of <see cref="IInputInfo"/>.
    /// </summary>
    public class TestInputInfo : IInputInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets the input hash.
        /// </summary>
        public string InputHash => CalculateInputHash();

        /// <summary>
        /// Calculates the input hash.
        /// </summary>
        /// <returns>The calculated hash.</returns>
        private string CalculateInputHash()
        {
            using var writer = new StringWriter();
            WriteToHash(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Writes the model state to a TextWriter for hash calculation.
        /// </summary>
        /// <param name="writer">The text writer to write to.</param>
        public void WriteToHash(TextWriter writer)
        {
            writer.Write(Name);
            writer.Write(Value);
        }
    }

    /// <summary>
    /// Another test implementation of <see cref="IInputInfo"/> with different properties.
    /// </summary>
    public class ComplexInputInfo : IInputInfo
    {
        private readonly List<string> _tags = [];
        private readonly Dictionary<string, string> _properties = [];

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets the tags.
        /// </summary>
        public IReadOnlyList<string> Tags => _tags.AsReadOnly();

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public IReadOnlyDictionary<string, string> Properties => _properties;

        private string _cachedHash = string.Empty;

        /// <summary>
        /// Gets the input hash.
        /// </summary>
        public string InputHash
        {
            get
            {
                if (!string.IsNullOrEmpty(_cachedHash))
                {
                    return _cachedHash;
                }

                using var writer = new StringWriter();
                WriteToHash(writer);
                _cachedHash = writer.ToString();

                return _cachedHash;
            }
        }

        /// <summary>
        /// Adds a tag.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        public void AddTag(string tag)
        {
            _tags.Add(tag);
            _cachedHash = string.Empty; // Invalidate cache
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public void SetProperty(string key, string value)
        {
            _properties[key] = value;
            _cachedHash = string.Empty; // Invalidate cache
        }

        /// <summary>
        /// Writes the model state to a TextWriter for hash calculation.
        /// </summary>
        /// <param name="writer">The text writer to write to.</param>
        public void WriteToHash(TextWriter writer)
        {
            writer.Write(Id);

            foreach (var tag in _tags)
            {
                writer.Write(tag);
            }

            foreach (var kvp in _properties.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }
    }

    /// <summary>
    /// A mock implementation of a source generator that reports system errors.
    /// </summary>
    public class ErrorSourceGenerator : IIncrementalGenerator
    {
        private readonly string _message;
        private readonly string _id;
        private readonly Action _onExceptionAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorSourceGenerator"/> class.
        /// </summary>
        /// <param name="message">The error message to report.</param>
        /// <param name="id">The error ID.</param>
        /// <param name="onExceptionAction">Optional action to execute when exception is thrown.</param>
        public ErrorSourceGenerator(string message, string id = "TEST001", Action? onExceptionAction = null)
        {
            _message = message;
            _id = id;
            _onExceptionAction = onExceptionAction ?? (() => { });
        }

        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                try
                {
                    // Force a specific exception
                    throw new InvalidOperationException(_message);
                }
                catch (InvalidOperationException ex)
                {
                    _onExceptionAction();

                    // Report diagnostic
                    var descriptor = new DiagnosticDescriptor(
                        _id,
                        "Test Error",
                        ex.Message,
                        "Test",
                        DiagnosticSeverity.Error,
                        true);

                    spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
                }
            });
        }
    }

    /// <summary>
    /// A mock implementation of a source generator that throws an unhandled exception.
    /// </summary>
    public class UnhandledExceptionGenerator : IIncrementalGenerator
    {
        private readonly string _message;
        private readonly string _exceptionType;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledExceptionGenerator"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="exceptionType">The exception type to throw.</param>
        public UnhandledExceptionGenerator(string message, string exceptionType = "InvalidOperationException")
        {
            _message = message;
            _exceptionType = exceptionType;
        }

        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                switch (_exceptionType)
                {
                    case "ArgumentException":
                        throw new ArgumentException(_message);
                    case "NullReferenceException":
#pragma warning disable CA2201
                        throw new NullReferenceException($"Null reference error: {_message}");
#pragma warning restore CA2201
                    case "InvalidCastException":
                        throw new InvalidCastException(_message);
                    case "NotImplementedException":
                        throw new NotImplementedException(_message);
                    default:
                        throw new InvalidOperationException(_message);
                }
            });
        }
    }
}
