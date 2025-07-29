using System;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

/// <summary>
/// Provides XML documentation for class declarations.
/// </summary>
public class ClassDocumentationProvider : IXmlDocumentationProvider
{
    /// <summary>
    /// Gets or sets the custom XML documentation summary.
    /// </summary>
    public string? CustomDocumentation { get; set; }

    /// <summary>
    /// Gets the name of the class.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of the element (not applicable for classes).
    /// </summary>
    public string? ElementType => null;

    /// <summary>
    /// Gets a value indicating whether this element has custom documentation.
    /// </summary>
    public bool HasCustomDocumentation => !string.IsNullOrWhiteSpace(CustomDocumentation);

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDocumentationProvider"/> class.
    /// </summary>
    /// <param name="className">The name of the class.</param>
    public ClassDocumentationProvider(string className)
    {
        Name = className ?? throw new ArgumentNullException(nameof(className));
    }

    /// <summary>
    /// Gets the documentation for this class.
    /// </summary>
    /// <returns>The documentation text to use in the XML summary tag.</returns>
    public string GetDocumentation()
    {
        if (HasCustomDocumentation)
        {
            return CustomDocumentation!;
        }

        var readableName = XmlDocumentationFormatter.SplitPascalCase(Name);
        return $"Represents a {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
    }
}
