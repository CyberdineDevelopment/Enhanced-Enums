using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks a property for which to generate lookup methods.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class EnumLookupAttribute : Attribute
{
    /// <summary>
    /// Gets or sets custom method name for the lookup (e.g. ByName).
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether allow multiple results per lookup key.
    /// </summary>
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// Gets or sets the return type for this specific lookup method.
    /// If not specified, inherits from the EnhancedEnumBaseAttribute.ReturnType or auto-detected type.
    /// </summary>
    public string? ReturnType { get; set; }
}
