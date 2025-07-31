using System;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.SmartGenerators;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for building factory methods in generated collection classes.
/// </summary>
public static class FactoryMethodsBuilder
{
    /// <summary>
    /// Adds factory methods for each enum value to the collection class.
    /// </summary>
    public static void AddFactoryMethods(ClassBuilder classBuilder, EnumTypeInfo def, EquatableArray<EnumValueInfo> values, string effectiveReturnType)
    {
        if (values.IsEmpty)
            return;

        classBuilder.AddCodeBlock("// Static factory methods");
        
        foreach (var value in values)
        {
            AddFactoryMethod(classBuilder, def, value, effectiveReturnType);
        }
    }

    private static void AddFactoryMethod(ClassBuilder classBuilder, EnumTypeInfo def, EnumValueInfo value, string effectiveReturnType)
    {
        // Use the Name property which comes from EnumOptionAttribute.Name or falls back to class name
        var methodName = MakeValidIdentifier(value.Name);
        
        classBuilder.AddMethod(methodName, effectiveReturnType ?? def.FullTypeName, method => method
            .MakePublic()
            .MakeStatic()
            .WithBody($"return new {value.FullTypeName}();")
            .WithXmlDocSummary($"Creates a new {value.Name} instance."));
    }

    /// <summary>
    /// Makes a valid C# identifier from a string.
    /// </summary>
    private static string MakeValidIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "_";
        
        // Replace spaces and special characters with underscores
        var result = System.Text.RegularExpressions.Regex.Replace(name, @"[^\w]", "_");
        
        // Ensure it doesn't start with a number
        if (char.IsDigit(result[0]))
            result = "_" + result;
        
        // Remove consecutive underscores
        result = System.Text.RegularExpressions.Regex.Replace(result, @"_+", "_");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"_", string.Empty);
        
        // Trim underscores from ends
        result = result.Trim('_');
        
        return string.IsNullOrEmpty(result) ? "_" : result;
    }
}