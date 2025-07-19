using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Attribute to mark a base class or interface for enhanced enum code generation.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
public sealed class EnhancedEnumBaseAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedEnumBaseAttribute"/> class with the specified collection name.
    /// </summary>
    /// <param name="collectionName">The name of the generated collection class.</param>
    public EnhancedEnumBaseAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedEnumBaseAttribute"/> class.
    /// The collection name will be automatically generated as the plural form of the enum base class name.
    /// </summary>
    public EnhancedEnumBaseAttribute()
    {
        CollectionName = string.Empty; // Generator will use default naming
    }

    /// <summary>Gets specifies the generated collection class name.</summary>
    public string CollectionName { get; }

    /// <summary>Gets or sets a value indicating whether enable factory-based instance creation on lookup.</summary>
    public bool UseFactory { get; set; }

    /// <summary>Gets or sets string comparison mode for name-based lookups.</summary>
    public StringComparison NameComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>Gets or sets a value indicating whether enable scanning of referenced assemblies for enum options.</summary>
    public bool IncludeReferencedAssemblies { get; set; }

    /// <summary>
    /// Gets or sets the return type for generated static properties and methods.
    /// If not specified, the generator will auto-detect interfaces that extend IEnhancedEnumOption,
    /// or fall back to the concrete base class type.
    /// </summary>
    public string? ReturnType { get; set; }
}
