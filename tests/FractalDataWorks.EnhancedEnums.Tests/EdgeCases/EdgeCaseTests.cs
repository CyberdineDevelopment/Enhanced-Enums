using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Services;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests.EdgeCases;

public class EdgeCaseTests
{
    [Fact]
    public void HandlesNullCollectionNameGracefully()
    {
        var definition = new EnumTypeInfo
        {
            Namespace = "TestNamespace",
            ClassName = "TestEnum",
            FullTypeName = "TestNamespace.TestEnum",
            CollectionName = null!, // Null collection name
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false
        };

        var values = new List<EnumValueInfo>();
        var compilation = CreateTestCompilation();

        Should.Throw<ArgumentException>(() => 
            EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation));
    }

    [Fact]  
    public void HandlesEmptyCollectionNameGracefully()
    {
        var definition = new EnumTypeInfo
        {
            Namespace = "TestNamespace",
            ClassName = "TestEnum",
            FullTypeName = "TestNamespace.TestEnum",
            CollectionName = string.Empty, // Empty collection name
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false
        };

        var values = new List<EnumValueInfo>();
        var compilation = CreateTestCompilation();

        Should.Throw<ArgumentException>(() => 
            EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation));
    }

    [Fact]
    public void HandlesVeryLongNames()
    {
        var longName = new string('A', 1000);
        var definition = new EnumTypeInfo
        {
            Namespace = "TestNamespace",
            ClassName = longName,
            FullTypeName = $"TestNamespace.{longName}",
            CollectionName = $"{longName}Collection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false
        };

        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = $"{longName}Value",
                FullTypeName = $"TestNamespace.{longName}Value",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            }
        };

        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, longName, null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain($"{longName}Collection");
        result.ShouldContain($"{longName}Value");
    }

    [Fact]
    public void HandlesSpecialCharactersInNames()
    {
        var definition = new EnumTypeInfo
        {
            Namespace = "Test.Name-space_123",
            ClassName = "Test_Enum-Class$123",
            FullTypeName = "Test.Name-space_123.Test_Enum-Class$123",
            CollectionName = "Test_Collection$456",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false
        };

        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = "Test_Value$789",
                FullTypeName = "Test.Name-space_123.Test_Value$789",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            }
        };

        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "Test_Enum-Class$123", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("Test_Collection$456");
        result.ShouldContain("Test_Value$789");
    }

    [Fact]
    public void HandlesExtremelyLargeNumberOfValues()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = new List<EnumValueInfo>();

        // Create 10,000 enum values
        for (int i = 0; i < 10000; i++)
        {
            values.Add(new EnumValueInfo
            {
                Name = $"Value{i:D5}",
                FullTypeName = $"TestNamespace.Value{i:D5}",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            });
        }

        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        
        // Should contain references to first and last values
        result.ShouldContain("Value00000");
        result.ShouldContain("Value09999");
    }

    [Fact]
    public void HandlesCircularGenericConstraints()
    {
        var definition = new EnumTypeInfo
        {
            Namespace = "TestNamespace",
            ClassName = "TestEnum",
            FullTypeName = "TestNamespace.TestEnum",
            CollectionName = "TestCollection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = true,
            IsGenericType = true,
            UnboundTypeName = "TestEnum`1"
        };

        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = "TestValue",
                FullTypeName = "TestNamespace.TestValue",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            }
        };

        var compilation = CreateTestCompilation();

        // Should handle gracefully without infinite loops
        Should.NotThrow(() => 
            EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation));
    }

    [Fact]
    public void HandlesNullCompilation()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = new List<EnumValueInfo>();

        Should.Throw<ArgumentNullException>(() => 
            EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, null!));
    }

    [Fact]
    public void HandlesEmptyNamespace()
    {
        var definition = new EnumTypeInfo
        {
            Namespace = string.Empty,
            ClassName = "TestEnum",
            FullTypeName = "TestEnum", // No namespace
            CollectionName = "TestCollection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false
        };

        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = "TestValue",
                FullTypeName = "TestValue",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            }
        };

        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldNotContain("namespace");
        result.ShouldContain("class TestCollection");
    }

    [Fact]
    public void HandlesInvalidReturnType()
    {
        var definition = new EnumTypeInfo
        {
            Namespace = "TestNamespace",
            ClassName = "TestEnum",
            FullTypeName = "TestNamespace.TestEnum",
            CollectionName = "TestCollection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false,
            ReturnType = "NonExistent.Invalid.Type"
        };

        var values = new List<EnumValueInfo>();
        var compilation = CreateTestCompilation();

        // Should handle gracefully and generate with the invalid type name
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "NonExistent.Invalid.Type", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("NonExistent.Invalid.Type");
    }

    [Fact]
    public void HandlesConflictingPropertyNames()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.LookupProperties = new EquatableArray<PropertyLookupInfo>(new[]
        {
            new PropertyLookupInfo
            {
                PropertyName = "All", // Conflicts with built-in property
                PropertyType = "string",
                LookupMethodName = "GetByAll",
                AllowMultiple = false
            }
        });

        var values = new List<EnumValueInfo>();
        var compilation = CreateTestCompilation();

        // Should handle name conflicts gracefully
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("GetByAll"); // Lookup method should still be generated
    }

    [Fact]
    public void HandlesDuplicateValueNames()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = "DuplicateName",
                FullTypeName = "TestNamespace.FirstDuplicate",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            },
            new()
            {
                Name = "DuplicateName", // Same name
                FullTypeName = "TestNamespace.SecondDuplicate",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            }
        };

        var compilation = CreateTestCompilation();

        // Should handle gracefully - both should be included with their full type names
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("FirstDuplicate");
        result.ShouldContain("SecondDuplicate");
    }

    [Fact]
    public void HandlesValueWithNoConstructors()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = "NoConstructor",
                FullTypeName = "TestNamespace.NoConstructor",
                Constructors = new List<ConstructorInfo>() // Empty constructors list
            }
        };

        var compilation = CreateTestCompilation();

        // Should handle gracefully
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("NoConstructor");
    }

    [Fact]
    public void HandlesValueWithPrivateConstructorOnly()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = "PrivateConstructor",
                FullTypeName = "TestNamespace.PrivateConstructor",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Private } // Private constructor
                }
            }
        };

        var compilation = CreateTestCompilation();

        // Should handle gracefully but might not generate factory method
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("PrivateConstructor");
    }

    [Fact]
    public void HandlesNullLookupProperties()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.LookupProperties = EquatableArray.Empty<PropertyLookupInfo>(); // Empty instead of null

        var values = new List<EnumValueInfo>();
        var compilation = CreateTestCompilation();

        // Should handle gracefully
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("TestCollection");
    }

    [Fact]
    public void HandlesInvalidStringComparison()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.NameComparison = (StringComparison)999; // Invalid enum value

        var values = new List<EnumValueInfo>();
        var compilation = CreateTestCompilation();

        // Should handle gracefully and default to something safe
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void HandlesExtremelyDeepNamespaces()
    {
        var deepNamespace = string.Join(".", Enumerable.Range(0, 100).Select(i => $"Level{i}"));
        var definition = new EnumTypeInfo
        {
            Namespace = deepNamespace,
            ClassName = "TestEnum",
            FullTypeName = $"{deepNamespace}.TestEnum",
            CollectionName = "TestCollection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false
        };

        var values = new List<EnumValueInfo>();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain($"namespace {deepNamespace}");
    }

    private static EnumTypeInfo CreateTestEnumTypeInfo()
    {
        return new EnumTypeInfo
        {
            Namespace = "TestNamespace",
            ClassName = "TestEnum",
            FullTypeName = "TestNamespace.TestEnum",
            CollectionName = "TestCollection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false,
            NameComparison = StringComparison.Ordinal,
            LookupProperties = EquatableArray.Empty<PropertyLookupInfo>()
        };
    }

    private static Compilation CreateTestCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace
            {
                public abstract class TestEnum { }
            }");

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
    }
}