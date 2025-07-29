using System;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;

/// <summary>
/// Attribute used to mark classes for code generation in tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class GenerateCodeAttribute : Attribute
{
}
