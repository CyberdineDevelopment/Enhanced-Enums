using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Discovery;
using FractalDataWorks.EnhancedEnums.Services;
using FractalDataWorks.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FractalDataWorks.EnhancedEnums.CrossAssembly.Generators;

/// <summary>
/// Global source generator that discovers EnhancedEnum types across ALL referenced assemblies.
/// Unlike EnhancedEnumGenerator which only processes types where the base class is defined,
/// this generator runs in consuming projects and discovers all EnhancedEnum types from 
/// all referenced packages/assemblies.
/// 
/// HOW IT WORKS:
/// 1. Runs in the consuming project (not in the base class library)
/// 2. Scans ALL referenced assemblies for types with [EnumOption] attribute
/// 3. Groups them by their base type  
/// 4. Generates collection classes for each base type found
/// 
/// This enables true cross-assembly discovery with packages:
/// - Base type defined in one package (e.g., ColorOption.Library)
/// - Option types defined in other packages (e.g., Red.Library, Blue.Library)  
/// - Consumer project references all packages
/// - This generator runs in consumer and finds all types across packages
/// </summary>
[Generator]
public class GlobalEnhancedEnumGenerator : IIncrementalGenerator
{
    // Cache for assembly types to avoid re-scanning
    private static readonly ConcurrentDictionary<string, List<INamedTypeSymbol>> _assemblyTypeCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes the incremental source generator.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add a simple diagnostic to verify the generator is being loaded
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("GlobalEnhancedEnumGenerator_Debug.cs", 
                "// GlobalEnhancedEnumGenerator Initialize() was called - generator is loaded!");
        });

        // Create a provider that discovers all EnhancedEnum base types across all assemblies
        var enumDefinitionsProvider = context.CompilationProvider
            .Select((compilation, _) => DiscoverAllEnumDefinitions(compilation));

        // Register the source output
        context.RegisterSourceOutput(enumDefinitionsProvider, (context, enumDefinitions) =>
        {
            foreach (var enumDefinition in enumDefinitions)
            {
                Execute(context, enumDefinition.EnumTypeInfo, enumDefinition.Compilation, enumDefinition.DiscoveredOptionTypes);
            }
        });

        // Add discovery debug output
        var discoveryDebugProvider = context.CompilationProvider.Select((compilation, _) =>
        {
            var enumCollectionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var totalOptionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            // Scan for EnumCollection types
            foreach (var reference in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
                {
                    ScanForEnumCollectionTypes(assemblySymbol.GlobalNamespace, enumCollectionTypes);
                }
            }

            // For each EnumCollection, scan for its option types
            foreach (var enumCollectionType in enumCollectionTypes)
            {
                var optionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                
                foreach (var reference in compilation.References)
                {
                    if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
                    {
                        ScanForOptionTypesOfBase(assemblySymbol.GlobalNamespace, enumCollectionType, optionTypes);
                    }
                }
                
                foreach (var optionType in optionTypes)
                {
                    totalOptionTypes.Add(optionType);
                }
            }

            var debugInfo = new StringBuilder();
            debugInfo.AppendLine($"// DEBUG: Found {enumCollectionTypes.Count} EnumCollection types and {totalOptionTypes.Count} total option types");
            debugInfo.AppendLine($"// References: {compilation.References.Count()}");
            
            // Show relevant assemblies only
            var relevantAssemblies = new HashSet<string>();
            foreach (var baseType in enumCollectionTypes)
            {
                relevantAssemblies.Add(baseType.ContainingAssembly.Name);
            }
            foreach (var optionType in totalOptionTypes)
            {
                relevantAssemblies.Add(optionType.ContainingAssembly.Name);
            }
            
            debugInfo.AppendLine($"// Relevant assemblies: {string.Join(", ", relevantAssemblies)}");
            
            foreach (var baseType in enumCollectionTypes)
            {
                debugInfo.AppendLine($"// EnumCollection type: {baseType.ToDisplayString()} (from {baseType.ContainingAssembly.Name})");
            }
            foreach (var optionType in totalOptionTypes)
            {
                debugInfo.AppendLine($"// Option type: {optionType.ToDisplayString()} (from {optionType.ContainingAssembly.Name})");
            }
            return debugInfo.ToString();
        });

        context.RegisterSourceOutput(discoveryDebugProvider, (context, debugInfo) =>
        {
            context.AddSource("GlobalEnhancedEnumGenerator_DiscoveryDebug.cs", debugInfo);
        });

        // Add lookup properties debug output
        var lookupDebugProvider = context.CompilationProvider.Select((compilation, _) =>
        {
            var enumCollectionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            
            // Scan for EnumCollection types
            foreach (var reference in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
                {
                    ScanForEnumCollectionTypes(assemblySymbol.GlobalNamespace, enumCollectionTypes);
                }
            }

            var debugOutput = new StringBuilder();
            foreach (var baseType in enumCollectionTypes)
            {
                debugOutput.AppendLine($"// DEBUG: Processing properties for {baseType.Name}:");
                
                foreach (var prop in baseType.GetMembers().OfType<IPropertySymbol>())
                {
                    debugOutput.AppendLine($"// Property: {prop.Name} ({prop.Type.ToDisplayString()})");
                    
                    var lookupAttr = prop.GetAttributes()
                        .FirstOrDefault(ad => string.Equals(ad.AttributeClass?.Name, "EnumLookupAttribute", StringComparison.Ordinal) ||
                                             string.Equals(ad.AttributeClass?.Name, "EnumLookup", StringComparison.Ordinal));
                    if (lookupAttr == null)
                    {
                        debugOutput.AppendLine($"//   No EnumLookup attribute found");
                        continue;
                    }

                    debugOutput.AppendLine($"//   Found EnumLookup attribute");
                    
                    // Get constructor arguments - MethodName is the first parameter
                    var constructorArgs = lookupAttr.ConstructorArguments;
                    if (constructorArgs.Length == 0 || constructorArgs[0].Value is not string methodName || string.IsNullOrEmpty(methodName))
                    {
                        debugOutput.AppendLine($"//   No MethodName provided in constructor - skipping");
                        continue;
                    }
                    
                    debugOutput.AppendLine($"//   MethodName: {methodName}");
                }
            }
            
            return debugOutput.ToString();
        });

        context.RegisterSourceOutput(lookupDebugProvider, (context, debugInfo) =>
        {
            context.AddSource("GlobalEnhancedEnumGenerator_LookupDebug.cs", debugInfo);
        });
    }

    /// <summary>
    /// Discovers all enhanced enum definitions by scanning each assembly independently.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>Collection of discovered enum definitions with compilation context.</returns>
    private ImmutableArray<EnumTypeInfoWithCompilation> DiscoverAllEnumDefinitions(Compilation compilation)
    {
        var results = new List<EnumTypeInfoWithCompilation>();

        // Step 1: Scan for [EnumCollection] base types across all referenced assemblies
        var enumCollectionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
            {
                ScanForEnumCollectionTypes(assemblySymbol.GlobalNamespace, enumCollectionTypes);
            }
        }

        // Step 2: For each EnumCollection type found, scan for its option types
        foreach (var enumCollectionType in enumCollectionTypes)
        {
            var optionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            
            // Scan all assemblies for types that derive from this specific EnumCollection type
            foreach (var reference in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
                {
                    ScanForOptionTypesOfBase(assemblySymbol.GlobalNamespace, enumCollectionType, optionTypes);
                }
            }

            // Step 3: Create enum definition for this base type if it has options
            if (optionTypes.Any())
            {
                var enumDefinition = BuildEnumDefinition(enumCollectionType, optionTypes.ToList(), compilation);
                if (enumDefinition != null)
                {
                    results.Add(new EnumTypeInfoWithCompilation(enumDefinition, compilation, optionTypes.ToList()));
                }
            }
        }

        return results.ToImmutableArray();
    }

    /// <summary>
    /// Scans a namespace for types with [EnumCollection] attribute.
    /// </summary>
    /// <param name="namespaceSymbol">The namespace to scan.</param>
    /// <param name="enumCollectionTypes">HashSet to add discovered EnumCollection types to.</param>
    private void ScanForEnumCollectionTypes(INamespaceSymbol namespaceSymbol, HashSet<INamedTypeSymbol> enumCollectionTypes)
    {
        // Scan all types in this namespace
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            // Check for [EnumCollection] attribute AND verify it inherits from EnumOptionBase
            if (type.GetAttributes().Any(attr => 
                attr.AttributeClass?.Name == "EnumCollectionAttribute" || 
                attr.AttributeClass?.Name == "EnumCollection"))
            {
                // Verify this type inherits from EnumOptionBase
                if (InheritsFromEnumOptionBase(type))
                {
                    enumCollectionTypes.Add(type);
                }
            }

            // Recursively scan nested types
            ScanNestedTypesForEnumCollection(type, enumCollectionTypes);
        }

        // Recursively scan nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanForEnumCollectionTypes(nestedNamespace, enumCollectionTypes);
        }
    }

    /// <summary>
    /// Scans a namespace for types with [EnumOption] attribute that derive from a specific base type.
    /// </summary>
    /// <param name="namespaceSymbol">The namespace to scan.</param>
    /// <param name="baseType">The base type to look for derivations from.</param>
    /// <param name="optionTypes">HashSet to add discovered option types to.</param>
    private void ScanForOptionTypesOfBase(INamespaceSymbol namespaceSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)
    {
        // Scan all types in this namespace
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            // Check for [EnumOption] attribute AND verify it derives from the specific base type
            if (type.GetAttributes().Any(attr => 
                attr.AttributeClass?.Name == "EnumOptionAttribute" || 
                attr.AttributeClass?.Name == "EnumOption"))
            {
                if (!type.IsAbstract && DerivesFromBaseType(type, baseType))
                {
                    optionTypes.Add(type);
                }
            }

            // Recursively scan nested types
            ScanNestedTypesForOptionTypesOfBase(type, baseType, optionTypes);
        }

        // Recursively scan nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanForOptionTypesOfBase(nestedNamespace, baseType, optionTypes);
        }
    }

    /// <summary>
    /// Scans nested types within a type for EnumCollection attributes.
    /// </summary>
    /// <param name="typeSymbol">The type to scan for nested types.</param>
    /// <param name="enumCollectionTypes">HashSet to add discovered EnumCollection types to.</param>
    private void ScanNestedTypesForEnumCollection(INamedTypeSymbol typeSymbol, HashSet<INamedTypeSymbol> enumCollectionTypes)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            // Check for [EnumCollection] attribute AND verify it inherits from EnumOptionBase
            if (nestedType.GetAttributes().Any(attr => 
                attr.AttributeClass?.Name == "EnumCollectionAttribute" || 
                attr.AttributeClass?.Name == "EnumCollection"))
            {
                // Verify this type inherits from EnumOptionBase
                if (InheritsFromEnumOptionBase(nestedType))
                {
                    enumCollectionTypes.Add(nestedType);
                }
            }

            // Recursively scan further nested types
            ScanNestedTypesForEnumCollection(nestedType, enumCollectionTypes);
        }
    }

    /// <summary>
    /// Scans nested types within a type for option types that derive from a specific base.
    /// </summary>
    /// <param name="typeSymbol">The type to scan for nested types.</param>
    /// <param name="baseType">The base type to look for derivations from.</param>
    /// <param name="optionTypes">HashSet to add discovered option types to.</param>
    private void ScanNestedTypesForOptionTypesOfBase(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            // Check for [EnumOption] attribute AND verify it derives from the specific base type
            if (nestedType.GetAttributes().Any(attr => 
                attr.AttributeClass?.Name == "EnumOptionAttribute" || 
                attr.AttributeClass?.Name == "EnumOption"))
            {
                if (!nestedType.IsAbstract && DerivesFromBaseType(nestedType, baseType))
                {
                    optionTypes.Add(nestedType);
                }
            }

            // Recursively scan further nested types
            ScanNestedTypesForOptionTypesOfBase(nestedType, baseType, optionTypes);
        }
    }

    /// <summary>
    /// Checks if a type derives from a specific base type.
    /// </summary>
    /// <param name="derivedType">The type to check.</param>
    /// <param name="baseType">The base type to check for.</param>
    /// <returns>True if derivedType derives from baseType.</returns>
    private bool DerivesFromBaseType(INamedTypeSymbol derivedType, INamedTypeSymbol baseType)
    {
        var currentBase = derivedType.BaseType;
        while (currentBase != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentBase, baseType))
            {
                return true;
            }
            currentBase = currentBase.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Builds an EnumTypeInfo from a base type and its derived enum option types.
    /// </summary>
    /// <param name="baseType">The base type symbol.</param>
    /// <param name="enumTypes">The enum option types that derive from the base type.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>The built EnumTypeInfo or null if it can't be built.</returns>
    private EnumTypeInfo? BuildEnumDefinition(INamedTypeSymbol baseType, List<INamedTypeSymbol> enumTypes, Compilation compilation)
    {
        // Process EnumLookup attributes on base type properties
        var lookupProperties = new List<PropertyLookupInfo>();
        var debugInfo = new StringBuilder();
        debugInfo.AppendLine($"// DEBUG: Processing properties for {baseType.Name}:");
        
        foreach (var prop in baseType.GetMembers().OfType<IPropertySymbol>())
        {
            debugInfo.AppendLine($"// Property: {prop.Name} ({prop.Type.ToDisplayString()})");
            
            var lookupAttr = prop.GetAttributes()
                .FirstOrDefault(ad => string.Equals(ad.AttributeClass?.Name, "EnumLookupAttribute", StringComparison.Ordinal) ||
                                     string.Equals(ad.AttributeClass?.Name, "EnumLookup", StringComparison.Ordinal));
            if (lookupAttr == null)
            {
                debugInfo.AppendLine($"//   No EnumLookup attribute found");
                continue;
            }

            debugInfo.AppendLine($"//   Found EnumLookup attribute");
            
            // Get constructor arguments - MethodName is the first parameter
            var constructorArgs = lookupAttr.ConstructorArguments;
            if (constructorArgs.Length == 0 || constructorArgs[0].Value is not string methodName || string.IsNullOrEmpty(methodName))
            {
                debugInfo.AppendLine($"//   No MethodName provided in constructor - skipping");
                continue; // Skip this property if MethodName is not provided
            }
            
            debugInfo.AppendLine($"//   MethodName: {methodName}");
            var allowMultiple = constructorArgs.Length > 1 && constructorArgs[1].Value is bool mu && mu;
            var returnType = constructorArgs.Length > 2 && constructorArgs[2].Value is INamedTypeSymbol rts ? rts.ToDisplayString() : null;

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
            debugInfo.AppendLine($"//   Added lookup property: {methodName}");
        }
        
        debugInfo.AppendLine($"// Total lookup properties found: {lookupProperties.Count}");

        // Create the EnumTypeInfo similar to how EnhancedEnumGenerator does it
        var enumTypeInfo = new EnumTypeInfo
        {
            Namespace = GetNamespace(baseType),
            ClassName = baseType.Name,
            FullTypeName = GetFullTypeName(baseType),
            CollectionName = GetCollectionName(baseType),
            GenerateFactoryMethods = true,
            NameComparison = StringComparison.OrdinalIgnoreCase,
            IncludeReferencedAssemblies = true,
            LookupProperties = new EquatableArray<PropertyLookupInfo>(lookupProperties)
        };

        return enumTypeInfo;
    }

    /// <summary>
    /// Executes the generation of a collection class for a single enum definition.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="def">The enum type definition information.</param>
    /// <param name="compilation">The compilation containing the enum type.</param>
    /// <param name="discoveredOptionTypes">The already-discovered option types to avoid re-scanning.</param>
    private void Execute(SourceProductionContext context, EnumTypeInfo def, Compilation compilation, List<INamedTypeSymbol> discoveredOptionTypes)
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

        // Convert already-discovered option types to EnumValueInfo objects
        var values = new List<EnumValueInfo>();
        foreach (var optionType in discoveredOptionTypes)
        {
            var enumValueInfo = new EnumValueInfo
            {
                ShortTypeName = optionType.Name,
                FullTypeName = optionType.ToDisplayString(),
                Name = optionType.Name, // Use type name as display name
                ReturnTypeNamespace = optionType.ContainingNamespace?.ToDisplayString() ?? string.Empty
            };
            values.Add(enumValueInfo);
        }

        // Generate the collection class
        GenerateCollection(context, def, new EquatableArray<EnumValueInfo>(values), compilation);
    }

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
    /// Checks if a type inherits from EnumOptionBase (directly or indirectly).
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <returns>True if the type inherits from EnumOptionBase, false otherwise.</returns>
    private bool InheritsFromEnumOptionBase(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            // Check if this base type is EnumOptionBase or generic EnumOptionBase<T>
            if (baseType.Name == "EnumOptionBase" && 
                (baseType.ContainingNamespace?.ToDisplayString() == "FractalDataWorks" ||
                 baseType.ContainingNamespace?.ToDisplayString() == "FractalDataWorks.EnhancedEnums"))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Gets the namespace of a type symbol.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <returns>The namespace string.</returns>
    private string GetNamespace(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the full type name of a type symbol.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <returns>The full type name string.</returns>
    private string GetFullTypeName(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString();
    }

    /// <summary>
    /// Gets the collection name from [EnumCollection] attribute.
    /// </summary>
    /// <param name="baseType">The base type symbol.</param>
    /// <returns>The collection name from the attribute.</returns>
    private string GetCollectionName(INamedTypeSymbol baseType)
    {
        // Get collection name from [EnumCollection] attribute
        var enumCollectionAttr = baseType.GetAttributes()
            .FirstOrDefault(attr => 
                attr.AttributeClass?.Name == "EnumCollectionAttribute" ||
                attr.AttributeClass?.Name == "EnumCollection");

        if (enumCollectionAttr != null)
        {
            // Look for CollectionName named argument
            var collectionNameArg = enumCollectionAttr.NamedArguments
                .FirstOrDefault(arg => arg.Key == "CollectionName");
            
            if (collectionNameArg.Value.Value is string collectionName && !string.IsNullOrEmpty(collectionName))
            {
                return collectionName;
            }
        }

        // Should not happen since CollectionName is required
        throw new InvalidOperationException($"CollectionName not found in [EnumCollection] attribute for type {baseType.Name}");
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
                    "ENUM003",
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

/// <summary>
/// Helper class to carry EnumTypeInfo with its compilation context and discovered option types.
/// </summary>
internal class EnumTypeInfoWithCompilation
{
    public EnumTypeInfo EnumTypeInfo { get; }
    public Compilation Compilation { get; }
    public List<INamedTypeSymbol> DiscoveredOptionTypes { get; }

    public EnumTypeInfoWithCompilation(EnumTypeInfo enumTypeInfo, Compilation compilation, List<INamedTypeSymbol> discoveredOptionTypes)
    {
        EnumTypeInfo = enumTypeInfo;
        Compilation = compilation;
        DiscoveredOptionTypes = discoveredOptionTypes;
    }
}