using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using FractalDataWorks.EnhancedEnums.Analyzers;

namespace FractalDataWorks.EnhancedEnums.CodeFixes;

/// <summary>
/// Code fix provider that converts abstract properties to constructor-based pattern.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnhancedEnumConstructorCodeFixProvider)), Shared]
public class EnhancedEnumConstructorCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EnhancedEnumConstructorAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the class declaration identified by the diagnostic
        var classDeclaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration == null)
            return;

        // Register a code action that will invoke the fix
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to constructor-based pattern",
                createChangedDocument: c => ConvertToConstructorPatternAsync(context.Document, classDeclaration, c),
                equivalenceKey: nameof(EnhancedEnumConstructorCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> ConvertToConstructorPatternAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var classSymbol = semanticModel?.GetDeclaredSymbol(classDeclaration, cancellationToken);

        if (classSymbol == null)
            return document;

        // Find all abstract properties (excluding Name)
        var abstractProperties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.IsAbstract && !string.Equals(p.Name, "Name", System.StringComparison.Ordinal))
            .ToList();

        // Create constructor parameters
        var parameters = abstractProperties
            .Select(p => SyntaxFactory.Parameter(
                SyntaxFactory.Identifier(ToCamelCase(p.Name)))
                .WithType(SyntaxFactory.ParseTypeName(p.Type.ToDisplayString())))
            .ToList();

        // Add name parameter at the beginning if Name property is abstract
        var nameProperty = classSymbol.GetMembers("Name").OfType<IPropertySymbol>().FirstOrDefault(p => p.IsAbstract);
        if (nameProperty != null)
        {
            parameters.Insert(0, SyntaxFactory.Parameter(SyntaxFactory.Identifier("name"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))));
        }

        // Create constructor
        var constructor = SyntaxFactory.ConstructorDeclaration(classDeclaration.Identifier)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)))
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
            .WithBody(SyntaxFactory.Block(CreateConstructorBody(nameProperty != null, abstractProperties)))
            .WithLeadingTrivia(SyntaxFactory.TriviaList(
                SyntaxFactory.Trivia(
                    SyntaxFactory.DocumentationCommentTrivia(
                        SyntaxKind.SingleLineDocumentationCommentTrivia,
                        SyntaxFactory.List(new XmlNodeSyntax[]
                        {
                            SyntaxFactory.XmlText("/// "),
                            SyntaxFactory.XmlElement(
                                SyntaxFactory.XmlElementStartTag(SyntaxFactory.XmlName("summary")),
                                SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("summary")))
                                .WithContent(SyntaxFactory.SingletonList<XmlNodeSyntax>(
                                    SyntaxFactory.XmlText($"Initializes a new instance of the {classDeclaration.Identifier.Text} class."))),
                            SyntaxFactory.XmlText("\n")
                        })))))
            .NormalizeWhitespace()
            .WithAdditionalAnnotations(Formatter.Annotation);

        // Convert abstract properties to concrete read-only properties
        var propertiesToReplace = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
            .ToList();

        foreach (var property in propertiesToReplace)
        {
            var propertySymbol = semanticModel.GetDeclaredSymbol(property, cancellationToken);
            if (propertySymbol == null || (string.Equals(propertySymbol.Name, "Name", System.StringComparison.Ordinal) && nameProperty == null))
                continue;

            var newProperty = SyntaxFactory.PropertyDeclaration(property.Type, property.Identifier)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
                .WithLeadingTrivia(property.GetLeadingTrivia())
                .WithTrailingTrivia(property.GetTrailingTrivia())
                .WithAdditionalAnnotations(Formatter.Annotation);

            editor.ReplaceNode(property, newProperty);
        }

        // Add constructor to class
        var firstMember = classDeclaration.Members.FirstOrDefault();
        if (firstMember != null)
        {
            editor.InsertBefore(firstMember, constructor);
        }
        else
        {
            editor.AddMember(classDeclaration, constructor);
        }

        return editor.GetChangedDocument();
    }

    private static StatementSyntax[] CreateConstructorBody(bool hasName, System.Collections.Generic.List<IPropertySymbol> properties)
    {
        var statements = new System.Collections.Generic.List<StatementSyntax>();

        if (hasName)
        {
            statements.Add(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName("Name"),
                    SyntaxFactory.IdentifierName("name"))));
        }

        foreach (var property in properties)
        {
            statements.Add(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(property.Name),
                    SyntaxFactory.IdentifierName(ToCamelCase(property.Name)))));
        }

        return statements.ToArray();
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}