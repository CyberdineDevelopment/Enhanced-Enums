using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks;
using FractalDataWorks.SmartGenerators;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Discovery;
using FractalDataWorks.EnhancedEnums.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.Generators;

/// <summary>
/// Source generator for EnhancedEnumOption definitions.
/// Sets up syntax providers and generates collection classes for enums.
/// 
/// HOW IT WORKS:
/// 1. Scans for classes/interfaces with [EnumCollection] attribute
/// 2. For each base type found, discovers all types with [EnumOption] that derive from it
/// 3. Generates a static collection class with all discovered options
/// 
/// CROSS-ASSEMBLY DISCOVERY:
/// When IncludeReferencedAssemblies=true on [EnumCollection], the generator:
/// 1. Checks the IncludedEnhancedEnumAssemblies MSBuild property
/// 2. Scans ONLY the assemblies listed in that property (semicolon-separated)
/// 3. Discovers [EnumOption] types in those assemblies that derive from the base
/// 4. Includes them in the generated collection
/// 
/// This ENABLES the Service Type Pattern:
/// - Define base enum type in a shared assembly with [EnumCollection]
/// - Create option types in plugin assemblies with [EnumOption]
/// - In the main app, reference all assemblies and set IncludedEnhancedEnumAssemblies
/// - The generated collection will include ALL options from ALL scanned assemblies
/// 
/// IMPORTANT: The assemblies must be built and referenced for this to work.
/// The generator reads compiled assembly metadata, not source code.
/// </summary>
[Generator]
public class EnhancedEnumOptionGenerator : IncrementalGeneratorBase<EnumTypeInfo>
{
    // Cache for assembly types to avoid re-scanning
    private static readonly ConcurrentDictionary<string, List<INamedTypeSymbol>> _assemblyTypeCache = new(StringComparer.Ordinal);
    
    // Cross-assembly discovery service
    private static readonly CrossAssemblyTypeDiscoveryService _discoveryService = new CrossAssemblyTypeDiscoveryService();
    
