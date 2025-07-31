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
public static class EnumCollectionBuilder
{
    /// <summary>
    /// Builds the complete collection class source code.
    /// </summary>
    public static string BuildCollection(
        EnumTypeInfo definition, 
        List<EnumValueInfo> values, 
        string effectiveReturnType,
        INamedTypeSymbol? baseTypeSymbol,
        Compilation compilation)
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
        
        // Determine the return type to use for method signatures
        var methodReturnType = definition.Generic ? "T" : effectiveReturnType;
        
        // Build the class
        var classBuilder = CreateClassBuilder(definition);
        
        // Determine if members should be static (static collections and non-generic collections)
        var isStaticCollection = definition.GenerateStaticCollection && !definition.Generic;
        
        // Add members
        AddStaticFields(classBuilder, definition, effectiveReturnType);
        AddStaticConstructor(classBuilder, definition, values, effectiveReturnType);
        AddAllProperty(classBuilder, methodReturnType, isStaticCollection);
        AddGetByNameMethods(classBuilder, definition, methodReturnType, isStaticCollection);
        AddLookupMethods(classBuilder, definition, methodReturnType, isStaticCollection);
        
        // Add factory methods if enabled
        if (definition.GenerateFactoryMethods)
        {
            AddFactoryMethods(classBuilder, values, methodReturnType, isStaticCollection);
        }
        
        // Add Empty property (without the nested class)
        AddEmptyProperty(classBuilder, definition, methodReturnType, isStaticCollection);
        
        // Add the class to the namespace
        namespaceBuilder.AddClass(classBuilder);
        
        // Add the empty class as a separate public class
        var emptyClass = CreateEmptyClass(definition, effectiveReturnType, baseTypeSymbol, values, compilation);
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
        var classBuilder = new ClassBuilder(definition.CollectionName)
            .MakePublic()
            .WithXmlDocSummary($"Collection of all {definition.ClassName} values.");

        if (definition.Generic)
        {
            classBuilder.AddGenericParameter("T")
                       .AddGenericConstraint("T", $"{definition.FullTypeName}");
            // Generic classes cannot be static, so we don't apply MakeStatic()
        }
        else if (definition.GenerateStaticCollection)
        {
            classBuilder.MakeStatic();
        }

        return classBuilder;
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

    private static void AddAllProperty(ClassBuilder classBuilder, string methodReturnType, bool isStaticCollection)
    {
        var propBuilder = classBuilder.AddProperty($"ImmutableArray<{methodReturnType}>", "All", prop => prop
            .MakePublic()
            .WithGetter("return _all;")
            .WithXmlDocSummary("Gets all enum values."));
            
        if (isStaticCollection)
        {
            propBuilder.MakeStatic();
        }
    }

    private static void AddGetByNameMethods(ClassBuilder classBuilder, EnumTypeInfo definition, string methodReturnType, bool isStaticCollection)
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
        
        var getByNameMethod = classBuilder.AddMethod("GetByName", methodReturnType, method => method
            .MakePublic()
            .AddParameter("string", "name")
            .WithXmlDocSummary("Gets an enum value by its name.")
            .WithXmlDocParam("name", "The name of the enum value.")
            .WithXmlDocReturns("The enum value with the specified name, or Empty if not found.")
            .WithBody(getByNameBody.Build()));
            
        if (isStaticCollection)
        {
            getByNameMethod.MakeStatic();
        }

        // TryGetByName method
        var tryGetByNameBody = new CodeBuilder(8);
        tryGetByNameBody.AppendLine("if (string.IsNullOrEmpty(name))");
        tryGetByNameBody.AppendLine("{");
        tryGetByNameBody.AppendLine("    value = null;");
        tryGetByNameBody.AppendLine("    return false;");
        tryGetByNameBody.AppendLine("}");
        tryGetByNameBody.AppendLine();
        tryGetByNameBody.AppendLine("return _byName.TryGetValue(name, out value);");
        
