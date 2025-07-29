using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Collects and validates enum values from discovered types.
/// </summary>
internal static class EnumValueCollector
{
    /// <summary>
    /// Collects enum values from discovered types that match the definition.
    /// </summary>
    public static List<EnumValueInfo> CollectValues(
        IEnumerable<INamedTypeSymbol> discoveredTypes, 
        EnumTypeInfo definition, 
        INamedTypeSymbol? baseTypeSymbol)
    {
        var values = new List<EnumValueInfo>();

        foreach (var typeSymbol in discoveredTypes)
        {
            var enumValues = ProcessTypeSymbol(typeSymbol, definition, baseTypeSymbol);
            values.AddRange(enumValues);
        }

        return values;
    }

    private static List<EnumValueInfo> ProcessTypeSymbol(
        INamedTypeSymbol typeSymbol, 
        EnumTypeInfo definition, 
        INamedTypeSymbol? baseTypeSymbol)
    {
        var values = new List<EnumValueInfo>();

        var attrs = typeSymbol.GetAttributes()
            .Where(ad => string.Equals(ad.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) ||
                        string.Equals(ad.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal))
            .ToList();

        if (attrs.Count == 0)
        {
            return values;
        }

        // Process each EnumOption attribute separately (for multiple collections support)
        foreach (var attr in attrs)
        {
            var (name, collectionName, generateFactoryMethod) = EnumAttributeParser.ParseEnumOption(attr, typeSymbol);

            // If a collection name is specified, only include this option in matching collections
            if (!string.IsNullOrEmpty(collectionName) && 
                !string.Equals(collectionName, definition.CollectionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Verify the type derives from the base type
            if (baseTypeSymbol != null && !MatchesDefinition(typeSymbol, baseTypeSymbol))
            {
                continue;
            }

            // Detect return type if not specified
            var returnType = GetOptionReturnType(attr, typeSymbol);

            // Find constructors
            var constructors = FindConstructors(typeSymbol);

            values.Add(new EnumValueInfo
            {
                Name = name ?? typeSymbol.Name,
                ShortTypeName = typeSymbol.Name,
                FullTypeName = typeSymbol.ToDisplayString(),
                Constructors = constructors,
                ReturnType = returnType,
                ReturnTypeNamespace = GetReturnTypeNamespace(returnType, typeSymbol),
                GenerateFactoryMethod = generateFactoryMethod,
            });
        }

        return values;
    }

    /// <summary>
    /// Checks if a type matches (derives from) the base enum definition.
    /// </summary>
    public static bool MatchesDefinition(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var current = typeSymbol.BaseType;
        while (current != null)
        {
            // For generic types, compare unbound types
            if (baseTypeSymbol.IsGenericType && current.IsGenericType)
            {
                var currentUnbound = current.ConstructUnboundGenericType();
                var baseUnbound = baseTypeSymbol.ConstructUnboundGenericType();
                
                if (SymbolEqualityComparer.Default.Equals(currentUnbound, baseUnbound))
                {
                    return true;
                }
            }
            else if (SymbolEqualityComparer.Default.Equals(current, baseTypeSymbol))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static string? GetOptionReturnType(AttributeData attr, INamedTypeSymbol typeSymbol)
    {
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        // Check for explicit return type in attribute
        if (named.TryGetValue("ReturnType", out var rt) && rt.Value is string rs)
        {
            return rs;
        }

        // Try to detect common interface
        return ReturnTypeResolver.DetectCommonInterface(typeSymbol);
    }

    private static string? GetReturnTypeNamespace(string? returnType, INamedTypeSymbol typeSymbol)
    {
        if (string.IsNullOrEmpty(returnType))
            return null;

        // If it's a simple type name, try to find the namespace from the type's interfaces
        if (!returnType!.Contains("."))
        {
            var matchingInterface = typeSymbol.AllInterfaces
                .FirstOrDefault(i => string.Equals(i.Name, returnType,StringComparison.OrdinalIgnoreCase));
            
            if (matchingInterface != null)
            {
                return matchingInterface.ContainingNamespace?.ToDisplayString();
            }
        }

        return null;
    }

    private static List<ConstructorInfo> FindConstructors(INamedTypeSymbol typeSymbol)
    {
        var constructors = new List<ConstructorInfo>();

        // Check if the type has a primary constructor
        // Primary constructors are identified by having parameters on the type declaration itself
        var primaryConstructor = typeSymbol.Constructors
            .FirstOrDefault(c => !c.IsStatic && IsPrimaryConstructor(c, typeSymbol));

        foreach (var ctor in typeSymbol.Constructors.Where(c => !c.IsStatic))
        {
            var parameters = new List<ParameterInfo>();
            
            foreach (var param in ctor.Parameters)
            {
                parameters.Add(new ParameterInfo
                {
                    Name = param.Name,
                    TypeName = param.Type.ToDisplayString(),
                    Namespace = param.Type.ContainingNamespace?.ToDisplayString(),
                    HasDefaultValue = param.HasExplicitDefaultValue,
                    DefaultValue = param.HasExplicitDefaultValue ? param.ExplicitDefaultValue?.ToString() : null,
                });
            }

            constructors.Add(new ConstructorInfo
            {
                Parameters = parameters,
                Accessibility = ctor.DeclaredAccessibility,
                IsPrimary = primaryConstructor != null && SymbolEqualityComparer.Default.Equals(ctor, primaryConstructor)
            });
        }

        return constructors;
    }

    /// <summary>
    /// Determines if a constructor is a primary constructor.
    /// Primary constructors have the same parameters as the type declaration and are compiler-generated.
    /// </summary>
    private static bool IsPrimaryConstructor(IMethodSymbol constructor, INamedTypeSymbol typeSymbol)
    {
        // Primary constructors are implicitly declared (compiler-generated)
        if (!constructor.IsImplicitlyDeclared)
            return false;

        // Primary constructors have the same accessibility as the type
        if (constructor.DeclaredAccessibility != typeSymbol.DeclaredAccessibility)
            return false;

        // For records, check if it's the record's primary constructor
        // Records always have primary constructors if they have parameters
        if (typeSymbol.IsRecord)
        {
            // The primary constructor for a record has parameters matching the record's properties
            var recordProperties = typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.IsReadOnly && !p.IsStatic && p.GetMethod?.IsImplicitlyDeclared == true)
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .ToList();

            if (constructor.Parameters.Length == recordProperties.Count)
            {
                for (int i = 0; i < constructor.Parameters.Length; i++)
                {
                    var param = constructor.Parameters[i];
                    var prop = recordProperties.FirstOrDefault(p => string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase));
                    if (prop == null || !SymbolEqualityComparer.Default.Equals(param.Type, prop.Type))
                        return false;
                }
                return true;
            }
        }

        // For regular classes/structs with primary constructors (C# 12+)
        // Primary constructors are the implicitly declared constructors with parameters
        // They don't have explicit constructor syntax in the source
        return constructor.Parameters.Length > 0;
    }
}