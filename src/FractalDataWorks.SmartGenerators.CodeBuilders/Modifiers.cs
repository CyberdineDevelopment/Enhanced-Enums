using System;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Defines additional modifiers for class members and types.
/// </summary>
[Flags]
public enum Modifiers
{
    /// <summary>
    /// No modifiers specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// The static modifier.
    /// </summary>
    Static = 1 << 0,

    /// <summary>
    /// The readonly modifier.
    /// </summary>
    ReadOnly = 1 << 1,

    /// <summary>
    /// The override modifier.
    /// </summary>
    Override = 1 << 2,

    /// <summary>
    /// The sealed modifier.
    /// </summary>
    Sealed = 1 << 3,

    /// <summary>
    /// The virtual modifier.
    /// </summary>
    Virtual = 1 << 4,

    /// <summary>
    /// The abstract modifier.
    /// </summary>
    Abstract = 1 << 5,

    /// <summary>
    /// The async modifier.
    /// </summary>
    Async = 1 << 6,

    /// <summary>
    /// The partial modifier.
    /// </summary>
    Partial = 1 << 7,

    /// <summary>
    /// The this modifier for parameters.
    /// </summary>
    This = 1 << 8,

    /// <summary>
    /// The required modifier for properties.
    /// </summary>
    Required = 1 << 9,
}
