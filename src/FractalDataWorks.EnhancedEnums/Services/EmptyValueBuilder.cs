using System;
using System.Linq;
using System.Text;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for building the Empty value singleton for collection classes.
/// </summary>
public static class EmptyValueBuilder
{
    /// <summary>
    /// Generates the Empty value singleton for the enum collection.
    /// </summary>
    public static void AddEmptyValue(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType, INamedTypeSymbol? baseTypeSymbol)
    {
        AddEmptyField(classBuilder);
        AddEmptyProperty(classBuilder, def, effectiveReturnType);
        AddEmptyValueNestedClass(classBuilder, def, baseTypeSymbol);
    }

    private static void AddEmptyField(ClassBuilder classBuilder)
    {
        classBuilder.AddField("EmptyValue", "_empty", field => field
            .MakePrivate()
            .MakeStatic()
            .MakeReadOnly()
            .WithInitializer("new EmptyValue()"));
    }

    private static void AddEmptyProperty(ClassBuilder classBuilder, EnumTypeInfo def, string effectiveReturnType)
    {
        classBuilder.AddProperty("Empty", effectiveReturnType ?? def.FullTypeName, prop => prop
            .MakePublic()
            .MakeStatic()
            .WithExpressionBody("_empty")
            .WithXmlDocSummary("Gets an empty instance representing no selection."));
    }

    private static void AddEmptyValueNestedClass(ClassBuilder classBuilder, EnumTypeInfo def, INamedTypeSymbol? baseTypeSymbol)
    {
        classBuilder.AddNestedClass(builder => 
        {
            builder.WithName("EmptyValue")
                .MakePrivate()
                .MakeSealed()
                .WithBaseType(def.FullTypeName);

            AddEmptyValueConstructor(builder, baseTypeSymbol);
            AddLookupPropertiesOverrides(builder, def);
        });
    }

    private static void AddEmptyValueConstructor(ClassBuilder builder, INamedTypeSymbol? baseTypeSymbol)
    {
        if (baseTypeSymbol == null)
            return;

        var constructors = baseTypeSymbol.Constructors
            .Where(c => !c.IsStatic && c.DeclaredAccessibility != Accessibility.Private)
            .OrderBy(c => c.Parameters.Length)
            .ThenBy(c => c.DeclaredAccessibility == Accessibility.Protected ? 0 : 1)
            .ToList();

        if (constructors.Count > 0)
        {
            var ctor = constructors.First();
            var args = string.Join(", ", ctor.Parameters.Select(p => GetDefaultValueForType(p.Type.ToDisplayString())));
            
            builder.AddConstructor(ctorBuilder => ctorBuilder
                .MakePublic()
                .WithBaseCall(args));
        }
    }

    private static void AddLookupPropertiesOverrides(ClassBuilder builder, EnumTypeInfo def)
    {
        var sb = new StringBuilder();
        
        // Generate default implementations only for abstract/virtual properties that require override
        foreach (var lookup in def.LookupProperties.Where(l => l.RequiresOverride))
        {
            var defaultValue = GetDefaultValueForType(lookup.PropertyType);
            sb.AppendLine($"public override {lookup.PropertyType} {lookup.PropertyName} => {defaultValue};");
        }
        
        var overrides = sb.ToString().TrimEnd();
        if (!string.IsNullOrEmpty(overrides))
        {
            builder.AddCodeBlock(overrides);
        }
    }

    /// <summary>
    /// Gets the default value string for a given type.
    /// </summary>
    private static string GetDefaultValueForType(string typeName)
    {
        // Remove nullable annotations for comparison
        var cleanType = typeName.TrimEnd('?');
        
        return cleanType switch
        {
            "string" => "string.Empty",
            "int" => "0",
            "long" => "0L",
            "short" => "0",
            "byte" => "0",
            "double" => "0.0",
            "float" => "0.0f",
            "decimal" => "0m",
            "bool" => "false",
            "System.Guid" or "Guid" => "Guid.Empty",
            "System.DateTime" or "DateTime" => "DateTime.MinValue",
            "System.DateTimeOffset" or "DateTimeOffset" => "DateTimeOffset.MinValue",
            "System.TimeSpan" or "TimeSpan" => "TimeSpan.Zero",
            _ => typeName.EndsWith("?", StringComparison.Ordinal) ? "null" : $"default({cleanType})"
        };
    }
}