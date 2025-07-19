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

namespace FractalDataWorks.EnhancedEnums.Analyzers;

/// <summary>
/// Code fix provider that implements IEnhancedEnumOption on enhanced enum base classes.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnhancedEnumBaseCodeFixProvider)), Shared]
public class EnhancedEnumBaseCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EnhancedEnumBaseAnalyzer.DiagnosticId);

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
                title: "Implement IEnhancedEnumOption",
                createChangedDocument: c => ImplementIEnhancedEnumOptionAsync(context.Document, classDeclaration, c),
                equivalenceKey: nameof(EnhancedEnumBaseCodeFixProvider)),
            diagnostic);
    }

    private async Task<Document> ImplementIEnhancedEnumOptionAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var generator = editor.Generator;
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol == null)
            return document;

        // Add interface to base list
        var interfaceTypeSyntax = SyntaxFactory.SimpleBaseType(
            SyntaxFactory.QualifiedName(
                SyntaxFactory.IdentifierName("FractalDataWorks"),
                SyntaxFactory.IdentifierName("IEnhancedEnumOption")));

        var newClassDeclaration = classDeclaration;

        // Add interface to base list
        if (classDeclaration.BaseList == null)
        {
            newClassDeclaration = classDeclaration.WithBaseList(
                SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(interfaceTypeSyntax)));
        }
        else
        {
            newClassDeclaration = classDeclaration.WithBaseList(
                classDeclaration.BaseList.AddTypes(interfaceTypeSyntax));
        }

        // Check if Name property already exists
        var hasNameProperty = classSymbol.GetMembers("Name").OfType<IPropertySymbol>().Any();
        
        // Check if Id property already exists
        var hasIdProperty = classSymbol.GetMembers("Id").OfType<IPropertySymbol>().Any();

        // Add missing properties
        var members = newClassDeclaration.Members.ToList();

        if (!hasNameProperty)
        {
            // Add abstract Name property
            var nameProperty = SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                    SyntaxFactory.Identifier("Name"))
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.AbstractKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
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
                                        SyntaxFactory.XmlText("Gets the name of this enum option."))),
                                SyntaxFactory.XmlText("\n")
                            })))))
                .NormalizeWhitespace()
                .WithAdditionalAnnotations(Formatter.Annotation);

            members.Insert(0, nameProperty);
        }

        if (!hasIdProperty)
        {
            // Add Id property with default implementation
            var idProperty = SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                    SyntaxFactory.Identifier("Id"))
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.VirtualKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithExpressionBody(
                                SyntaxFactory.ArrowExpressionClause(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.CoalesceExpression,
                                        SyntaxFactory.ConditionalAccessExpression(
                                            SyntaxFactory.IdentifierName("Name"),
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberBindingExpression(
                                                    SyntaxFactory.IdentifierName("GetHashCode")))),
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(0)))))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
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
                                        SyntaxFactory.XmlText("Gets the unique identifier for this enum option."))),
                                SyntaxFactory.XmlText("\n")
                            })))))
                .NormalizeWhitespace()
                .WithAdditionalAnnotations(Formatter.Annotation);

            members.Add(idProperty);
        }

        newClassDeclaration = newClassDeclaration.WithMembers(SyntaxFactory.List(members));

        // Replace the old class declaration with the new one
        editor.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using directive if needed
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var compilationUnit = root as CompilationUnitSyntax;
        if (compilationUnit != null)
        {
            var hasUsing = compilationUnit.Usings.Any(u => u.Name?.ToString() == "FractalDataWorks");
            if (!hasUsing)
            {
                var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("FractalDataWorks"))
                    .WithAdditionalAnnotations(Formatter.Annotation);
                var lastUsing = compilationUnit.Usings.LastOrDefault();
                if (lastUsing != null)
                {
                    editor.InsertAfter(lastUsing, newUsing);
                }
                else if (compilationUnit.Members.FirstOrDefault() is { } firstMember)
                {
                    editor.InsertBefore(firstMember, newUsing);
                }
            }
        }

        return editor.GetChangedDocument();
    }
}