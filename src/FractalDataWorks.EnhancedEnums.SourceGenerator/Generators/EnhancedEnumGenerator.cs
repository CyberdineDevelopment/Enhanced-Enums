using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FractalDataWorks.SmartGenerators;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Discovery;
using FractalDataWorks.EnhancedEnums.Services;
using FractalDataWorks.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.SourceGenerator.Generators;

/// <summary>
/// Source generator for EnhancedEnumOption definitions.
/// Sets up syntax providers and generates collection classes for enums.
/// 
/// HOW IT WORKS:
/// 1. Scans for classes/interfaces with [EnumOptionBase] attribute
/// 2. For each base type found, discovers all types with [EnumOption] that derive from it
/// 3. Generates a static collection class with all discovered options
/// 
/// CROSS-ASSEMBLY DISCOVERY:
/// When IncludeReferencedAssemblies=true on [EnumOptionBase], the generator:
/// 1. Checks the IncludedEnhancedEnumAssemblies MSBuild property
/// 2. Scans ONLY the assemblies listed in that property (semicolon-separated)
/// 3. Discovers [EnumOption] types in those assemblies that derive from the base
/// 4. Includes them in the generated collection
/// 
/// This ENABLES the Service Type Pattern:
/// - Define base enum type in a shared assembly with [EnumOptionBase]
/// - Create option types in plugin assemblies with [EnumOption]
/// - In the main app, reference all assemblies and set IncludedEnhancedEnumAssemblies
/// - The generated collection will include ALL options from ALL scanned assemblies
/// 
/// IMPORTANT: The assemblies must be built and referenced for this to work.
/// The generator reads compiled assembly metadata, not source code.
/// </summary>
//[Generator]
public class EnhancedEnumOptionGenerator : IncrementalGeneratorBase<EnumTypeInfo>
{
    // Cache for assembly types to avoid re-scanning
    private static readonly ConcurrentDictionary<string, List<INamedTypeSymbol>> _assemblyTypeCache = new(StringComparer.Ordinal);
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

        var baseTypeSymbol = GetBaseTypeSymbol(def, compilation);
        var effectiveReturnType = DetermineEffectiveReturnType(def, baseTypeSymbol, compilation);
        var implementsEnhancedOption = CheckIfImplementsEnhancedOption(baseTypeSymbol);

        var classBuilder = BuildCollectionClass(def, values, effectiveReturnType, implementsEnhancedOption, baseTypeSymbol);
        var generatedCode = SourceCodeBuilder.BuildSourceCode(def, effectiveReturnType, classBuilder.Build());
        
        var fileName = $"{def.CollectionName}.g.cs";
        context.AddSource(fileName, generatedCode);
        
