using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Builds the generated collection class code for Enhanced Enums.
/// </summary>
internal static class EnumCollectionBuilder
{
    /// <summary>
    /// Builds the complete collection class source code.
    /// </summary>
    public static string BuildCollection(
        EnumTypeInfo definition, 
        List<EnumValueInfo> values, 
        string effectiveReturnType,
        INamedTypeSymbol? baseTypeSymbol)
    {
        // Create code builder for the entire file
        var codeBuilder = new CodeBuilder();
        
        // Add nullable directive
        codeBuilder.AppendLine("#nullable enable");
        codeBuilder.AppendLine();
        
        // Create namespace builder
        var namespaceBuilder = new NamespaceBuilder(definition.Namespace)
            .AddUsing("System")
            .AddUsing("System.Linq")
            .AddUsing("System.Collections.Generic")
            .AddUsing("System.Collections.Immutable");
        
        // Add conditional using for .NET 8+
        codeBuilder.AppendLine("#if NET8_0_OR_GREATER");
        codeBuilder.AppendLine("using System.Collections.Frozen;");
        codeBuilder.AppendLine("#endif");
        codeBuilder.AppendLine();
        
        // Extract and add additional namespaces
        var additionalNamespaces = ExtractAdditionalNamespaces(definition, values);
        foreach (var ns in additionalNamespaces.OrderBy(n => n, StringComparer.Ordinal))
        {
            namespaceBuilder.AddUsing(ns);
        }
        
        // Build the class
        var classBuilder = CreateClassBuilder(definition);
        
        // Add members
        AddStaticFields(classBuilder, definition, effectiveReturnType);
        AddStaticConstructor(classBuilder, definition, values, effectiveReturnType);
        AddAllProperty(classBuilder, effectiveReturnType);
        AddGetByNameMethods(classBuilder, definition, effectiveReturnType);
        AddLookupMethods(classBuilder, definition, effectiveReturnType);
        
        // Add factory methods if enabled
        if (definition.GenerateFactoryMethods)
        {
            AddFactoryMethods(classBuilder, values, effectiveReturnType);
        }
        
        // Add Empty property (without the nested class)
        AddEmptyProperty(classBuilder, definition, effectiveReturnType);
        
        // Add the class to the namespace
        namespaceBuilder.AddClass(classBuilder);
        
        // Add the empty class as a separate public class
        var emptyClass = CreateEmptyClass(definition, effectiveReturnType, baseTypeSymbol);
        namespaceBuilder.AddClass(emptyClass);
        
        // Build the complete source
        return codeBuilder.Append(namespaceBuilder.Build()).Build();
    }

    private static HashSet<string> ExtractAdditionalNamespaces(EnumTypeInfo definition, List<EnumValueInfo> values)
    {
        var additionalNamespaces = new HashSet<string>(StringComparer.Ordinal);
        
        foreach (var value in values)
        {
            if (!string.IsNullOrEmpty(value.ReturnTypeNamespace))
            {
                additionalNamespaces.Add(value.ReturnTypeNamespace!);
            }
            
            foreach (var ctor in value.Constructors)
            {
                foreach (var param in ctor.Parameters)
                {
                    if (!string.IsNullOrEmpty(param.Namespace) && !param.Namespace!.StartsWith("System", StringComparison.Ordinal))
                    {
                        additionalNamespaces.Add(param.Namespace);
                    }
                }
            }
        }
        
        // Add return type namespace if specified
        if (!string.IsNullOrEmpty(definition.ReturnTypeNamespace))
        {
            additionalNamespaces.Add(definition.ReturnTypeNamespace!);
        }
        
        if (!string.IsNullOrEmpty(definition.DefaultGenericReturnTypeNamespace))
        {
            additionalNamespaces.Add(definition.DefaultGenericReturnTypeNamespace!);
        }
        
        return additionalNamespaces;
    }

    private static ClassBuilder CreateClassBuilder(EnumTypeInfo definition)
    {
        return new ClassBuilder(definition.CollectionName)
            .MakePublic()
            .MakeStatic()
            .WithXmlDocSummary($"Collection of all {definition.ClassName} values.");
    }

