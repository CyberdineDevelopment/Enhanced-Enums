using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides expectations for a class declaration.
/// </summary>
public class ClassExpectations
{
    private readonly ClassDeclarationSyntax _classDeclaration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassExpectations"/> class.
    /// </summary>
    /// <param name="classDeclaration">The class declaration to verify.</param>
    public ClassExpectations(ClassDeclarationSyntax classDeclaration)
    {
        _classDeclaration = classDeclaration ?? throw new ArgumentNullException(nameof(classDeclaration));
    }

    /// <summary>
    /// Expects the class to have a method with the specified name.
    /// </summary>
    /// <param name="methodName">The name of the method to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasMethod(string methodName)
    {
        _ = SyntaxNodeHelpers.FindMethod(_classDeclaration, methodName);
        return this;
    }

    /// <summary>
    /// Expects the class to have a method with the specified name and adds more expectations for that method.
    /// </summary>
    /// <param name="methodName">The name of the method to find.</param>
    /// <param name="methodExpectations">A callback to add expectations for the found method.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasMethod(string methodName, Action<MethodExpectations> methodExpectations)
    {
        var methodDecl = SyntaxNodeHelpers.FindMethod(_classDeclaration, methodName);
        var methodExp = new MethodExpectations(methodDecl);
        methodExpectations(methodExp);
        return this;
    }

    /// <summary>
    /// Expects the class to have a property with the specified name.
    /// </summary>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasProperty(string propertyName)
    {
        _ = SyntaxNodeHelpers.FindProperty(_classDeclaration, propertyName);
        return this;
    }

    /// <summary>
    /// Expects the class to have a property with the specified name and adds more expectations for that property.
    /// </summary>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <param name="propertyExpectations">A callback to add expectations for the found property.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasProperty(string propertyName, Action<PropertyExpectations> propertyExpectations)
    {
        var propertyDecl = SyntaxNodeHelpers.FindProperty(_classDeclaration, propertyName);
        var propertyExp = new PropertyExpectations(propertyDecl);
        propertyExpectations(propertyExp);
        return this;
    }

    /// <summary>
    /// Expects the class to have a field with the specified name.
    /// </summary>
    /// <param name="fieldName">The name of the field to find.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasField(string fieldName)
    {
        _ = SyntaxNodeHelpers.FindField(_classDeclaration, fieldName);
        return this;
    }

    /// <summary>
    /// Expects the class to have a field with the specified name and adds more expectations for that field.
    /// </summary>
    /// <param name="fieldName">The name of the field to find.</param>
    /// <param name="fieldExpectations">A callback to add expectations for the found field.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasField(string fieldName, Action<FieldExpectations> fieldExpectations)
    {
        var fieldDecl = SyntaxNodeHelpers.FindField(_classDeclaration, fieldName);
        var fieldExp = new FieldExpectations(fieldDecl);
        fieldExpectations(fieldExp);
        return this;
    }

    /// <summary>
    /// Expects the class to have an instance constructor matching the specified parameters.
    /// </summary>
    /// <param name="parameterTypes">The parameter types (e.g., "string, int" or empty string for parameterless).</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasConstructor(string parameterTypes)
    {
        return HasConstructor(parameterTypes, _ => { });
    }

