using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FractalDataWorks.EnhancedEnums.Analyzers;

/// <summary>
/// Analyzer that enforces IEnhancedEnumOption implementation on enhanced enum base classes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnhancedEnumBaseAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ENH1001";

    private static readonly LocalizableString Title = "Enhanced enum base class should implement IEnhancedEnumOption";
    private static readonly LocalizableString MessageFormat = "Enhanced enum base class '{0}' should implement IEnhancedEnumOption for full functionality";
    private static readonly LocalizableString Description = "Enhanced enum base classes should implement IEnhancedEnumOption to enable features like GetById generation and proper interface-based return types.";
    private const string Category = "Usage";

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

        if (classSymbol == null)
            return;

        // Check if class has [EnhancedEnumBase] attribute
        var hasEnhancedEnumBaseAttribute = classSymbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name == "EnhancedEnumBaseAttribute" || 
                         attr.AttributeClass?.Name == "EnhancedEnumBase");

        if (!hasEnhancedEnumBaseAttribute)
            return;

        // Check if class implements IEnhancedEnumOption
        var enhancedEnumOptionInterface = context.Compilation.GetTypeByMetadataName("FractalDataWorks.IEnhancedEnumOption");
        if (enhancedEnumOptionInterface == null)
        {
            // If the interface isn't available in the compilation, we can't check
            return;
        }

        var implementsInterface = classSymbol.AllInterfaces.Contains(enhancedEnumOptionInterface, SymbolEqualityComparer.Default);

        if (!implementsInterface)
        {
            // Report diagnostic
            var diagnostic = Diagnostic.Create(
                Rule,
                classDeclaration.Identifier.GetLocation(),
                classSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}