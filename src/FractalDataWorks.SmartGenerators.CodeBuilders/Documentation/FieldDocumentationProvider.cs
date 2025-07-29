using System;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

/// <summary>
/// Provides XML documentation for field declarations.
/// </summary>
public class FieldDocumentationProvider : IXmlDocumentationProvider
{
    /// <summary>
    /// Gets or sets the custom XML documentation summary.
    /// </summary>
    public string? CustomDocumentation { get; set; }

    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of the field.
    /// </summary>
    public string? ElementType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this field is a backing field for a property.
    /// </summary>
    public bool IsBackingField { get; set; }

    /// <summary>
    /// Gets a value indicating whether this element has custom documentation.
    /// </summary>
    public bool HasCustomDocumentation => !string.IsNullOrWhiteSpace(CustomDocumentation);

    /// <summary>
    /// Gets or sets the name of the property this field is backing (if applicable).
    /// </summary>
    public string? BackedPropertyName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldDocumentationProvider"/> class.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="fieldType">The type of the field.</param>
    public FieldDocumentationProvider(string fieldName, string fieldType)
    {
        Name = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
        ElementType = fieldType ?? throw new ArgumentNullException(nameof(fieldType));

        // Automatically detect if this is likely a backing field based on naming conventions
        if (fieldName.StartsWith("_", StringComparison.Ordinal) && fieldName.Length > 1)
        {
            var possiblePropertyName = XmlDocumentationFormatter.CapitalizeFirst(fieldName.Substring(1));
            BackedPropertyName = possiblePropertyName;
            IsBackingField = true;
        }
    }

    /// <summary>
    /// Gets the documentation for this field.
    /// </summary>
    /// <returns>The documentation text to use in the XML summary tag.</returns>
    public string GetDocumentation()
    {
        if (HasCustomDocumentation)
        {
            return CustomDocumentation!;
        }

        // If this is a backing field for a property, reference the property
        if (IsBackingField && !string.IsNullOrEmpty(BackedPropertyName))
        {
            var readablePropertyName = XmlDocumentationFormatter.SplitPascalCase(BackedPropertyName!);
            return $"Backing field for the {XmlDocumentationFormatter.ToLowerCaseFirst(readablePropertyName)} property.";
        }

        // Otherwise, create standard documentation based on the field name
        var readableName = XmlDocumentationFormatter.SplitPascalCase(Name);

        // Remove leading underscore if present
        if (readableName.StartsWith("_ ", StringComparison.Ordinal))
        {
            readableName = readableName.Substring(2);
        }

        return $"The {XmlDocumentationFormatter.ToLowerCaseFirst(readableName)}.";
    }
}
