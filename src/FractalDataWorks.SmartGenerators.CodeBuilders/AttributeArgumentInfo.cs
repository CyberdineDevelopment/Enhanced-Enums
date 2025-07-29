namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Represents an argument in an attribute.
/// </summary>
internal sealed class AttributeArgumentInfo
{
    /// <summary>
    /// Gets or sets the name of the argument.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the argument.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is a named argument.
    /// </summary>
    public bool IsNamed { get; set; }
}