        // Conditionally emit to disk if GeneratorOutPutTo is specified
        EmitFileToDiskIfRequested(context, fileName, generatedCode);
    }

    private INamedTypeSymbol? GetBaseTypeSymbol(EnumTypeInfo def, Compilation compilation)
    {
        return def.IsGenericType && !string.IsNullOrEmpty(def.UnboundTypeName)
            ? compilation.GetTypeByMetadataName(def.UnboundTypeName)
            : compilation.GetTypeByMetadataName(def.FullTypeName);
    }

    private string DetermineEffectiveReturnType(EnumTypeInfo def, INamedTypeSymbol? baseTypeSymbol, Compilation compilation)
    {
        if (!string.IsNullOrEmpty(def.ReturnType))
            return def.ReturnType;
        
        if (def.IsGenericType && !string.IsNullOrEmpty(def.DefaultGenericReturnType))
            return def.DefaultGenericReturnType;
        
        return baseTypeSymbol != null ? DetectReturnType(baseTypeSymbol, compilation) : def.FullTypeName;
    }

    private bool CheckIfImplementsEnhancedOption(INamedTypeSymbol? baseTypeSymbol)
    {
        return baseTypeSymbol?.AllInterfaces.Any(i => 
            string.Equals(i.ToDisplayString(), "FractalDataWorks.IEnumOption", StringComparison.Ordinal)) ?? false;
    }

    private ClassBuilder BuildCollectionClass(EnumTypeInfo def, EquatableArray<EnumValueInfo> values, string effectiveReturnType, bool implementsEnhancedOption, INamedTypeSymbol? baseTypeSymbol)
    {
        var classBuilder = new ClassBuilder(def.CollectionName)
            .MakePublic()
            .MakeStatic()
            .WithNamespace(def.Namespace)
            .WithSummary($"Collection of all {def.ClassName} values.");

        // Add fields
        CollectionFieldsBuilder.AddFields(classBuilder, def, effectiveReturnType, implementsEnhancedOption);

        // Add static constructor
        var constructorBody = StaticConstructorBuilder.BuildConstructorBody(def, values, effectiveReturnType, implementsEnhancedOption);
        classBuilder.AddCodeBlock($"static {def.CollectionName}()\n{{\n{constructorBody}\n}}");

        // Add All property
        classBuilder.AddProperty("All", $"ImmutableArray<{effectiveReturnType}>", prop => prop
            .MakePublic()
            .MakeStatic()
            .WithExpressionBody("_cachedAll")
            .WithXmlDocSummary($"Gets all available {def.ClassName} values."));

        // Add lookup methods
        LookupMethodsBuilder.AddLookupMethods(classBuilder, def, effectiveReturnType, implementsEnhancedOption);

        // Add factory methods if enabled
        if (def.GenerateFactoryMethods)
        {
            FactoryMethodsBuilder.AddFactoryMethods(classBuilder, def, values, effectiveReturnType);
        }

        // Add empty value
        EmptyValueBuilder.AddEmptyValue(classBuilder, def, effectiveReturnType, baseTypeSymbol);

        return classBuilder;
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

        var baseTypeSymbol = FindBaseTypeSymbol(def, compilation, context);
        if (baseTypeSymbol == null)
            return;

        // Auto-detect return type if not specified
        if (string.IsNullOrEmpty(def.ReturnType))
        {
            def.ReturnType = DetectReturnType(baseTypeSymbol, compilation);
        }

        // Discover all enum values
        var values = EnumValueDiscoveryService.DiscoverEnumValues(def, baseTypeSymbol, compilation, context);

        // Generate the collection class
        GenerateCollection(context, def, new EquatableArray<EnumValueInfo>(values), compilation);
    }

    private INamedTypeSymbol? FindBaseTypeSymbol(EnumTypeInfo def, Compilation compilation, SourceProductionContext context)
    {
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
        }

        return baseTypeSymbol;
    }



    /// <summary>
    /// Transforms a generator syntax context into an enum type info.
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
            var methodName = lnamed.TryGetValue(nameof(EnumLookupAttribute.MethodName), out var mn) && mn.Value is string ms
                ? ms : $"GetBy{prop.Name}";
            var allowMultiple = lnamed.TryGetValue(nameof(EnumLookupAttribute.AllowMultiple), out var am) && am.Value is bool mu && mu;
            var returnType = lnamed.TryGetValue(nameof(EnumLookupAttribute.ReturnType), out var rt) && rt.Value is INamedTypeSymbol rts ? rts.ToDisplayString() : null;

            lookupProperties.Add(new PropertyLookupInfo
            {
                PropertyName = prop.Name,
                PropertyType = prop.Type.ToDisplayString(),
                LookupMethodName = methodName,
                AllowMultiple = allowMultiple,
                IsNullable = prop.Type.NullableAnnotation == NullableAnnotation.Annotated,
                ReturnType = returnType,
                RequiresOverride = prop.IsAbstract,
            });
        }

        // Process each EnhancedEnumOption attribute to create separate collections
        var results = new List<EnumTypeInfo>();

        foreach (var attr in attrs)
        {
            // Use the proper ExtractCollectionName method that handles named arguments
            string coll = Services.EnumAttributeParser.ExtractCollectionName(attr, symbol);

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
                GenerateFactoryMethods = named.TryGetValue("GenerateFactoryMethods", out var uf) && uf.Value is bool b && b,
                NameComparison = named.TryGetValue("NameComparison", out var nc) && nc.Value is int ic
                    ? (StringComparison)ic : StringComparison.OrdinalIgnoreCase,
                IncludeReferencedAssemblies = named.TryGetValue("IncludeReferencedAssemblies", out var ira) && ira.Value is bool iraB && iraB,
                ReturnType = named.TryGetValue("ReturnType", out var rt) && rt.Value is INamedTypeSymbol rts ? rts.ToDisplayString() : null,
                LookupProperties = new EquatableArray<PropertyLookupInfo>(lookupProperties),
            };

            // Extract generic type information
            ExtractGenericTypeInfo(symbol, collectionInfo);

            // Get default generic return type from attribute
            collectionInfo.DefaultGenericReturnType = named.TryGetValue("DefaultGenericReturnType", out var dgrt) && dgrt.Value is INamedTypeSymbol dgrts ? dgrts.ToDisplayString() : null;

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
        // Get IEnumOption interface from FractalDataWorks core
        var enhancedEnumInterface = compilation.GetTypeByMetadataName("FractalDataWorks.IEnumOption") 
            ?? compilation.GetTypeByMetadataName("FractalDataWorks.EnhancedEnums.IEnumOption"); // Fallback for compatibility
        if (enhancedEnumInterface == null)
        {
            // If we can't find the interface, just return the base type
            return baseTypeSymbol.ToDisplayString();
        }

        // Check all interfaces implemented by the base type
        foreach (var iface in baseTypeSymbol.AllInterfaces)
        {
            // Check if this interface extends IEnumOption
            if (iface.AllInterfaces.Contains(enhancedEnumInterface, SymbolEqualityComparer.Default))
            {
                // Return the first interface that extends IEnumOption
                return iface.ToDisplayString();
            }
            
            // Also check if it directly is IEnumOption
            if (SymbolEqualityComparer.Default.Equals(iface, enhancedEnumInterface))
            {
                // If directly implementing IEnumOption, keep looking for a more specific interface
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

    /// <summary>
    /// Gets all types in a namespace recursively.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetAllTypesInNamespace(INamespaceSymbol namespaceSymbol)
    {
        // Get types in this namespace
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;
            
            // Get nested types
            foreach (var nestedType in GetNestedTypes(type))
            {
                yield return nestedType;
            }
        }
        
        // Recurse into nested namespaces
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
            
            // Recursively get nested types within nested types
            foreach (var deeplyNested in GetNestedTypes(nestedType))
            {
                yield return deeplyNested;
            }
        }
    }
    
    /// <summary>
    /// Conditionally emits the generated file to disk if GeneratorOutPutTo MSBuild property is set.
    /// </summary>
    private void EmitFileToDiskIfRequested(SourceProductionContext context, string fileName, string content)
    {
        try
        {
            var outputPath = GetMSBuildProperty("GeneratorOutPutTo");
            if (string.IsNullOrWhiteSpace(outputPath))
                return;

            var fullPath = System.IO.Path.Combine(outputPath, fileName);
            var directory = System.IO.Path.GetDirectoryName(fullPath);
            
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            System.IO.File.WriteAllText(fullPath, content, System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
            // Report diagnostic but don't fail generation
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    "ENUM002",
                    "Failed to emit file to disk",
                    "Failed to write generated file {0} to disk: {1}",
                    "CodeGeneration",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true),
                Location.None,
                fileName,
                ex.Message);

            context.ReportDiagnostic(diagnostic);
        }
    }
    
    /// <summary>
    /// Gets an MSBuild property value from the generator execution context.
    /// </summary>
    private string? GetMSBuildProperty(string propertyName)
    {
        try
        {
            // Access MSBuild properties through the analyzer config options
            if (OptionsProvider?.GlobalOptions?.TryGetValue($"build_property.{propertyName}", out var value) == true)
            {
                return value;
            }
        }
        catch
        {
            // Ignore errors accessing properties
        }
        
        return null;
    }
}
