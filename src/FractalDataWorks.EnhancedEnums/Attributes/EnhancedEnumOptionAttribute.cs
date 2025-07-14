using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Attribute to mark a class or interface for EnhancedEnumOption code generation.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
public sealed class EnhancedEnumOptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedEnumOptionAttribute"/> class with the specified collection name.
    /// </summary>
    /// <param name="collectionName">The name of the generated collection class.</param>
    public EnhancedEnumOptionAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedEnumOptionAttribute"/> class.
    /// The collection name will be automatically generated as the plural form of the enum base class name.
    /// </summary>
    public EnhancedEnumOptionAttribute()
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
}