    private static void AddStaticFields(ClassBuilder classBuilder, EnumTypeInfo definition, string effectiveReturnType)
    {
        // Add _all field
        classBuilder.AddField($"ImmutableArray<{effectiveReturnType}>", "_all", field => field
            .MakePrivate()
            .MakeStatic());

        // Add _byName dictionary - we'll handle conditional compilation in the static constructor
        // For now, use the base type that both dictionaries share
        classBuilder.AddField($"IReadOnlyDictionary<string, {effectiveReturnType}>", "_byName", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly());

        // Add lookup dictionaries for each lookup property
        foreach (var lookup in definition.LookupProperties)
        {
            var fieldName = $"_{ToCamelCase(lookup.PropertyName)}Lookup";
            
            if (lookup.AllowMultiple)
            {
                classBuilder.AddField($"IReadOnlyDictionary<{lookup.PropertyType}, ImmutableArray<{effectiveReturnType}>>", fieldName, field => field
                    .MakePrivate()
                    .MakeStatic()
                    .MakeReadOnly());
            }
            else
            {
                classBuilder.AddField($"IReadOnlyDictionary<{lookup.PropertyType}, {effectiveReturnType}>", fieldName, field => field
                    .MakePrivate()
                    .MakeStatic()
                    .MakeReadOnly());
            }
        }
    }

    private static void AddStaticConstructor(ClassBuilder classBuilder, EnumTypeInfo definition, List<EnumValueInfo> values, string effectiveReturnType)
    {
        var body = new CodeBuilder(8); // Use 8 spaces for constructor body indentation
        
        // Create instances
        body.AppendLine("var values = new []");
        body.AppendLine("{");
        body.Indent();
        
        foreach (var value in values)
        {
            var valueReturnType = !string.IsNullOrEmpty(value.ReturnType) 
                ? value.ReturnType 
                : effectiveReturnType;
            
            body.AppendLine($"new {value.FullTypeName}() as {valueReturnType},");
        }
        
        body.Outdent();
        body.AppendLine("};");
        body.AppendLine();
        body.AppendLine("_all = values.ToImmutableArray();");
        body.AppendLine();
        
        // Build name dictionary
        var comparison = definition.NameComparison == StringComparison.OrdinalIgnoreCase 
            ? "StringComparer.OrdinalIgnoreCase" 
            : "StringComparer.Ordinal";
        
        body.AppendLine("#if NET8_0_OR_GREATER");
        body.AppendLine($"_byName = _all.ToFrozenDictionary(v => ((dynamic)v).Name, {comparison});");
        body.AppendLine("#else");
        body.AppendLine($"_byName = _all.ToImmutableDictionary(v => ((dynamic)v).Name, {comparison});");
        body.AppendLine("#endif");
        
        // Build lookup dictionaries
        foreach (var lookup in definition.LookupProperties)
        {
            AddLookupDictionaryInitialization(body, lookup, comparison);
        }
        
        classBuilder.AddConstructor(ctor => ctor
            .MakeStatic()
            .WithBody(body.Build()));
    }

    private static void AddLookupDictionaryInitialization(CodeBuilder body, PropertyLookupInfo lookup, string comparison)
    {
        var fieldName = $"_{ToCamelCase(lookup.PropertyName)}Lookup";
        
        body.AppendLine();
        
        if (lookup.AllowMultiple)
        {
            body.AppendLine("#if NET8_0_OR_GREATER");
            body.AppendLine($"{fieldName} = _all.GroupBy(v => ((dynamic)v).{lookup.PropertyName}).ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray(), {comparison});");
            body.AppendLine("#else");
            body.AppendLine($"{fieldName} = _all.GroupBy(v => ((dynamic)v).{lookup.PropertyName}).ToImmutableDictionary(g => g.Key, g => g.ToImmutableArray(), {comparison});");
            body.AppendLine("#endif");
        }
        else
        {
            body.AppendLine("#if NET8_0_OR_GREATER");
            body.AppendLine($"{fieldName} = _all.ToFrozenDictionary(v => ((dynamic)v).{lookup.PropertyName}, {comparison});");
            body.AppendLine("#else");
            body.AppendLine($"{fieldName} = _all.ToImmutableDictionary(v => ((dynamic)v).{lookup.PropertyName}, {comparison});");
            body.AppendLine("#endif");
        }
    }

