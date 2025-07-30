using System;

namespace FractalDataWorks;

/// <summary>
/// Marks a concrete enum option with a custom display name and configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EnumOptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOptionAttribute"/> class.
    /// </summary>
    public EnumOptionAttribute()
    {
        Name = null;
        Order = 0;
        CollectionName = null;
        ReturnType = null;
        GenerateFactoryMethod = null;
        MethodName = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOptionAttribute"/> class with all options.
    /// </summary>
    /// <param name="name">Display name for the enum option.</param>
    /// <param name="order">Ordering within the collection.</param>
    /// <param name="collectionName">The collection name this option belongs to.</param>
    /// <param name="returnType">The return type for this specific enum option.</param>
    /// <param name="generateFactoryMethod">Whether to generate a factory method for this specific option.</param>
    /// <param name="methodName">The custom method name for the factory method.</param>
    public EnumOptionAttribute(
        string? name = null,
        int order = 0,
        string? collectionName = null,
        Type? returnType = null,
        bool? generateFactoryMethod = null,
        string? methodName = null)
    {
        Name = name;
        Order = order;
        CollectionName = collectionName;
        ReturnType = returnType;
        GenerateFactoryMethod = generateFactoryMethod;
        MethodName = methodName;
    }

    /// <summary>Gets the display name for the enum option.</summary>
    public string? Name { get; }

    /// <summary>Gets the ordering within the collection.</summary>
    public int Order { get; }

    /// <summary>
    /// Gets the collection name this option belongs to.
    /// When the base enum has multiple collections defined, this specifies which collection(s) to include this option in.
    /// </summary>
    public string? CollectionName { get; }
    
    /// <summary>
    /// Gets the return type for this specific enum option.
    /// This overrides the default return type specified in the EnumCollection attribute.
    /// </summary>
    public Type? ReturnType { get; }

    /// <summary>
    /// Gets whether to generate a factory method for this specific option.
    /// If not specified, inherits from the collection's GenerateFactoryMethods setting.
    /// </summary>
    public bool? GenerateFactoryMethod { get; }

    /// <summary>
    /// Gets the custom method name for the factory method.
    /// If not specified, uses the Name property or class name.
    /// </summary>
    public string? MethodName { get; }
}