using System;

namespace FractalDataWorks;

/// <summary>
/// Marks a class as an enhanced enum collection base type.
/// The source generator will create a static collection class for all types that inherit from this base.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class EnumCollectionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the generated collection class.
    /// If not specified, defaults to the plural form of the base class name.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the return type for factory methods and properties.
    /// Can be an interface or base type that all enum values implement.
    /// </summary>
    public string? ReturnType { get; set; }

    /// <summary>
    /// Gets or sets whether to generate factory methods for enum values.
    /// Defaults to true.
    /// </summary>
    public bool GenerateFactoryMethods { get; set; }

    /// <summary>
    /// Gets or sets the string comparison type for name lookups.
    /// Defaults to Ordinal.
    /// </summary>
    public StringComparison NameComparison { get; set; }

    /// <summary>
    /// Gets or sets the namespace for the generated collection.
    /// If not specified, uses the base class namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the namespace for the return type if it's not in the same namespace.
    /// </summary>
    public string? ReturnTypeNamespace { get; set; }

    /// <summary>
    /// Gets or sets the default return type for generic enum bases.
    /// </summary>
    public string? DefaultGenericReturnType { get; set; }

    /// <summary>
    /// Gets or sets the namespace for the default generic return type.
    /// </summary>
    public string? DefaultGenericReturnTypeNamespace { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumCollectionAttribute"/> class.
    /// </summary>
    public EnumCollectionAttribute()
    {
        GenerateFactoryMethods = true;
        NameComparison = StringComparison.Ordinal;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumCollectionAttribute"/> class with all options.
    /// </summary>
    /// <param name="collectionName">The name of the generated collection class.</param>
    /// <param name="returnType">The return type for factory methods and properties.</param>
    /// <param name="generateFactoryMethods">Whether to generate factory methods for enum values.</param>
    /// <param name="nameComparison">The string comparison type for name lookups.</param>
    /// <param name="namespace">The namespace for the generated collection.</param>
    /// <param name="returnTypeNamespace">The namespace for the return type if it's not in the same namespace.</param>
    /// <param name="defaultGenericReturnType">The default return type for generic enum bases.</param>
    /// <param name="defaultGenericReturnTypeNamespace">The namespace for the default generic return type.</param>
    public EnumCollectionAttribute(
        string? collectionName = null,
        string? returnType = null,
        bool generateFactoryMethods = true,
        StringComparison nameComparison = StringComparison.Ordinal,
        string? @namespace = null,
        string? returnTypeNamespace = null,
        string? defaultGenericReturnType = null,
        string? defaultGenericReturnTypeNamespace = null)
    {
        CollectionName = collectionName;
        ReturnType = returnType;
        GenerateFactoryMethods = generateFactoryMethods;
        NameComparison = nameComparison;
        Namespace = @namespace;
        ReturnTypeNamespace = returnTypeNamespace;
        DefaultGenericReturnType = defaultGenericReturnType;
        DefaultGenericReturnTypeNamespace = defaultGenericReturnTypeNamespace;
    }
}