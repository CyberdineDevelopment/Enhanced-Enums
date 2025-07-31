using System;
using System.Linq;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for building private fields in generated collection classes.
/// </summary>
public static class CollectionFieldsBuilder
{
    /// <summary>
    /// Adds all necessary private fields to the collection class.
    /// </summary>
    public static void AddFields(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType, bool implementsEnhancedOption)
    {
        AddListField(classBuilder, def);
        AddCachedAllField(classBuilder, def, effectiveReturnType);
        AddDictionaryFields(classBuilder, def, effectiveReturnType, implementsEnhancedOption);
    }

    private static void AddListField(ClassBuilder classBuilder, EnumTypeInfo def)
    {
        var fieldBuilder = classBuilder.AddField($"List<{def.FullTypeName}>", "_all", field => field
            .MakePrivate()
            .MakeReadOnly()
            .WithInitializer($"new List<{def.FullTypeName}>()"));

        if (def.GenerateStaticCollection)
        {
            fieldBuilder.MakeStatic();
        }
    }

    private static void AddCachedAllField(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType)
    {
        var fieldBuilder = classBuilder.AddField($"ImmutableArray<{effectiveReturnType}>", "_cachedAll", field => field
            .MakePrivate()
            .MakeReadOnly());

        if (def.GenerateStaticCollection)
        {
            fieldBuilder.MakeStatic();
        }
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