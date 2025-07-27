using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FractalDataWorks.SmartGenerators;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Discovery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.Generators;

/// <summary>
/// Source generator for EnhancedEnumOption definitions.
/// Sets up syntax providers and generates collection classes for enums.
/// 
/// HOW IT WORKS:
/// 1. Scans for classes/interfaces with [EnhancedEnumBase] attribute
/// 2. For each base type found, discovers all types with [EnumOption] that derive from it
/// 3. Generates a static collection class with all discovered options
/// 
/// CROSS-ASSEMBLY DISCOVERY:
/// When IncludeReferencedAssemblies=true on [EnhancedEnumBase], the generator:
/// 1. Checks the IncludedEnhancedEnumAssemblies MSBuild property
/// 2. Scans ONLY the assemblies listed in that property (semicolon-separated)
/// 3. Discovers [EnumOption] types in those assemblies that derive from the base
/// 4. Includes them in the generated collection
/// 
/// This ENABLES the Service Type Pattern:
/// - Define base enum type in a shared assembly with [EnhancedEnumBase]
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

        // Determine the appropriate namespace for the generated class
        // Use the namespace of the definition class (ScanOptionEnumBase)
        string targetNamespace = def.Namespace;

        // Get the base type symbol for various checks
        var baseTypeSymbol = def.IsGenericType && !string.IsNullOrEmpty(def.UnboundTypeName)
            ? compilation.GetTypeByMetadataName(def.UnboundTypeName)
            : compilation.GetTypeByMetadataName(def.FullTypeName);
        
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
        else if (def.IsGenericType)
        {
            // For generic types without a default return type, try to use constraints
            effectiveReturnType = GetReturnTypeFromConstraints(baseTypeSymbol, def);
            
            if (effectiveReturnType == null)
            {
                // No suitable constraint found, use object as fallback
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "ENH003",
                        "Generic enum base requires DefaultGenericReturnType",
                        "Enhanced enum base type '{0}' is generic but doesn't specify DefaultGenericReturnType. Using 'object' as fallback.",
                        "EnhancedEnumOptions",
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    null,
                    def.FullTypeName));
                effectiveReturnType = "object";
            }
        }
        else
        {
            effectiveReturnType = baseTypeSymbol != null ? DetectReturnType(baseTypeSymbol, compilation) : def.FullTypeName;
        }

        // Create a class builder for the collection class
        var classBuilder = new ClassBuilder(def.CollectionName)
            .MakePublic()
            .MakeStatic()
            .WithNamespace(targetNamespace)
            .WithSummary($"Collection of all {def.ClassName} values.");


        // Add private fields for storing instances
        // For generic types, use the effective return type for the list
        var listType = def.IsGenericType && !string.IsNullOrEmpty(effectiveReturnType) 
            ? effectiveReturnType 
            : def.FullTypeName;
            
        
        classBuilder.AddField($"List<{listType}>", "_all", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly()
            .WithInitializer($"new List<{listType}>()"));
        
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
        
        // Add id dictionary if implementing IEnhancedEnumOption
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
        
        // Add id dictionary if implementing IEnhancedEnumOption
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

        // Add static constructor to initialize instances
        var constructorBody = new StringBuilder();
        
        // Add each enum value to the collection
#pragma warning disable S3267 // This is not a simple mapping - different code is generated based on UseFactory
        foreach (var value in values)