    private static void AddAllProperty(ClassBuilder classBuilder, string effectiveReturnType)
    {
        classBuilder.AddProperty($"ImmutableArray<{effectiveReturnType}>", "All", prop => prop
            .MakePublic()
            .MakeStatic()
            .WithGetter("return _all;")
            .WithXmlDocSummary("Gets all enum values."));
    }

    private static void AddGetByNameMethods(ClassBuilder classBuilder, EnumTypeInfo definition, string effectiveReturnType)
    {
        // GetByName method
        var getByNameBody = new CodeBuilder(8);
        getByNameBody.AppendLine("if (string.IsNullOrEmpty(name))");
        getByNameBody.AppendLine("    return Empty;");
        getByNameBody.AppendLine();
        getByNameBody.AppendLine("if (_byName.TryGetValue(name, out var value))");
        getByNameBody.AppendLine("    return value;");
        getByNameBody.AppendLine();
        getByNameBody.AppendLine("return Empty;");
        
        classBuilder.AddMethod("GetByName", effectiveReturnType, method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter("string", "name")
            .WithXmlDocSummary("Gets an enum value by its name.")
            .WithXmlDocParam("name", "The name of the enum value.")
            .WithXmlDocReturns("The enum value with the specified name, or Empty if not found.")
            .WithBody(getByNameBody.Build()));

        // TryGetByName method
        var tryGetByNameBody = new CodeBuilder(8);
        tryGetByNameBody.AppendLine("if (string.IsNullOrEmpty(name))");
        tryGetByNameBody.AppendLine("{");
        tryGetByNameBody.AppendLine("    value = null;");
        tryGetByNameBody.AppendLine("    return false;");
        tryGetByNameBody.AppendLine("}");
        tryGetByNameBody.AppendLine();
        tryGetByNameBody.AppendLine("return _byName.TryGetValue(name, out value);");
        
        classBuilder.AddMethod("TryGetByName", "bool", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter("string", "name")
            .AddParameter($"out {effectiveReturnType}?", "value")
            .WithXmlDocSummary("Tries to get an enum value by its name.")
            .WithXmlDocParam("name", "The name of the enum value.")
            .WithXmlDocParam("value", "When this method returns, contains the enum value if found; otherwise, null.")
            .WithXmlDocReturns("true if an enum value with the specified name was found; otherwise, false.")
            .WithBody(tryGetByNameBody.Build()));
    }

    private static void AddLookupMethods(ClassBuilder classBuilder, EnumTypeInfo definition, string effectiveReturnType)
    {
        foreach (var lookup in definition.LookupProperties)
        {
            var lookupReturnType = !string.IsNullOrEmpty(lookup.ReturnType) ? lookup.ReturnType : effectiveReturnType;
            
            if (lookup.AllowMultiple)
            {
                AddMultiValueLookupMethod(classBuilder, lookup, lookupReturnType!);
            }
            else
            {
                AddSingleValueLookupMethod(classBuilder, lookup, lookupReturnType!);
            }
        }
    }

    private static void AddMultiValueLookupMethod(ClassBuilder classBuilder, PropertyLookupInfo lookup, string returnType)
    {
        var fieldName = $"_{ToCamelCase(lookup.PropertyName)}Lookup";
        var paramName = ToCamelCase(lookup.PropertyName);
        
        var body = new CodeBuilder(8);
        body.AppendLine($"if ({fieldName}.TryGetValue({paramName}, out var values))");
        body.AppendLine("    return values;");
        body.AppendLine();
        body.AppendLine($"return ImmutableArray<{returnType}>.Empty;");
        
        classBuilder.AddMethod(lookup.LookupMethodName, $"ImmutableArray<{returnType}>", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter(lookup.PropertyType, paramName)
            .WithXmlDocSummary($"Gets all enum values with the specified {lookup.PropertyName}.")
            .WithXmlDocParam(paramName, $"The {lookup.PropertyName} value to search for.")
            .WithXmlDocReturns($"All enum values with the specified {lookup.PropertyName}, or an empty array if none found.")
            .WithBody(body.Build()));
    }

