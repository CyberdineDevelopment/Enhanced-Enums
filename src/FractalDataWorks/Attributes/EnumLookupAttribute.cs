using System;

namespace FractalDataWorks.Attributes;

/// <summary>
/// Marks a property for which to generate lookup methods.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class EnumLookupAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumLookupAttribute"/> class.
    /// </summary>
    /// <param name="methodName">Custom method name for the lookup (e.g. ByName).</param>
    /// <param name="allowMultiple">Allow multiple results per lookup key.</param>
    /// <param name="returnType">The return type for this specific lookup method.</param>
    public EnumLookupAttribute(
        string methodName = "",
        bool allowMultiple = false,
        string? returnType = null)
    {
        MethodName = methodName;
        AllowMultiple = allowMultiple;
        ReturnType = returnType;
    }

    /// <summary>
    /// Gets the custom method name for the lookup (e.g. ByName).
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Gets whether to allow multiple results per lookup key.
    /// </summary>
    public bool AllowMultiple { get; }

    /// <summary>
    /// Gets the return type for this specific lookup method.
    /// If not specified, inherits from the EnhancedEnumBaseAttribute.ReturnType or auto-detected type.
    /// </summary>
    public string? ReturnType { get; }
}