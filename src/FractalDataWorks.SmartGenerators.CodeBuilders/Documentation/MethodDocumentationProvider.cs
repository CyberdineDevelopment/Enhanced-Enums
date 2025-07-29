using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

#pragma warning disable CA1308 // Normalize strings to uppercase

/// <summary>
/// Provides XML documentation for method declarations.
/// </summary>
public class MethodDocumentationProvider : IXmlDocumentationProvider
{
    private readonly string _returnType;
    private readonly IReadOnlyList<ParameterInfo> _parameters;
    private readonly Dictionary<string, string> _parameterDocs = [];
    private string? _returnsDoc;

    /// <summary>
    /// Gets or sets the custom XML documentation summary.
    /// </summary>
    public string? CustomDocumentation { get; set; }

    /// <summary>
    /// Gets the name of the method.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the return type of the method.
    /// </summary>
    public string? ElementType => _returnType;

    /// <summary>
    /// Gets a value indicating whether this element has custom documentation.
    /// </summary>
    public bool HasCustomDocumentation => !string.IsNullOrWhiteSpace(CustomDocumentation);

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodDocumentationProvider"/> class.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnType">The return type of the method.</param>
    /// <param name="parameters">The parameters of the method (optional).</param>
    internal MethodDocumentationProvider(string methodName, string returnType, IReadOnlyList<ParameterInfo>? parameters = null)
    {
        Name = methodName ?? throw new ArgumentNullException(nameof(methodName));
        _returnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
        _parameters = parameters ?? [];
    }

