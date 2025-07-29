using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.Utilities;

/// <summary>
/// Helper class for creating mock objects used in testing.
/// </summary>
internal static class TestMocks
{
    /// <summary>
    /// Result of a source production context for testing diagnostic reporting.
    /// </summary>
    internal class GeneratorContextResult
    {
        /// <summary>
        /// Gets list of reported diagnostics.
        /// </summary>
        public List<Diagnostic> ReportedDiagnostics { get; } = [];

        /// <summary>
        /// Gets dictionary of generated sources.
        /// </summary>
        public Dictionary<string, string> GeneratedSources { get; } = [];
    }

    /// <summary>
    /// A standard interface for any object that can provide input for hash calculation.
    /// </summary>
    internal interface IInputInfo
    {
        /// <summary>
        /// Gets a unique hash for this input.
        /// </summary>
        string InputHash { get; }
    }

    /// <summary>
    /// Utility class for calculating input hashes.
    /// </summary>
    internal static class InputTracker
    {
        /// <summary>
        /// Calculates a hash for the provided input object.
        /// </summary>
        /// <param name="input">The input to calculate a hash for.</param>
        /// <returns>A string representation of the hash.</returns>
        public static string CalculateInputHash(IInputInfo input)
        {
            return input.GetHashCode().ToString();
        }
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

    /// <summary>
    /// Interface for a dictionary-like container that maps strings to values.
    /// </summary>
    /// <typeparam name="T">The type of values in the set.</typeparam>
    internal interface IStringValueSet<T> : IEnumerable<KeyValuePair<string, T>>
    {
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value associated with the key, or default if not found.</returns>
        T this[string key] { get; }

        /// <summary>
        /// Gets the collection of keys in the set.
        /// </summary>
        IEnumerable<string> Keys { get; }

        /// <summary>
        /// Gets the collection of values in the set.
        /// </summary>
        IEnumerable<T> Values { get; }

        /// <summary>
        /// Gets the number of key-value pairs in the set.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Determines whether the set contains the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is found; otherwise, false.</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">When this method returns, contains the value associated with the key, if found; otherwise, the default value.</param>
        /// <returns>True if the key was found; otherwise, false.</returns>
        bool TryGetValue(string key, out T value);
    }

    /// <summary>
    /// Mock implementation of <see cref="IStringValueSet{T}"/>.
    /// </summary>
    internal class TestStringValueSet<T> : IStringValueSet<T>
    {
        private readonly IReadOnlyDictionary<string, T> _values;

        public TestStringValueSet(IReadOnlyDictionary<string, T> values)
        {
            _values = values ?? new Dictionary<string, T>();
        }

        /// <inheritdoc/>
        public T this[string key] => _values.TryGetValue(key, out var value) ? value : default!;

        /// <inheritdoc/>
        public IEnumerable<string> Keys => _values.Keys;

        /// <inheritdoc/>
        public IEnumerable<T> Values => _values.Values;

        /// <inheritdoc/>
        public int Count => _values.Count;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => _values.ContainsKey(key);

        /// <inheritdoc/>
        public bool TryGetValue(string key, out T value) => _values.TryGetValue(key, out value!);

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => _values.GetEnumerator();

        /// <inheritdoc/>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
