using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Models;
using Humanizer;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Parses attributes related to Enhanced Enums and extracts configuration.
/// </summary>
internal static class EnumAttributeParser
{
    /// <summary>
    /// Parses EnumCollection attributes from a type symbol and returns EnumTypeInfo for each collection.
    /// </summary>
    public static List<EnumTypeInfo> ParseEnhancedEnumBase(INamedTypeSymbol symbol)
    {
        var attrs = symbol.GetAttributes()
            .Where(ad => string.Equals(ad.AttributeClass?.Name, "EnumCollectionAttribute", StringComparison.Ordinal) ||
                        string.Equals(ad.AttributeClass?.Name, "EnumCollection", StringComparison.Ordinal))
            .ToList();

        if (attrs.Count == 0)
        {
            return new List<EnumTypeInfo>();
        }
        
        // Verify the type inherits from EnhancedEnumBase<T>
        if (!InheritsFromEnhancedEnumBase(symbol))
        {
            // Type must inherit from EnhancedEnumBase<T> to use EnumCollection attribute
            return new List<EnumTypeInfo>();
        }

        // First, collect lookup properties (same for all collections)
        var lookupProperties = ExtractLookupProperties(symbol);

        // Process each EnumCollection attribute to create separate collections
        var results = new List<EnumTypeInfo>();

        foreach (var attr in attrs)
        {
            var collectionInfo = CreateEnumTypeInfo(symbol, attr, lookupProperties);
            results.Add(collectionInfo);
        }

        return results;
    }

    /// <summary>
    /// Extracts the collection name from an attribute.
    /// </summary>
    public static string ExtractCollectionName(AttributeData attr, INamedTypeSymbol symbol)
    {
        // Check constructor argument first
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string collName && !string.IsNullOrEmpty(collName))
        {
            return collName;
        }

        // Default to plural following convention: ColorEnumBase -> Colors
        var baseName = symbol.Name;
        if (baseName.EndsWith("EnumBase", StringComparison.Ordinal))
        {
            baseName = baseName.Substring(0, baseName.Length - 8); // Remove "EnumBase"
        }
        else if (baseName.EndsWith("Base", StringComparison.Ordinal))
        {
            baseName = baseName.Substring(0, baseName.Length - 4); // Remove "Base"
        }
        
