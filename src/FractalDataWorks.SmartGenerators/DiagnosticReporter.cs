using System;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.SmartGenerators;

/// <summary>
/// Helper for reporting standardized diagnostics from source generators.
/// </summary>
public static class DiagnosticReporter
{
    /// <summary>
    /// Reports an error diagnostic.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="descriptor">The diagnostic descriptor.</param>
    /// <param name="location">Optional location for the diagnostic.</param>
    /// <param name="args">Optional message format arguments.</param>
    public static void ReportError(
        SourceProductionContext context,
        DiagnosticDescriptor descriptor,
        Location? location = null,
        params object?[] args)
    {
        if (descriptor == null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        var diagnostic = Diagnostic.Create(descriptor, location ?? Location.None, args ?? []);
        context.ReportDiagnostic(diagnostic);
    }

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
            helpLinkUri: helpLinkUri?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Creates a diagnostic descriptor for a warning.
    /// </summary>
    /// <param name="id">The diagnostic ID.</param>
    /// <param name="title">The diagnostic title.</param>
    /// <param name="messageFormat">The message format.</param>
    /// <param name="category">The diagnostic category.</param>
    /// <returns>A diagnostic descriptor.</returns>
    public static DiagnosticDescriptor CreateWarning(
        string id,
        string title,
        string messageFormat,
        string category) =>
        CreateWarning(id, title, messageFormat, category, string.Empty);

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
        string helpLinkUri)
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
}
