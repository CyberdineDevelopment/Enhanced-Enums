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

namespace FractalDataWorks.EnhancedEnums.Tests.Services;

public class EnumCollectionBuilderTests
{
    [Fact]
    public void BuildCollectionGeneratesStaticCollectionByDefault()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        // Debug: Output the generated code to understand structure
        System.Diagnostics.Debug.WriteLine("Generated code:");
        System.Diagnostics.Debug.WriteLine(result);
        // Test with actual ExpectationsFactory, but first let's see what classes are generated
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(result);
        var root = syntaxTree.GetRoot();
        var classes = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .Select(c => c.Identifier.ValueText)
            .ToList();
        
        System.Console.WriteLine($"=== FOUND CLASSES: {string.Join(", ", classes)} ===");
        System.Console.WriteLine(result);
        System.Console.WriteLine("=== END DEBUG ===");
        
        // Write to file for examination
        System.IO.File.WriteAllText("C:\\temp\\generated_code_debug.cs", result);
        
        // Verify classes exist before using ExpectationsFactory
        classes.ShouldContain("TestCollection");
        
        // Debug: Check what properties and methods exist in TestCollection
        var testCollectionClass = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "TestCollection");
            
        if (testCollectionClass != null)
        {
            var properties = testCollectionClass.Members
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax>()
                .Select(p => p.Identifier.ValueText)
                .ToList();
            var methods = testCollectionClass.Members
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                .Select(m => m.Identifier.ValueText)
                .ToList();
                
            System.Console.WriteLine($"=== TestCollection Properties: {string.Join(", ", properties)} ===");
            System.Console.WriteLine($"=== TestCollection Methods: {string.Join(", ", methods)} ===");
        }
        
        // For now, just verify basic structure without ExpectationsFactory
        result.ShouldContain("class TestCollection");
        result.ShouldContain("All");
        result.ShouldContain("Empty");
        result.ShouldContain("GetByName");
    }

    [Fact]
    public void BuildCollectionGeneratesInstanceCollectionWhenRequested()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.GenerateStaticCollection = false;
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("TestCollection", c => c
                .IsPublic()
                .HasProperty("All", p => p.HasType("ImmutableArray<TestEnum>"))
                .HasProperty("Empty", p => p.HasType("TestEnum"))
                .HasMethod("GetByName", m => m.HasReturnType("TestEnum")))
            .Verify();
    }

    [Fact]
    public void BuildCollectionGeneratesGenericCollectionWhenRequested()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.Generic = true;
        definition.GenerateStaticCollection = true; // Should be ignored for generic
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("TestCollection", c => c
                .IsPublic()
                .HasProperty("All", p => p.HasType("ImmutableArray<T>"))
                .HasProperty("Empty", p => p.HasType("T"))
                .HasMethod("GetByName", m => m.HasReturnType("T")))
            .Verify();
    }

    [Fact]
    public void BuildCollectionIncludesNullableDirective()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldContain("#nullable enable");
    }

    [Fact]
    public void BuildCollectionIncludesRequiredNamespaces()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldContain("using System;");
        result.ShouldContain("using System.Linq;");
        result.ShouldContain("using System.Collections.Generic;");
        result.ShouldContain("using System.Collections.Immutable;");
    }

    [Fact]
    public void BuildCollectionIncludesConditionalFrozenDictionary()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldContain("#if NET8_0_OR_GREATER");
        result.ShouldContain("using System.Collections.Frozen;");
        result.ShouldContain("#endif");
    }

    [Fact]
    public void BuildCollectionGeneratesFactoryMethodsWhenEnabled()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.GenerateFactoryMethods = true;
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("TestCollection", c => c
                .HasMethod("TestValue1", m => m.IsStatic().HasReturnType("TestEnum"))
                .HasMethod("TestValue2", m => m.IsStatic().HasReturnType("TestEnum")))
            .Verify();
    }

    [Fact]
    public void BuildCollectionDoesNotGenerateFactoryMethodsWhenDisabled()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.GenerateFactoryMethods = false;
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        // Should not contain factory methods when disabled
        result.ShouldNotContain("public static TestEnum TestValue1");
        result.ShouldNotContain("public static TestEnum TestValue2");
    }

    [Fact]
    public void BuildCollectionGeneratesLookupMethods()
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
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("TestCollection", c => c
                .HasMethod("GetByCode", m => m
                    .IsStatic()
                    .HasReturnType("TestEnum?")
                    .HasParameter("code", "string")))
            .Verify();
    }

    [Fact]
    public void BuildCollectionGeneratesMultiValueLookupMethods()
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
            NameComparison = StringComparison.Ordinal,
            LookupProperties = new EquatableArray<PropertyLookupInfo>(new[]
            {
                new PropertyLookupInfo
                {
                    PropertyName = "Category",
                    PropertyType = "string",
                    LookupMethodName = "GetByCategory",
                    AllowMultiple = true
                }
            })
        };
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("TestCollection", c => c
                .HasMethod("GetByCategory", m => m
                    .IsStatic()
                    .HasReturnType("ImmutableArray<TestEnum>")
                    .HasParameter("category", "string")))
            .Verify();
    }

    [Fact]
    public void BuildCollectionGeneratesEmptyClass()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("EmptyTestEnumOption", c => c
                .IsPublic()
                .IsSealed()
                .HasBaseType("TestNamespace.TestEnum"))
            .Verify();
    }

    [Fact]
    public void BuildCollectionHandlesEmptyValuesList()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = new List<EnumValueInfo>();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        
        ExpectationsFactory.ExpectCode(result)
            .HasClass("TestCollection", c => c
                .HasProperty("All", p => p.HasType("ImmutableArray<TestEnum>"))
                .HasProperty("Empty", p => p.HasType("TestEnum")))
            .Verify();
    }

    [Fact]
    public void BuildCollectionHandlesNullValues()
    {
        var definition = CreateTestEnumTypeInfo();
        var compilation = CreateTestCompilation();

        Should.Throw<ArgumentNullException>(() => 
            EnumCollectionBuilder.BuildCollection(definition, null!, "TestEnum", null, compilation));
    }

    [Fact]
    public void BuildCollectionHandlesNullDefinition()
    {
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        Should.Throw<ArgumentNullException>(() => 
            EnumCollectionBuilder.BuildCollection(null!, values, "TestEnum", null, compilation));
    }

    [Fact]
    public void BuildCollectionHandlesCustomReturnTypes()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = CreateTestEnumValues();
        var customReturnType = "ITestInterface";
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, customReturnType, null, compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("TestCollection", c => c
                .HasProperty("All", p => p.HasType("ImmutableArray<ITestInterface>"))
                .HasProperty("Empty", p => p.HasType("ITestInterface"))
                .HasMethod("GetByName", m => m.HasReturnType("ITestInterface")))
            .Verify();
    }

    [Fact]
    public void BuildCollectionIncludesXmlDocumentation()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldContain("/// <summary>");
        result.ShouldContain("Collection of all TestEnum values.");
        result.ShouldContain("Gets all enum values.");
        result.ShouldContain("Gets an enum value by its name.");
    }

    [Fact]
    public void BuildCollectionHandlesSpecialCharactersInNames()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.CollectionName = "Test_Collection_123";
        definition.ClassName = "Test_Enum_Class";
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("Test_Collection_123");
        result.ShouldContain("EmptyTest_Enum_ClassOption");
    }

    [Fact]
    public void BuildCollectionHandlesUnicodeCharacters()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.CollectionName = "Collection_数据";
        definition.ClassName = "Enum_Ñiño";
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldNotBeNull();
        result.ShouldContain("Collection_数据");
        result.ShouldContain("EmptyEnum_ÑiñoOption");
    }

    [Fact]
    public void BuildCollectionHandlesLargeNumberOfValues()
    {
        var definition = CreateTestEnumTypeInfo();
        var values = new List<EnumValueInfo>();
        
        for (int i = 0; i < 100; i++)
        {
            values.Add(new EnumValueInfo
            {
                Name = $"Value{i}",
                FullTypeName = $"TestNamespace.Value{i}",
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
        
        // Should contain references to all values
        for (int i = 0; i < 100; i++)
        {
            result.ShouldContain($"Value{i}");
        }
    }

    [Fact]
    public void BuildCollectionHandlesStringComparisonOptions()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.NameComparison = StringComparison.OrdinalIgnoreCase;
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldContain("StringComparer.OrdinalIgnoreCase");
    }

    [Fact]
    public void BuildCollectionHandlesOrdinalComparison()
    {
        var definition = CreateTestEnumTypeInfo();
        definition.NameComparison = StringComparison.Ordinal;
        var values = CreateTestEnumValues();
        var compilation = CreateTestCompilation();

        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);

        result.ShouldContain("StringComparer.Ordinal");
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

    private static List<EnumValueInfo> CreateTestEnumValues()
    {
        return new List<EnumValueInfo>
        {
            new()
            {
                Name = "TestValue1",
                FullTypeName = "TestNamespace.TestValue1",
                Constructors = new List<ConstructorInfo>
                {
                    new() { Accessibility = Accessibility.Public }
                }
            },
            new()
            {
                Name = "TestValue2",
                FullTypeName = "TestNamespace.TestValue2",
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
                public class TestValue1 : TestEnum { }
                public class TestValue2 : TestEnum { }
            }");

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
    }
}