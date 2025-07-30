using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FractalDataWorks.EnhancedEnums.Analyzers;

/// <summary>
/// Analyzer that ensures EnumCollection attribute has CollectionName specified and validates inheritance.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnumCollectionAttributeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for missing CollectionName in EnumCollection attribute.
    /// </summary>
    public const string MissingCollectionNameDiagnosticId = "ENH008";
    
    /// <summary>
    /// Diagnostic ID for EnumCollection classes not inheriting from EnumOptionBase&lt;T&gt;.
    /// </summary>
    public const string MissingInheritanceDiagnosticId = "ENH009";

    private static readonly LocalizableString MissingCollectionNameTitle = "EnumCollection attribute must specify CollectionName";
    private static readonly LocalizableString MissingCollectionNameMessageFormat = "EnumCollection attribute on class '{0}' must explicitly specify the CollectionName parameter";
    private static readonly LocalizableString MissingCollectionNameDescription = "Enhanced enum collection classes must explicitly specify the CollectionName in the EnumCollection attribute for clarity and consistency.";

    private static readonly LocalizableString MissingInheritanceTitle = "EnumCollection class must inherit from EnumOptionBase<T>";
    private static readonly LocalizableString MissingInheritanceMessageFormat = "Class '{0}' with EnumCollection attribute must inherit from EnumOptionBase<T>";
    private static readonly LocalizableString MissingInheritanceDescription = "Classes marked with EnumCollection attribute must inherit from EnumOptionBase<T> to provide the required Id and Name properties.";

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor MissingCollectionNameRule = new DiagnosticDescriptor(
        MissingCollectionNameDiagnosticId,
        MissingCollectionNameTitle,
        MissingCollectionNameMessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: MissingCollectionNameDescription);

    private static readonly DiagnosticDescriptor MissingInheritanceRule = new DiagnosticDescriptor(
        MissingInheritanceDiagnosticId,
        MissingInheritanceTitle,
        MissingInheritanceMessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: MissingInheritanceDescription);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(MissingCollectionNameRule, MissingInheritanceRule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol == null)
            return;

        // Find EnumCollection attributes
        var enumCollectionAttributes = classSymbol.GetAttributes()
            .Where(attr => string.Equals(attr.AttributeClass?.Name, "EnumCollectionAttribute", StringComparison.Ordinal) ||
                          string.Equals(attr.AttributeClass?.Name, "EnumCollection", StringComparison.Ordinal))
            .ToList();

        if (enumCollectionAttributes.Count == 0)
            return;

        // Check each EnumCollection attribute
        foreach (var attr in enumCollectionAttributes)
        {
            // Check if CollectionName is specified
            if (!HasCollectionNameSpecified(attr))
            {
                var diagnostic = Diagnostic.Create(
                    MissingCollectionNameRule,
                    classDeclaration.Identifier.GetLocation(),
                    classSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        // Check inheritance from EnumOptionBase<T>
        if (!InheritsFromEnumOptionBase(classSymbol))
        {
            var diagnostic = Diagnostic.Create(
                MissingInheritanceRule,
                classDeclaration.Identifier.GetLocation(),
                classSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasCollectionNameSpecified(AttributeData attr)
    {
        // Check constructor arguments first (primary way to specify CollectionName)
        if (attr.ConstructorArguments.Length > 0)
        {
            var firstArg = attr.ConstructorArguments[0];
            if (firstArg.Value is string collectionName && !string.IsNullOrEmpty(collectionName))
            {
                return true;
            }
        }

        // Check named arguments as fallback
        foreach (var namedArg in attr.NamedArguments)
        {
            if (string.Equals(namedArg.Key, "CollectionName", StringComparison.Ordinal) &&
                namedArg.Value.Value is string namedCollectionName && !string.IsNullOrEmpty(namedCollectionName))
            {
                return true;
            }
        }

        return false;
    }

    private static bool InheritsFromEnumOptionBase(INamedTypeSymbol classSymbol)
    {
        var current = classSymbol.BaseType;
        
        while (current != null)
        {
            // Check if this is EnumOptionBase<T>
            if (current.IsGenericType && 
                string.Equals(current.OriginalDefinition.Name, "EnumOptionBase", StringComparison.Ordinal))
            {
                // Verify it's from the correct namespace
                var namespaceName = current.OriginalDefinition.ContainingNamespace?.ToDisplayString();
                if (string.Equals(namespaceName, "FractalDataWorks", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            
            current = current.BaseType;
        }

        return false;
    }
}