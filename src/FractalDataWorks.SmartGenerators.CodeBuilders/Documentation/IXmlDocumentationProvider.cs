namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

/// <summary>
/// Defines an interface for generating XML documentation for code elements.
/// </summary>
public interface IXmlDocumentationProvider
{
    /// <summary>
    /// Gets or sets the custom XML documentation summary.
    /// If set, this will be used instead of the auto-generated documentation.
    /// </summary>
    string? CustomDocumentation { get; set; }

    /// <summary>
    /// Gets a value indicating whether this element has custom documentation.
    /// </summary>
    bool HasCustomDocumentation { get; }

    /// <summary>
    /// Gets the name of the element.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of the element (if applicable).
    /// </summary>
    string? ElementType { get; }

    /// <summary>
    /// Gets the documentation for this element.
    /// </summary>
    /// <returns>The documentation text to use in the XML summary tag.</returns>
    string GetDocumentation();
}
