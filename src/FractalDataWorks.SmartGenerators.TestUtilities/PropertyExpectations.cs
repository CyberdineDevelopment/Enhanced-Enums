using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides fluent assertions and expectations for a <see cref="PropertyDeclarationSyntax"/> node.
/// </summary>
/// <remarks>
/// All methods throw <see cref="ShouldAssertException"/> on failure and return <see cref="PropertyExpectations"/> for method chaining.
/// </remarks>
public class PropertyExpectations
{
    private readonly PropertyDeclarationSyntax _propertyDeclaration;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyExpectations"/> class.
    /// </summary>
    /// <param name="propertyDeclaration">The property declaration to verify.</param>
    public PropertyExpectations(PropertyDeclarationSyntax propertyDeclaration)
    {
        _propertyDeclaration = propertyDeclaration ?? throw new ArgumentNullException(nameof(propertyDeclaration));
    }

    /// <summary>
    /// Asserts that the property is of the specified type.
    /// </summary>
    /// <param name="typeName">The expected type name of the property.</param>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the property type does not match.</exception>
    public PropertyExpectations HasType(string typeName)
    {
        // Verify the property has the expected type
        if (!TypeComparer.AreEquivalent(_propertyDeclaration.Type, typeName))
        {
            throw new ShouldAssertException($"Property '{_propertyDeclaration.Identifier.Text}' has type '{_propertyDeclaration.Type}', but expected '{typeName}'.");
        }

        return this;
    }

    /// <summary>
    /// Asserts that the property has a getter accessor.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the getter is not present.</exception>
    public PropertyExpectations HasGetter()
    {
        // Check if there's an accessor list with a get accessor
        var hasAccessorListGetter = _propertyDeclaration.AccessorList != null &&
            _propertyDeclaration.AccessorList.Accessors.Any(a => string.Equals(a.Keyword.ValueText, "get", StringComparison.Ordinal));

        // Expression-bodied properties implicitly have a getter
        var hasExpressionBody = _propertyDeclaration.ExpressionBody != null;

        var hasGetter = hasAccessorListGetter || hasExpressionBody;

        hasGetter.ShouldBeTrue($"Property '{_propertyDeclaration.Identifier.Text}' does not have a getter.");
        return this;
    }

    /// <summary>
    /// Asserts that the property has a setter accessor.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the setter is not present.</exception>
    public PropertyExpectations HasSetter()
    {
        // Check if there's an accessor list with a set accessor
        var hasSetter = _propertyDeclaration.AccessorList != null &&
            _propertyDeclaration.AccessorList.Accessors.Any(a => string.Equals(a.Keyword.ValueText, "set", StringComparison.Ordinal));

        hasSetter.ShouldBeTrue($"Property '{_propertyDeclaration.Identifier.Text}' does not have a setter.");
        return this;
    }

    /// <summary>
    /// Asserts that the property has no setter accessor (read-only property).
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if a setter is present.</exception>
    public PropertyExpectations HasNoSetter()
    {
        var setter = _propertyDeclaration.AccessorList?.Accessors
            .FirstOrDefault(a => string.Equals(a.Keyword.ValueText, "set", StringComparison.Ordinal) || string.Equals(a.Keyword.ValueText, "init", StringComparison.Ordinal));

        setter.ShouldBeNull($"Property '{_propertyDeclaration.Identifier.Text}' has a setter, but expected to be read-only.");
        return this;
    }

    /// <summary>
    /// Asserts that the property is read-only (has no setter).
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if a setter is present.</exception>
    public PropertyExpectations IsReadOnly()
    {
        return HasNoSetter();
    }

