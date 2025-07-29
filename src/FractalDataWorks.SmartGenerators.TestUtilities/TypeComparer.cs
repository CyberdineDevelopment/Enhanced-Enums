using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Provides semantic type comparison for testing generated code.
/// </summary>
internal static class TypeComparer
{
    /// <summary>
    /// Determines if two type strings are semantically equivalent.
    /// </summary>
    /// <param name="actualTypeSyntax">The actual type syntax from generated code.</param>
    /// <param name="expectedType">The expected type string.</param>
    /// <returns>True if the types are semantically equivalent; otherwise, false.</returns>
    public static bool AreEquivalent(TypeSyntax actualTypeSyntax, string expectedType)
    {
        if (actualTypeSyntax == null || string.IsNullOrWhiteSpace(expectedType))
            return false;

        // Parse the expected type string into syntax
        var expectedTypeSyntax = SyntaxFactory.ParseTypeName(expectedType);
        
        return AreEquivalent(actualTypeSyntax, expectedTypeSyntax);
    }

    /// <summary>
    /// Determines if two type syntax nodes are semantically equivalent.
    /// </summary>
    private static bool AreEquivalent(TypeSyntax actual, TypeSyntax expected)
    {
        return (actual, expected) switch
        {
            // Handle nullable types (e.g., CountryBase? vs TestNamespace.CountryBase?)
            (NullableTypeSyntax actualNullable, NullableTypeSyntax expectedNullable) =>
                AreEquivalent(actualNullable.ElementType, expectedNullable.ElementType),
            
            // Handle nullable where only one side has the ? annotation
            (NullableTypeSyntax actualNullable, _) =>
                AreEquivalent(actualNullable.ElementType, expected),
            
            (_, NullableTypeSyntax expectedNullable) =>
                AreEquivalent(actual, expectedNullable.ElementType),
            
            // Handle generic types (e.g., List<T>, IEnumerable<T>)
            (GenericNameSyntax actualGeneric, GenericNameSyntax expectedGeneric) =>
                AreGenericTypesEquivalent(actualGeneric, expectedGeneric),
            
            // Handle qualified names (e.g., System.String vs String)
            (QualifiedNameSyntax actualQualified, _) =>
                AreEquivalent(actualQualified.Right, expected),
            
            (_, QualifiedNameSyntax expectedQualified) =>
                AreEquivalent(actual, expectedQualified.Right),
            
            // Handle array types
            (ArrayTypeSyntax actualArray, ArrayTypeSyntax expectedArray) =>
                AreEquivalent(actualArray.ElementType, expectedArray.ElementType) &&
                actualArray.RankSpecifiers.Count == expectedArray.RankSpecifiers.Count,
            
            // Handle simple identifiers
            (IdentifierNameSyntax actualId, IdentifierNameSyntax expectedId) =>
                AreIdentifiersEquivalent(actualId.Identifier.Text, expectedId.Identifier.Text),
            
            // Handle predefined types (int, string, etc.)
            (PredefinedTypeSyntax actualPredefined, PredefinedTypeSyntax expectedPredefined) =>
                actualPredefined.Keyword.Kind() == expectedPredefined.Keyword.Kind(),
            
            // Handle predefined type vs identifier (e.g., string vs String)
            (PredefinedTypeSyntax actualPredefined, IdentifierNameSyntax expectedId) =>
                IsPredefinedTypeEquivalent(actualPredefined, expectedId.Identifier.Text),
            
            (IdentifierNameSyntax actualId, PredefinedTypeSyntax expectedPredefined) =>
                IsPredefinedTypeEquivalent(expectedPredefined, actualId.Identifier.Text),
            
            // Default case
            _ => false
        };
    }

    private static bool AreGenericTypesEquivalent(GenericNameSyntax actual, GenericNameSyntax expected)
    {
        // Check if base type names match
        if (!AreIdentifiersEquivalent(actual.Identifier.Text, expected.Identifier.Text))
            return false;
        
        // Check if type argument counts match
        var actualArgs = actual.TypeArgumentList.Arguments;
        var expectedArgs = expected.TypeArgumentList.Arguments;
        
        if (actualArgs.Count != expectedArgs.Count)
            return false;
        
        // Check each type argument
        for (int i = 0; i < actualArgs.Count; i++)
        {
            if (!AreEquivalent(actualArgs[i], expectedArgs[i]))
                return false;
        }
        
        return true;
    }

    private static bool AreIdentifiersEquivalent(string actual, string expected)
    {
        // Direct match
        if (string.Equals(actual, expected, StringComparison.Ordinal))
            return true;
        
        // Check for common type aliases
        return (actual, expected) switch
        {
            ("String", "string") => true,
            ("string", "String") => true,
            ("Int32", "int") => true,
            ("int", "Int32") => true,
            ("Boolean", "bool") => true,
            ("bool", "Boolean") => true,
            ("Object", "object") => true,
            ("object", "Object") => true,
            ("Decimal", "decimal") => true,
            ("decimal", "Decimal") => true,
            ("Double", "double") => true,
            ("double", "Double") => true,
            ("Single", "float") => true,
            ("float", "Single") => true,
            ("Int64", "long") => true,
            ("long", "Int64") => true,
            ("Int16", "short") => true,
            ("short", "Int16") => true,
            ("Byte", "byte") => true,
            ("byte", "Byte") => true,
            _ => false
        };
    }

    private static bool IsPredefinedTypeEquivalent(PredefinedTypeSyntax predefined, string identifier)
    {
        return predefined.Keyword.Kind() switch
        {
            SyntaxKind.StringKeyword => identifier == "String",
            SyntaxKind.IntKeyword => identifier == "Int32",
            SyntaxKind.BoolKeyword => identifier == "Boolean",
            SyntaxKind.ObjectKeyword => identifier == "Object",
            SyntaxKind.DecimalKeyword => identifier == "Decimal",
            SyntaxKind.DoubleKeyword => identifier == "Double",
            SyntaxKind.FloatKeyword => identifier == "Single",
            SyntaxKind.LongKeyword => identifier == "Int64",
            SyntaxKind.ShortKeyword => identifier == "Int16",
            SyntaxKind.ByteKeyword => identifier == "Byte",
            _ => false
        };
    }
}