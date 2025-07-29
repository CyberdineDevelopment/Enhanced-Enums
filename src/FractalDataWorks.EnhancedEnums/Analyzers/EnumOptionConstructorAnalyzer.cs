using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FractalDataWorks.EnhancedEnums.Analyzers;

/// <summary>
/// Analyzer that ensures enum options have a public parameterless constructor when factory methods are not generated.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnumOptionConstructorAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for missing parameterless constructor.
    /// </summary>
    public const string DiagnosticId = "ENH005";

    private static readonly LocalizableString Title = "Missing public parameterless constructor";
    private static readonly LocalizableString MessageFormat = "Enum option '{0}' must have a public parameterless constructor when factory method generation is disabled";
    private static readonly LocalizableString Description = "Enhanced enum options require a public parameterless constructor for instantiation unless factory method generation is enabled.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration);
    }

    private static void AnalyzeType(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
        if (typeSymbol == null) return;

        // Check if this type has EnumOption attribute
        var enumOptionAttr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) || 
                                string.Equals(a.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal));
        
        if (enumOptionAttr == null) return;

        // Check if factory method generation is disabled at the option level
        var generateFactoryMethod = GetGenerateFactoryMethodValue(enumOptionAttr);
        
        // If explicitly enabled at option level, no check needed
        if (generateFactoryMethod == true) return;

        // Find the base type and check collection settings
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            // Check if base has EnumCollection attribute
            var collectionAttrs = baseType.GetAttributes()
                .Where(a => string.Equals(a.AttributeClass?.Name, "EnumCollectionAttribute", StringComparison.Ordinal) || 
                           string.Equals(a.AttributeClass?.Name, "EnumCollection", StringComparison.Ordinal))
                .ToList();

            if (collectionAttrs.Count > 0)
            {
                // If generateFactoryMethod is null (not specified), use collection setting
                if (generateFactoryMethod == null)
                {
                    // Check if ANY collection has factory methods enabled (default is true)
                    var anyCollectionHasFactoryMethods = false;
                    
                    foreach (var collectionAttr in collectionAttrs)
                    {
                        var collectionGeneratesFactoryMethods = GetGenerateFactoryMethodsValue(collectionAttr);
                        if (collectionGeneratesFactoryMethods != false) // null or true
                        {
                            anyCollectionHasFactoryMethods = true;
                            break;
                        }
                    }

                    // If at least one collection generates factory methods, no check needed
                    if (anyCollectionHasFactoryMethods) return;
                }
                
                break;
            }

            baseType = baseType.BaseType;
        }

        // At this point, factory method generation is disabled
        // Check for public parameterless constructor
        var hasPublicParameterlessConstructor = typeSymbol.Constructors
            .Any(c => !c.IsStatic && 
                     c.DeclaredAccessibility == Accessibility.Public && 
                     c.Parameters.Length == 0);

        if (!hasPublicParameterlessConstructor)
        {
            // Report diagnostic
            var diagnostic = Diagnostic.Create(
                Rule,
                typeDeclaration.Identifier.GetLocation(),
                typeSymbol.Name);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool? GetGenerateFactoryMethodValue(AttributeData attr)
    {
        foreach (var arg in attr.NamedArguments)
        {
            if (string.Equals(arg.Key, "GenerateFactoryMethod", StringComparison.Ordinal) && 
                arg.Value.Value is bool value)
            {
                return value;
            }
        }
        return null;
    }

    private static bool? GetGenerateFactoryMethodsValue(AttributeData attr)
    {
        foreach (var arg in attr.NamedArguments)
        {
            if (string.Equals(arg.Key, "GenerateFactoryMethods", StringComparison.Ordinal) && 
                arg.Value.Value is bool value)
            {
                return value;
            }
        }
        return null; // Default is true in the generator
    }
}