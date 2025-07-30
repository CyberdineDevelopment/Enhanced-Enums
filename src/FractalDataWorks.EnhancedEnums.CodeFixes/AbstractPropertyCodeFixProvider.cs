using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.CodeFixes;

/// <summary>
/// Code fix provider that converts abstract properties to virtual properties with constructor initialization.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AbstractPropertyCodeFixProvider)), Shared]
public class AbstractPropertyCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => 
        ImmutableArray.Create(AbstractMemberAnalyzer.AbstractPropertyDiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => 
        WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the property declaration
        var propertyDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .First();

        if (propertyDeclaration == null) return;

        // Register code fix
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to virtual property with constructor parameter",
                createChangedDocument: c => ConvertToVirtualPropertyAsync(context.Document, propertyDeclaration, c),
                equivalenceKey: "ConvertToVirtualProperty"),
            diagnostic);
    }

    private async Task<Document> ConvertToVirtualPropertyAsync(
        Document document, 
        PropertyDeclarationSyntax propertyDeclaration, 
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null) return document;

        // Get the containing class
        var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDeclaration == null) return document;

        // Convert abstract to virtual
        var newModifiers = propertyDeclaration.Modifiers.Replace(
            propertyDeclaration.Modifiers.First(m => m.IsKind(SyntaxKind.AbstractKeyword)),
            SyntaxFactory.Token(SyntaxKind.VirtualKeyword));

        // Create auto-property with getter and protected setter
        var newProperty = propertyDeclaration
            .WithModifiers(newModifiers)
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List(new[]
                    {
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    })))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));

        // Find the constructor
        var constructor = classDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault(c => c.Modifiers.Any(SyntaxKind.ProtectedKeyword));

        if (constructor != null)
        {
            // Add parameter to constructor
            var propertyName = propertyDeclaration.Identifier.Text;
            var parameterName = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
            var propertyType = propertyDeclaration.Type;

            var newParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                .WithType(propertyType);

            var newParameterList = constructor.ParameterList.AddParameters(newParameter);
            var newConstructor = constructor.WithParameterList(newParameterList);

            // Add assignment to constructor body
            var assignment = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.IdentifierName(parameterName)))
                .WithLeadingTrivia(SyntaxFactory.Whitespace("        "))
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

            if (newConstructor.Body != null)
            {
                var newBody = newConstructor.Body.AddStatements(assignment);
                newConstructor = newConstructor.WithBody(newBody);
            }
            else if (newConstructor.ExpressionBody != null)
            {
                // Convert expression body to block body
                var existingExpression = SyntaxFactory.ExpressionStatement(newConstructor.ExpressionBody.Expression)
                    .WithLeadingTrivia(SyntaxFactory.Whitespace("        "))
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                
                var newBody = SyntaxFactory.Block(existingExpression, assignment);
                newConstructor = newConstructor
                    .WithExpressionBody(null)
                    .WithBody(newBody)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
            }

            // Replace the class with updated property and constructor
            var newClass = classDeclaration
                .ReplaceNode(propertyDeclaration, newProperty)
                .ReplaceNode(
                    classDeclaration.Members.First(m => m == constructor), 
                    newConstructor);

            var newRoot = root.ReplaceNode(classDeclaration, newClass);
            return document.WithSyntaxRoot(newRoot);
        }
        else
        {
            // Just replace the property if no constructor found
            var newRoot = root.ReplaceNode(propertyDeclaration, newProperty);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}