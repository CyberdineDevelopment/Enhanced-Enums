using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public class DebugGeneratedCode
{
    public static void Main()
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
            LookupProperties = EquatableArray.Empty<PropertyLookupInfo>()
        };
        
        var values = new List<EnumValueInfo>
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
        
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace
            {
                public abstract class TestEnum { }
                public class TestValue1 : TestEnum { }
                public class TestValue2 : TestEnum { }
            }");

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
        
        var result = EnumCollectionBuilder.BuildCollection(definition, values, "TestEnum", null, compilation);
        
        Console.WriteLine("=== GENERATED CODE ===");
        Console.WriteLine(result);
        Console.WriteLine("=== END GENERATED CODE ===");
    }
}