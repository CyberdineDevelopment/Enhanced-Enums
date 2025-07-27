using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FractalDataWorks.EnhancedEnums.Analyzers;

/// <summary>
/// Analyzer that detects duplicate enum options within the same collection.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DuplicateEnumOptionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ENH1003";

    private static readonly LocalizableString Title = "Duplicate enum option detected";
    private static readonly LocalizableString MessageFormat = "Enum option '{0}' is already defined in collection '{1}' by class '{2}'";
    private static readonly LocalizableString Description = "Each enum option name must be unique within its collection to avoid naming conflicts in generated code.";
    private const string Category = "Naming";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        customTags: new[] { "CompilationEnd" });

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        // Dictionary to track options by collection name
        var optionsByCollection = new Dictionary<string, List<(string OptionName, INamedTypeSymbol Type, AttributeData Attribute, Location Location)>>(StringComparer.OrdinalIgnoreCase);

        // Find all types with [EnumOption] attribute
        var enumOptionAttribute = context.Compilation.GetTypeByMetadataName("FractalDataWorks.EnhancedEnums.Attributes.EnumOptionAttribute");
        if (enumOptionAttribute == null)
            return;

        // Get all named types in the compilation
        var allTypes = GetAllNamedTypes(context.Compilation);
        
        foreach (var classSymbol in allTypes)
        {
            // Check for [EnumOption] attribute
            var enumOptionAttr = classSymbol.GetAttributes()
                .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, enumOptionAttribute));

            if (enumOptionAttr == null)
                continue;

            // Extract collection name and option name
            var collectionName = GetCollectionName(enumOptionAttr);
            var optionName = GetOptionName(enumOptionAttr, classSymbol);

            if (string.IsNullOrEmpty(collectionName))
            {
                // If no collection name is specified, we can't check for duplicates across collections
                continue;
            }

            // Add to tracking dictionary
            if (!optionsByCollection.TryGetValue(collectionName, out var list))
            {
                list = new List<(string, INamedTypeSymbol, AttributeData, Location)>();
                optionsByCollection[collectionName] = list;
            }

            // Get location from the first syntax reference
            var location = classSymbol.Locations.FirstOrDefault() ?? Location.None;
            list.Add((optionName, classSymbol, enumOptionAttr, location));
        }

        // Check for duplicates within each collection
        foreach (var collection in optionsByCollection)
        {
            var collectionName = collection.Key;
            var options = collection.Value;

            // Group by option name to find duplicates
            var duplicates = options
                .GroupBy(o => o.OptionName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var duplicateGroup in duplicates)
            {
                // Report diagnostic for all but the first occurrence
                var first = duplicateGroup.First();
                foreach (var duplicate in duplicateGroup.Skip(1))
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        duplicate.Location,
                        duplicate.OptionName,
                        collectionName,
                        first.Type.Name);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static string GetCollectionName(AttributeData attribute)
    {
        // Check constructor arguments
        if (attribute.ConstructorArguments.Length > 0)
        {
            var firstArg = attribute.ConstructorArguments[0];
            if (firstArg.Value is string collectionName)
                return collectionName;
        }

        // Check named arguments
        var collectionNameArg = attribute.NamedArguments
            .FirstOrDefault(arg => string.Equals(arg.Key, "CollectionName", StringComparison.Ordinal));

        if (collectionNameArg.Value.Value is string namedCollectionName)
            return namedCollectionName;

        return string.Empty;
    }

    private static string GetOptionName(AttributeData attribute, INamedTypeSymbol classSymbol)
    {
        // Check for explicit name in attribute
        var nameArg = attribute.NamedArguments
            .FirstOrDefault(arg => string.Equals(arg.Key, "Name", StringComparison.Ordinal));

        if (nameArg.Value.Value is string explicitName && !string.IsNullOrEmpty(explicitName))
            return explicitName;

        // Check constructor arguments for name (second parameter if collection name is first)
        if (attribute.ConstructorArguments.Length > 1)
        {
            var secondArg = attribute.ConstructorArguments[1];
            if (secondArg.Value is string constructorName && !string.IsNullOrEmpty(constructorName))
                return constructorName;
        }

        // Use class name without "Type" suffix as default
        var className = classSymbol.Name;
        if (className.EndsWith("Type", StringComparison.Ordinal) && className.Length > 4)
        {
            return className.Substring(0, className.Length - 4);
        }

        return className;
    }
    
    private static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(Compilation compilation)
    {
        var stack = new Stack<INamespaceOrTypeSymbol>();
        stack.Push(compilation.GlobalNamespace);
        
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            
            if (current is INamespaceSymbol ns)
            {
                foreach (var member in ns.GetMembers())
                {
                    if (member is INamespaceOrTypeSymbol nsOrType)
                        stack.Push(nsOrType);
                }
            }
            else if (current is INamedTypeSymbol type)
            {
                yield return type;
                
                // Add nested types
                foreach (var nestedType in type.GetTypeMembers())
                {
                    stack.Push(nestedType);
                }
            }
        }
    }
}