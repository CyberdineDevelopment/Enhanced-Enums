using System;
using System.Text;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for building lookup methods in generated collection classes.
/// </summary>
internal static class LookupMethodsBuilder
{
    /// <summary>
    /// Adds all necessary lookup methods to the collection class.
    /// </summary>
    public static void AddLookupMethods(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType, bool implementsEnhancedOption)
    {
        AddGetByNameMethod(classBuilder, def, effectiveReturnType);
        
        if (implementsEnhancedOption)
        {
            AddGetByIdMethod(classBuilder, def, effectiveReturnType);
        }
        
        foreach (var lookup in def.LookupProperties)
        {
            AddCustomLookupMethod(classBuilder, def, lookup, effectiveReturnType);
        }
    }

    private static void AddGetByNameMethod(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType)
    {
        var getByNameReturnType = effectiveReturnType?.EndsWith("?", StringComparison.Ordinal) == true ? effectiveReturnType : $"{effectiveReturnType}?";
        
        classBuilder.AddMethod("GetByName", getByNameReturnType!, method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter("string", "name")
            .WithXmlDocSummary($"Gets the {def.ClassName} with the specified name.")
            .WithXmlDocParam("name", "The name to search for.")
            .WithXmlDocReturns($"The {def.ClassName} with the specified name, or null if not found.")
            .WithBody(@"
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }
                
                _nameDict.TryGetValue(name, out var result);
                return result;
            "));
    }

    private static void AddGetByIdMethod(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType)
    {
        var getByIdReturnType = effectiveReturnType?.EndsWith("?", StringComparison.Ordinal) == true ? effectiveReturnType : $"{effectiveReturnType}?";
        
        classBuilder.AddMethod("GetById", getByIdReturnType!, method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter("int", "id")
            .WithXmlDocSummary($"Gets the {def.ClassName} with the specified id.")
            .WithXmlDocParam("id", "The id to search for.")
            .WithXmlDocReturns($"The {def.ClassName} with the specified id, or null if not found.")
            .WithBody(@"
                _idDict.TryGetValue(id, out var result);
                return result;
            "));
    }

    private static void AddCustomLookupMethod(ClassBuilder classBuilder, EnumTypeInfo def, PropertyLookupInfo lookup, string effectiveReturnType)
    {
        var paramName = ToCamelCase(lookup.PropertyName);
        var lookupReturnType = !string.IsNullOrEmpty(lookup.ReturnType) ? lookup.ReturnType : (effectiveReturnType ?? def.FullTypeName);
        
        if (lookup.AllowMultiple)
        {
            AddMultipleLookupMethod(classBuilder, def, lookup, lookupReturnType, paramName);
        }
        else
        {
            AddSingleLookupMethod(classBuilder, def, lookup, lookupReturnType, paramName);
        }
    }

    private static void AddMultipleLookupMethod(ClassBuilder classBuilder, EnumTypeInfo def, PropertyLookupInfo lookup, string lookupReturnType, string paramName)
    {
        string methodBody;
        
        // Handle collection types - when the property is a collection, check if it contains the search value
        if (lookup.PropertyType.Contains("[]") || lookup.PropertyType.Contains("IEnumerable") || lookup.PropertyType.Contains("List"))
        {
            methodBody = $"return _all.Where(x => x.{lookup.PropertyName}?.Contains({paramName}) ?? false);";
        }
        else
        {
            // Handle simple types - search for all items that have the matching property value
            methodBody = $"return _all.Where(x => Equals(x.{lookup.PropertyName}, {paramName}));";
        }
        
        classBuilder.AddMethod(lookup.LookupMethodName, $"IEnumerable<{lookupReturnType}>", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter(lookup.PropertyType, paramName)
            .WithXmlDocSummary($"Gets the {def.ClassName} with the specified {lookup.PropertyName}.")
            .WithXmlDocParam(paramName, $"The {lookup.PropertyName} to search for.")
            .WithXmlDocReturns($"All {def.ClassName} instances with the specified {lookup.PropertyName}.")
            .WithBody(methodBody));
    }

    private static void AddSingleLookupMethod(ClassBuilder classBuilder, EnumTypeInfo def, PropertyLookupInfo lookup, string lookupReturnType, string paramName)
    {
        var dictName = $"_{ToCamelCase(lookup.PropertyName)}Dict";
        var methodBody = new StringBuilder();
        
        // Add null check for string types
        if (string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal))
        {
            methodBody.AppendLine($"if (string.IsNullOrEmpty({paramName}))");
            methodBody.AppendLine("{");
            methodBody.AppendLine("    return null;");
            methodBody.AppendLine("}");
            methodBody.AppendLine();
        }
        
        // Use dictionary lookup
        methodBody.AppendLine($"{dictName}.TryGetValue({paramName}, out var result);");
        methodBody.AppendLine("return result;");
        
        classBuilder.AddMethod(lookup.LookupMethodName, $"{lookupReturnType}?", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter(lookup.PropertyType, paramName)
            .WithXmlDocSummary($"Gets the {def.ClassName} with the specified {lookup.PropertyName}.")
            .WithXmlDocParam(paramName, $"The {lookup.PropertyName} to search for.")
            .WithXmlDocReturns($"The {def.ClassName} with the specified {lookup.PropertyName}, or null if not found.")
            .WithBody(methodBody.ToString().TrimEnd()));
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}