    /// <summary>
    /// Generates the collection class for an enum definition with its values.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="def">The enum type definition information.</param>
    /// <param name="values">The enum values to include in the collection.</param>
    /// <param name="compilation">The compilation context for type symbol resolution.</param>
    protected virtual void GenerateCollection(SourceProductionContext context, EnumTypeInfo def, EquatableArray<EnumValueInfo> values, Compilation compilation)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));

        // Convert values to a List for the builder
        var valueList = values.ToList();
        
        var baseTypeSymbol = compilation.GetTypeByMetadataName(def.FullTypeName);
        
        // Determine the effective return type
        string? effectiveReturnType;
        if (!string.IsNullOrEmpty(def.ReturnType))
        {
            effectiveReturnType = def.ReturnType;
        }
        else if (def.IsGenericType && !string.IsNullOrEmpty(def.DefaultGenericReturnType))
        {
            effectiveReturnType = def.DefaultGenericReturnType;
        }
        else
        {
            effectiveReturnType = baseTypeSymbol != null ? DetectReturnType(baseTypeSymbol, compilation) : def.FullTypeName;
        }

        // Use EnumCollectionBuilder to generate the collection
        var generatedCode = EnumCollectionBuilder.BuildCollection(def, valueList, effectiveReturnType ?? def.FullTypeName, baseTypeSymbol);
        context.AddSource($"{def.CollectionName}.g.cs", generatedCode);
    }

    /// <summary>
    /// Determines whether a syntax node is relevant for this generator.
    /// </summary>
    /// <param name="syntaxNode">The syntax node to check.</param>
    /// <returns>True if the syntax node is relevant, false otherwise.</returns>
    protected override bool IsRelevantSyntax(SyntaxNode syntaxNode)
    {
        return syntaxNode is BaseTypeDeclarationSyntax btd && btd.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => a.Name.ToString().Contains("EnumCollection"));
    }

    /// <summary>
    /// Transforms a syntax context into an input model.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>The transformed input model, or null if the syntax is not relevant.</returns>
    protected override EnumTypeInfo? TransformSyntax(GeneratorSyntaxContext context)
    {
        var results = TransformDefinitions(context);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Registers source output for this generator.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    /// <param name="syntaxProvider">The syntax provider.</param>
    protected override void RegisterSourceOutput(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<EnumTypeInfo?> syntaxProvider)
    {
        // We need to re-create the syntax provider to handle multiple attributes per class
        var multiAttributeProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => IsRelevantSyntax(node),
                (ctx, _) => TransformDefinitions(ctx))
            .SelectMany((x, _) => x)
            .Where(x => x != null)
            .Select((x, _) => x!);

        // Combine with compilation for execution
        var combo = multiAttributeProvider.Combine(context.CompilationProvider);
        
        context.RegisterSourceOutput(combo, (spc, tuple) =>
            Execute(spc, tuple.Left, tuple.Right));
    }

    /// <summary>
    /// Executes the generation of a collection class for a single enum definition.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="def">The enum type definition information.</param>
    /// <param name="compilation">The compilation containing the enum type.</param>
    private void Execute(SourceProductionContext context, EnumTypeInfo def, Compilation compilation)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));
        if (compilation == null)
            throw new ArgumentNullException(nameof(compilation));

        var values = new List<EnumValueInfo>();

        // Find the base type symbol from the compilation
        INamedTypeSymbol? baseTypeSymbol = null;
        
        // For generic types, we need to find them differently
        if (def.IsGenericType)
        {
            // Try to find the type by searching through all types in the namespace
            var namespaceSymbol = GetNamespaceSymbol(compilation, def.Namespace);
            if (namespaceSymbol != null)
            {
                baseTypeSymbol = FindTypeInNamespace(namespaceSymbol, def.ClassName);
            }
        }
        else
        {
            // For non-generic types, use the metadata name
            baseTypeSymbol = compilation.GetTypeByMetadataName(def.FullTypeName);
        }
        
        if (baseTypeSymbol == null)
        {
            // Type not found - this shouldn't happen but let's handle it gracefully
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "ENH002",
                    "Base type not found",
                    "Enhanced enum base type '{0}' could not be found in compilation.",
                    "EnhancedEnumOptions",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                null,
                def.FullTypeName));
            return;
        }

        // Determine the effective return type
        if (string.IsNullOrEmpty(def.ReturnType))
        {
            // Auto-detect from implemented interfaces
            def.ReturnType = DetectReturnType(baseTypeSymbol, compilation);
        }

        // Collect all types with EnumOption attribute
        var allTypes = new List<INamedTypeSymbol>();

        // Step 1: Scan current compilation
        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var model = compilation.GetSemanticModel(tree);
            
            // Find all type declarations (classes, structs, records)
            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (!HasEnumOptionAttribute(typeDecl))
                {
                    continue;
                }

                var symbol = model.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (symbol != null)
                {
                    allTypes.Add(symbol);
                }
            }
        }

        // Step 2: Scan referenced assemblies if enabled
        // CROSS-ASSEMBLY DISCOVERY:
        // This is where the "magic" happens for the Service Type Pattern.
        // We scan compiled assemblies (DLLs) that are referenced by this project.
        // Each assembly declares if it wants to be discovered via IncludeInEnhancedEnumAssemblies.
        // We're reading assembly METADATA, not executing code, so this is safe and fast.
        if (def.IncludeReferencedAssemblies)
        {
            // Find the EnumOption attribute type
            var enumOptionAttribute = compilation.GetTypeByMetadataName("FractalDataWorks.EnhancedEnums.Attributes.EnumOptionAttribute");
            
            if (enumOptionAttribute != null)
            {
                // Use the discovery service to find types with EnumOption attribute
                // The service will only return types from assemblies that have opted in
                var typesWithAttribute = _discoveryService.FindTypesWithAttribute(enumOptionAttribute, compilation);
                
                foreach (var type in typesWithAttribute)
                {
                    allTypes.Add(type);
                }
                
                // Report diagnostic about cross-assembly scanning
                var typeCount = typesWithAttribute.Count();
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "ENH_INFO_001",
                        "Cross-assembly scan complete",
                        $"Found {typeCount} types with EnumOption attribute from assemblies that opted in",
                        "EnhancedEnumOptions",
                        DiagnosticSeverity.Info,
                        isEnabledByDefault: true),
                    null));
            }
        }

        // Process all found types
        foreach (var typeSymbol in allTypes)
        {
            var attrs = typeSymbol.GetAttributes()
                .Where(ad => string.Equals(ad.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) ||
                           string.Equals(ad.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal))
                .ToList();

            if (attrs.Count == 0)
            {
                continue;
            }

            // Process each EnumOption attribute separately (for multiple collections support)
            foreach (var attr in attrs)
            {
                var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var name = named.TryGetValue("Name", out var n) && n.Value is string ns
                    ? ns : typeSymbol.Name;

                // Check if this enum option specifies a collection name
                var collectionName = named.TryGetValue("CollectionName", out var cn) && cn.Value is string cns
                    ? cns : null;

                // If a collection name is specified, only include this option in matching collections
                if (!string.IsNullOrEmpty(collectionName) && !string.Equals(collectionName, def.CollectionName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (MatchesDefinition(typeSymbol, baseTypeSymbol))
                {
                    var info = new EnumValueInfo
                    {
                        ShortTypeName = typeSymbol.Name,
                        FullTypeName = typeSymbol.ToDisplayString(),
                        Name = name,
                    };
                    values.Add(info);
                    
                    // Extract namespaces from the option type's base type arguments
                    ExtractNamespacesFromOptionType(typeSymbol, baseTypeSymbol, def);
                    
                    break; // Only add once per type per collection, even if multiple matching attributes
                }
            }
        }

        // Generate the collection class
        GenerateCollection(context, def, new EquatableArray<EnumValueInfo>(values), compilation);
    }

    /// <summary>
    /// Determines whether an enum value belongs to a given enum definition.
    /// </summary>
    /// <param name="valueSymbol">The symbol of the enum value to check.</param>
    /// <param name="baseTypeSymbol">The symbol of the base type to match against.</param>
    /// <returns>True if the value matches the definition; otherwise, false.</returns>
    private static bool MatchesDefinition(INamedTypeSymbol valueSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        // interface implementation
        if (valueSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, baseTypeSymbol)))
        {
            return true;
        }

        // base type inheritance
        for (var baseType = valueSymbol.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, baseTypeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Transforms a generator syntax context into enum type info.
    /// </summary>
    /// <param name="ctx">The generator syntax context.</param>
    /// <returns>An EnumTypeInfo representing the enum definition.</returns>
    private static IEnumerable<EnumTypeInfo> TransformDefinitions(GeneratorSyntaxContext ctx)
    {
        var decl = (BaseTypeDeclarationSyntax)ctx.Node;
        var symbol = (INamedTypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(decl)!;
        var attrs = symbol.GetAttributes()
            .Where(ad => string.Equals(ad.AttributeClass?.Name, "EnumCollectionAttribute", StringComparison.Ordinal) ||
                        string.Equals(ad.AttributeClass?.Name, "EnumCollection", StringComparison.Ordinal))
            .ToList();

        if (attrs.Count == 0)
        {
            return Enumerable.Empty<EnumTypeInfo>();
        }

        // First, collect lookup properties (same for all collections)
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

        // Process each EnumCollection attribute to create separate collections
        var results = new List<EnumTypeInfo>();

        foreach (var attr in attrs)
        {
            // Get collection name from constructor argument
            string coll;
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string collName && !string.IsNullOrEmpty(collName))
            {
                coll = collName;
            }
            else
            {
                // Default to plural of class name
                coll = symbol.Name + "s";
            }

            // Get named arguments for other properties
            var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Create a separate EnumTypeInfo for each collection
            var collectionInfo = new EnumTypeInfo
            {
                Namespace = symbol.ContainingNamespace.ToDisplayString(),
                ClassName = symbol.Name,
                FullTypeName = symbol.ToDisplayString(),
                IsGenericType = symbol.IsGenericType,
                CollectionName = coll,
                GenerateFactoryMethods = named.TryGetValue("GenerateFactoryMethods", out var uf) && uf.Value is bool b ? b : false,
                NameComparison = named.TryGetValue("NameComparison", out var nc) && nc.Value is int ic
                    ? (StringComparison)ic : StringComparison.Ordinal,
                IncludeReferencedAssemblies = false, // EnumCollectionAttribute doesn't have this property
                ReturnType = named.TryGetValue("ReturnType", out var rt) && rt.Value is string rs ? rs : null,
                ReturnTypeNamespace = named.TryGetValue("ReturnTypeNamespace", out var rtn) && rtn.Value is string rtns ? rtns : null,
                LookupProperties = new EquatableArray<PropertyLookupInfo>(lookupProperties),
            };

            // Extract generic type information
            ExtractGenericTypeInfo(symbol, collectionInfo);

            // Get default generic return type from attribute
            collectionInfo.DefaultGenericReturnType = named.TryGetValue("DefaultGenericReturnType", out var dgrt) && dgrt.Value is string dgrts ? dgrts : null;
            collectionInfo.DefaultGenericReturnTypeNamespace = named.TryGetValue("DefaultGenericReturnTypeNamespace", out var dgrtn) && dgrtn.Value is string dgrtns ? dgrtns : null;

            // Store the symbol temporarily for further processing
            // This will be used in Execute method but won't be part of equality
            results.Add(collectionInfo);
        }

        return results;
    }

    /// <summary>
    /// Detects the appropriate return type based on implemented interfaces.
    /// </summary>
    /// <param name="baseTypeSymbol">The base type symbol to analyze.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>The detected return type or the base type's full name.</returns>
    private static string DetectReturnType(INamedTypeSymbol baseTypeSymbol, Compilation compilation)
    {
        // Get IEnhancedEnumOption interface from FractalDataWorks core
        var enhancedEnumInterface = compilation.GetTypeByMetadataName("FractalDataWorks.IEnhancedEnumOption") 
            ?? compilation.GetTypeByMetadataName("FractalDataWorks.EnhancedEnums.IEnhancedEnumOption"); // Fallback for compatibility
        if (enhancedEnumInterface == null)
        {
            // If we can't find the interface, just return the base type
            return baseTypeSymbol.ToDisplayString();
        }

        // Check all interfaces implemented by the base type
        foreach (var iface in baseTypeSymbol.AllInterfaces)
        {
            // Check if this interface extends IEnhancedEnumOption
            if (iface.AllInterfaces.Contains(enhancedEnumInterface, SymbolEqualityComparer.Default))
            {
                // Return the first interface that extends IEnhancedEnumOption
                return iface.ToDisplayString();
            }
            
            // Also check if it directly is IEnhancedEnumOption
            if (SymbolEqualityComparer.Default.Equals(iface, enhancedEnumInterface))
            {
                // If directly implementing IEnhancedEnumOption, keep looking for a more specific interface
                continue;
            }
        }

        // No custom interface found, use the base type
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
            
            // Build constraint string
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
                
            if (constraints.Count > 0)
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
        
        // Handle generic type arguments
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            foreach (var arg in namedType.TypeArguments)
            {
                ExtractNamespacesFromType(arg, namespaces);
            }
        }
    }
    
    /// <summary>
    /// Extracts namespaces from an option type's inheritance hierarchy.
    /// </summary>
    private static void ExtractNamespacesFromOptionType(INamedTypeSymbol optionType, INamedTypeSymbol baseTypeSymbol, EnumTypeInfo def)
    {
        // Extract namespaces from the option type itself
        ExtractNamespacesFromType(optionType, def.RequiredNamespaces);
        
        // Walk up the inheritance chain to find the base type
        for (var current = optionType.BaseType; current != null; current = current.BaseType)
        {
            // If this is a generic type, extract namespaces from type arguments
            if (current.IsGenericType)
            {
                foreach (var typeArg in current.TypeArguments)
                {
                    ExtractNamespacesFromType(typeArg, def.RequiredNamespaces);
                }
            }
            
            // If we've reached the base type, stop
            // For generic types, we need to compare the unbound/original definition
            var currentToCompare = current.IsGenericType ? current.OriginalDefinition : current;
            var baseToCompare = baseTypeSymbol.IsGenericType ? baseTypeSymbol.OriginalDefinition : baseTypeSymbol;
            
            if (SymbolEqualityComparer.Default.Equals(currentToCompare, baseToCompare))
            {
                break;
            }
        }
        
        // Also extract from interfaces in case the base type is an interface
        foreach (var iface in optionType.AllInterfaces)
        {
            if (iface.IsGenericType)
            {
                foreach (var typeArg in iface.TypeArguments)
                {
                    ExtractNamespacesFromType(typeArg, def.RequiredNamespaces);
                }
            }
        }
    }

    /// <summary>
    /// Checks if a type declaration has the EnumOption attribute.
    /// </summary>
    private static bool HasEnumOptionAttribute(TypeDeclarationSyntax syntax)
    {
        return syntax.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(attr => attr.Name.ToString().Contains("EnumOption"));
    }

    /// <summary>
    /// Gets a namespace symbol from a compilation by its full name.
    /// </summary>
    private static INamespaceSymbol? GetNamespaceSymbol(Compilation compilation, string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName))
            return compilation.GlobalNamespace;
            
        var parts = namespaceName.Split('.');
        var current = compilation.GlobalNamespace;
        
        foreach (var part in parts)
        {
            current = current.GetNamespaceMembers().FirstOrDefault(n => string.Equals(n.Name, part, StringComparison.Ordinal));
            if (current == null)
                return null;
        }
        
        return current;
    }

    /// <summary>
    /// Finds a type in a namespace by name.
    /// </summary>
    private static INamedTypeSymbol? FindTypeInNamespace(INamespaceSymbol namespaceSymbol, string typeName)
    {
        // Direct search in the namespace
        var type = namespaceSymbol.GetTypeMembers(typeName).FirstOrDefault();
        if (type != null)
            return type;
            
        // Search in nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            type = FindTypeInNamespace(nestedNamespace, typeName);
            if (type != null)
                return type;
        }
        
        return null;
    }
}