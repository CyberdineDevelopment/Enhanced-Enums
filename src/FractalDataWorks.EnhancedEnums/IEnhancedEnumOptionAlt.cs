using System;

namespace FractalDataWorks.EnhancedEnums;

/// <summary>
/// Marker interface for enum options that should be collected via interface-based discovery.
/// This enables the Service Type Pattern where enum options can be defined in separate assemblies
/// without creating circular dependencies.
/// </summary>
/// <typeparam name="TBase">The base type or interface that defines the enum contract.</typeparam>
public interface IEnhancedEnumOptionAlt<TBase>
{
}