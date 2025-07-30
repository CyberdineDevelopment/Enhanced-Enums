using System;

namespace FractalDataWorks.Attributes;

/// <summary>
/// Attribute to mark a base class or interface for enhanced enum code generation.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
public sealed class EnhancedEnumBaseAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedEnumBaseAttribute"/> class.
    /// The collection name will be automatically generated as the plural form of the enum base class name.
    /// </summary>
    public EnhancedEnumBaseAttribute()
    {
        CollectionName = string.Empty; // Generator will use default naming
        UseFactory = false;
        NameComparison = StringComparison.OrdinalIgnoreCase;
        IncludeReferencedAssemblies = false;
        ReturnType = null;
        ReturnTypeNamespace = null;
        DefaultGenericReturnType = null;
        DefaultGenericReturnTypeNamespace = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedEnumBaseAttribute"/> class with the specified collection name.
    /// </summary>
    /// <param name="collectionName">The name of the generated collection class.</param>
    /// <param name="useFactory">Enable factory-based instance creation on lookup.</param>
    /// <param name="nameComparison">String comparison mode for name-based lookups.</param>
    /// <param name="includeReferencedAssemblies">Enable scanning of referenced assemblies for enum options.</param>
    /// <param name="returnType">The return type for generated static properties and methods.</param>
    /// <param name="returnTypeNamespace">The namespace to import for the return type.</param>
    /// <param name="defaultGenericReturnType">The default return type for collections when the base type is generic.</param>
    /// <param name="defaultGenericReturnTypeNamespace">The namespace for the default generic return type.</param>
    public EnhancedEnumBaseAttribute(
        string collectionName,
        bool useFactory = false,
        StringComparison nameComparison = StringComparison.OrdinalIgnoreCase,
        bool includeReferencedAssemblies = false,
        string? returnType = null,
        string? returnTypeNamespace = null,
        string? defaultGenericReturnType = null,
        string? defaultGenericReturnTypeNamespace = null)
    {
        CollectionName = collectionName;
        UseFactory = useFactory;
        NameComparison = nameComparison;
        IncludeReferencedAssemblies = includeReferencedAssemblies;
        ReturnType = returnType;
        ReturnTypeNamespace = returnTypeNamespace;
        DefaultGenericReturnType = defaultGenericReturnType;
        DefaultGenericReturnTypeNamespace = defaultGenericReturnTypeNamespace;
    }

    /// <summary>Gets the generated collection class name.</summary>
    public string CollectionName { get; }

    /// <summary>Gets whether to enable factory-based instance creation on lookup.</summary>
    public bool UseFactory { get; }

    /// <summary>Gets the string comparison mode for name-based lookups.</summary>
    public StringComparison NameComparison { get; }

    /// <summary>Gets whether to enable scanning of referenced assemblies for enum options.</summary>
    public bool IncludeReferencedAssemblies { get; }

    /// <summary>
    /// Gets the return type for generated static properties and methods.
    /// If not specified, the generator will auto-detect interfaces that extend IEnumOption,
    /// or fall back to the concrete base class type.
    /// </summary>
    public string? ReturnType { get; }

    /// <summary>
    /// Gets the namespace to import for the return type.
    /// If not specified, the generator will attempt to extract it from the ReturnType.
    /// </summary>
    public string? ReturnTypeNamespace { get; }

    /// <summary>
    /// Gets the default return type for collections when the base type is generic.
    /// Use this to specify what type should be used in the generated collection.
    /// Example: For ServiceType&lt;T&gt;, you might specify "IFdwService" as DefaultGenericReturnType.
    /// </summary>
    public string? DefaultGenericReturnType { get; }

    /// <summary>
    /// Gets the namespace for the default generic return type.
    /// If not specified, will attempt to extract from DefaultGenericReturnType.
    /// </summary>
    public string? DefaultGenericReturnTypeNamespace { get; }
}