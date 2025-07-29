using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides fluent assertions and expectations for a <see cref="FieldDeclarationSyntax"/> node.
/// </summary>
/// <remarks>
/// All methods throw <see cref="ShouldAssertException"/> on failure and return <see cref="FieldExpectations"/> for method chaining.
/// </remarks>
public class FieldExpectations
{
    private readonly FieldDeclarationSyntax _fieldDeclaration;
    private readonly VariableDeclaratorSyntax _variable;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldExpectations"/> class.
    /// </summary>
    /// <param name="fieldDeclaration">The field declaration to verify.</param>
    public FieldExpectations(FieldDeclarationSyntax fieldDeclaration)
    {
        _fieldDeclaration = fieldDeclaration ?? throw new ArgumentNullException(nameof(fieldDeclaration));

        // Get the variable declarator for this field
        _variable = fieldDeclaration.Declaration.Variables.First();
        _ = _variable.ShouldNotBeNull($"Field declaration should have at least one variable.");
    }

    /// <summary>
    /// Asserts that the field is declared with the <c>private</c> modifier.
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public FieldExpectations IsPrivate()
    {
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.PrivateKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the field is declared with the <c>protected</c> modifier.
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public FieldExpectations IsProtected()
    {
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the field is declared with the <c>public</c> modifier.
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public FieldExpectations IsPublic()
    {
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.PublicKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the field is declared with the <c>readonly</c> modifier.
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public FieldExpectations IsReadOnly()
    {
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.ReadOnlyKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the field is declared with the <c>static</c> modifier.
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public FieldExpectations IsStatic()
    {
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.StaticKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the field is of the specified type.
    /// </summary>
    /// <param name="typeName">The expected type name of the field.</param>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the field type does not match.</exception>
    public FieldExpectations HasType(string typeName)
    {
        if (!TypeComparer.AreEquivalent(_fieldDeclaration.Declaration.Type, typeName))
        {
            throw new ShouldAssertException($"Expected field '{_variable.Identifier}' to have type '{typeName}' but was '{_fieldDeclaration.Declaration.Type}'");
        }
        return this;
    }

    /// <summary>
    /// Asserts that the field has the specified initializer expression.
    /// </summary>
    /// <param name="initializer">The expected initializer expression as a string.</param>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the initializer is missing or does not match.</exception>
    public FieldExpectations HasInitializer(string initializer)
    {
        _ = _variable.Initializer.ShouldNotBeNull($"Expected field '{_variable.Identifier}' to have an initializer.");
        var initializerText = _variable.Initializer.Value.ToString();
        initializerText.ShouldBe(initializer, $"Expected field '{_variable.Identifier}' to be initialized with '{initializer}' but was '{initializerText}'");
        return this;
    }

    /// <summary>
    /// Asserts that the field has an initializer.
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the initializer is missing.</exception>
    public FieldExpectations HasInitializer()
    {
        _ = _variable.Initializer.ShouldNotBeNull($"Expected field '{_variable.Identifier}' to have an initializer.");
        return this;
    }

    /// <summary>
    /// Asserts that the field does not have an initializer.
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the initializer is present.</exception>
    public FieldExpectations HasNoInitializer()
    {
        _variable.Initializer.ShouldBeNull($"Expected field '{_variable.Identifier}' not to have an initializer.");
        return this;
    }

    /// <summary>
    /// Asserts that the field is declared with the <c>internal</c> modifier.
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public FieldExpectations IsInternal()
    {
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.InternalKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the field is declared with both <c>protected</c> and <c>internal</c> modifiers (protected internal).
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if either modifier is not present.</exception>
    public FieldExpectations IsProtectedInternal()
    {
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.InternalKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the field is declared with both <c>private</c> and <c>protected</c> modifiers (private protected).
    /// </summary>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if either modifier is not present.</exception>
    public FieldExpectations IsPrivateProtected()
    {
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.PrivateKeyword);
        _ = _fieldDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the field is declared with all specified modifiers.
    /// </summary>
    /// <param name="modifiers">The modifiers to check for on the field declaration.</param>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if any modifier is not present.</exception>
    public FieldExpectations HasModifiers(params SyntaxKind[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _ = _fieldDeclaration.ShouldHaveModifier(modifier);
        }

        return this;
    }

    /// <summary>
    /// Asserts that the field has the specified XML documentation summary.
    /// </summary>
    /// <param name="expectedSummary">The expected summary text.</param>
    /// <returns>This <see cref="FieldExpectations"/> instance for chaining.</returns>
    public FieldExpectations HasXmlDocSummary(string expectedSummary)
    {
        var docTrivia = _fieldDeclaration.GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();
        docTrivia.ShouldNotBeNull($"Field '{_variable.Identifier}' does not have XML documentation.");
        var summaryElement = docTrivia.ChildNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(e => string.Equals(e.StartTag.Name.LocalName.Text, "summary", StringComparison.Ordinal));
        _ = summaryElement.ShouldNotBeNull($"Field '{_variable.Identifier}' does not have a <summary> doc element.");
        var actualText = string.Concat(summaryElement.Content
            .OfType<XmlTextSyntax>()
            .SelectMany(x => x.TextTokens)
            .Select(t => t.Text.Trim()));
        actualText.ShouldBe(expectedSummary, $"Expected XML doc summary to be '{expectedSummary}' but was '{actualText}'.");
        return this;
    }

    /// <summary>
    /// Asserts that the field has the specified name.
    /// </summary>
    /// <param name="name">The expected name of the field.</param>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    public FieldExpectations HasName(string name)
    {
        _variable.Identifier.ValueText.ShouldBe(name, $"Expected field name to be '{name}' but was '{_variable.Identifier.ValueText}'");
        return this;
    }

    /// <summary>
    /// Asserts that the field has the specified modifier.
    /// </summary>
    /// <param name="modifier">The modifier to check for (e.g., "public", "static", "readonly").</param>
    /// <returns>This <see cref="FieldExpectations"/> instance for method chaining.</returns>
    public FieldExpectations HasModifier(string modifier)
    {
        _fieldDeclaration.Modifiers.Any(m => string.Equals(m.ValueText, modifier, StringComparison.Ordinal))
            .ShouldBeTrue($"Expected field '{_variable.Identifier}' to have modifier '{modifier}'");
        return this;
    }
}
