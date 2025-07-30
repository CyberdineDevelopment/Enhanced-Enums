using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FractalDataWorks.EnhancedEnums.Analyzers;

/// <summary>
/// Analyzer that enforces constructor-based patterns for enhanced enum base classes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnhancedEnumConstructorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ENH1002";

    private static readonly LocalizableString Title = "Enhanced enum base class should use constructor-based pattern";
    private static readonly LocalizableString MessageFormat = "Enhanced enum base class '{0}' uses abstract properties instead of constructor parameters. Consider using a constructor-based pattern for better immutability and Empty value generation.";
    private static readonly LocalizableString Description = "Enhanced enum base classes should use constructor parameters and read-only properties instead of abstract properties. This enables proper Empty value generation and follows immutability best practices.";
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol == null || !classSymbol.IsAbstract)
            return;

        // Check if class has [EnumOptionBase] attribute
        var hasEnhancedEnumBaseAttribute = classSymbol.GetAttributes()
            .Any(attr => string.Equals(attr.AttributeClass?.Name, "EnhancedEnumBaseAttribute", StringComparison.Ordinal) || 
                         string.Equals(attr.AttributeClass?.Name, "EnumOptionBase", StringComparison.Ordinal));

        if (!hasEnhancedEnumBaseAttribute)
            return;

        // Check for abstract properties (excluding Name which is required)
        var abstractProperties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.IsAbstract && !string.Equals(p.Name, "Name", StringComparison.Ordinal))
            .ToList();

        // Check if class has any constructors
        var hasConstructor = classSymbol.Constructors
            .Any(c => !c.IsImplicitlyDeclared && c.DeclaredAccessibility != Accessibility.Private);

        // If it has abstract properties but no constructor, report diagnostic
        if (abstractProperties.Count > 0 && !hasConstructor)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                classDeclaration.Identifier.GetLocation(),
                classSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}