#pragma warning restore S3267
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
        
        // Build the conditional compilation logic for dictionary initialization
        constructorBody.AppendLine();
        constructorBody.AppendLine("// Populate dictionaries for fast lookups");
        constructorBody.AppendLine("#if NET8_0_OR_GREATER");
        constructorBody.AppendLine("// Create temp dictionaries for FrozenDictionary initialization");
        constructorBody.AppendLine($"var tempNameDict = new Dictionary<string, {effectiveReturnType}>(StringComparer.{def.NameComparison});");
        
        // Add temp id dictionary if implementing IEnhancedEnumOption
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
        
        // Populate id dictionary if implementing IEnhancedEnumOption
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
        
        // Convert id dictionary if implementing IEnhancedEnumOption
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
        
        // Populate id dictionary if implementing IEnhancedEnumOption
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
        
        // Add the static constructor to the class
        classBuilder.AddCodeBlock($"static {def.CollectionName}()\n{{\n{constructorBody}\n}}");

        // Add All property
        classBuilder.AddProperty("All", $"ImmutableArray<{effectiveReturnType}>", prop => prop
            .MakePublic()
            .MakeStatic()
            .WithExpressionBody("_cachedAll")
            .WithXmlDocSummary($"Gets all available {def.ClassName} values."));

        // Add GetByName method - always generate since Name property is required by design
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
            
        // Generate GetById method if implementing IEnhancedEnumOption
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

        // Generate lookup methods for marked properties
        foreach (var lookup in def.LookupProperties)
        {
            GenerateLookupMethod(classBuilder, def, lookup, effectiveReturnType);
        }

        // Generate static property accessors for each enum value
        if (!values.IsEmpty)
        {
            classBuilder.AddCodeBlock("// Static property accessors");
            foreach (var value in values)
            {
                // Use the Name property which comes from EnumOptionAttribute.Name or falls back to class name
                var propertyName = MakeValidIdentifier(value.Name);
                
                classBuilder.AddProperty(propertyName, effectiveReturnType ?? def.FullTypeName, prop => prop
                    .MakePublic()
                    .MakeStatic()
                    .WithExpressionBody($"_all.OfType<{value.FullTypeName}>().First()")
                    .WithXmlDocSummary($"Gets the {value.Name} instance."));
            }
        }
        
        // Generate static factory methods for each enum value
        if (!values.IsEmpty)
        {
            classBuilder.AddCodeBlock("// Static factory methods");
            foreach (var value in values)
            {
                var methodName = $"Create{MakeValidIdentifier(value.Name)}";
                
                // Determine the return type for this specific value
                var valueReturnType = !string.IsNullOrEmpty(value.ReturnType) 
                    ? value.ReturnType 
                    : (effectiveReturnType ?? def.FullTypeName);
                
                classBuilder.AddMethod(methodName, valueReturnType!, method => method
                    .MakePublic()
                    .MakeStatic()
                    .WithXmlDocSummary($"Creates a new instance of {value.Name}.")
                    .WithXmlDocReturns($"A new {value.Name} instance.")
                    .WithExpressionBody($"new {value.FullTypeName}()"));
            }
        }

        // Generate Empty value
        GenerateEmptyValue(classBuilder, def, effectiveReturnType, baseTypeSymbol);

        // Build the complete source code with headers
        var sourceCode = new StringBuilder();
        
        // Add headers and using statements
        sourceCode.AppendLine("#nullable enable");
        sourceCode.AppendLine();
        sourceCode.AppendLine("using System;");
        sourceCode.AppendLine("using System.Linq;");
        sourceCode.AppendLine("using System.Collections.Generic;");
        sourceCode.AppendLine("using System.Collections.Immutable;");
        sourceCode.AppendLine("#if NET8_0_OR_GREATER");
        sourceCode.AppendLine("using System.Collections.Frozen;");
        sourceCode.AppendLine("#endif");
        
        // Extract additional namespaces from enum values
        var additionalNamespaces = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            // From return type overrides
            if (!string.IsNullOrEmpty(value.ReturnTypeNamespace))
            {
                additionalNamespaces.Add(value.ReturnTypeNamespace!);
            }
            
            // From constructor parameters
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
        
        // Merge additional namespaces with RequiredNamespaces
        foreach (var ns in additionalNamespaces)
        {
            def.RequiredNamespaces.Add(ns);
        }
        
        // Add required namespaces from generic constraints
        foreach (var ns in def.RequiredNamespaces.OrderBy(n => n, StringComparer.Ordinal))
        {
            // Skip the current namespace and the standard System namespaces we already added
            if (!string.Equals(ns, targetNamespace, StringComparison.Ordinal) &&
                !string.Equals(ns, "System", StringComparison.Ordinal) &&
                !string.Equals(ns, "System.Linq", StringComparison.Ordinal) &&
                !string.Equals(ns, "System.Collections.Generic", StringComparison.Ordinal) &&
                !string.Equals(ns, "System.Collections.Immutable", StringComparison.Ordinal))
            {
                sourceCode.AppendLine($"using {ns};");
            }
        }

        // Add default generic return type namespace if specified
        if (!string.IsNullOrEmpty(def.DefaultGenericReturnTypeNamespace))
        {
            var ns = def.DefaultGenericReturnTypeNamespace!;
            if (!ns.StartsWith("System", StringComparison.Ordinal) && 
                !string.Equals(ns, targetNamespace, StringComparison.Ordinal))
            {
                sourceCode.AppendLine($"using {ns};");
            }
        }
        
        // Add namespace for ReturnType if specified
        if (!string.IsNullOrEmpty(def.ReturnTypeNamespace))
        {
            // Use the explicitly provided namespace
            if (!def.ReturnTypeNamespace!.StartsWith("System", StringComparison.Ordinal) && 
                !string.Equals(def.ReturnTypeNamespace, targetNamespace, StringComparison.Ordinal))
            {
                sourceCode.AppendLine($"using {def.ReturnTypeNamespace};");
            }
        }
        else if (!string.IsNullOrEmpty(effectiveReturnType))
        {
            // Extract namespace from ReturnType if not explicitly provided
            var cleanReturnType = effectiveReturnType!.TrimEnd('?');
            var lastDotIndex = cleanReturnType.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                var returnTypeNamespace = cleanReturnType.Substring(0, lastDotIndex);
                // Don't add System namespaces or the current namespace
                if (!returnTypeNamespace.StartsWith("System", StringComparison.Ordinal) && 
                    !string.Equals(returnTypeNamespace, targetNamespace, StringComparison.Ordinal))
                {
                    sourceCode.AppendLine($"using {returnTypeNamespace};");
                }
            }
        }
        
        sourceCode.AppendLine();
        
        // Add the generated class
        sourceCode.Append(classBuilder.Build());
        
        // Add the source
        var generatedCode = sourceCode.ToString();
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
            .Any(a => a.Name.ToString().Contains("EnhancedEnumBase"));
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

        // Don't modify def.ReturnType here - it should remain as specified in the attribute or empty
        // The GenerateCollection method will handle determining the effective return type

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
                
                // Extract values from constructor arguments (new way)
                string? collectionName = null;
                string? name = null;
                int order = 0;
                string? returnType = null;
                string? returnTypeNamespace = null;
                
                if (attr.ConstructorArguments.Length > 0)
                {
                    // Constructor: EnumOptionAttribute(string? collectionName = null, string? name = null, int order = 0, string? returnType = null, string? returnTypeNamespace = null)
                    if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string cname)
                        collectionName = cname;
                    if (attr.ConstructorArguments.Length > 1 && attr.ConstructorArguments[1].Value is string nm)
                        name = nm;
                    if (attr.ConstructorArguments.Length > 2 && attr.ConstructorArguments[2].Value is int o)
                        order = o;
                    if (attr.ConstructorArguments.Length > 3 && attr.ConstructorArguments[3].Value is string rt)
                        returnType = rt;
                    if (attr.ConstructorArguments.Length > 4 && attr.ConstructorArguments[4].Value is string rtn)
                        returnTypeNamespace = rtn;
                }
                
                // Fall back to named arguments for backward compatibility
                if (name == null && named.TryGetValue(nameof(EnumOptionAttribute.Name), out var n) && n.Value is string ns)
                    name = ns;
                if (named.TryGetValue(nameof(EnumOptionAttribute.Order), out var ord) && ord.Value is int o2)
                    order = o2;
                if (returnType == null && named.TryGetValue(nameof(EnumOptionAttribute.ReturnType), out var r) && r.Value is string rts)
                    returnType = rts;
                if (returnTypeNamespace == null && named.TryGetValue(nameof(EnumOptionAttribute.ReturnTypeNamespace), out var rn) && rn.Value is string rtns)
                    returnTypeNamespace = rtns;
                if (collectionName == null && named.TryGetValue(nameof(EnumOptionAttribute.CollectionName), out var cn) && cn.Value is string cns)
                    collectionName = cns;
                
                // Default name to type name if not specified
                if (string.IsNullOrEmpty(name))
                    name = typeSymbol.Name;

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
                        Name = name!,
                        Order = order,
                        ReturnType = returnType,
                        ReturnTypeNamespace = returnTypeNamespace,
                        Constructors = ExtractConstructors(typeSymbol)
                    };
                    values.Add(info);
                    
                    // Extract namespaces from the option type's base type arguments
                    ExtractNamespacesFromOptionType(typeSymbol, baseTypeSymbol, def);
                    
                    break; // Only add once per type per collection, even if multiple matching attributes
                }
            }
        }

        // Sort values by Order, then by Name
        var sortedValues = values.OrderBy(v => v.Order).ThenBy(v => v.Name, StringComparer.Ordinal).ToList();

        // Generate the collection class
        GenerateCollection(context, def, new EquatableArray<EnumValueInfo>(sortedValues), compilation);
    }

    /// <summary>
    /// Determines whether an enum value belongs to a given enum definition.
    /// </summary>
    /// <param name="valueSymbol">The symbol of the enum value to check.</param>
    /// <param name="baseTypeSymbol">The symbol of the base type to match against.</param>
    /// <returns>True if the value matches the definition; otherwise, false.</returns>
    private static bool MatchesDefinition(INamedTypeSymbol valueSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        // For generic types, we need to compare the unbound type definitions
        var baseToCompare = baseTypeSymbol.IsGenericType ? baseTypeSymbol.OriginalDefinition : baseTypeSymbol;
        
        // interface implementation
        if (valueSymbol.AllInterfaces.Any(i => 
        {
            var interfaceToCompare = i.IsGenericType ? i.OriginalDefinition : i;
            return SymbolEqualityComparer.Default.Equals(interfaceToCompare, baseToCompare);
        }))
        {
            return true;
        }

        // base type inheritance
        for (var baseType = valueSymbol.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            var currentToCompare = baseType.IsGenericType ? baseType.OriginalDefinition : baseType;
            if (SymbolEqualityComparer.Default.Equals(currentToCompare, baseToCompare))
            {
                return true;
            }
        }

        return false;
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
            .Where(ad => string.Equals(ad.AttributeClass?.Name, "EnhancedEnumBaseAttribute", StringComparison.Ordinal) ||
string.Equals(ad.AttributeClass?.Name, "EnhancedEnumBase", StringComparison.Ordinal))
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
            var returnType = lnamed.TryGetValue(nameof(EnumLookupAttribute.ReturnType), out var rt) && rt.Value is string rs ? rs : null;

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

        // Process each EnhancedEnumOption attribute to create separate collections
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
                UseFactory = named.TryGetValue(nameof(EnhancedEnumBaseAttribute.UseFactory), out var uf) && uf.Value is bool b && b,
                NameComparison = named.TryGetValue(nameof(EnhancedEnumBaseAttribute.NameComparison), out var nc) && nc.Value is int ic
                    ? (StringComparison)ic : StringComparison.OrdinalIgnoreCase,
                IncludeReferencedAssemblies = named.TryGetValue(nameof(EnhancedEnumBaseAttribute.IncludeReferencedAssemblies), out var ira) && 
                    ira.Value is bool iraValue && iraValue,
                ReturnType = named.TryGetValue(nameof(EnhancedEnumBaseAttribute.ReturnType), out var rt) && rt.Value is string rs ? rs : null,
                ReturnTypeNamespace = named.TryGetValue(nameof(EnhancedEnumBaseAttribute.ReturnTypeNamespace), out var rtn) && rtn.Value is string rtns ? rtns : null,
                LookupProperties = new EquatableArray<PropertyLookupInfo>(lookupProperties),
            };

            // Extract generic type information
            ExtractGenericTypeInfo(symbol, collectionInfo);

            // Get default generic return type from attribute
            collectionInfo.DefaultGenericReturnType = named.TryGetValue(nameof(EnhancedEnumBaseAttribute.DefaultGenericReturnType), out var dgrt) && dgrt.Value is string dgrts ? dgrts : null;
            collectionInfo.DefaultGenericReturnTypeNamespace = named.TryGetValue(nameof(EnhancedEnumBaseAttribute.DefaultGenericReturnTypeNamespace), out var dgrtn) && dgrtn.Value is string dgrtns ? dgrtns : null;

            // Store the symbol temporarily for further processing
            // This will be used in Execute method but won't be part of equality
            results.Add(collectionInfo);
        }

        return results;
    }

    /// <summary>
    /// Generates a lookup method for a specific property.
    /// </summary>
    private static void GenerateLookupMethod(ClassBuilder classBuilder, EnumTypeInfo def, PropertyLookupInfo lookup, string? effectiveReturnType)
    {
        var paramName = ToCamelCase(lookup.PropertyName);
        
        // Determine the return type for this specific lookup
        var lookupReturnType = !string.IsNullOrEmpty(lookup.ReturnType) ? lookup.ReturnType : (effectiveReturnType ?? def.FullTypeName);
        
        if (lookup.AllowMultiple)
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
        else
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

            // Find the most accessible constructor and generate appropriate call
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
        
        // Generate default implementations for lookup properties
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
        // Remove nullable annotations for comparison
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
        #pragma warning disable MA0009 // Ignore regex warning for this simple case
        // Replace spaces and special characters with underscores
        var result = System.Text.RegularExpressions.Regex.Replace(name, @"[^\w]", "_");
        #pragma warning restore MA0009          
        // Ensure it doesn't start with a number
        if (char.IsDigit(result[0]))
            result = "_" + result;
        
        #pragma warning disable MA0009 // Ignore regex warning for this simple case
        // Remove consecutive underscores
        result = System.Text.RegularExpressions.Regex.Replace(result, @"_+", "_");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"_", string.Empty);
        #pragma warning restore MA0009
        // Trim underscores from ends
        result = result.Trim('_');
        
        return string.IsNullOrEmpty(result) ? "_" : result;
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
    /// Gets the return type from generic type constraints.
    /// </summary>
    private static string? GetReturnTypeFromConstraints(INamedTypeSymbol? baseTypeSymbol, EnumTypeInfo def)
    {
        if (baseTypeSymbol == null || !baseTypeSymbol.IsGenericType)
            return null;
            
        // Check if we have exactly one type parameter with a single interface or class constraint
        if (baseTypeSymbol.TypeParameters.Length == 1)
        {
            var typeParam = baseTypeSymbol.TypeParameters[0];
            var constraints = typeParam.ConstraintTypes;
            
            // If there's exactly one constraint that's not a value type, use it
            if (constraints.Length == 1)
            {
                var constraint = constraints[0];
                if (constraint.TypeKind == TypeKind.Interface || constraint.TypeKind == TypeKind.Class)
                {
                    return constraint.ToDisplayString();
                }
            }
        }
        
        return null;
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
    /// Gets the full enum type name for StringComparison values.
    /// </summary>
    private static string GetStringComparisonName(StringComparison comparison)
    {
        return $"StringComparison.{comparison}";
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
    /// Checks if a type symbol has the EnumOption attribute.
    /// </summary>
    private static bool HasEnumOptionAttribute(INamedTypeSymbol type)
    {
        return type.GetAttributes().Any(attr =>
            string.Equals(attr.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal));
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
    /// Extracts constructor information from a type symbol.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to extract constructors from.</param>
    /// <returns>A list of constructor information.</returns>
    private static List<Models.ConstructorInfo> ExtractConstructors(INamedTypeSymbol typeSymbol)
    {
        var constructors = new List<Models.ConstructorInfo>();
        
        // Get all public constructors
        foreach (var ctor in typeSymbol.Constructors)
        {
            if (ctor.DeclaredAccessibility != Accessibility.Public)
                continue;
                
            var ctorInfo = new Models.ConstructorInfo
            {
                Accessibility = ctor.DeclaredAccessibility,
                IsPrimary = IsPrimaryConstructor(ctor)
            };
            
            foreach (var param in ctor.Parameters)
            {
                ctorInfo.Parameters.Add(new Models.ParameterInfo
                {
                    TypeName = param.Type.ToDisplayString(),
                    Name = param.Name,
                    HasDefaultValue = param.HasExplicitDefaultValue,
                    DefaultValue = param.HasExplicitDefaultValue 
                        ? GetDefaultValueString(param) : null,
                    Namespace = param.Type.ContainingNamespace?.ToDisplayString()
                });
            }
            
            constructors.Add(ctorInfo);
        }
        
        return constructors;
    }
    
    /// <summary>
    /// Determines if a constructor is a primary constructor.
    /// </summary>
    private static bool IsPrimaryConstructor(IMethodSymbol constructor)
    {
        // For now, we'll consider a constructor primary if it's the only public constructor
        // In C# 12+, we'd check for actual primary constructor syntax
        return constructor.ContainingType.Constructors
            .Count(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic) == 1;
    }
    
    /// <summary>
    /// Gets the default value string representation for a parameter.
    /// </summary>
    private static string? GetDefaultValueString(IParameterSymbol parameter)
    {
        if (!parameter.HasExplicitDefaultValue)
            return null;
            
        var value = parameter.ExplicitDefaultValue;
        if (value == null)
            return "null";
            
        if (value is string str)
            return $"\"{str}\"";
            
        if (value is char ch)
            return $"'{ch}'";
            
        if (value is bool b)
            return b ? "true" : "false";
            
        return value.ToString();
    }
}
