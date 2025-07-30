using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FractalDataWorks.EnhancedEnums.Analyzers;

/// <summary>
/// Analyzer that detects duplicate enum option names within the same collection.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DuplicateEnumOptionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for duplicate enum option names.
    /// </summary>
    public const string DiagnosticId = "ENH004";

    private static readonly LocalizableString Title = "Duplicate enum option name";
    private static readonly LocalizableString MessageFormat = "Enum option '{0}' is already defined in the '{1}' collection";
    private static readonly LocalizableString Description = "Each enum option must have a unique name within its collection.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        // Register for syntax nodes instead of compilation to avoid RS1030
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration);
    }

    private static void AnalyzeType(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
        if (typeSymbol == null) return;

        // Check if this type has EnumOption attribute
        var enumOptionAttr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) || string.Equals(a.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal));
        
        if (enumOptionAttr == null) return;

        // Get the collection name this option belongs to
        var collectionName = GetOptionCollectionName(enumOptionAttr, typeSymbol);
        if (collectionName == null) return;

        // Get the option name
        var optionName = GetOptionName(enumOptionAttr, typeSymbol);

        // Check for duplicates in the same compilation unit
        // Note: This is a simplified check within the current compilation only
        var compilation = context.SemanticModel.Compilation;
        var duplicates = FindDuplicateOptions(compilation, collectionName, optionName, typeSymbol);

        if (duplicates.Count > 0)
        {
            // Report diagnostic
            var diagnostic = Diagnostic.Create(
                Rule,
                typeDeclaration.Identifier.GetLocation(),
                optionName,
                collectionName);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static List<INamedTypeSymbol> FindDuplicateOptions(Compilation compilation, string collectionName, string optionName, INamedTypeSymbol currentType)
    {
        var duplicates = new List<INamedTypeSymbol>();
        
        // Get all types in the compilation
        var visitor = new TypeCollectorVisitor();
        compilation.Assembly.GlobalNamespace.Accept(visitor);
        
        foreach (var type in visitor.Types)
        {
            // Skip the current type
            if (SymbolEqualityComparer.Default.Equals(type, currentType))
                continue;

            // Check if this type has EnumOption attribute
            var attrs = type.GetAttributes()
                .Where(a => string.Equals(a.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) || string.Equals(a.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal))
                .ToList();

            foreach (var attr in attrs)
            {
                var otherCollectionName = GetOptionCollectionName(attr, type);
                var otherOptionName = GetOptionName(attr, type);

                // Check if it's in the same collection with the same name
                if (string.Equals(collectionName, otherCollectionName, StringComparison.Ordinal) &&
                    string.Equals(optionName, otherOptionName, StringComparison.OrdinalIgnoreCase))
                {
                    duplicates.Add(type);
                }
            }
        }

        return duplicates;
    }

    private static string? GetOptionCollectionName(AttributeData attr, INamedTypeSymbol typeSymbol)
    {
        // Check for explicit CollectionName in EnumOption attribute
        foreach (var arg in attr.NamedArguments)
        {
            if (string.Equals(arg.Key, "CollectionName", StringComparison.Ordinal) && arg.Value.Value is string explicitName && !string.IsNullOrEmpty(explicitName))
                return explicitName;
        }

        // Find which base type this derives from and infer collection name
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            var baseAttrs = baseType.GetAttributes()
                .Where(a => string.Equals(a.AttributeClass?.Name, "EnhancedEnumBaseAttribute", StringComparison.Ordinal) || string.Equals(a.AttributeClass?.Name, "EnumOptionBase", StringComparison.Ordinal))
                .ToList();

            if (baseAttrs.Count > 0)
            {
                // Found the base type with EnumOptionBase attribute
                return GetCollectionName(baseAttrs.First(), baseType);
            }

            baseType = baseType.BaseType;
        }

        return null;
    }

    private static string GetCollectionName(AttributeData attr, INamedTypeSymbol typeSymbol)
    {
        // Check for explicit CollectionName parameter
        foreach (var arg in attr.NamedArguments)
        {
            if (string.Equals(arg.Key, "CollectionName", StringComparison.Ordinal) && arg.Value.Value is string explicitName && !string.IsNullOrEmpty(explicitName))
                return explicitName;
        }

        // Check constructor arguments
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string ctorName)
            return ctorName;

        // Default to type name + "s"
        var typeName = typeSymbol.Name;
        if (typeName.EndsWith("Base", StringComparison.Ordinal))
            typeName = typeName.Substring(0, typeName.Length - 4);
            
        // Handle common pluralization patterns
        if (typeName.EndsWith("y", StringComparison.Ordinal))
            return typeName.Substring(0, typeName.Length - 1) + "ies";
        else if (typeName.EndsWith("s", StringComparison.Ordinal))
            return typeName + "es";
        else
            return typeName + "s";
    }

    private static string GetOptionName(AttributeData attr, INamedTypeSymbol typeSymbol)
    {
        // Check for explicit Name parameter
        foreach (var arg in attr.NamedArguments)
        {
            if (string.Equals(arg.Key, "Name", StringComparison.Ordinal) && arg.Value.Value is string explicitName && !string.IsNullOrEmpty(explicitName))
                return explicitName;
        }

        // Default to type name
        return typeSymbol.Name;
    }

    private sealed class TypeCollectorVisitor : SymbolVisitor
    {
        public List<INamedTypeSymbol> Types { get; } = new List<INamedTypeSymbol>();

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            Types.Add(symbol);
            
            // Visit nested types
            foreach (var nestedType in symbol.GetTypeMembers())
            {
                nestedType.Accept(this);
            }
        }
    }
}