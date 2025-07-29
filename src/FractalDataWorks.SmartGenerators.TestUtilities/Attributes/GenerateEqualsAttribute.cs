using System;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Attributes;

/// <summary>
/// Attribute used to mark classes for generating Equals and GetHashCode methods in tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateEqualsAttribute : Attribute
{
}