    /// <summary>
    /// Asserts that the property has a private setter.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the setter is not private or not present.</exception>
    public PropertyExpectations HasPrivateSetter()
    {
        var setter = _propertyDeclaration.AccessorList?.Accessors
            .FirstOrDefault(a => string.Equals(a.Keyword.ValueText, "set", StringComparison.Ordinal));

        setter.ShouldNotBeNull($"Property '{_propertyDeclaration.Identifier.Text}' does not have a setter.");

        var hasPrivateModifier = setter.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));
        hasPrivateModifier.ShouldBeTrue($"Property '{_propertyDeclaration.Identifier.Text}' setter is not private.");

        return this;
    }

    /// <summary>
    /// Asserts that the property has an init accessor.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the init accessor is not present.</exception>
    public PropertyExpectations HasInitSetter()
    {
        var hasInit = _propertyDeclaration.AccessorList != null &&
            _propertyDeclaration.AccessorList.Accessors.Any(a => string.Equals(a.Keyword.ValueText, "init", StringComparison.Ordinal));

        hasInit.ShouldBeTrue($"Property '{_propertyDeclaration.Identifier.Text}' does not have an init accessor.");
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with the <c>public</c> modifier.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public PropertyExpectations IsPublic()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.PublicKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with the <c>static</c> modifier.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public PropertyExpectations IsStatic()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.StaticKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with the <c>override</c> modifier.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public PropertyExpectations IsOverride()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.OverrideKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with the <c>virtual</c> modifier.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public PropertyExpectations IsVirtual()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.VirtualKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with the <c>abstract</c> modifier.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public PropertyExpectations IsAbstract()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.AbstractKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property has XML documentation containing the specified content.
    /// </summary>
    /// <param name="xmlDocContent">The expected XML documentation content.</param>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the XML documentation is missing or does not contain the expected content.</exception>
    public PropertyExpectations HasXmlDocs(string xmlDocContent)
    {
        var syntaxWithStructuredTrivia = _propertyDeclaration.SyntaxTree.GetRoot().DescendantNodes().FirstOrDefault(n => n == _propertyDeclaration);
        syntaxWithStructuredTrivia.ShouldNotBeNull("Could not find property declaration with structured trivia");

        var leadingTrivia = syntaxWithStructuredTrivia.GetLeadingTrivia();
        var xmlComments = leadingTrivia
            .Where(t => t.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineDocumentationCommentTrivia))
            .SelectMany(t => t.GetStructure()
                ?.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextSyntax>()
                .SelectMany(x => x.TextTokens.Select(tt => tt.ToString().Trim())))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        xmlComments.ShouldContain(
            xmlDocContent,
            $"Expected property '{_propertyDeclaration.Identifier}' to have XML documentation containing '{xmlDocContent}'");

        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with the <c>private</c> modifier.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public PropertyExpectations IsPrivate()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.PrivateKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with the <c>protected</c> modifier.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public PropertyExpectations IsProtected()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with the <c>internal</c> modifier.
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the modifier is not present.</exception>
    public PropertyExpectations IsInternal()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.InternalKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with both <c>protected</c> and <c>internal</c> modifiers (protected internal).
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if either modifier is not present.</exception>
    public PropertyExpectations IsProtectedInternal()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.InternalKeyword);
        return this;
    }

    /// <summary>
    /// Asserts that the property is declared with both <c>private</c> and <c>protected</c> modifiers (private protected).
    /// </summary>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    /// <exception cref="ShouldAssertException">Thrown if either modifier is not present.</exception>
    public PropertyExpectations IsPrivateProtected()
    {
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.PrivateKeyword);
        _propertyDeclaration.ShouldHaveModifier(SyntaxKind.ProtectedKeyword);
        return this;
    }

    /// <summary>
    /// Expects the property to have the specified modifiers.
    /// </summary>
    /// <param name="modifiers">The modifiers to check for.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public PropertyExpectations HasModifiers(params SyntaxKind[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _propertyDeclaration.ShouldHaveModifier(modifier);
        }

        return this;
    }

    /// <summary>
    /// Expects the property to be an auto-property (with no explicit backing field or custom accessors).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public PropertyExpectations IsAutoProperty()
    {
        // Check if the property has an accessor list
        _propertyDeclaration.AccessorList.ShouldNotBeNull(
            $"Property '{_propertyDeclaration.Identifier}' has no accessor list.");

        // Check if the accessors have no bodies (which would make them non-auto properties)
        var accessors = _propertyDeclaration.AccessorList.Accessors;
        var nonAutoAccessors = accessors.Where(a => a.Body != null || a.ExpressionBody != null).ToList();

        nonAutoAccessors.Count.ShouldBe(
            0,
            $"Property '{_propertyDeclaration.Identifier}' has {nonAutoAccessors.Count} accessor(s) with bodies, making it not an auto-property.");

        return this;
    }

    /// <summary>
    /// Asserts that the property has the specified name.
    /// </summary>
    /// <param name="name">The expected name of the property.</param>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    public PropertyExpectations HasName(string name)
    {
        _propertyDeclaration.Identifier.ValueText
            .ShouldBe(name, $"Expected property name to be '{name}' but was '{_propertyDeclaration.Identifier.ValueText}'");
        return this;
    }

    /// <summary>
    /// Asserts that the property getter matches the specified code.
    /// </summary>
    /// <param name="expected">The expected code for the getter.</param>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    public PropertyExpectations HasGetter(string expected)
    {
        var getter = _propertyDeclaration.AccessorList?.Accessors
            .FirstOrDefault(a => string.Equals(a.Keyword.ValueText, "get", StringComparison.Ordinal));
        getter.ShouldNotBeNull($"Expected property '{_propertyDeclaration.Identifier}' to have a getter");
        getter.ToString().Trim().ShouldBe(expected, $"Expected getter to be '{expected}' but was '{getter}'");
        return this;
    }

    /// <summary>
    /// Asserts that the property setter matches the specified code.
    /// </summary>
    /// <param name="expected">The expected code for the setter.</param>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    public PropertyExpectations HasSetter(string expected)
    {
        var setter = _propertyDeclaration.AccessorList?.Accessors
            .FirstOrDefault(a => string.Equals(a.Keyword.ValueText, "set", StringComparison.Ordinal));
        setter.ShouldNotBeNull($"Expected property '{_propertyDeclaration.Identifier}' to have a setter");
        setter.ToString().Trim().ShouldBe(expected, $"Expected setter to be '{expected}' but was '{setter}'");
        return this;
    }

    /// <summary>
    /// Asserts that the property has an expression-bodied member with the specified expression.
    /// </summary>
    /// /// <param name="expression">The expected code for the expression body.</param>
    /// <returns>This <see cref="PropertyExpectations"/> instance for method chaining.</returns>
    public PropertyExpectations HasExpressionBody(string expression)
    {
        var arrow = _propertyDeclaration.ExpressionBody;
        arrow.ShouldNotBeNull($"Property '{_propertyDeclaration.Identifier}' does not have an expression body.");
        arrow.Expression.ToString().ShouldBe(expression, $"Expected expression body '{expression}' but was '{arrow.Expression}'");
        return this;
    }
}
