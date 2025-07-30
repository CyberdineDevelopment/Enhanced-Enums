using System;
using System.Linq;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for building private fields in generated collection classes.
/// </summary>
internal static class CollectionFieldsBuilder
{
    /// <summary>
    /// Adds all necessary private fields to the collection class.
    /// </summary>
    public static void AddFields(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType, bool implementsEnhancedOption)
    {
        AddListField(classBuilder, def);
        AddCachedAllField(classBuilder, effectiveReturnType);
        AddDictionaryFields(classBuilder, def, effectiveReturnType, implementsEnhancedOption);
    }

    private static void AddListField(ClassBuilder classBuilder, EnumTypeInfo def)
    {
        classBuilder.AddField($"List<{def.FullTypeName}>", "_all", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly()
            .WithInitializer($"new List<{def.FullTypeName}>()"));
    }

    private static void AddCachedAllField(ClassBuilder classBuilder, string effectiveReturnType)
    {
        classBuilder.AddField($"ImmutableArray<{effectiveReturnType}>", "_cachedAll", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly());
    }

    private static void AddDictionaryFields(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType, bool implementsEnhancedOption)
    {
        // Add conditional compilation fields for NET8+ frozen dictionaries
        classBuilder.AddCodeBlock($@"#if NET8_0_OR_GREATER
private static readonly FrozenDictionary<string, {effectiveReturnType}> _nameDict;");
        
        if (implementsEnhancedOption)
        {
            classBuilder.AddCodeBlock($"private static readonly FrozenDictionary<int, {effectiveReturnType}> _idDict;");
        }
        
        foreach (var lookup in def.LookupProperties.Where(l => !l.AllowMultiple))
        {
            classBuilder.AddCodeBlock($"private static readonly FrozenDictionary<{lookup.PropertyType}, {effectiveReturnType}> _{ToCamelCase(lookup.PropertyName)}Dict;");
        }
        
        // Add legacy dictionary fields for older .NET versions
        classBuilder.AddCodeBlock("#else");
        
        classBuilder.AddField($"Dictionary<string, {effectiveReturnType}>", "_nameDict", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly()
            .WithInitializer($"new Dictionary<string, {effectiveReturnType}>(StringComparer.{def.NameComparison})"));
        
        if (implementsEnhancedOption)
        {
            classBuilder.AddField($"Dictionary<int, {effectiveReturnType}>", "_idDict", field => field
                .MakePrivate()
                .MakeStatic()
                .MakeReadOnly()
                .WithInitializer($"new Dictionary<int, {effectiveReturnType}>()"));
        }
        
        foreach (var lookup in def.LookupProperties.Where(l => !l.AllowMultiple))
        {
            var comparerStr = string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal) 
                ? $"(StringComparer.{def.NameComparison})" 
                : "()";
            classBuilder.AddField($"Dictionary<{lookup.PropertyType}, {effectiveReturnType}>", $"_{ToCamelCase(lookup.PropertyName)}Dict", field => field
                .MakePrivate()
                .MakeStatic()
                .MakeReadOnly()
                .WithInitializer($"new Dictionary<{lookup.PropertyType}, {effectiveReturnType}>{comparerStr}"));
        }
        
        classBuilder.AddCodeBlock("#endif");
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}