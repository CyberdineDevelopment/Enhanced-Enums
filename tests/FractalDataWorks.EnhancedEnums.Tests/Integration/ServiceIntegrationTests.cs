using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests.Integration;

public class ServiceIntegrationTests
{
    [Fact]
    public void EnumCollectionBuilderIntegratesWithAllServices()
    {
        var definition = CreateComplexEnumTypeInfo();
        var values = CreateComplexEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        
        // Should contain all expected parts
        result.ShouldContain("TestCollection");
        result.ShouldContain("ImmutableArray<TestEnum>");
        result.ShouldContain("GetByName");
        result.ShouldContain("GetByCode"); // From lookup property
        result.ShouldContain("Value1"); // From factory method
        result.ShouldContain("Value2");
        result.ShouldContain("Empty");
    }

    [Fact]
    public void EnumCollectionBuilderHandlesGenericCollections()
    {
        var definition = CreateGenericEnumTypeInfo();
        var values = CreateComplexEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("class TestCollection<T>");
        result.ShouldContain("where T : TestNamespace.TestEnum");
        result.ShouldContain("ImmutableArray<T>");
        result.ShouldContain("public T GetByName");
        result.ShouldNotContain("static"); // Generic collections are not static
    }

    [Fact]
    public void EnumCollectionBuilderHandlesInstanceCollections()
    {
        var definition = CreateInstanceEnumTypeInfo();
        var values = CreateComplexEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("class TestCollection");
        result.ShouldNotContain("static class"); // Should not be static
        result.ShouldContain("public ImmutableArray<TestEnum>"); // Instance properties
        result.ShouldContain("public TestEnum GetByName");
    }

    [Fact]
    public void EnumCollectionBuilderHandlesComplexLookupProperties()
    {
        var definition = CreateEnumTypeInfoWithMultipleLookups();
        var values = CreateComplexEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("GetByCode");
        result.ShouldContain("GetByCategory");
        result.ShouldContain("ImmutableArray<TestEnum>"); // Multiple return type
        result.ShouldContain("TestEnum?"); // Single return type
    }

    [Fact]
    public void EnumCollectionBuilderPerformsWellWithLargeDatasets()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = new List<EnumValueInfo>();

        // Create 5000 enum values to test performance
        for (int i = 0; i < 5000; i++)
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

        var startTime = DateTime.UtcNow;
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);
        var elapsed = DateTime.UtcNow - startTime;

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        elapsed.TotalSeconds.ShouldBeLessThan(10.0); // Should complete within 10 seconds
        
        // Should contain references to first and last values
        result.ShouldContain("Value00000");
        result.ShouldContain("Value04999");
    }

    [Fact]
    public void EnumCollectionBuilderHandlesEdgeCaseNamespaces()
    {
        var definition = new EnumTypeInfo
        {
            Namespace = "Very.Deep.Nested.Namespace.Structure.Level1.Level2.Level3",
            ClassName = "TestEnum",
            FullTypeName = "Very.Deep.Nested.Namespace.Structure.Level1.Level2.Level3.TestEnum",
            CollectionName = "TestCollection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false,
            NameComparison = StringComparison.Ordinal,
            LookupProperties = EquatableArray.Empty<PropertyLookupInfo>()
        };

        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = "TestValue",
                FullTypeName = "Very.Deep.Nested.Namespace.Structure.Level1.Level2.Level3.TestValue",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            }
        };

        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("namespace Very.Deep.Nested.Namespace.Structure.Level1.Level2.Level3");
        result.ShouldContain("TestCollection");
        result.ShouldContain("TestValue");
    }

    [Fact]
    public void EnumCollectionBuilderHandlesSpecialCharactersInNames()
    {
        var definition = new EnumTypeInfo
        {
            Namespace = "Test_Namespace$123",
            ClassName = "Test_Enum_With_Underscores",
            FullTypeName = "Test_Namespace$123.Test_Enum_With_Underscores",
            CollectionName = "Test_Collection_Name",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = false,
            NameComparison = StringComparison.Ordinal,
            LookupProperties = EquatableArray.Empty<PropertyLookupInfo>()
        };

        var values = new List<EnumValueInfo>
        {
            new()
            {
                Name = "Test_Value_With_Special_Chars",
                FullTypeName = "Test_Namespace$123.Test_Value_With_Special_Chars",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            }
        };

        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "Test_Enum_With_Underscores", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("Test_Collection_Name");
        result.ShouldContain("Test_Value_With_Special_Chars");
        result.ShouldContain("namespace Test_Namespace$123");
    }

    private static EnumTypeInfo CreateComplexEnumTypeInfo()
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
            LookupProperties = new EquatableArray<PropertyLookupInfo>(new[]
            {
                new PropertyLookupInfo
                {
                    PropertyName = "Code",
                    PropertyType = "string",
                    LookupMethodName = "GetByCode",
                    AllowMultiple = false
                }
            })
        };
    }

    private static EnumTypeInfo CreateGenericEnumTypeInfo()
    {
        return new EnumTypeInfo
        {
            Namespace = "TestNamespace",
            ClassName = "TestEnum",
            FullTypeName = "TestNamespace.TestEnum",
            CollectionName = "TestCollection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = true,
            Generic = true, // Generic collection
            NameComparison = StringComparison.Ordinal,
            LookupProperties = EquatableArray.Empty<PropertyLookupInfo>()
        };
    }

    private static EnumTypeInfo CreateInstanceEnumTypeInfo()
    {
        return new EnumTypeInfo
        {
            Namespace = "TestNamespace",
            ClassName = "TestEnum",
            FullTypeName = "TestNamespace.TestEnum",
            CollectionName = "TestCollection",
            GenerateFactoryMethods = true,
            GenerateStaticCollection = false, // Instance collection
            Generic = false,
            NameComparison = StringComparison.Ordinal,
            LookupProperties = EquatableArray.Empty<PropertyLookupInfo>()
        };
    }

    private static EnumTypeInfo CreateEnumTypeInfoWithMultipleLookups()
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
            LookupProperties = new EquatableArray<PropertyLookupInfo>(new[]
            {
                new PropertyLookupInfo
                {
                    PropertyName = "Code",
                    PropertyType = "string",
                    LookupMethodName = "GetByCode",
                    AllowMultiple = false
                },
                new PropertyLookupInfo
                {
                    PropertyName = "Category",
                    PropertyType = "string",
                    LookupMethodName = "GetByCategory",
                    AllowMultiple = true
                }
            })
        };
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

    private static List<EnumValueInfo> CreateComplexEnumValues()
    {
        return new List<EnumValueInfo>
        {
            new()
            {
                Name = "Value1",
                FullTypeName = "TestNamespace.Value1",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            },
            new()
            {
                Name = "Value2",
                FullTypeName = "TestNamespace.Value2",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            }
        };
    }

    private static Compilation CreateTestCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace
            {
                public abstract class TestEnum { }
                public class Value1 : TestEnum { }
                public class Value2 : TestEnum { }
            }");

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
    }
}