    /// <summary>
    /// Sets the documentation for a parameter.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="documentation">The documentation for the parameter.</param>
    /// <returns>The current instance for chaining.</returns>
    public MethodDocumentationProvider WithParameterDoc(string parameterName, string documentation)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or empty.", nameof(parameterName));
        }

        if (_parameters.All(p => !string.Equals(p.Name, parameterName, StringComparison.Ordinal)))
        {
            throw new ArgumentException($"Parameter '{parameterName}' does not exist in the method.", nameof(parameterName));
        }

        _parameterDocs[parameterName] = documentation;
        return this;
    }

    /// <summary>
    /// Sets the documentation for the return value.
    /// </summary>
    /// <param name="documentation">The documentation for the return value.</param>
    /// <returns>The current instance for chaining.</returns>
    public MethodDocumentationProvider WithReturnsDoc(string documentation)
    {
        _returnsDoc = documentation;
        return this;
    }

    /// <summary>
    /// Gets the documentation for this method.
    /// </summary>
    /// <returns>The documentation text to use in the XML summary tag.</returns>
    public string GetDocumentation()
    {
        if (HasCustomDocumentation)
        {
            return CustomDocumentation!;
        }

        // Extract the verb from the method name (common method naming convention)
        var readableName = XmlDocumentationFormatter.SplitPascalCase(Name);
        string[] parts = readableName.Split(' ');

        if (parts.Length > 0)
        {
            var verb = parts[0].ToLowerInvariant();
            var rest = string.Join(" ", parts.Skip(1)).ToLowerInvariant();

            // Check if the method is a getter
            if (string.Equals(verb, "get", StringComparison.Ordinal) && !string.IsNullOrEmpty(rest))
            {
                return $"Gets the {rest}.";
            }

            // Check if the method is a setter
            if (string.Equals(verb, "set", StringComparison.Ordinal) && !string.IsNullOrEmpty(rest))
            {
                return $"Sets the {rest}.";
            }

            // Check if the method is a validator
            if (string.Equals(verb, "is", StringComparison.Ordinal) || string.Equals(verb, "has", StringComparison.Ordinal) || string.Equals(verb, "can", StringComparison.Ordinal) || string.Equals(verb, "should", StringComparison.Ordinal))
            {
                return $"Determines whether {XmlDocumentationFormatter.ToLowerCaseFirst(rest)}.";
            }

            // For other methods
            if (!string.IsNullOrEmpty(rest))
            {
                return $"{XmlDocumentationFormatter.CapitalizeFirst(verb)}s the {rest.ToLowerInvariant()}.";
            }
        }

        // Default case if no patterns match
        return $"Executes the {readableName.ToLowerInvariant()} operation.";
    }

    /// <summary>
    /// Gets parameter documentation for all parameters.
    /// </summary>
    /// <returns>A dictionary mapping parameter names to their documentation.</returns>
    public Dictionary<string, string> GetParameterDocs()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var param in _parameters)
        {
            if (_parameterDocs.TryGetValue(param.Name, out var customDoc))
            {
                result[param.Name] = customDoc;
            }
            else
            {
                // Build default documentation based on parameter name
                var readableName = XmlDocumentationFormatter.SplitPascalCase(param.Name);
                result[param.Name] = $"The {readableName.ToLowerInvariant()}.";
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the documentation for the return value.
    /// </summary>
    /// <returns>The documentation for the return value.</returns>
    public string? GetReturnsDoc()
    {
        if (!string.IsNullOrEmpty(_returnsDoc))
        {
            return _returnsDoc;
        }

        if (string.Equals(_returnType, "void", StringComparison.Ordinal))
        {
            return null; // No return value documentation for void methods
        }

        if (string.Equals(_returnType, "bool", StringComparison.Ordinal) || string.Equals(_returnType, "Boolean", StringComparison.Ordinal))
        {
            return "true if successful; otherwise, false.";
        }

        if (_returnType.StartsWith("IEnumerable", StringComparison.Ordinal) ||
            _returnType.EndsWith("[]", StringComparison.Ordinal))
        {
            return "A collection of items.";
        }

        if (string.Equals(_returnType, "Task", StringComparison.Ordinal))
        {
            return "A task that represents the asynchronous operation.";
        }

        if (_returnType.StartsWith("Task<", StringComparison.Ordinal))
        {
            var innerType = _returnType.Substring(5, _returnType.Length - 6);
            return $"A task that represents the asynchronous operation. The task result contains {GetArticle(innerType)} {innerType}.";
        }

        return $"{GetArticle(_returnType)} {_returnType}.";
    }

    private static string GetArticle(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return "a";
        }

        var firstChar = word.ToLowerInvariant()[0];
        return "aeiou".Contains(firstChar) ? "an" : "a";
    }

#pragma warning disable CA1054 // URI parameters should not be strings
    /// <summary>
    /// Creates a diagnostic descriptor for an error.
    /// </summary>
    /// <param name="id">The diagnostic ID.</param>
    /// <param name="title">The diagnostic title.</param>
    /// <param name="messageFormat">The message format.</param>
    /// <param name="category">The diagnostic category.</param>
    /// <param name="helpLinkUri">The help link URI.</param>
    /// <returns>A diagnostic descriptor.</returns>
    public static DiagnosticDescriptor CreateError(
        string id,
        string title,
        string messageFormat,
        string category,
        string? helpLinkUri = null)
    {
        return new DiagnosticDescriptor(
            id: id,
            title: title,
            messageFormat: messageFormat,
            category: category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: [WellKnownDiagnosticTags.Build],
            helpLinkUri: helpLinkUri ?? string.Empty);
    }

    /// <summary>
    /// Creates a diagnostic descriptor for a warning.
    /// </summary>
    /// <param name="id">The diagnostic ID.</param>
    /// <param name="title">The diagnostic title.</param>
    /// <param name="messageFormat">The message format.</param>
    /// <param name="category">The diagnostic category.</param>
    /// <param name="helpLinkUri">The help link URI.</param>
    /// <returns>A diagnostic descriptor.</returns>
    public static DiagnosticDescriptor CreateWarning(
        string id,
        string title,
        string messageFormat,
        string category,
        string? helpLinkUri = null)
    {
        return new DiagnosticDescriptor(
            id: id,
            title: title,
            messageFormat: messageFormat,
            category: category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: [WellKnownDiagnosticTags.Build],
            helpLinkUri: helpLinkUri ?? string.Empty);
    }
#pragma warning restore CA1054 // URI parameters should not be strings

    /// <summary>
    /// Represents information about a method parameter.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Used as parameter type in internal constructor")]
    internal sealed class ParameterInfo
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        public ParameterInfo(string name, string type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}

#pragma warning restore CA1308