    private static void AddSingleValueLookupMethod(ClassBuilder classBuilder, PropertyLookupInfo lookup, string returnType)
    {
        var fieldName = $"_{ToCamelCase(lookup.PropertyName)}Lookup";
        var paramName = ToCamelCase(lookup.PropertyName);
        
        var body = new CodeBuilder(8);
        body.AppendLine($"{fieldName}.TryGetValue({paramName}, out var value);");
        body.AppendLine("return value;");
        
        classBuilder.AddMethod(lookup.LookupMethodName, $"{returnType}?", method => method
            .MakePublic()
            .MakeStatic()
            .AddParameter(lookup.PropertyType, paramName)
            .WithXmlDocSummary($"Gets the enum value with the specified {lookup.PropertyName}.")
            .WithXmlDocParam(paramName, $"The {lookup.PropertyName} value to search for.")
            .WithXmlDocReturns($"The enum value with the specified {lookup.PropertyName}, or null if not found.")
            .WithBody(body.Build()));
    }

    private static void AddFactoryMethods(ClassBuilder classBuilder, List<EnumValueInfo> values, string effectiveReturnType)
    {
        foreach (var value in values)
        {
            // Skip if this specific option has GenerateFactoryMethod = false
            if (value.GenerateFactoryMethod == false)
            {
                continue;
            }
            
            var methodName = value.Name;
            var valueReturnType = !string.IsNullOrEmpty(value.ReturnType) ? value.ReturnType! : effectiveReturnType;
            
            // Get public constructors for this type
            var publicConstructors = value.Constructors
                .Where(c => c.Accessibility == Accessibility.Public)
                .ToList();
            
            if (publicConstructors.Count == 0)
            {
                // No public constructors - shouldn't happen with analyzer
                continue;
            }
            
            // Generate a factory method for each public constructor
            foreach (var ctor in publicConstructors)
            {
                if (ctor.Parameters.Count == 0)
                {
                    // Parameterless constructor - simple factory method
                    classBuilder.AddMethod(methodName, valueReturnType!, method => method
                        .MakePublic()
                        .MakeStatic()
                        .WithXmlDocSummary($"Creates a new instance of {value.Name}.")
                        .WithXmlDocReturns($"A new {value.Name} instance.")
                        .WithExpressionBody($"new {value.FullTypeName}()"));
                }
                else
                {
                    // Constructor with parameters - generate overload
                    classBuilder.AddMethod(methodName, valueReturnType, method => 
                    {
                        method.MakePublic()
                              .MakeStatic()
                              .WithXmlDocSummary($"Creates a new instance of {value.Name} with the specified parameters.");
                        
                        // Add parameters
                        var paramNames = new List<string>();
                        foreach (var param in ctor.Parameters)
                        {
                            method.AddParameter(param.TypeName, param.Name);
                            paramNames.Add(param.Name);
                            method.WithXmlDocParam(param.Name, $"The {param.Name} parameter.");
                        }
                        
                        method.WithXmlDocReturns($"A new {value.Name} instance.");
                        
                        // Generate the constructor call
                        var paramList = string.Join(", ", paramNames);
                        method.WithExpressionBody($"new {value.FullTypeName}({paramList})");
                    });
                }
            }
        }
    }

    private static void AddEmptyProperty(ClassBuilder classBuilder, EnumTypeInfo definition, string effectiveReturnType)
    {
        // Generate the empty class name (e.g., FooBase -> EmptyFooOption)
        var emptyClassName = GetEmptyClassName(definition.ClassName);
        
        // Add Empty property that references the separate empty class
        classBuilder.AddProperty(effectiveReturnType, "Empty", prop => prop
            .MakePublic()
            .MakeStatic()
            .WithGetter($"return {emptyClassName}.Instance;")
            .WithXmlDocSummary("Gets an empty/null enum value."));
    }
    
