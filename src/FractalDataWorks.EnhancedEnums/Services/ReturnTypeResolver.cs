using System;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Resolves the effective return type for enum collections.
/// </summary>
public static class ReturnTypeResolver
{
    /// <summary>
    /// Resolves the effective return type for a collection based on various settings and constraints.
    /// </summary>
    public static string ResolveEffectiveReturnType(EnumTypeInfo definition, INamedTypeSymbol? baseTypeSymbol)
    {
        // Priority 1: Explicit ReturnType from attribute
        if (!string.IsNullOrEmpty(definition.ReturnType))
        {
            return definition.ReturnType!;
        }

        // Priority 2: For generic types, use DefaultGenericReturnType
        if (definition.IsGenericType && !string.IsNullOrEmpty(definition.DefaultGenericReturnType))
        {
            return definition.DefaultGenericReturnType!;
        }

        // Priority 3: For generic types, try to infer from constraints
        if (definition.IsGenericType && baseTypeSymbol != null)
        {
            var constraintType = GetReturnTypeFromConstraints(baseTypeSymbol, definition);
            if (constraintType != null)
            {
                return constraintType;
            }
        }

        // Priority 4: For non-generic types, use the full type name
        if (!definition.IsGenericType)
        {
            return definition.FullTypeName;
        }

        // Fallback: Use object for generic types without better options
        return "object";
    }

    /// <summary>
    /// Attempts to get a suitable return type from generic type constraints.
    /// </summary>
    public static string? GetReturnTypeFromConstraints(INamedTypeSymbol baseTypeSymbol, EnumTypeInfo definition)
    {
        if (!baseTypeSymbol.IsGenericType)
        {
            return null;
        }

        foreach (var typeParam in baseTypeSymbol.TypeParameters)
        {
            // Look for interface constraints first
            var interfaceConstraint = typeParam.ConstraintTypes
                .OfType<INamedTypeSymbol>()
                .FirstOrDefault(t => t.TypeKind == TypeKind.Interface);

            if (interfaceConstraint != null)
            {
                return interfaceConstraint.ToDisplayString();
            }

            // Then look for class constraints
            var classConstraint = typeParam.ConstraintTypes
                .OfType<INamedTypeSymbol>()
                .FirstOrDefault(t => t.TypeKind == TypeKind.Class);

            if (classConstraint != null)
            {
                return classConstraint.ToDisplayString();
            }
        }

        return null;
    }

    /// <summary>
    /// Detects if there's a common interface implemented by enum values and uses it as return type.
    /// </summary>
    public static string? DetectCommonInterface(INamedTypeSymbol valueType)
    {
        // Get all interfaces implemented by the type
        var interfaces = valueType.AllInterfaces
            .Where(i => !IsSystemInterface(i))
            .ToList();

        // Return the first non-system interface if any
        return interfaces.FirstOrDefault()?.ToDisplayString();
    }

    private static bool IsSystemInterface(INamedTypeSymbol interfaceSymbol)
    {
        var ns = interfaceSymbol.ContainingNamespace?.ToDisplayString() ?? "";
        return ns.StartsWith("System", StringComparison.Ordinal) || 
               ns.StartsWith("Microsoft", StringComparison.Ordinal);
    }
}