        return baseName.Pluralize();
    }

    /// <summary>
    /// Parses an EnumOption attribute and returns the configuration.
    /// </summary>
    public static (string? name, string? collectionName, bool? generateFactoryMethod) ParseEnumOption(AttributeData attr, INamedTypeSymbol typeSymbol)
    {
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        var name = named.TryGetValue("Name", out var n) && n.Value is string ns
            ? ns : typeSymbol.Name;

        var collectionName = named.TryGetValue("CollectionName", out var cn) && cn.Value is string cns
            ? cns : null;
            
        var generateFactoryMethod = named.TryGetValue("GenerateFactoryMethod", out var gfm) && gfm.Value is bool gfmb
            ? (bool?)gfmb : null;

        return (name, collectionName, generateFactoryMethod);
    }

    private static List<PropertyLookupInfo> ExtractLookupProperties(INamedTypeSymbol symbol)
    {
        var lookupProperties = new List<PropertyLookupInfo>();
        
        foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            var lookupAttr = prop.GetAttributes()
                .FirstOrDefault(ad => string.Equals(ad.AttributeClass?.Name, "EnumLookupAttribute", StringComparison.Ordinal) ||
                                    string.Equals(ad.AttributeClass?.Name, "EnumLookup", StringComparison.Ordinal));
            
            if (lookupAttr == null)
            {
                continue;
            }

            var lnamed = lookupAttr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var methodName = lnamed.TryGetValue("MethodName", out var mn) && mn.Value is string ms
                ? ms : $"GetBy{prop.Name}";
            var allowMultiple = lnamed.TryGetValue("AllowMultiple", out var am) && am.Value is bool mu && mu;
            var returnType = lnamed.TryGetValue("ReturnType", out var rt) && rt.Value is string rs ? rs : null;

            lookupProperties.Add(new PropertyLookupInfo
            {
                PropertyName = prop.Name,
                PropertyType = prop.Type.ToDisplayString(),
                LookupMethodName = methodName,
                AllowMultiple = allowMultiple,
                IsNullable = prop.Type.NullableAnnotation == NullableAnnotation.Annotated,
                ReturnType = returnType,
            });
        }

        return lookupProperties;
    }

    private static EnumTypeInfo CreateEnumTypeInfo(INamedTypeSymbol symbol, AttributeData attr, List<PropertyLookupInfo> lookupProperties)
    {
        var collectionName = ExtractCollectionName(attr, symbol);
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var collectionInfo = new EnumTypeInfo
        {
            Namespace = named.TryGetValue("Namespace", out var ns) && ns.Value is string nsStr && !string.IsNullOrEmpty(nsStr)
                ? nsStr : symbol.ContainingNamespace.ToDisplayString(),
            ClassName = symbol.Name,
            FullTypeName = symbol.ToDisplayString(),
            IsGenericType = symbol.IsGenericType,
            CollectionName = collectionName,
            GenerateFactoryMethods = named.TryGetValue("GenerateFactoryMethods", out var gfm) && gfm.Value is bool gfmb ? gfmb : true, // Default to true
            NameComparison = named.TryGetValue("NameComparison", out var nc) && nc.Value is int ic
                ? (StringComparison)ic : StringComparison.OrdinalIgnoreCase,
            ReturnType = named.TryGetValue("ReturnType", out var rt) && rt.Value is string rs ? rs : null,
            ReturnTypeNamespace = named.TryGetValue("ReturnTypeNamespace", out var rtn) && rtn.Value is string rtns ? rtns : null,
            LookupProperties = new EquatableArray<PropertyLookupInfo>(lookupProperties),
        };

        // Extract generic type information
        if (symbol.IsGenericType)
        {
            ExtractGenericTypeInfo(symbol, collectionInfo);
        }

        // Get default generic return type from attribute
        collectionInfo.DefaultGenericReturnType = named.TryGetValue("DefaultGenericReturnType", out var dgrt) && dgrt.Value is string dgrts ? dgrts : null;
        collectionInfo.DefaultGenericReturnTypeNamespace = named.TryGetValue("DefaultGenericReturnTypeNamespace", out var dgrtn) && dgrtn.Value is string dgrtns ? dgrtns : null;

        return collectionInfo;
    }

    private static void ExtractGenericTypeInfo(INamedTypeSymbol symbol, EnumTypeInfo info)
    {
        if (!symbol.IsGenericType)
            return;

        var unboundType = symbol.ConstructUnboundGenericType();
        info.UnboundTypeName = unboundType.ToDisplayString();

        info.TypeParameters = symbol.TypeParameters.Select(tp => tp.Name).ToList();
        
        info.TypeConstraints = symbol.TypeParameters
            .Where(tp => tp.ConstraintTypes.Length > 0)
            .Select(tp => $"where {tp.Name} : {string.Join(", ", tp.ConstraintTypes.Select(ct => ct.ToDisplayString()))}")
            .ToList();
    }
    
    /// <summary>
    /// Checks if a type inherits from EnhancedEnumBase&lt;T&gt;.
    /// </summary>
    private static bool InheritsFromEnhancedEnumBase(INamedTypeSymbol symbol)
    {
        var currentType = symbol.BaseType;
        while (currentType != null)
        {
            var typeName = currentType.Name;
            var fullName = currentType.ToDisplayString();
            
            // Check if it's EnhancedEnumBase or EnhancedEnumBase<T>
            if (string.Equals(typeName, "EnhancedEnumBase", StringComparison.Ordinal) ||
                fullName.Contains("EnhancedEnumBase<"))
            {
                return true;
            }
            
            currentType = currentType.BaseType;
        }
        
        return false;
    }
}