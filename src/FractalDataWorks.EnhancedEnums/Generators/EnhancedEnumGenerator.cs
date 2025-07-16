using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.SmartGenerators;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.Generators;

/// <summary>
/// Source generator for EnhancedEnumOption definitions.
/// Sets up syntax providers and generates collection classes for enums.
/// </summary>
[Generator]
public class EnhancedEnumOptionGenerator : FractalDataWorks.SmartGenerators.IncrementalGeneratorBase<EnumTypeInfo>
{
    /// <summary>
    /// Generates the collection class for an enum definition with its values.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="def">The enum type definition information.</param>
    /// <param name="values">The enum values to include in the collection.</param>
    protected virtual void GenerateCollection(SourceProductionContext context, EnumTypeInfo def, EquatableArray<EnumValueInfo> values)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));

        // Determine the appropriate namespace for the generated class
        // Use the namespace of the definition class (ScanOptionEnumBase)
        string targetNamespace = def.Namespace;

        // Create a code builder for the source file
        var codeBuilder = new CodeBuilder();

        // Add nullable directive and using directives
        codeBuilder.AppendLine("#nullable enable");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("using System;");
        codeBuilder.AppendLine("using System.Linq;");
        codeBuilder.AppendLine("using System.Collections.Generic;");
        codeBuilder.AppendLine("using System.Collections.Immutable;");
        codeBuilder.AppendLine("#if NET8_0_OR_GREATER");
        codeBuilder.AppendLine("using System.Collections.Frozen;");
        codeBuilder.AppendLine("#endif");
        codeBuilder.AppendLine();

        // Add namespace declaration if needed
        if (!string.IsNullOrEmpty(targetNamespace))
        {
            codeBuilder.AppendLine($"namespace {targetNamespace}");
            codeBuilder.AppendLine("{");
            codeBuilder.Indent();
        }

        // Add class declaration
        codeBuilder.AppendLine($"/// <summary>");
        codeBuilder.AppendLine($"/// Collection of all {def.ClassName} values.");
        codeBuilder.AppendLine($"/// </summary>");
        codeBuilder.AppendLine($"public static class {def.CollectionName}");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();

        // Add private field for storing instances
        codeBuilder.AppendLine($"private static readonly List<{def.FullTypeName}> _all = new List<{def.FullTypeName}>();");
        codeBuilder.AppendLine($"private static readonly ImmutableArray<{def.FullTypeName}> _cachedAll;");
        
        // Use FrozenDictionary on .NET 8+ for better performance
        codeBuilder.AppendLine("#if NET8_0_OR_GREATER");
        codeBuilder.AppendLine($"private static readonly FrozenDictionary<string, {def.FullTypeName}> _nameDict;");
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                codeBuilder.AppendLine($"private static readonly FrozenDictionary<{lookup.PropertyType}, {def.FullTypeName}> _{ToCamelCase(lookup.PropertyName)}Dict;");
            }
        }
        codeBuilder.AppendLine("#else");
        codeBuilder.AppendLine($"private static readonly Dictionary<string, {def.FullTypeName}> _nameDict = new Dictionary<string, {def.FullTypeName}>(StringComparer.{def.NameComparison});");
        
        // Add dictionary fields for each lookup property
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                var comparerStr = string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal) 
                    ? $"(StringComparer.{def.NameComparison})" 
                    : "()";
                codeBuilder.AppendLine($"private static readonly Dictionary<{lookup.PropertyType}, {def.FullTypeName}> _{ToCamelCase(lookup.PropertyName)}Dict = new Dictionary<{lookup.PropertyType}, {def.FullTypeName}>{comparerStr};");
            }
        }
        codeBuilder.AppendLine("#endif");
        
        codeBuilder.AppendLine();

        // Add static constructor to initialize instances
        codeBuilder.AppendLine($"static {def.CollectionName}()");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();

        // Add each enum value to the collection
#pragma warning disable S3267 // This is not a simple mapping - different code is generated based on UseFactory
        foreach (var value in values)
