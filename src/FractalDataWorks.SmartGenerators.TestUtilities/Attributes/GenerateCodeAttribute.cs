using System;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Attributes;

/// <summary>
/// Attribute used to mark classes for code generation in tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateCodeAttribute : Attribute
{
}
