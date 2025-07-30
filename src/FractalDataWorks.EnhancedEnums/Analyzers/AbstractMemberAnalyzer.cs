using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FractalDataWorks.EnhancedEnums.Analyzers;

/// <summary>
/// Analyzer that warns about abstract properties and fields in enhanced enum base classes.
/// Suggests using virtual properties with constructor initialization instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AbstractMemberAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for abstract properties.
    /// </summary>
    public const string AbstractPropertyDiagnosticId = "ENH006";
    
    /// <summary>
    /// Diagnostic ID for abstract fields.
    /// </summary>
    public const string AbstractFieldDiagnosticId = "ENH007";

    private static readonly LocalizableString AbstractPropertyTitle = "Abstract property in enhanced enum";
    private static readonly LocalizableString AbstractPropertyMessageFormat = "Property '{0}' should be virtual and initialized via constructor instead of abstract";
    private static readonly LocalizableString AbstractPropertyDescription = "Enhanced enum values should initialize all properties through constructors. Use virtual properties instead of abstract to allow constructor initialization.";
    
    private static readonly LocalizableString AbstractFieldTitle = "Abstract field in enhanced enum";
    private static readonly LocalizableString AbstractFieldMessageFormat = "Field '{0}' cannot be abstract. Use a virtual property instead";
    private static readonly LocalizableString AbstractFieldDescription = "Fields cannot be abstract in C#. Use virtual properties initialized via constructor.";
    
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor AbstractPropertyRule = new DiagnosticDescriptor(
        AbstractPropertyDiagnosticId,
        AbstractPropertyTitle,
        AbstractPropertyMessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: AbstractPropertyDescription);
    
    private static readonly DiagnosticDescriptor AbstractFieldRule = new DiagnosticDescriptor(
        AbstractFieldDiagnosticId,
        AbstractFieldTitle,
        AbstractFieldMessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: AbstractFieldDescription);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(AbstractPropertyRule, AbstractFieldRule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        
        // Check if property is abstract
        if (!propertyDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            return;
        
        // Check if the containing type has EnumCollection attribute
        var containingType = GetContainingType(propertyDeclaration);
        if (containingType == null)
            return;
            
        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(containingType);
        if (typeSymbol == null)
            return;
            
        if (!HasEnumCollectionAttribute(typeSymbol))
            return;
        
        // Report diagnostic
        var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
        if (propertySymbol != null)
        {
            var diagnostic = Diagnostic.Create(
                AbstractPropertyRule,
                propertyDeclaration.Identifier.GetLocation(),
                propertySymbol.Name);
            
            context.ReportDiagnostic(diagnostic);
        }
    }
    
    private static void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        
        // Fields can't actually be abstract in C#, but check for abstract modifier anyway
        if (!fieldDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            return;
        
        // Check if the containing type has EnumCollection attribute
        var containingType = GetContainingType(fieldDeclaration);
        if (containingType == null)
            return;
            
        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(containingType);
        if (typeSymbol == null)
            return;
            
        if (!HasEnumCollectionAttribute(typeSymbol))
            return;
        
        // Report diagnostic for each variable
        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            var diagnostic = Diagnostic.Create(
                AbstractFieldRule,
                variable.Identifier.GetLocation(),
                variable.Identifier.Text);
            
            context.ReportDiagnostic(diagnostic);
        }
    }
    
    private static TypeDeclarationSyntax? GetContainingType(SyntaxNode node)
    {
        var parent = node.Parent;
        while (parent != null)
        {
            if (parent is TypeDeclarationSyntax typeDeclaration)
                return typeDeclaration;
            parent = parent.Parent;
        }
        return null;
    }
    
    private static bool HasEnumCollectionAttribute(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes()
            .Any(a => string.Equals(a.AttributeClass?.Name, "EnumCollectionAttribute", StringComparison.Ordinal) || 
                     string.Equals(a.AttributeClass?.Name, "EnumCollection", StringComparison.Ordinal));
    }
}