#pragma warning restore S3267
        {
            if (def.UseFactory)
            {
                // Factory method is on the base class, not the derived class
                codeBuilder.AppendLine($"_all.Add({def.FullTypeName}.Create(typeof({value.FullTypeName})));");
            }
            else
            {
                codeBuilder.AppendLine($"_all.Add(new {value.FullTypeName}());");
            }
        }

        // Cache the All collection to prevent allocations
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("// Cache the immutable array to prevent repeated allocations");
        codeBuilder.AppendLine("_cachedAll = _all.ToImmutableArray();");
        
        // Populate dictionaries for fast lookups
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("// Populate dictionaries for fast lookups");
        codeBuilder.AppendLine("#if NET8_0_OR_GREATER");
        codeBuilder.AppendLine("// Create temp dictionaries for FrozenDictionary initialization");
        codeBuilder.AppendLine($"var tempNameDict = new Dictionary<string, {def.FullTypeName}>(StringComparer.{def.NameComparison});");
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                var comparerStr = string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal) 
                    ? $"(StringComparer.{def.NameComparison})" 
                    : "()";
                codeBuilder.AppendLine($"var temp{lookup.PropertyName}Dict = new Dictionary<{lookup.PropertyType}, {def.FullTypeName}>{comparerStr};");
            }
        }
        
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("foreach (var item in _all)");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();
        codeBuilder.AppendLine("if (!tempNameDict.ContainsKey(item.Name))");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();
        codeBuilder.AppendLine("tempNameDict[item.Name] = item;");
        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                codeBuilder.AppendLine();
                codeBuilder.AppendLine($"if (!temp{lookup.PropertyName}Dict.ContainsKey(item.{lookup.PropertyName}))");
                codeBuilder.AppendLine("{");
                codeBuilder.Indent();
                codeBuilder.AppendLine($"temp{lookup.PropertyName}Dict[item.{lookup.PropertyName}] = item;");
                codeBuilder.Outdent();
                codeBuilder.AppendLine("}");
            }
        }
        
        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");
        
        // Convert to FrozenDictionaries
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("// Convert to FrozenDictionaries for better performance");
        codeBuilder.AppendLine("_nameDict = tempNameDict.ToFrozenDictionary(StringComparer." + def.NameComparison + ");");
        
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                var comparerStr = string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal) 
                    ? "(StringComparer." + def.NameComparison + ")" 
                    : "()";
                codeBuilder.AppendLine($"_{ToCamelCase(lookup.PropertyName)}Dict = temp{lookup.PropertyName}Dict.ToFrozenDictionary{comparerStr};");
            }
        }
        
        codeBuilder.AppendLine("#else");
        codeBuilder.AppendLine("foreach (var item in _all)");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();
        codeBuilder.AppendLine("if (!_nameDict.ContainsKey(item.Name))");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();
        codeBuilder.AppendLine("_nameDict[item.Name] = item;");
        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");
        
        // Populate dictionaries for lookup properties
        foreach (var lookup in def.LookupProperties)
        {
            if (!lookup.AllowMultiple)
            {
                var dictName = $"_{ToCamelCase(lookup.PropertyName)}Dict";
                codeBuilder.AppendLine();
                codeBuilder.AppendLine($"if (!{dictName}.ContainsKey(item.{lookup.PropertyName}))");
                codeBuilder.AppendLine("{");
                codeBuilder.Indent();
                codeBuilder.AppendLine($"{dictName}[item.{lookup.PropertyName}] = item;");
                codeBuilder.Outdent();
                codeBuilder.AppendLine("}");
            }
        }
        
        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");
        codeBuilder.AppendLine("#endif");

        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");
        codeBuilder.AppendLine();

        // Add All property
        codeBuilder.AppendLine("/// <summary>");
        codeBuilder.AppendLine($"/// Gets all available {def.ClassName} values.");
        codeBuilder.AppendLine("/// </summary>");
        codeBuilder.AppendLine($"public static ImmutableArray<{def.FullTypeName}> All => _cachedAll;");

        // Add GetByName method - always generate since Name property is required by design
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("/// <summary>");
        codeBuilder.AppendLine($"/// Gets the {def.ClassName} with the specified name.");
        codeBuilder.AppendLine("/// </summary>");
        codeBuilder.AppendLine("/// <param name=\"name\">The name to search for.</param>");
        codeBuilder.AppendLine($"/// <returns>The {def.ClassName} with the specified name, or null if not found.</returns>");
        codeBuilder.AppendLine($"public static {def.FullTypeName}? GetByName(string name)");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();
        codeBuilder.AppendLine("if (string.IsNullOrEmpty(name))");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();
        codeBuilder.AppendLine("return null;");
        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("_nameDict.TryGetValue(name, out var result);");
        codeBuilder.AppendLine("return result;");
        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");

        // Generate lookup methods for marked properties
        foreach (var lookup in def.LookupProperties)
        {
            GenerateLookupMethod(codeBuilder, def, lookup);
        }

        // Generate static property accessors for each enum value
        if (!values.IsEmpty)
        {
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("// Static property accessors");
            foreach (var value in values)
            {
                // Use the Name property which comes from EnumOptionAttribute.Name or falls back to class name
                var propertyName = MakeValidIdentifier(value.Name);
                
                codeBuilder.AppendLine();
                codeBuilder.AppendLine("/// <summary>");
                codeBuilder.AppendLine($"/// Gets the {value.Name} instance.");
                codeBuilder.AppendLine("/// </summary>");
                codeBuilder.AppendLine($"public static {def.FullTypeName} {propertyName} => _all.OfType<{value.FullTypeName}>().First();");
            }
        }

        // Generate Empty value
        GenerateEmptyValue(codeBuilder, def);

        // Close class
        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");

        // Close namespace if needed
        if (!string.IsNullOrEmpty(targetNamespace))
        {
            codeBuilder.Outdent();
            codeBuilder.AppendLine("}");
        }

        // Add the source
        context.AddSource($"{def.CollectionName}.g.cs", codeBuilder.ToString());
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
            .Any(a => a.Name.ToString().Contains("EnhancedEnumOption"));
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

        // Validate: Generic types are not supported
        if (def.IsGenericType)
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    "ENH001",
                    "Generic enhanced enum types are not supported",
                    "Enhanced enum base type '{0}' cannot be generic. Generic types are not supported for enhanced enums.",
                    "EnhancedEnumOptions",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                null,
                def.ClassName);

            context.ReportDiagnostic(diagnostic);
            return;
        }

        var values = new List<EnumValueInfo>();

        // Find the base type symbol from the compilation
        var baseTypeSymbol = compilation.GetTypeByMetadataName(def.FullTypeName);
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

        // Create a scanner to find all types
        var scanner = new AssemblyScanner(compilation);

        // Scan all types in the compilation and referenced assemblies (if enabled)
        var allTypes = new List<INamedTypeSymbol>();

        // Add types from current compilation
        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var model = compilation.GetSemanticModel(tree);
            foreach (var cds in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (!cds.AttributeLists.SelectMany(al => al.Attributes)
                    .Any(a => a.Name.ToString().Contains("EnumOption")))
                {
                    continue;
                }

                var sym = (INamedTypeSymbol)model.GetDeclaredSymbol(cds)!;
                allTypes.Add(sym);
            }
        }

        // Add types from referenced assemblies if enabled
        if (def.IncludeReferencedAssemblies)
        {
            var referencedTypesWithAttribute = scanner.AllNamedTypes
                .Where(typeSymbol => typeSymbol.GetAttributes().Any(ad => string.Equals(ad.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) ||
string.Equals(ad.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal)));
            allTypes.AddRange(referencedTypesWithAttribute);
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
                var name = named.TryGetValue(nameof(EnumOptionAttribute.Name), out var n) && n.Value is string ns
                    ? ns : typeSymbol.Name;

                // Check if this enum option specifies a collection name
                var collectionName = named.TryGetValue(nameof(EnumOptionAttribute.CollectionName), out var cn) && cn.Value is string cns
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
                    break; // Only add once per type per collection, even if multiple matching attributes
                }
            }
        }

        // Generate the collection class
        GenerateCollection(context, def, new EquatableArray<EnumValueInfo>(values));
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
    /// Transforms a generator syntax context into an enum type info.
    /// </summary>
    /// <param name="ctx">The generator syntax context.</param>
    /// <returns>An EnumTypeInfo representing the enum definition.</returns>
    private static IEnumerable<EnumTypeInfo> TransformDefinitions(GeneratorSyntaxContext ctx)
    {
        var decl = (BaseTypeDeclarationSyntax)ctx.Node;
        var symbol = (INamedTypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(decl)!;
        var attrs = symbol.GetAttributes()
            .Where(ad => string.Equals(ad.AttributeClass?.Name, "EnhancedEnumOptionAttribute", StringComparison.Ordinal) ||
string.Equals(ad.AttributeClass?.Name, "EnhancedEnumOption", StringComparison.Ordinal))
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

            lookupProperties.Add(new PropertyLookupInfo
            {
                PropertyName = prop.Name,
                PropertyType = prop.Type.ToDisplayString(),
                LookupMethodName = methodName,
                AllowMultiple = allowMultiple,
                IsNullable = prop.Type.NullableAnnotation == NullableAnnotation.Annotated,
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
                UseFactory = named.TryGetValue(nameof(EnhancedEnumOptionAttribute.UseFactory), out var uf) && uf.Value is bool b && b,
                NameComparison = named.TryGetValue(nameof(EnhancedEnumOptionAttribute.NameComparison), out var nc) && nc.Value is int ic
                    ? (StringComparison)ic : StringComparison.OrdinalIgnoreCase,
                IncludeReferencedAssemblies = named.TryGetValue(nameof(EnhancedEnumOptionAttribute.IncludeReferencedAssemblies), out var ira) && 
                    ira.Value is bool iraValue && iraValue,
                LookupProperties = new EquatableArray<PropertyLookupInfo>(lookupProperties),
            };

            // Store the symbol temporarily for further processing
            // This will be used in Execute method but won't be part of equality
            results.Add(collectionInfo);
        }

        return results;
    }

    /// <summary>
    /// Generates a lookup method for a specific property.
    /// </summary>
    private static void GenerateLookupMethod(CodeBuilder codeBuilder, EnumTypeInfo def, PropertyLookupInfo lookup)
    {
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("/// <summary>");
        codeBuilder.AppendLine($"/// Gets the {def.ClassName} with the specified {lookup.PropertyName}.");
        codeBuilder.AppendLine("/// </summary>");
        codeBuilder.AppendLine($"/// <param name=\"{ToCamelCase(lookup.PropertyName)}\">The {lookup.PropertyName} to search for.</param>");

        if (lookup.AllowMultiple)
        {
            codeBuilder.AppendLine($"/// <returns>All {def.ClassName} instances with the specified {lookup.PropertyName}.</returns>");
            var paramName = ToCamelCase(lookup.PropertyName);
            codeBuilder.AppendLine($"public static IEnumerable<{def.FullTypeName}> {lookup.LookupMethodName}({lookup.PropertyType} {paramName})");
            codeBuilder.AppendLine("{");
            codeBuilder.Indent();

            // Handle collection types - when the property is a collection, check if it contains the search value
            if (lookup.PropertyType.Contains("[]") || lookup.PropertyType.Contains("IEnumerable") || lookup.PropertyType.Contains("List"))
            {
                codeBuilder.AppendLine($"return _all.Where(x => x.{lookup.PropertyName}?.Contains({paramName}) ?? false);");
            }
            else
            {
                // Handle simple types - search for all items that have the matching property value
                codeBuilder.AppendLine($"return _all.Where(x => Equals(x.{lookup.PropertyName}, {paramName}));");
            }

            codeBuilder.Outdent();
            codeBuilder.AppendLine("}");
        }
        else
        {
            codeBuilder.AppendLine($"/// <returns>The {def.ClassName} with the specified {lookup.PropertyName}, or null if not found.</returns>");
            var paramName = ToCamelCase(lookup.PropertyName);
            var dictName = $"_{ToCamelCase(lookup.PropertyName)}Dict";
            // PropertyType already includes nullable annotation if needed
            codeBuilder.AppendLine($"public static {def.FullTypeName}? {lookup.LookupMethodName}({lookup.PropertyType} {paramName})");
            codeBuilder.AppendLine("{");
            codeBuilder.Indent();

            // Add null check for string types
            if (string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal))
            {
                codeBuilder.AppendLine($"if (string.IsNullOrEmpty({paramName}))");
                codeBuilder.AppendLine("{");
                codeBuilder.Indent();
                codeBuilder.AppendLine("return null;");
                codeBuilder.Outdent();
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine();
            }

            // Use dictionary lookup
            codeBuilder.AppendLine($"{dictName}.TryGetValue({paramName}, out var result);");
            codeBuilder.AppendLine("return result;");

            codeBuilder.Outdent();
            codeBuilder.AppendLine("}");
        }
    }

    /// <summary>
    /// Generates the Empty value singleton for the enum collection.
    /// </summary>
    private static void GenerateEmptyValue(CodeBuilder codeBuilder, EnumTypeInfo def)
    {
        // Add Empty property singleton field
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"private static readonly EmptyValue _empty = new EmptyValue();");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("/// <summary>");
        codeBuilder.AppendLine("/// Gets an empty instance representing no selection.");
        codeBuilder.AppendLine("/// </summary>");
        codeBuilder.AppendLine($"public static {def.FullTypeName} Empty => _empty;");
        
        // Generate nested EmptyValue class
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"private sealed class EmptyValue : {def.FullTypeName}");
        codeBuilder.AppendLine("{");
        codeBuilder.Indent();
        
        // Generate common property implementations with default values
        // Name property is almost always present in enums
        codeBuilder.AppendLine("public override string Name => string.Empty;");
        
        // Generate default implementations for lookup properties
        foreach (var lookup in def.LookupProperties)
        {
            var defaultValue = GetDefaultValueForType(lookup.PropertyType);
            codeBuilder.AppendLine($"public override {lookup.PropertyType} {lookup.PropertyName} => {defaultValue};");
        }
        
        // Note: We can't know all abstract properties at generation time, 
        // so users may need to create custom empty classes for complex enums
        
        codeBuilder.Outdent();
        codeBuilder.AppendLine("}");
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
            _ => typeName.EndsWith("?") ? "null" : $"default({cleanType})"
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
}
