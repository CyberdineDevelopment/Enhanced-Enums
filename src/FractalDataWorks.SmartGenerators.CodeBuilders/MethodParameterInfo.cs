namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Represents a parameter in a method.
/// </summary>
internal sealed class MethodParameterInfo
{
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the parameter.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default value of the parameter (optional).
    /// </summary>
    public string? DefaultValue { get; set; }
}
