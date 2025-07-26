using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks a concrete enum option with a custom display name and ordering.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class EnumOptionAttribute : Attribute
{
    /// <summary>Gets or sets display name for the enum option.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets ordering within the collection.</summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the collection name this option belongs to.
    /// When the base enum has multiple collections defined, this specifies which collection(s) to include this option in.
    /// </summary>
    public string? CollectionName { get; set; }
    
    /// <summary>
    /// Gets or sets the return type for this specific enum option.
    /// This overrides the default return type specified in the EnhancedEnumBase attribute.
    /// </summary>
    public string? ReturnType { get; set; }
    
    /// <summary>
    /// Gets or sets the namespace for the return type if it's not in the same namespace.
    /// </summary>
    public string? ReturnTypeNamespace { get; set; }
}