    private static ClassBuilder CreateEmptyClass(EnumTypeInfo definition, string effectiveReturnType, INamedTypeSymbol? baseTypeSymbol)
    {
        var emptyClassName = GetEmptyClassName(definition.ClassName);
        
        var emptyClass = new ClassBuilder(emptyClassName)
            .MakePublic()
            .MakeSealed()
            .WithBaseType(definition.FullTypeName)
            .WithXmlDocSummary("Represents an empty/null enum value with default property values.");
        
        // Add constructor that calls base with default values
        emptyClass.AddConstructor(ctor => ctor
            .MakePublic()
            .WithBaseCall("0", "string.Empty"));
        
        // Add singleton instance
        emptyClass.AddField($"{effectiveReturnType}?", "_instance", field => field
            .MakePrivate()
            .MakeStatic());
        
        emptyClass.AddProperty(effectiveReturnType, "Instance", prop => prop
            .MakePublic()
            .MakeStatic()
            .WithGetter($"return _instance ??= new {emptyClassName}();"));
        
        // Implement abstract members
        if (baseTypeSymbol != null)
        {
            ImplementAbstractMembers(emptyClass, baseTypeSymbol);
        }
        
        return emptyClass;
    }
    
    private static string GetEmptyClassName(string baseClassName)
    {
        // Remove "Base" suffix if present
        var nameWithoutBase = baseClassName.EndsWith("Base", StringComparison.Ordinal) 
            ? baseClassName.Substring(0, baseClassName.Length - 4) 
            : baseClassName;
        
        return $"Empty{nameWithoutBase}Option";
    }

    // Removed - no longer needed since we build the empty class inline

    private static void ImplementAbstractMembers(ClassBuilder emptyClass, INamedTypeSymbol baseTypeSymbol)
    {
        var abstractMembers = GetAbstractMembers(baseTypeSymbol);
        
        foreach (var member in abstractMembers)
        {
            if (member is IPropertySymbol property)
            {
                var defaultValue = GetDefaultValue(property.Type);
                emptyClass.AddProperty(property.Type.ToDisplayString(), property.Name, prop => prop
                    .MakePublic()
                    .MakeOverride()
                    .WithGetter($"return {defaultValue};")
                    .WithXmlDocSummary($"Gets the default value for {property.Name}."));
            }
            else if (member is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)
            {
                var body = method.ReturnsVoid 
                    ? "" 
                    : $"return {GetDefaultValue(method.ReturnType)};";
                    
                emptyClass.AddMethod(method.Name, method.ReturnType.ToDisplayString(), m => 
                {
                    m.MakePublic()
                     .MakeOverride()
                     .WithBody(body)
                     .WithXmlDocSummary(method.ReturnsVoid 
                         ? $"Empty implementation of {method.Name}." 
                         : $"Returns the default value for {method.Name}.");
                    
                    foreach (var param in method.Parameters)
                    {
                        m.AddParameter(param.Type.ToDisplayString(), param.Name);
                    }
                });
            }
        }
    }

    private static List<ISymbol> GetAbstractMembers(INamedTypeSymbol typeSymbol)
    {
        var members = new List<ISymbol>();
        var current = typeSymbol;
        
        while (current != null)
        {
            members.AddRange(current.GetMembers()
                .Where(m => m.IsAbstract && !m.IsStatic && m.DeclaredAccessibility == Accessibility.Public));
            current = current.BaseType;
        }
        
        return members;
    }

    private static string GetDefaultValue(ITypeSymbol type)
    {
        // Handle nullable types
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            return "null";
        
        // Handle reference types
        if (type.IsReferenceType)
        {
            // Special handling for string to return empty string
            if (type.SpecialType == SpecialType.System_String)
                return "string.Empty";
            
            return "null";
        }
        
        // Handle value types with specific defaults
        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
                return "false";
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                return "0";
            case SpecialType.System_Single:
                return "0f";
            case SpecialType.System_Double:
                return "0d";
            case SpecialType.System_Decimal:
                return "0m";
            case SpecialType.System_Char:
                return "'\\0'";
            default:
                return "default";
        }
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}