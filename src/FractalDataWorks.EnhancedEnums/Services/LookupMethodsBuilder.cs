using System;
using System.Text;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for building lookup methods in generated collection classes.
/// </summary>
public static class LookupMethodsBuilder
{
    /// <summary>
    /// Adds all necessary lookup methods to the collection class.
    /// </summary>
    public static void AddLookupMethods(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType, bool implementsEnhancedOption)
    {
        AddGetByNameMethod(classBuilder, def, effectiveReturnType);
        AddTryGetByNameMethod(classBuilder, def, effectiveReturnType);
        
        AddGetByTypeMethod(classBuilder, def, effectiveReturnType);
        AddTryGetByTypeMethod(classBuilder, def, effectiveReturnType);
        
        if (implementsEnhancedOption)
        {
            AddGetByIdMethod(classBuilder, def, effectiveReturnType);
            AddTryGetByIdMethod(classBuilder, def, effectiveReturnType);
        }
        
        foreach (var lookup in def.LookupProperties)
        {
            AddCustomLookupMethod(classBuilder, def, lookup, effectiveReturnType);
            AddCustomTryLookupMethod(classBuilder, def, lookup, effectiveReturnType);
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
            .WithXmlDocReturns($"The {def.ClassName} with the specified name, or Empty if not found.")
            .WithBody(@"
                if (string.IsNullOrEmpty(name))
                {
                    return Empty;
                }
                
                return _nameDict.TryGetValue(name, out var result) ? result : Empty;
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
            .WithXmlDocReturns($"The {def.ClassName} with the specified id, or Empty if not found.")
            .WithBody(@"
                return _idDict.TryGetValue(id, out var result) ? result : Empty;
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
        if (lookup.PropertyType.Contains("[]", StringComparison.Ordinal) || lookup.PropertyType.Contains("IEnumerable", StringComparison.Ordinal) || lookup.PropertyType.Contains("List", StringComparison.Ordinal))
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
            methodBody.AppendLine("    return Empty;");
            methodBody.AppendLine("}");
            methodBody.AppendLine();
        }
        
        // Use dictionary lookup
        methodBody.AppendLine($"return {dictName}.TryGetValue({paramName}, out var result) ? result : Empty;");
        
        classBuilder.AddMethod(lookup.LookupMethodName, $"{lookupReturnType}?", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter(lookup.PropertyType, paramName)
            .WithXmlDocSummary($"Gets the {def.ClassName} with the specified {lookup.PropertyName}.")
            .WithXmlDocParam(paramName, $"The {lookup.PropertyName} to search for.")
            .WithXmlDocReturns($"The {def.ClassName} with the specified {lookup.PropertyName}, or Empty if not found.")
            .WithBody(methodBody.ToString().TrimEnd()));
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    private static void AddTryGetByNameMethod(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType)
    {
        classBuilder.AddMethod("TryGetByName", "bool", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter("string", "name")
            .AddParameter($"out {effectiveReturnType}?", "result")
            .WithXmlDocSummary($"Tries to get the {def.ClassName} with the specified name.")
            .WithXmlDocParam("name", "The name to search for.")
            .WithXmlDocParam("result", $"When this method returns, contains the {def.ClassName} with the specified name, or Empty if not found.")
            .WithXmlDocReturns("true if an item with the specified name was found; otherwise, false.")
            .WithBody(@"
                if (string.IsNullOrEmpty(name))
                {
                    result = Empty;
                    return false;
                }
                
                if (_nameDict.TryGetValue(name, out result))
                {
                    return true;
                }
                
                result = Empty;
                return false;
            "));
    }

    private static void AddTryGetByIdMethod(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType)
    {
        classBuilder.AddMethod("TryGetById", "bool", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter("int", "id")
            .AddParameter($"out {effectiveReturnType}?", "result")
            .WithXmlDocSummary($"Tries to get the {def.ClassName} with the specified id.")
            .WithXmlDocParam("id", "The id to search for.")
            .WithXmlDocParam("result", $"When this method returns, contains the {def.ClassName} with the specified id, or Empty if not found.")
            .WithXmlDocReturns("true if an item with the specified id was found; otherwise, false.")
            .WithBody(@"
                if (_idDict.TryGetValue(id, out result))
                {
                    return true;
                }
                
                result = Empty;
                return false;
            "));
    }

    private static void AddCustomTryLookupMethod(ClassBuilder classBuilder, EnumTypeInfo def, PropertyLookupInfo lookup, string effectiveReturnType)
    {
        var paramName = ToCamelCase(lookup.PropertyName);
        var lookupReturnType = !string.IsNullOrEmpty(lookup.ReturnType) ? lookup.ReturnType : (effectiveReturnType ?? def.FullTypeName);
        
        // Only add TryGet for single lookups, not multiple
        if (!lookup.AllowMultiple)
        {
            AddSingleTryLookupMethod(classBuilder, def, lookup, lookupReturnType, paramName);
        }
    }

    private static void AddSingleTryLookupMethod(ClassBuilder classBuilder, EnumTypeInfo def, PropertyLookupInfo lookup, string lookupReturnType, string paramName)
    {
        var dictName = $"_{ToCamelCase(lookup.PropertyName)}Dict";
        var methodBody = new StringBuilder();
        
        // Add null check for string types
        if (string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal))
        {
            methodBody.AppendLine($"if (string.IsNullOrEmpty({paramName}))");
            methodBody.AppendLine("{");
            methodBody.AppendLine("    result = Empty;");
            methodBody.AppendLine("    return false;");
            methodBody.AppendLine("}");
            methodBody.AppendLine();
        }
        
        // Use dictionary lookup
        methodBody.AppendLine($"if ({dictName}.TryGetValue({paramName}, out result))");
        methodBody.AppendLine("{");
        methodBody.AppendLine("    return true;");
        methodBody.AppendLine("}");
        methodBody.AppendLine();
        methodBody.AppendLine("result = Empty;");
        methodBody.AppendLine("return false;");
        
        var tryMethodName = $"TryGet{lookup.PropertyName}";
        if (!string.IsNullOrEmpty(lookup.LookupMethodName) && lookup.LookupMethodName.StartsWith("GetBy", StringComparison.Ordinal))
        {
            tryMethodName = lookup.LookupMethodName.Replace("GetBy", "TryGetBy");
        }
        
        classBuilder.AddMethod(tryMethodName, "bool", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter(lookup.PropertyType, paramName)
            .AddParameter($"out {lookupReturnType}?", "result")
            .WithXmlDocSummary($"Tries to get the {def.ClassName} with the specified {lookup.PropertyName}.")
            .WithXmlDocParam(paramName, $"The {lookup.PropertyName} to search for.")
            .WithXmlDocParam("result", $"When this method returns, contains the {def.ClassName} with the specified {lookup.PropertyName}, or Empty if not found.")
            .WithXmlDocReturns("true if an item with the specified " + lookup.PropertyName + " was found; otherwise, false.")
            .WithBody(methodBody.ToString().TrimEnd()));
    }

    private static void AddGetByTypeMethod(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType)
    {
        var getByTypeReturnType = effectiveReturnType?.EndsWith("?", StringComparison.Ordinal) == true ? effectiveReturnType : $"{effectiveReturnType}?";
        
        classBuilder.AddMethod("GetByType", getByTypeReturnType!, method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter("Type", "type")
            .WithXmlDocSummary($"Gets the {def.ClassName} with the specified type.")
            .WithXmlDocParam("type", "The type to search for.")
            .WithXmlDocReturns($"The {def.ClassName} with the specified type, or Empty if not found.")
            .WithBody(@"
                if (type == null)
                {
                    return Empty;
                }
                
                return _all.FirstOrDefault(x => x.GetType() == type) ?? Empty;
            "));
    }

    private static void AddTryGetByTypeMethod(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType)
    {
        classBuilder.AddMethod("TryGetByType", "bool", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter("Type", "type")
            .AddParameter($"out {effectiveReturnType}?", "result")
            .WithXmlDocSummary($"Tries to get the {def.ClassName} with the specified type.")
            .WithXmlDocParam("type", "The type to search for.")
            .WithXmlDocParam("result", $"When this method returns, contains the {def.ClassName} with the specified type, or Empty if not found.")
            .WithXmlDocReturns("true if an item with the specified type was found; otherwise, false.")
            .WithBody(@"
                if (type == null)
                {
                    result = Empty;
                    return false;
                }
                
                result = _all.FirstOrDefault(x => x.GetType() == type);
                if (result != null)
                {
                    return true;
                }
                
                result = Empty;
                return false;
            "));
    }
}