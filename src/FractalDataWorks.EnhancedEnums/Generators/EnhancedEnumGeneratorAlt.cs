using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FractalDataWorks.SmartGenerators;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.Generators;

/// <summary>
/// Alternative source generator that uses interface-based discovery for EnhancedEnumOption definitions.
/// This generator discovers types implementing IEnhancedEnumOptionAlt&lt;T&gt; and generates collection classes.
/// </summary>
[Generator]
public class EnhancedEnumGeneratorAlt : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the generator.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add a diagnostic to verify the generator runs
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("EnhancedEnumGeneratorAlt.Diagnostic.g.cs", @"
// This file verifies that EnhancedEnumGeneratorAlt is running
namespace FractalDataWorks.EnhancedEnums.Generated
{
    internal static class EnhancedEnumGeneratorAltDiagnostic
    {
        public const string Message = ""EnhancedEnumGeneratorAlt is active"";
    }
}");
        });

        // Create a provider that triggers on compilation changes
        var provider = context.CompilationProvider
            .Select((compilation, ct) => DiscoverInterfaceBasedOptions(compilation))
            .SelectMany((discoveries, ct) => discoveries);

        // Register source output
        context.RegisterSourceOutput(provider.Combine(context.CompilationProvider), 
            (spc, tuple) => GenerateCollection(spc, tuple.Left, tuple.Right));
    }

    /// <summary>
    /// Discovers all types implementing IEnhancedEnumOptionAlt&lt;T&gt; and groups them by base type.
    /// </summary>
    private static ImmutableArray<EnumTypeInfo> DiscoverInterfaceBasedOptions(Compilation compilation)
    {
        var results = new Dictionary<INamedTypeSymbol, List<EnumValueInfo>>(SymbolEqualityComparer.Default);
        var enhancedOptionInterface = compilation.GetTypeByMetadataName("FractalDataWorks.EnhancedEnums.IEnhancedEnumOptionAlt`1");
        
        if (enhancedOptionInterface == null)
            return ImmutableArray<EnumTypeInfo>.Empty;

        // Scan all types in compilation and referenced assemblies
        var allTypes = GetAllTypesInCompilation(compilation);
        
        foreach (var type in allTypes)
        {
            // Skip abstract types and interfaces
            if (type.IsAbstract || type.TypeKind == TypeKind.Interface)
                continue;

            // Check if type has accessible constructor
            if (!type.Constructors.Any(c => !c.IsStatic && c.DeclaredAccessibility != Accessibility.Private))
                continue;

            foreach (var iface in type.AllInterfaces)
            {
                if (iface.IsGenericType && 
                    SymbolEqualityComparer.Default.Equals(iface.ConstructedFrom, enhancedOptionInterface))
                {
                    var baseType = iface.TypeArguments[0] as INamedTypeSymbol;
                    if (baseType != null)
                    {
                        if (!results.ContainsKey(baseType))
                            results[baseType] = new List<EnumValueInfo>();
                        
                        // Check for EnumOption attribute for custom name/order
                        var enumOptionAttr = type.GetAttributes()
                            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) || 
                                               string.Equals(a.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal));
                        
                        string name = type.Name;
                        int order = 0;
                        
                        if (enumOptionAttr != null)
                        {
                            var named = enumOptionAttr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                            if (named.TryGetValue("Name", out var n) && n.Value is string ns)
                                name = ns;
                            if (named.TryGetValue("Order", out var o) && o.Value is int oi)
                                order = oi;
                        }
                        
                        results[baseType].Add(new EnumValueInfo
                        {
                            FullTypeName = type.ToDisplayString(),
                            ShortTypeName = type.Name,
                            Name = name,
                            Order = order
                        });
                    }
                }
            }
        }
        
        // Convert to EnumTypeInfo instances
        var enumTypes = new List<EnumTypeInfo>();
        foreach (var kvp in results)
        {
            var baseType = kvp.Key;
            var values = kvp.Value;
            
            // Skip if no concrete implementations found
            if (values.Count == 0)
                continue;
            
            // Determine collection name
            string collectionName = baseType.Name + "Collection";
            
            // Check if base type has EnhancedEnumBase attribute for configuration
            var enhancedEnumAttr = baseType.GetAttributes()
                .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "EnhancedEnumBaseAttribute", StringComparison.Ordinal) || 
                                   string.Equals(a.AttributeClass?.Name, "EnhancedEnumBase", StringComparison.Ordinal));
            
            bool useFactory = false;
            StringComparison nameComparison = StringComparison.OrdinalIgnoreCase;
            string? returnType = null;
            string? returnTypeNamespace = null;
            
            if (enhancedEnumAttr != null)
            {
                // Get collection name from constructor if provided
                if (enhancedEnumAttr.ConstructorArguments.Length > 0 && 
                    enhancedEnumAttr.ConstructorArguments[0].Value is string cn && 
                    !string.IsNullOrEmpty(cn))
                {
                    collectionName = cn;
                }
                
                // Get named arguments
                var named = enhancedEnumAttr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (named.TryGetValue("UseFactory", out var uf) && uf.Value is bool ufb)
                    useFactory = ufb;
                if (named.TryGetValue("NameComparison", out var nc) && nc.Value is int nci)
                    nameComparison = (StringComparison)nci;
                if (named.TryGetValue("ReturnType", out var rt) && rt.Value is string rts)
                    returnType = rts;
                if (named.TryGetValue("ReturnTypeNamespace", out var rtn) && rtn.Value is string rtns)
                    returnTypeNamespace = rtns;
            }
            
            // Collect lookup properties
            var lookupProperties = new List<PropertyLookupInfo>();
            foreach (var prop in baseType.GetMembers().OfType<IPropertySymbol>())
            {
                var lookupAttr = prop.GetAttributes()
                    .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "EnumLookupAttribute", StringComparison.Ordinal) || 
                                       string.Equals(a.AttributeClass?.Name, "EnumLookup", StringComparison.Ordinal));
                if (lookupAttr != null)
                {
                    var lnamed = lookupAttr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    var methodName = lnamed.TryGetValue("MethodName", out var mn) && mn.Value is string ms
                        ? ms : $"GetBy{prop.Name}";
                    var allowMultiple = lnamed.TryGetValue("AllowMultiple", out var am) && am.Value is bool amb && amb;
                    var lookupReturnType = lnamed.TryGetValue("ReturnType", out var lrt) && lrt.Value is string lrts ? lrts : null;
                    
                    lookupProperties.Add(new PropertyLookupInfo
                    {
                        PropertyName = prop.Name,
                        PropertyType = prop.Type.ToDisplayString(),
                        LookupMethodName = methodName,
                        AllowMultiple = allowMultiple,
                        IsNullable = prop.Type.NullableAnnotation == NullableAnnotation.Annotated,
                        ReturnType = lookupReturnType,
                    });
                }
            }
            
            var enumTypeInfo = new EnumTypeInfo
            {
                Namespace = baseType.ContainingNamespace.ToDisplayString(),
                ClassName = baseType.Name,
                FullTypeName = baseType.ToDisplayString(),
                IsGenericType = baseType.IsGenericType,
                CollectionName = collectionName,
                UseFactory = useFactory,
                NameComparison = nameComparison,
                ReturnType = returnType ?? DetectReturnType(baseType, compilation),
                ReturnTypeNamespace = returnTypeNamespace,
                LookupProperties = new EquatableArray<PropertyLookupInfo>(lookupProperties),
                ConcreteTypes = new EquatableArray<EnumValueInfo>(values.OrderBy(v => v.Order).ThenBy(v => v.Name, StringComparer.OrdinalIgnoreCase))
            };
            
            // Extract generic type information if needed
            if (baseType.IsGenericType)
            {
                ExtractGenericTypeInfo(baseType, enumTypeInfo);
            }
            
            enumTypes.Add(enumTypeInfo);
        }
        
        return enumTypes.ToImmutableArray();
    }

    /// <summary>
    /// Gets all types in the compilation including referenced assemblies.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetAllTypesInCompilation(Compilation compilation)
    {
        // Get types from current compilation
        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var model = compilation.GetSemanticModel(tree);
            
            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(typeDecl) is INamedTypeSymbol symbol)
                {
                    yield return symbol;
                }
            }
        }
        
        // Get types from referenced assemblies
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
            {
                foreach (var type in GetAllTypesInNamespace(assembly.GlobalNamespace))
                {
                    yield return type;
                }
            }
        }
    }

    /// <summary>
    /// Gets all types in a namespace recursively.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetAllTypesInNamespace(INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;
            foreach (var nestedType in GetNestedTypes(type))
            {
                yield return nestedType;
            }
        }
        
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypesInNamespace(nestedNamespace))
            {
                yield return type;
            }
        }
    }

    /// <summary>
    /// Gets all nested types within a type.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nestedType in type.GetTypeMembers())
        {
            yield return nestedType;
            foreach (var deeplyNested in GetNestedTypes(nestedType))
            {
                yield return deeplyNested;
            }
        }
    }

    /// <summary>
    /// Detects the appropriate return type based on implemented interfaces.
    /// </summary>
    private static string DetectReturnType(INamedTypeSymbol baseTypeSymbol, Compilation compilation)
    {
        // Check if the base type itself is an interface that should be used as return type
        if (baseTypeSymbol.TypeKind == TypeKind.Interface)
        {
            return baseTypeSymbol.ToDisplayString();
        }
        
        // Otherwise use the base type
        return baseTypeSymbol.ToDisplayString();
    }

    /// <summary>
    /// Extracts generic type information from a symbol.
    /// </summary>
    private static void ExtractGenericTypeInfo(INamedTypeSymbol symbol, EnumTypeInfo info)
    {
        if (!symbol.IsGenericType) return;
        
        info.UnboundTypeName = symbol.ConstructUnboundGenericType().ToDisplayString();
        
        foreach (var typeParam in symbol.TypeParameters)
        {
            info.TypeParameters.Add(typeParam.Name);
            
            var constraints = new List<string>();
            if (typeParam.HasReferenceTypeConstraint)
                constraints.Add("class");
            if (typeParam.HasValueTypeConstraint)
                constraints.Add("struct");
            
            foreach (var constraintType in typeParam.ConstraintTypes)
            {
                constraints.Add(constraintType.ToDisplayString());
                ExtractNamespacesFromType(constraintType, info.RequiredNamespaces);
            }
            
            if (typeParam.HasConstructorConstraint)
                constraints.Add("new()");
                
            if (constraints.Any())
                info.TypeConstraints.Add($"where {typeParam.Name} : {string.Join(", ", constraints)}");
        }
    }

    /// <summary>
    /// Extracts namespace information from a type symbol.
    /// </summary>
    private static void ExtractNamespacesFromType(ITypeSymbol type, HashSet<string> namespaces)
    {
        if (type.ContainingNamespace != null && !type.ContainingNamespace.IsGlobalNamespace)
        {
            namespaces.Add(type.ContainingNamespace.ToDisplayString());
        }
        
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            foreach (var arg in namedType.TypeArguments)
            {
                ExtractNamespacesFromType(arg, namespaces);
            }
        }
    }

    /// <summary>
    /// Generates the collection class for an enum definition.
    /// </summary>
    private void GenerateCollection(SourceProductionContext context, EnumTypeInfo def, Compilation compilation)
    {
        if (def == null || def.ConcreteTypes.IsEmpty)
            return;

        var baseTypeSymbol = compilation.GetTypeByMetadataName(def.FullTypeName);
        if (baseTypeSymbol == null)
            return;

        // Use the same generation logic as the original generator
        GenerateCollectionClass(context, def, def.ConcreteTypes, compilation, baseTypeSymbol);
    }

    /// <summary>
    /// Generates the collection class with the same structure as the original generator.
    /// </summary>
    private void GenerateCollectionClass(
        SourceProductionContext context, 
        EnumTypeInfo def, 
        EquatableArray<EnumValueInfo> values, 
        Compilation compilation,
        INamedTypeSymbol baseTypeSymbol)
    {
        string targetNamespace = def.Namespace;
        string effectiveReturnType = def.ReturnType ?? def.FullTypeName;
        
        var classBuilder = new ClassBuilder(def.CollectionName)
            .MakePublic()
            .MakeStatic()
            .WithNamespace(targetNamespace)
            .WithSummary($"Collection of all {def.ClassName} values.");

        // Add private fields
        classBuilder.AddField($"List<{def.FullTypeName}>", "_all", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly()
            .WithInitializer($"new List<{def.FullTypeName}>()"));
        
        classBuilder.AddField($"ImmutableArray<{effectiveReturnType}>", "_cachedAll", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly());

        // Check if base type implements IEnhancedEnumOption
        var implementsEnhancedOption = baseTypeSymbol?.AllInterfaces.Any(i => 
            string.Equals(i.ToDisplayString(), "FractalDataWorks.IEnhancedEnumOption", StringComparison.Ordinal)) ?? false;
            
        // Add conditional compilation fields for dictionaries
        classBuilder.AddCodeBlock($@"#if NET8_0_OR_GREATER
private static readonly FrozenDictionary<string, {effectiveReturnType}> _nameDict;");
        
        if (implementsEnhancedOption)
        {
            classBuilder.AddCodeBlock($"private static readonly FrozenDictionary<int, {effectiveReturnType}> _idDict;");
        }
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                classBuilder.AddCodeBlock($"private static readonly FrozenDictionary<{lookup.PropertyType}, {effectiveReturnType}> _{ToCamelCase(lookup.PropertyName)}Dict;");
            }
        }
        
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
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
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
        }
        
        classBuilder.AddCodeBlock("#endif");

        // Add static constructor
        var constructorBody = new StringBuilder();
        
        foreach (var value in values)
        {
            if (def.UseFactory)
            {
                constructorBody.AppendLine($"_all.Add({def.FullTypeName}.Create(typeof({value.FullTypeName})));");
            }
            else
            {
                constructorBody.AppendLine($"_all.Add(new {value.FullTypeName}());");
            }
        }
        
        constructorBody.AppendLine();
        constructorBody.AppendLine("// Cache the immutable array to prevent repeated allocations");
        constructorBody.AppendLine($"_cachedAll = _all.Cast<{effectiveReturnType}>().ToImmutableArray();");
        
        // Build dictionary initialization
        constructorBody.AppendLine();
        constructorBody.AppendLine("// Populate dictionaries for fast lookups");
        constructorBody.AppendLine("#if NET8_0_OR_GREATER");
        constructorBody.AppendLine($"var tempNameDict = new Dictionary<string, {effectiveReturnType}>(StringComparer.{def.NameComparison});");
        
        if (implementsEnhancedOption)
        {
            constructorBody.AppendLine($"var tempIdDict = new Dictionary<int, {effectiveReturnType}>();");
        }
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                var comparerStr = string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal) 
                    ? $"(StringComparer.{def.NameComparison})" 
                    : "()";
                constructorBody.AppendLine($"var temp{lookup.PropertyName}Dict = new Dictionary<{lookup.PropertyType}, {effectiveReturnType}>{comparerStr};");
            }
        }
        
        constructorBody.AppendLine();
        constructorBody.AppendLine("foreach (var item in _all)");
        constructorBody.AppendLine("{");
        constructorBody.AppendLine("    if (!tempNameDict.ContainsKey(item.Name))");
        constructorBody.AppendLine("    {");
        constructorBody.AppendLine("        tempNameDict[item.Name] = item;");
        constructorBody.AppendLine("    }");
        
        if (implementsEnhancedOption)
        {
            constructorBody.AppendLine();
            constructorBody.AppendLine("    if (!tempIdDict.ContainsKey(item.Id))");
            constructorBody.AppendLine("    {");
            constructorBody.AppendLine("        tempIdDict[item.Id] = item;");
            constructorBody.AppendLine("    }");
        }
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                constructorBody.AppendLine();
                constructorBody.AppendLine($"    if (!temp{lookup.PropertyName}Dict.ContainsKey(item.{lookup.PropertyName}))");
                constructorBody.AppendLine("    {");
                constructorBody.AppendLine($"        temp{lookup.PropertyName}Dict[item.{lookup.PropertyName}] = item;");
                constructorBody.AppendLine("    }");
            }
        }
        
        constructorBody.AppendLine("}");
        
        constructorBody.AppendLine();
        constructorBody.AppendLine("// Convert to FrozenDictionaries for better performance");
        constructorBody.AppendLine($"_nameDict = tempNameDict.ToFrozenDictionary(StringComparer.{def.NameComparison});");
        
        if (implementsEnhancedOption)
        {
            constructorBody.AppendLine("_idDict = tempIdDict.ToFrozenDictionary();");
        }
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                var comparerStr = string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal) 
                    ? $"(StringComparer.{def.NameComparison})" 
                    : "()";
                constructorBody.AppendLine($"_{ToCamelCase(lookup.PropertyName)}Dict = temp{lookup.PropertyName}Dict.ToFrozenDictionary{comparerStr};");
            }
        }
        
        constructorBody.AppendLine("#else");
        constructorBody.AppendLine("foreach (var item in _all)");
        constructorBody.AppendLine("{");
        constructorBody.AppendLine("    if (!_nameDict.ContainsKey(item.Name))");
        constructorBody.AppendLine("    {");
        constructorBody.AppendLine("        _nameDict[item.Name] = item;");
        constructorBody.AppendLine("    }");
        
        if (implementsEnhancedOption)
        {
            constructorBody.AppendLine();
            constructorBody.AppendLine("    if (!_idDict.ContainsKey(item.Id))");
            constructorBody.AppendLine("    {");
            constructorBody.AppendLine("        _idDict[item.Id] = item;");
            constructorBody.AppendLine("    }");
        }
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                var dictName = $"_{ToCamelCase(lookup.PropertyName)}Dict";
                constructorBody.AppendLine();
                constructorBody.AppendLine($"    if (!{dictName}.ContainsKey(item.{lookup.PropertyName}))");
                constructorBody.AppendLine("    {");
                constructorBody.AppendLine($"        {dictName}[item.{lookup.PropertyName}] = item;");
                constructorBody.AppendLine("    }");
            }
        }
        
        constructorBody.AppendLine("}");
        constructorBody.AppendLine("#endif");
        
        classBuilder.AddCodeBlock($"static {def.CollectionName}()\n{{\n{constructorBody}\n}}");

        // Add All property
        classBuilder.AddProperty("All", $"ImmutableArray<{effectiveReturnType}>", prop => prop
            .MakePublic()
            .MakeStatic()
            .WithExpressionBody("_cachedAll")
            .WithXmlDocSummary($"Gets all available {def.ClassName} values."));

        // Add GetByName method
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
            
        // Generate GetById if implementing IEnhancedEnumOption
        if (implementsEnhancedOption)
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

        // Generate lookup methods
        foreach (var lookup in def.LookupProperties)
        {
            GenerateLookupMethod(classBuilder, def, lookup, effectiveReturnType);
        }

        // Generate static property accessors
        if (!values.IsEmpty)
        {
            classBuilder.AddCodeBlock("// Static property accessors");
            foreach (var value in values)
            {
                var propertyName = MakeValidIdentifier(value.Name);
                
                classBuilder.AddProperty(propertyName, effectiveReturnType ?? def.FullTypeName, prop => prop
                    .MakePublic()
                    .MakeStatic()
                    .WithExpressionBody($"_all.OfType<{value.FullTypeName}>().First()")
                    .WithXmlDocSummary($"Gets the {value.Name} instance."));
            }
        }

        // Generate Empty value
        GenerateEmptyValue(classBuilder, def, effectiveReturnType, baseTypeSymbol);

        // Build the complete source code
        var sourceCode = new StringBuilder();
        
        sourceCode.AppendLine("#nullable enable");
        sourceCode.AppendLine();
        sourceCode.AppendLine("using System;");
        sourceCode.AppendLine("using System.Linq;");
        sourceCode.AppendLine("using System.Collections.Generic;");
        sourceCode.AppendLine("using System.Collections.Immutable;");
        sourceCode.AppendLine("#if NET8_0_OR_GREATER");
        sourceCode.AppendLine("using System.Collections.Frozen;");
        sourceCode.AppendLine("#endif");
        
        // Add required namespaces
        foreach (var ns in def.RequiredNamespaces.OrderBy(n => n, StringComparer.Ordinal))
        {
            if (!ns.StartsWith("System", StringComparison.Ordinal) && 
                !string.Equals(ns, targetNamespace, StringComparison.Ordinal))
            {
                sourceCode.AppendLine($"using {ns};");
            }
        }

        if (!string.IsNullOrEmpty(def.ReturnTypeNamespace))
        {
            if (!def.ReturnTypeNamespace!.StartsWith("System", StringComparison.Ordinal) && 
                !string.Equals(def.ReturnTypeNamespace, targetNamespace, StringComparison.Ordinal))
            {
                sourceCode.AppendLine($"using {def.ReturnTypeNamespace};");
            }
        }
        
        sourceCode.AppendLine();
        sourceCode.Append(classBuilder.Build());
        
        // Add the source
        context.AddSource($"{def.CollectionName}.g.cs", sourceCode.ToString());
    }

    /// <summary>
    /// Generates a lookup method for a specific property.
    /// </summary>
    private static void GenerateLookupMethod(ClassBuilder classBuilder, EnumTypeInfo def, PropertyLookupInfo lookup, string? effectiveReturnType)
    {
        var paramName = ToCamelCase(lookup.PropertyName);
        var lookupReturnType = !string.IsNullOrEmpty(lookup.ReturnType) ? lookup.ReturnType : (effectiveReturnType ?? def.FullTypeName);
        
        if (lookup.AllowMultiple)
        {
            string methodBody;
            if (lookup.PropertyType.Contains("[]") || lookup.PropertyType.Contains("IEnumerable") || lookup.PropertyType.Contains("List"))
            {
                methodBody = $"return _all.Where(x => x.{lookup.PropertyName}?.Contains({paramName}) ?? false);";
            }
            else
            {
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
        else
        {
            var dictName = $"_{ToCamelCase(lookup.PropertyName)}Dict";
            var methodBody = new StringBuilder();
            
            if (string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal))
            {
                methodBody.AppendLine($"if (string.IsNullOrEmpty({paramName}))");
                methodBody.AppendLine("{");
                methodBody.AppendLine("    return null;");
                methodBody.AppendLine("}");
                methodBody.AppendLine();
            }
            
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
    }

    /// <summary>
    /// Generates the Empty value singleton for the enum collection.
    /// </summary>
    private static void GenerateEmptyValue(ClassBuilder classBuilder, EnumTypeInfo def, string? effectiveReturnType, INamedTypeSymbol? baseTypeSymbol)
    {
        // Add Empty property singleton field
        classBuilder.AddField("EmptyValue", "_empty", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly()
            .WithInitializer("new EmptyValue()"));
        
        // Add Empty property
        classBuilder.AddProperty("Empty", effectiveReturnType ?? def.FullTypeName, prop => prop
            .MakePublic()
            .MakeStatic()
            .WithExpressionBody("_empty")
            .WithXmlDocSummary("Gets an empty instance representing no selection."));
        
        // Generate nested EmptyValue class
        classBuilder.AddNestedClass(builder => 
        {
            builder.WithName("EmptyValue")
                .MakePrivate()
                .MakeSealed()
                .WithBaseType(def.FullTypeName);

            // Find the most accessible constructor
            if (baseTypeSymbol != null)
            {
                var constructors = baseTypeSymbol.Constructors
                    .Where(c => !c.IsStatic && c.DeclaredAccessibility != Accessibility.Private)
                    .OrderBy(c => c.Parameters.Length)
                    .ThenBy(c => c.DeclaredAccessibility == Accessibility.Protected ? 0 : 1)
                    .ToList();

                if (constructors.Count > 0)
                {
                    var ctor = constructors.First();
                    var args = string.Join(", ", ctor.Parameters.Select(p => GetDefaultValueForType(p.Type.ToDisplayString())));
                    
                    builder.AddConstructor(ctorBuilder => ctorBuilder
                        .MakePublic()
                        .WithBaseCall(args));
                }
            }

            // Add lookup property overrides
            builder.AddCodeBlock(BuildLookupPropertiesForEmpty(def));
        });
    }
    
    /// <summary>
    /// Builds the lookup properties for the empty value class.
    /// </summary>
    private static string BuildLookupPropertiesForEmpty(EnumTypeInfo def)
    {
        var sb = new StringBuilder();
        
        foreach (var lookup in def.LookupProperties)
        {
            var defaultValue = GetDefaultValueForType(lookup.PropertyType);
            sb.AppendLine($"public override {lookup.PropertyType} {lookup.PropertyName} => {defaultValue};");
        }
        
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Gets the default value string for a given type.
    /// </summary>
    private static string GetDefaultValueForType(string typeName)
    {
        var cleanType = typeName.TrimEnd('?');
        
        return cleanType switch
        {
            "string" => "string.Empty",
            "int" => "0",
            "long" => "0L",
            "short" => "0",
            "byte" => "0",
            "double" => "0.0",
            "float" => "0.0f",
            "decimal" => "0m",
            "bool" => "false",
            "System.Guid" or "Guid" => "Guid.Empty",
            "System.DateTime" or "DateTime" => "DateTime.MinValue",
            "System.DateTimeOffset" or "DateTimeOffset" => "DateTimeOffset.MinValue",
            "System.TimeSpan" or "TimeSpan" => "TimeSpan.Zero",
            _ => typeName.EndsWith("?", StringComparison.Ordinal) ? "null" : $"default({cleanType})"
        };
    }

    /// <summary>
    /// Converts a PascalCase string to camelCase.
    /// </summary>
    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    /// <summary>
    /// Makes a valid C# identifier from a string.
    /// </summary>
    private static string MakeValidIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "_";
        
        var result = Regex.Replace(name, @"[^\w]", "_", RegexOptions.None, TimeSpan.FromSeconds(1));
        
        if (char.IsDigit(result[0]))
            result = "_" + result;
        
        result = Regex.Replace(result, @"_+", "_", RegexOptions.None, TimeSpan.FromSeconds(1));
        result = Regex.Replace(result, @"_", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1));
        result = result.Trim('_');
        
        return string.IsNullOrEmpty(result) ? "_" : result;
    }
}