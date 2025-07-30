using System;
using System.Linq;
using System.Text;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.SmartGenerators;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for building static constructor logic for collection classes.
/// </summary>
internal static class StaticConstructorBuilder
{
    /// <summary>
    /// Builds the static constructor body for initializing enum collections.
    /// </summary>
    public static string BuildConstructorBody(EnumTypeInfo def, EquatableArray<EnumValueInfo> values, string effectiveReturnType, bool implementsEnhancedOption)
    {
        var constructorBody = new StringBuilder();
        
        AddEnumValueInstantiation(constructorBody, values);
        AddImmutableArrayInitialization(constructorBody, effectiveReturnType);
        AddDictionaryInitialization(constructorBody, def, effectiveReturnType, implementsEnhancedOption);
        
        return constructorBody.ToString();
    }

    private static void AddEnumValueInstantiation(StringBuilder constructorBody, EquatableArray<EnumValueInfo> values)
    {
        // Add each enum value to the collection
        foreach (var value in values)
        {
            // Always use 'new' to create instances for the collection
            constructorBody.AppendLine($"_all.Add(new {value.FullTypeName}());");
        }
        constructorBody.AppendLine();
    }

    private static void AddImmutableArrayInitialization(StringBuilder constructorBody, string effectiveReturnType)
    {
        constructorBody.AppendLine("// Cache the immutable array to prevent repeated allocations");
        constructorBody.AppendLine($"_cachedAll = _all.Cast<{effectiveReturnType}>().ToImmutableArray();");
        constructorBody.AppendLine();
    }

    private static void AddDictionaryInitialization(StringBuilder constructorBody, EnumTypeInfo def, string effectiveReturnType, bool implementsEnhancedOption)
    {
        constructorBody.AppendLine("// Populate dictionaries for fast lookups");
        
        AddNet8DictionaryInitialization(constructorBody, def, effectiveReturnType, implementsEnhancedOption);
        AddLegacyDictionaryInitialization(constructorBody, def, implementsEnhancedOption);
    }

    private static void AddNet8DictionaryInitialization(StringBuilder constructorBody, EnumTypeInfo def, string effectiveReturnType, bool implementsEnhancedOption)
    {
        constructorBody.AppendLine("#if NET8_0_OR_GREATER");
        constructorBody.AppendLine("// Create temp dictionaries for FrozenDictionary initialization");
        constructorBody.AppendLine($"var tempNameDict = new Dictionary<string, {effectiveReturnType}>(StringComparer.{def.NameComparison});");
        
        if (implementsEnhancedOption)
        {
            constructorBody.AppendLine($"var tempIdDict = new Dictionary<int, {effectiveReturnType}>();");
        }
        
        foreach (var lookup in def.LookupProperties.Where(l => !l.AllowMultiple))
        {
            var comparerStr = string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal) 
                ? $"(StringComparer.{def.NameComparison})" 
                : "()";
            constructorBody.AppendLine($"var temp{lookup.PropertyName}Dict = new Dictionary<{lookup.PropertyType}, {effectiveReturnType}>{comparerStr};");
        }
        
        AddDictionaryPopulationLogic(constructorBody, def, implementsEnhancedOption, isNet8: true);
        
        constructorBody.AppendLine();
        constructorBody.AppendLine("// Convert to FrozenDictionaries for better performance");
        constructorBody.AppendLine($"_nameDict = tempNameDict.ToFrozenDictionary(StringComparer.{def.NameComparison});");
        
        if (implementsEnhancedOption)
        {
            constructorBody.AppendLine("_idDict = tempIdDict.ToFrozenDictionary();");
        }
        
        foreach (var lookup in def.LookupProperties.Where(l => !l.AllowMultiple))
        {
            var comparerStr = string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal) 
                ? $"(StringComparer.{def.NameComparison})" 
                : "()";
            constructorBody.AppendLine($"_{ToCamelCase(lookup.PropertyName)}Dict = temp{lookup.PropertyName}Dict.ToFrozenDictionary{comparerStr};");
        }
        
        constructorBody.AppendLine("#else");
    }

    private static void AddLegacyDictionaryInitialization(StringBuilder constructorBody, EnumTypeInfo def, bool implementsEnhancedOption)
    {
        AddDictionaryPopulationLogic(constructorBody, def, implementsEnhancedOption, isNet8: false);
        constructorBody.AppendLine("#endif");
    }

    private static void AddDictionaryPopulationLogic(StringBuilder constructorBody, EnumTypeInfo def, bool implementsEnhancedOption, bool isNet8)
    {
        var nameDict = isNet8 ? "tempNameDict" : "_nameDict";
        var idDict = isNet8 ? "tempIdDict" : "_idDict";
        
        constructorBody.AppendLine("foreach (var item in _all)");
        constructorBody.AppendLine("{");
        constructorBody.AppendLine($"    if (!{nameDict}.ContainsKey(item.Name))");
        constructorBody.AppendLine("    {");
        constructorBody.AppendLine($"        {nameDict}[item.Name] = item;");
        constructorBody.AppendLine("    }");
        
        if (implementsEnhancedOption)
        {
            constructorBody.AppendLine();
            constructorBody.AppendLine($"    if (!{idDict}.ContainsKey(item.Id))");
            constructorBody.AppendLine("    {");
            constructorBody.AppendLine($"        {idDict}[item.Id] = item;");
            constructorBody.AppendLine("    }");
        }
        
        foreach (var lookup in def.LookupProperties.Where(l => !l.AllowMultiple))
        {
            var dictName = isNet8 ? $"temp{lookup.PropertyName}Dict" : $"_{ToCamelCase(lookup.PropertyName)}Dict";
            constructorBody.AppendLine();
            constructorBody.AppendLine($"    if (!{dictName}.ContainsKey(item.{lookup.PropertyName}))");
            constructorBody.AppendLine("    {");
            constructorBody.AppendLine($"        {dictName}[item.{lookup.PropertyName}] = item;");
            constructorBody.AppendLine("    }");
        }
        
        constructorBody.AppendLine("}");
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}