    /// <summary>
    /// Expects the class to have an instance constructor.
    /// </summary>
    /// <param name="constructorExpectations">Additional constructor expectations.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasConstructor(Action<ConstructorExpectations> constructorExpectations)
    {
        var constructor = _classDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword));

        constructor.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have a constructor");

        var expectations = new ConstructorExpectations(constructor, _classDeclaration.Identifier.Text);
        constructorExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Expects the class to have an instance constructor with additional validation.
    /// </summary>
    /// <param name="parameterTypes">The parameter types (e.g., "string, int" or empty string for parameterless).</param>
    /// <param name="constructorExpectations">Additional constructor expectations.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasConstructor(string parameterTypes, Action<ConstructorExpectations> constructorExpectations)
    {
        if (constructorExpectations == null)
            throw new ArgumentNullException(nameof(constructorExpectations));

        var constructor = FindConstructorByParameters(_classDeclaration, parameterTypes);
        constructor.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have constructor with parameters ({parameterTypes})");

        var expectations = new ConstructorExpectations(constructor, _classDeclaration.Identifier.Text);
        constructorExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Expects the class to have a parameterless constructor.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasParameterlessConstructor()
    {
        return HasConstructor(string.Empty);
    }

    /// <summary>
    /// Expects the class to have a parameterless constructor with additional validation.
    /// </summary>
    /// <param name="constructorExpectations">Additional constructor expectations.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasParameterlessConstructor(Action<ConstructorExpectations> constructorExpectations)
    {
        return HasConstructor(string.Empty, constructorExpectations);
    }

    /// <summary>
    /// Expects the class to have a static constructor.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasStaticConstructor()
    {
        return HasStaticConstructor(_ => { });
    }

    /// <summary>
    /// Expects the class to have a static constructor with additional validation.
    /// </summary>
    /// <param name="constructorExpectations">Additional constructor expectations.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasStaticConstructor(Action<ConstructorExpectations> constructorExpectations)
    {
        if (constructorExpectations == null)
            throw new ArgumentNullException(nameof(constructorExpectations));

        var staticConstructor = _classDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault(c => c.Modifiers.Any(SyntaxKind.StaticKeyword));

        staticConstructor.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have a static constructor");

        var expectations = new ConstructorExpectations(staticConstructor, _classDeclaration.Identifier.Text, isStatic: true);
        constructorExpectations(expectations);

        return this;
    }

    /// <summary>
    /// Expects the class to have no constructors (uses default constructor).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNoConstructors()
    {
        var constructors = _classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().ToList();
        constructors.Count.ShouldBe(0, $"Expected class '{_classDeclaration.Identifier}' to have no explicit constructors but found {constructors.Count}");
        return this;
    }

    /// <summary>
    /// Expects the class to have exactly the specified number of constructors.
    /// </summary>
    /// <param name="count">The expected number of constructors.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasConstructorCount(int count)
    {
        var constructors = _classDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .Where(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword))
            .ToList();

        constructors.Count.ShouldBe(count, $"Expected class '{_classDeclaration.Identifier}' to have {count} constructor(s) but found {constructors.Count}");
        return this;
    }

    /// <summary>
    /// Expects the class to have no static constructor.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNoStaticConstructor()
    {
        var staticConstructor = _classDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault(c => c.Modifiers.Any(SyntaxKind.StaticKeyword));

        staticConstructor.ShouldBeNull($"Expected class '{_classDeclaration.Identifier}' to have no static constructor");
        return this;
    }

    /// <summary>
    /// Expects the class to be public.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsPublic()
    {
        _ = _classDeclaration.ShouldHaveModifier(SyntaxKind.PublicKeyword);
        return this;
    }

    /// <summary>
    /// Expects the class to be private.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsPrivate()
    {
        _ = _classDeclaration.ShouldHaveModifier(SyntaxKind.PrivateKeyword);
        return this;
    }

    /// <summary>
    /// Expects the class to be internal.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsInternal()
    {
        _ = _classDeclaration.ShouldHaveModifier(SyntaxKind.InternalKeyword);
        return this;
    }

    /// <summary>
    /// Expects the class to be static.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsStatic()
    {
        _classDeclaration.Modifiers.Any(m => string.Equals(m.ValueText, "static", StringComparison.Ordinal)).ShouldBeTrue(
            $"Expected class '{_classDeclaration.Identifier}' to be static");
        return this;
    }

    /// <summary>
    /// Expects the class to be abstract.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsAbstract()
    {
        _ = _classDeclaration.ShouldHaveModifier(SyntaxKind.AbstractKeyword);
        return this;
    }

    /// <summary>
    /// Expects the class to be partial.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsPartial()
    {
        _ = _classDeclaration.ShouldHaveModifier(SyntaxKind.PartialKeyword);
        return this;
    }

    /// <summary>
    /// Expects the class to be sealed.
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsSealed()
    {
        _ = _classDeclaration.ShouldHaveModifier(SyntaxKind.SealedKeyword);
        return this;
    }

    /// <summary>
    /// Expects the class to have the specified base type.
    /// </summary>
    /// <param name="baseTypeName">The name of the base type.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasBaseType(string baseTypeName)
    {
        var baseType = _classDeclaration.BaseList?.Types
            .FirstOrDefault(t => t.Type is IdentifierNameSyntax identifierName &&
string.Equals(identifierName.Identifier.ValueText, baseTypeName, StringComparison.Ordinal));

        _ = baseType.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have base type '{baseTypeName}'");
        return this;
    }

    /// <summary>
    /// Gets a collection of property names defined in this class.
    /// </summary>
    public IEnumerable<string> Properties =>
        _classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText);

    /// <summary>
    /// Gets a collection of method names defined in this class.
    /// </summary>
    public IEnumerable<string> Methods =>
        _classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Select(m => m.Identifier.ValueText);

    /// <summary>
    /// Expects the class to implement the specified interface.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to check for.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations ImplementsInterface(string interfaceName)
    {
        var baseList = _classDeclaration.BaseList;
        _ = baseList.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have a base list with interfaces.");

        // Look for the interface in the base list
        var hasInterface = baseList.Types
            .Any(t => string.Equals(t.Type.ToString(), interfaceName, StringComparison.Ordinal));

        hasInterface.ShouldBeTrue($"Expected class '{_classDeclaration.Identifier}' to implement interface '{interfaceName}'.");
        return this;
    }

    /// <summary>
    /// Alias for ImplementsInterface.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to check for.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasInterface(string interfaceName)
    {
        return ImplementsInterface(interfaceName);
    }

    /// <summary>
    /// Expects the class to have the specified modifier token.
    /// </summary>
    /// <param name="modifier">The modifier to check for (e.g., "public", "static", "abstract").</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasModifier(string modifier)
    {
        _classDeclaration.Modifiers.Any(m => string.Equals(m.ValueText, modifier, StringComparison.Ordinal))
            .ShouldBeTrue($"Expected class '{_classDeclaration.Identifier}' to have modifier '{modifier}'");
        return this;
    }

    /// <summary>
    /// Expects the class to have XML documentation for the given tag with specified content.
    /// </summary>
    /// <param name="tagName">The name of the XML documentation tag to check (e.g., "summary", "remarks").</param>
    /// <param name="content">The expected content of the XML documentation tag.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasXmlDoc(string tagName, string content)
    {
        var trivia = _classDeclaration.GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();
        _ = trivia.ShouldNotBeNull($"Class '{_classDeclaration.Identifier}' does not have XML documentation.");
        var element = trivia.ChildNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(e => string.Equals(e.StartTag.Name.LocalName.Text, tagName, StringComparison.Ordinal));
        _ = element.ShouldNotBeNull($"Expected XML documentation element '{tagName}'");
        var text = string.Concat(
            element.Content
                .OfType<XmlTextSyntax>()
                .SelectMany(x => x.TextTokens)
                .Select(tt => tt.Text)).Trim();
        text.ShouldBe(content, $"Expected XML documentation '{tagName}' to be '{content}' but was '{text}'");
        return this;
    }

    /// <summary>
    /// Finds a constructor by its parameter types.
    /// </summary>
    private static ConstructorDeclarationSyntax? FindConstructorByParameters(ClassDeclarationSyntax classSyntax, string parameterTypes)
    {
        var constructors = classSyntax.Members
            .OfType<ConstructorDeclarationSyntax>()
            .Where(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword));

        // Parse the parameter types
        var expectedTypes = string.IsNullOrWhiteSpace(parameterTypes)
            ? []
            : parameterTypes.Split(',').Select(t => t.Trim()).ToArray();

        foreach (var constructor in constructors)
        {
            var actualTypes = constructor.ParameterList.Parameters
                .Select(p => p.Type?.ToString() ?? string.Empty)
                .ToArray();

            if (actualTypes.Length == expectedTypes.Length)
            {
                bool match = true;
                for (int i = 0; i < actualTypes.Length; i++)
                {
                    // Simple comparison - could be enhanced for type aliases
                    if (!string.Equals(actualTypes[i], expectedTypes[i], StringComparison.Ordinal))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return constructor;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Expects the class to have a nested class with the specified name.
    /// </summary>
    /// <param name="nestedClassName">The name of the nested class.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNestedClass(string nestedClassName)
    {
        var nestedClass = _classDeclaration.Members
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => string.Equals(c.Identifier.Text, nestedClassName, StringComparison.Ordinal));

        nestedClass.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have nested class '{nestedClassName}'");
        return this;
    }

    /// <summary>
    /// Expects the class to have a nested class with the specified name and adds more expectations for that nested class.
    /// </summary>
    /// <param name="nestedClassName">The name of the nested class.</param>
    /// <param name="nestedClassExpectations">A callback to add expectations for the found nested class.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNestedClass(string nestedClassName, Action<ClassExpectations> nestedClassExpectations)
    {
        var nestedClass = _classDeclaration.Members
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => string.Equals(c.Identifier.Text, nestedClassName, StringComparison.Ordinal));

        nestedClass.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have nested class '{nestedClassName}'");

        var nestedClassExp = new ClassExpectations(nestedClass);
        nestedClassExpectations(nestedClassExp);

        return this;
    }

    /// <summary>
    /// Expects the class to have a nested enum with the specified name.
    /// </summary>
    /// <param name="nestedEnumName">The name of the nested enum.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNestedEnum(string nestedEnumName)
    {
        var nestedEnum = _classDeclaration.Members
            .OfType<EnumDeclarationSyntax>()
            .FirstOrDefault(e => string.Equals(e.Identifier.Text, nestedEnumName, StringComparison.Ordinal));

        nestedEnum.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have nested enum '{nestedEnumName}'");
        return this;
    }

    /// <summary>
    /// Expects the class to have a nested enum with the specified name and adds more expectations for that nested enum.
    /// </summary>
    /// <param name="nestedEnumName">The name of the nested enum.</param>
    /// <param name="nestedEnumExpectations">A callback to add expectations for the found nested enum.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNestedEnum(string nestedEnumName, Action<EnumExpectations> nestedEnumExpectations)
    {
        var nestedEnum = _classDeclaration.Members
            .OfType<EnumDeclarationSyntax>()
            .FirstOrDefault(e => string.Equals(e.Identifier.Text, nestedEnumName, StringComparison.Ordinal));

        nestedEnum.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have nested enum '{nestedEnumName}'");

        var nestedEnumExp = new EnumExpectations(nestedEnum);
        nestedEnumExpectations(nestedEnumExp);

        return this;
    }

    /// <summary>
    /// Expects the class to have a nested interface with the specified name.
    /// </summary>
    /// <param name="nestedInterfaceName">The name of the nested interface.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNestedInterface(string nestedInterfaceName)
    {
        var nestedInterface = _classDeclaration.Members
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => string.Equals(i.Identifier.Text, nestedInterfaceName, StringComparison.Ordinal));

        nestedInterface.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have nested interface '{nestedInterfaceName}'");
        return this;
    }

    /// <summary>
    /// Expects the class to have a nested interface with the specified name and adds more expectations for that nested interface.
    /// </summary>
    /// <param name="nestedInterfaceName">The name of the nested interface.</param>
    /// <param name="nestedInterfaceExpectations">A callback to add expectations for the found nested interface.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNestedInterface(string nestedInterfaceName, Action<InterfaceExpectations> nestedInterfaceExpectations)
    {
        var nestedInterface = _classDeclaration.Members
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => string.Equals(i.Identifier.Text, nestedInterfaceName, StringComparison.Ordinal));

        nestedInterface.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have nested interface '{nestedInterfaceName}'");

        var nestedInterfaceExp = new InterfaceExpectations(nestedInterface);
        nestedInterfaceExpectations(nestedInterfaceExp);

        return this;
    }

    /// <summary>
    /// Expects the class to have a nested record with the specified name.
    /// </summary>
    /// <param name="nestedRecordName">The name of the nested record.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNestedRecord(string nestedRecordName)
    {
        var nestedRecord = _classDeclaration.Members
            .OfType<RecordDeclarationSyntax>()
            .FirstOrDefault(r => string.Equals(r.Identifier.Text, nestedRecordName, StringComparison.Ordinal));

        nestedRecord.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have nested record '{nestedRecordName}'");
        return this;
    }

    /// <summary>
    /// Expects the class to have a nested record with the specified name and adds more expectations for that nested record.
    /// </summary>
    /// <param name="nestedRecordName">The name of the nested record.</param>
    /// <param name="nestedRecordExpectations">A callback to add expectations for the found nested record.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasNestedRecord(string nestedRecordName, Action<RecordExpectations> nestedRecordExpectations)
    {
        var nestedRecord = _classDeclaration.Members
            .OfType<RecordDeclarationSyntax>()
            .FirstOrDefault(r => string.Equals(r.Identifier.Text, nestedRecordName, StringComparison.Ordinal));

        nestedRecord.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have nested record '{nestedRecordName}'");

        var nestedRecordExp = new RecordExpectations(nestedRecord);
        nestedRecordExpectations(nestedRecordExp);

        return this;
    }

    /// <summary>
    /// Expects the class to have a type parameter with the specified name.
    /// </summary>
    /// <param name="typeParameterName">The name of the type parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasTypeParameter(string typeParameterName)
    {
        var typeParameter = _classDeclaration.TypeParameterList?.Parameters
            .FirstOrDefault(tp => string.Equals(tp.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        _ = typeParameter.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have type parameter '{typeParameterName}'");
        return this;
    }

    /// <summary>
    /// Expects the class to have a type parameter with the specified name and adds more expectations for that type parameter.
    /// </summary>
    /// <param name="typeParameterName">The name of the type parameter.</param>
    /// <param name="typeParameterExpectations">A callback to add expectations for the found type parameter.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasTypeParameter(string typeParameterName, Action<TypeParameterExpectations> typeParameterExpectations)
    {
        var typeParameter = _classDeclaration.TypeParameterList?.Parameters
            .FirstOrDefault(tp => string.Equals(tp.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        typeParameter.ShouldNotBeNull($"Expected class '{_classDeclaration.Identifier}' to have type parameter '{typeParameterName}'");

        var constraintClause = _classDeclaration.ConstraintClauses
            .FirstOrDefault(cc => string.Equals(cc.Name.Identifier.ValueText, typeParameterName, StringComparison.Ordinal));

        var typeParamExp = new TypeParameterExpectations(typeParameter, constraintClause);
        typeParameterExpectations(typeParamExp);

        return this;
    }

    /// <summary>
    /// Expects the class to have multiple type parameters with the specified names.
    /// </summary>
    /// <param name="typeParameterNames">The names of the type parameters.</param>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations HasTypeParameters(params string[] typeParameterNames)
    {
        foreach (var name in typeParameterNames)
        {
            HasTypeParameter(name);
        }
        return this;
    }

    /// <summary>
    /// Expects the class to be generic (have type parameters).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsGeneric()
    {
        var hasTypeParameters = _classDeclaration.TypeParameterList?.Parameters.Count > 0;
        hasTypeParameters.ShouldBeTrue($"Expected class '{_classDeclaration.Identifier}' to be generic (have type parameters)");
        return this;
    }

    /// <summary>
    /// Expects the class to not be generic (have no type parameters).
    /// </summary>
    /// <returns>The current expectations instance for chaining.</returns>
    public ClassExpectations IsNotGeneric()
    {
        var hasTypeParameters = _classDeclaration.TypeParameterList?.Parameters.Count > 0;
        hasTypeParameters.ShouldBe(false, $"Expected class '{_classDeclaration.Identifier}' to not be generic (have no type parameters)");
        return this;
    }
}