        var tryGetByNameMethod = classBuilder.AddMethod("TryGetByName", "bool", method => method
            .MakePublic()
            .AddParameter("string", "name")
            .AddParameter($"out {methodReturnType}?", "value")
            .WithXmlDocSummary("Tries to get an enum value by its name.")
            .WithXmlDocParam("name", "The name of the enum value.")
            .WithXmlDocParam("value", "When this method returns, contains the enum value if found; otherwise, null.")
            .WithXmlDocReturns("true if an enum value with the specified name was found; otherwise, false.")
            .WithBody(tryGetByNameBody.Build()));
            
        if (isStaticCollection)
        {
            tryGetByNameMethod.MakeStatic();
        }
    }

    private static void AddLookupMethods(ClassBuilder classBuilder, EnumTypeInfo definition, string methodReturnType, bool isStaticCollection)
    {
        foreach (var lookup in definition.LookupProperties)
        {
            var lookupReturnType = !string.IsNullOrEmpty(lookup.ReturnType) ? lookup.ReturnType : methodReturnType;
            
            if (lookup.AllowMultiple)
            {
                AddMultiValueLookupMethod(classBuilder, lookup, lookupReturnType!, isStaticCollection);
            }
            else
            {
                AddSingleValueLookupMethod(classBuilder, lookup, lookupReturnType!, isStaticCollection);
            }
        }
    }

    private static void AddMultiValueLookupMethod(ClassBuilder classBuilder, PropertyLookupInfo lookup, string returnType, bool isStaticCollection)
    {
        var fieldName = $"_{ToCamelCase(lookup.PropertyName)}Lookup";
        var paramName = ToCamelCase(lookup.PropertyName);
        
        var body = new CodeBuilder(8);
        body.AppendLine($"if ({fieldName}.TryGetValue({paramName}, out var values))");
        body.AppendLine("    return values;");
        body.AppendLine();
        body.AppendLine($"return ImmutableArray<{returnType}>.Empty;");
        
        var method = classBuilder.AddMethod(lookup.LookupMethodName, $"ImmutableArray<{returnType}>", method => method
            .MakePublic()
            .AddParameter(lookup.PropertyType, paramName)
            .WithXmlDocSummary($"Gets all enum values with the specified {lookup.PropertyName}.")
            .WithXmlDocParam(paramName, $"The {lookup.PropertyName} value to search for.")
            .WithXmlDocReturns($"All enum values with the specified {lookup.PropertyName}, or an empty array if none found.")
            .WithBody(body.Build()));
            
        if (isStaticCollection)
        {
            method.MakeStatic();
        }
    }

    private static void AddSingleValueLookupMethod(ClassBuilder classBuilder, PropertyLookupInfo lookup, string returnType, bool isStaticCollection)
    {
        var fieldName = $"_{ToCamelCase(lookup.PropertyName)}Lookup";
        var paramName = ToCamelCase(lookup.PropertyName);
        
        var body = new CodeBuilder(8);
        body.AppendLine($"{fieldName}.TryGetValue({paramName}, out var value);");
        body.AppendLine("return value;");
        
        var method = classBuilder.AddMethod(lookup.LookupMethodName, $"{returnType}?", method => method
            .MakePublic()
            .AddParameter(lookup.PropertyType, paramName)
            .WithXmlDocSummary($"Gets the enum value with the specified {lookup.PropertyName}.")
            .WithXmlDocParam(paramName, $"The {lookup.PropertyName} value to search for.")
            .WithXmlDocReturns($"The enum value with the specified {lookup.PropertyName}, or null if not found.")
            .WithBody(body.Build()));
            
        if (isStaticCollection)
        {
            method.MakeStatic();
        }
    }

    private static void AddFactoryMethods(ClassBuilder classBuilder, List<EnumValueInfo> values, string methodReturnType, bool isStaticCollection)
    {
        foreach (var value in values)
        {
            // Skip if this specific option has GenerateFactoryMethod = false
            if (value.GenerateFactoryMethod == false)
            {
                continue;
            }
            
            var methodName = value.Name;
            var valueReturnType = !string.IsNullOrEmpty(value.ReturnType) ? value.ReturnType! : methodReturnType;
            
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
                    var method = classBuilder.AddMethod(methodName, valueReturnType!, method => method
                        .MakePublic()
                        .WithXmlDocSummary($"Creates a new instance of {value.Name}.")
                        .WithXmlDocReturns($"A new {value.Name} instance.")
                        .WithExpressionBody($"new {value.FullTypeName}()"));
                        
                    if (isStaticCollection)
                    {
                        method.MakeStatic();
                    }
                }
                else
                {
                    // Constructor with parameters - generate overload
                    var method = classBuilder.AddMethod(methodName, valueReturnType, method => 
                    {
                        method.MakePublic()
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
                    
                    if (isStaticCollection)
                    {
                        method.MakeStatic();
                    }
                }
            }
        }
    }

    private static void AddEmptyProperty(ClassBuilder classBuilder, EnumTypeInfo definition, string methodReturnType, bool isStaticCollection)
    {
        // Generate the empty class name (e.g., FooBase -> EmptyFooOption)
        var emptyClassName = GetEmptyClassName(definition.ClassName);
        
        // Add Empty property that references the separate empty class
        var prop = classBuilder.AddProperty(methodReturnType, "Empty", prop => prop
            .MakePublic()
            .WithGetter($"return {emptyClassName}.Instance;")
            .WithXmlDocSummary("Gets an empty/null enum value."));
            
        if (isStaticCollection)
        {
            prop.MakeStatic();
        }
    }
    
    private static ClassBuilder CreateEmptyClass(EnumTypeInfo definition, string effectiveReturnType, INamedTypeSymbol? baseTypeSymbol, List<EnumValueInfo> values, Compilation compilation)
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
        
        // Implement all public members from child classes
        ImplementAllPublicMembers(emptyClass, values, compilation);
        
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

    /// <summary>
    /// Implements all public members from child classes in the Empty class.
    /// </summary>
    private static void ImplementAllPublicMembers(ClassBuilder emptyClass, List<EnumValueInfo> values, Compilation compilation)
    {
        var allMembers = GetAllPublicMembersFromChildren(values, compilation);
        
        foreach (var member in allMembers)
        {
            if (member is IPropertySymbol property)
            {
                var defaultValue = GetDefaultValue(property.Type);
                
                // Check if property is abstract (needs override) or virtual/regular (needs new)
                var modifier = property.IsAbstract ? "override" : "new";
                
                if (modifier == "override")
                {
                    emptyClass.AddProperty(property.Type.ToDisplayString(), property.Name, prop => prop
                        .MakePublic()
                        .MakeOverride()
                        .WithGetter($"return {defaultValue};")
                        .WithXmlDocSummary($"Gets the default value for {property.Name}."));
                }
                else
                {
                    emptyClass.AddProperty(property.Type.ToDisplayString(), property.Name, prop => prop
                        .MakePublic()
                        .WithGetter($"return {defaultValue};")
                        .WithXmlDocSummary($"Gets the default value for {property.Name}."));
                }
            }
            else if (member is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)
            {
                var body = method.ReturnsVoid 
                    ? "" 
                    : $"return {GetDefaultValue(method.ReturnType)};";
                
                // Check if method is abstract (needs override) or virtual/regular (needs new)
                var modifier = method.IsAbstract ? "override" : "new";
                
                if (modifier == "override")
                {
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
                else
                {
                    emptyClass.AddMethod(method.Name, method.ReturnType.ToDisplayString(), m => 
                    {
                        m.MakePublic()
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
    }

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

    /// <summary>
    /// Gets all public members from child EnumOption classes that need to be implemented in the Empty class.
    /// </summary>
    private static List<ISymbol> GetAllPublicMembersFromChildren(List<EnumValueInfo> values, Compilation compilation)
    {
        var allMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        
        foreach (var value in values)
        {
            var childTypeSymbol = compilation.GetTypeByMetadataName(value.FullTypeName);
            if (childTypeSymbol == null) continue;
            
            // Get all public properties and methods (non-static, non-constructor, non-inherited from base)
            var publicMembers = childTypeSymbol.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public && 
                           !m.IsStatic && 
                           (m.Kind == SymbolKind.Property || 
                            (m.Kind == SymbolKind.Method && m is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)) &&
                           !IsInheritedFromEnumOptionBase(m, childTypeSymbol))
                .ToList();
                
            foreach (var member in publicMembers)
            {
                allMembers.Add(member);
            }
        }
        
        return allMembers.ToList();
    }

    /// <summary>
    /// Checks if a member is inherited from EnumOptionBase (Id, Name, etc.) and should be excluded.
    /// </summary>
    private static bool IsInheritedFromEnumOptionBase(ISymbol member, INamedTypeSymbol childType)
    {
        // Skip Id and Name as they come from EnumOptionBase
        if (member.Name == "Id" || member.Name == "Name")
            return true;
            
        // Skip if the member is declared in a base type that is EnumOptionBase<T>
        var declaringType = member.ContainingType;
        while (declaringType != null)
        {
            if (declaringType.IsGenericType && 
                string.Equals(declaringType.OriginalDefinition.Name, "EnumOptionBase", StringComparison.Ordinal) &&
                string.Equals(declaringType.OriginalDefinition.ContainingNamespace?.ToDisplayString(), "FractalDataWorks", StringComparison.Ordinal))
            {
                return true;
            }
            declaringType = declaringType.BaseType;
        }
        
        return false;
    }

    private static string GetDefaultValue(ITypeSymbol type)
    {
        // Handle nullable types
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            return "null";
        
        // Handle arrays
        if (type.TypeKind == TypeKind.Array)
        {
            var arrayType = (IArrayTypeSymbol)type;
            var elementType = arrayType.ElementType.ToDisplayString();
            return $"new {elementType}[0]";
        }
        
        // Handle reference types
        if (type.IsReferenceType)
        {
            // Special handling for string to return empty string
            if (type.SpecialType == SpecialType.System_String)
                return "string.Empty";
            
            // Handle common collection types
            var typeName = type.ToDisplayString();
            
            // Handle generic collections
            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                var genericTypeName = namedType.OriginalDefinition.ToDisplayString();
                
                // Handle common collection interfaces and classes
                if (genericTypeName.StartsWith("System.Collections.Generic.IEnumerable<") ||
                    genericTypeName.StartsWith("System.Collections.Generic.ICollection<") ||
                    genericTypeName.StartsWith("System.Collections.Generic.IList<") ||
                    genericTypeName.StartsWith("System.Collections.Generic.IReadOnlyCollection<") ||
                    genericTypeName.StartsWith("System.Collections.Generic.IReadOnlyList<"))
                {
                    var elementType = namedType.TypeArguments[0].ToDisplayString();
                    return $"System.Array.Empty<{elementType}>()";
                }
                
                if (genericTypeName.StartsWith("System.Collections.Generic.List<"))
                {
                    var elementType = namedType.TypeArguments[0].ToDisplayString();
                    return $"new System.Collections.Generic.List<{elementType}>()";
                }
                
                if (genericTypeName.StartsWith("System.Collections.Generic.Dictionary<"))
                {
                    var keyType = namedType.TypeArguments[0].ToDisplayString();
                    var valueType = namedType.TypeArguments[1].ToDisplayString();
                    return $"new System.Collections.Generic.Dictionary<{keyType}, {valueType}>()";
                }
                
                if (genericTypeName.StartsWith("System.Collections.Immutable.ImmutableArray<"))
                {
                    var elementType = namedType.TypeArguments[0].ToDisplayString();
                    return $"System.Collections.Immutable.ImmutableArray<{elementType}>.Empty";
                }
                
                if (genericTypeName.StartsWith("System.Collections.Immutable.ImmutableList<"))
                {
                    var elementType = namedType.TypeArguments[0].ToDisplayString();
                    return $"System.Collections.Immutable.ImmutableList<{elementType}>.Empty";
                }
            }
            
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