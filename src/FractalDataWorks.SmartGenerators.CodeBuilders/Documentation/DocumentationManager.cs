using System;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

/// <summary>
/// Manages XML documentation generation for code builders.
/// </summary>
public class DocumentationManager
{
    private readonly IXmlDocumentationProvider _documentationProvider;

    /// <summary>
    /// Gets the XML documentation provider.
    /// </summary>
    public IXmlDocumentationProvider DocumentationProvider => _documentationProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentationManager"/> class.
    /// </summary>
    /// <param name="documentationProvider">The documentation provider to use.</param>
    public DocumentationManager(IXmlDocumentationProvider documentationProvider)
    {
        _documentationProvider = documentationProvider ?? throw new ArgumentNullException(nameof(documentationProvider));
    }

    /// <summary>
    /// Sets a custom documentation summary.
    /// </summary>
    /// <param name="summary">The custom summary to use.</param>
    public void SetCustomDocumentation(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException("Summary cannot be null or whitespace.", nameof(summary));
        }

        _documentationProvider.CustomDocumentation = summary;
    }

    /// <summary>
    /// Gets the documentation summary for the associated code element.
    /// </summary>
    /// <returns>The documentation summary.</returns>
    public string GetDocumentation() => _documentationProvider.GetDocumentation();

    /// <summary>
    /// Generates formatted XML documentation for the associated code element.
    /// </summary>
    /// <returns>The formatted XML documentation.</returns>
    public string GenerateDocumentation()
    {
        var summary = GetDocumentation();
        return XmlDocumentationFormatter.FormatSummary(summary);
    }
}
