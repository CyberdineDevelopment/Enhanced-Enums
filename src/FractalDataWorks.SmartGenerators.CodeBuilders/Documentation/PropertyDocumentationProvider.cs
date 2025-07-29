using System;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

/// <summary>
/// Provides XML documentation for property declarations.
/// </summary>
public class PropertyDocumentationProvider : IXmlDocumentationProvider
{
    /// <summary>
    /// Gets or sets the custom XML documentation summary.
    /// </summary>
    public string? CustomDocumentation { get; set; }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of the property.
    /// </summary>
    public string? ElementType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this property has a getter.
    /// </summary>
    public bool HasGetter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this property has a setter.
    /// </summary>
    public bool HasSetter { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether this element has custom documentation.
    /// </summary>
    public bool HasCustomDocumentation => !string.IsNullOrWhiteSpace(CustomDocumentation);

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDocumentationProvider"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyType">The type of the property.</param>
    public PropertyDocumentationProvider(string propertyName, string propertyType)
    {
        Name = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        ElementType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
    }

    /// <summary>
    /// Gets the documentation for this property.
    /// </summary>
    /// <returns>The documentation text to use in the XML summary tag.</returns>
    public string GetDocumentation()
    {
        if (HasCustomDocumentation)
        {
            return CustomDocumentation!;
        }

        var readableName = XmlDocumentationFormatter.SplitPascalCase(Name);

        // Special case for boolean properties
        if (string.Equals(ElementType, "bool", StringComparison.Ordinal) || string.Equals(ElementType, "Boolean", StringComparison.Ordinal) || string.Equals(ElementType, "System.Boolean", StringComparison.Ordinal))
        {
            if (HasGetter && HasSetter)
            {
                return $"Gets or sets a value indicating whether {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
            }
            else if (HasGetter)
            {
                return $"Gets a value indicating whether {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
            }
            else if (HasSetter)
            {
                return $"Sets a value indicating whether {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
            }
        }

        // Default documentation for other property types
        if (HasGetter && HasSetter)
        {
            return $"Gets or sets the {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
        }
        else if (HasGetter)
        {
            return $"Gets the {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
        }
        else if (HasSetter)
        {
            return $"Sets the {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
        }

        // Fallback if no accessors are defined (shouldn't happen in practice)
        return $"The {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
    }
}
