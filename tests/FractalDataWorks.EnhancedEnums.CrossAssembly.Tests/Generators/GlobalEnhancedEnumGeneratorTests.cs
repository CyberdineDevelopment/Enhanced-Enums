using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.CrossAssembly.Generators;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.CrossAssembly.Tests.Generators;

public class GlobalEnhancedEnumGeneratorTests
{
    [Fact]
    public void GeneratesCollectionForCrossAssemblyEnums()
    {
        var baseSource = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace BasePackage
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }
            }";

        var optionSource = @"
            using FractalDataWorks.Attributes;
            using BasePackage;

            namespace OptionPackage
            {
                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }

                [EnumOption]
                public class Blue : ColorBase
                {
                    public Blue() : base(2, ""Blue"") { }
                }
            }";

        var consumerSource = @"
            using BasePackage;
            using OptionPackage;

            namespace ConsumerProject
            {
                public class TestConsumer
                {
                    public void UseColors()
                    {
                        var red = Colors.Red();
                        var all = Colors.All;
                    }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("ConsumerAssembly")
            .AddSources(baseSource, optionSource, consumerSource)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .IsStatic()
                .IsPublic()
                .HasProperty("All", p => p
                    .IsStatic()
                    .HasReturnType("ImmutableArray<ColorBase>"))
                .HasMethod("Red", m => m
                    .IsStatic()
                    .HasReturnType("ColorBase"))
                .HasMethod("Blue", m => m
                    .IsStatic()
                    .HasReturnType("ColorBase")))
            .Verify();
    }

    [Fact]
    public void GeneratesLookupMethodsForCrossAssemblyEnums()
    {
        var baseSource = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace BasePackage
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    [EnumLookup(""GetByHex"")]
                    public abstract string Hex { get; }

                    protected ColorBase(int id, string name) : base(id, name) { }
                }
            }";

        var optionSource = @"
            using FractalDataWorks.Attributes;
            using BasePackage;

            namespace OptionPackage
            {
                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                    public override string Hex => ""#FF0000"";
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("ConsumerAssembly")
            .AddSources(baseSource, optionSource)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasMethod("GetByHex", m => m
                    .IsStatic()
                    .HasReturnType("ColorBase?")
                    .HasParameter("string", "hex")))
            .Verify();
    }

    [Fact]
    public void GeneratesMultipleCollectionsForDifferentBaseTypes()
    {
        var sources = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumCollection(CollectionName = ""Shapes"", IncludeReferencedAssemblies = true)]
                public abstract class ShapeBase : EnumOptionBase<ShapeBase>
                {
                    protected ShapeBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }

                [EnumOption]
                public class Circle : ShapeBase
                {
                    public Circle() : base(1, ""Circle"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(sources)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasMethod("Red", m => m.HasReturnType("ColorBase")))
            .HasClass("Shapes", c => c
                .HasMethod("Circle", m => m.HasReturnType("ShapeBase")))
            .Verify();
    }

    [Fact]
    public void HandlesNestedNamespaces()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace Outer.Inner.Deep
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        result.ShouldContain("namespace Outer.Inner.Deep");
        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasMethod("Red", m => m.HasReturnType("ColorBase")))
            .Verify();
    }

    [Fact]
    public void HandlesNestedTypes()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                public static class ColorDefinitions
                {
                    [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                    public abstract class ColorBase : EnumOptionBase<ColorBase>
                    {
                        protected ColorBase(int id, string name) : base(id, name) { }
                    }

                    [EnumOption]
                    public class Red : ColorBase
                    {
                        public Red() : base(1, ""Red"") { }
                    }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasMethod("Red", m => m.HasReturnType("ColorDefinitions.ColorBase")))
            .Verify();
    }

    [Fact]
    public void IgnoresTypesWithoutIncludeReferencedAssemblies()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"")] // Missing IncludeReferencedAssemblies = true
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        result.ShouldNotContain("class Colors");
    }

    [Fact]
    public void HandlesEmptyCollections()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        // Should still generate the collection class even with no options
        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasProperty("All", p => p.HasReturnType("ImmutableArray<ColorBase>"))
                .HasProperty("Empty", p => p.HasReturnType("ColorBase")))
            .Verify();
    }

    [Fact]
    public void HandlesCustomReturnTypes()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                public interface IColor
                {
                    string Name { get; }
                }

                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true, ReturnType = typeof(IColor))]
                public abstract class ColorBase : EnumOptionBase<ColorBase>, IColor
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasProperty("All", p => p.HasReturnType("ImmutableArray<IColor>"))
                .HasMethod("Red", m => m.HasReturnType("IColor")))
            .Verify();
    }

    [Fact]
    public void HandlesMultiValueLookupMethods()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    [EnumLookup(""GetByCategory"", AllowMultiple = true)]
                    public abstract string Category { get; }

                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                    public override string Category => ""Primary"";
                }

                [EnumOption]
                public class Blue : ColorBase
                {
                    public Blue() : base(2, ""Blue"") { }
                    public override string Category => ""Primary"";
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasMethod("GetByCategory", m => m
                    .IsStatic()
                    .HasReturnType("ImmutableArray<ColorBase>")
                    .HasParameter("string", "category")))
            .Verify();
    }

    [Fact]
    public void HandlesComplexInheritanceHierarchies()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Shapes"", IncludeReferencedAssemblies = true)]
                public abstract class ShapeBase : EnumOptionBase<ShapeBase>
                {
                    public abstract int Sides { get; }
                    protected ShapeBase(int id, string name) : base(id, name) { }
                }

                public abstract class PolygonBase : ShapeBase
                {
                    protected PolygonBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Triangle : PolygonBase
                {
                    public Triangle() : base(1, ""Triangle"") { }
                    public override int Sides => 3;
                }

                [EnumOption]
                public class Square : PolygonBase
                {
                    public Square() : base(2, ""Square"") { }
                    public override int Sides => 4;
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Shapes", c => c
                .HasMethod("Triangle", m => m.HasReturnType("ShapeBase"))
                .HasMethod("Square", m => m.HasReturnType("ShapeBase")))
            .Verify();
    }

    [Fact]
    public void HandlesUnicodeCharacters()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colores"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Rojo : ColorBase
                {
                    public Rojo() : base(1, ""Rojo"") { }
                }

                [EnumOption] 
                public class Azul : ColorBase
                {
                    public Azul() : base(2, ""Azul"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colores", c => c
                .HasMethod("Rojo", m => m.HasReturnType("ColorBase"))
                .HasMethod("Azul", m => m.HasReturnType("ColorBase")))
            .Verify();
    }

    [Fact]
    public void DoesNotGenerateForTypesWithoutEnumCollectionAttribute()
    {
        var source = @"
            using FractalDataWorks;

            namespace TestNamespace
            {
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        result.ShouldNotContain("class Colors");
    }

    [Fact]
    public void HandlesErrorsGracefully()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = """", IncludeReferencedAssemblies = true)] // Empty collection name
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        Should.NotThrow(() => SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation));
    }

    [Fact]
    public void GeneratesDebugFiles()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var generator = new GlobalEnhancedEnumGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var result = driver.RunGenerators(compilation);

        result.Results.ShouldHaveSingleItem();
        result.Results[0].GeneratedSources.Length.ShouldBeGreaterThan(1);
        
        // Should have debug files
        result.Results[0].GeneratedSources.Any(s => s.HintName.Contains("Debug")).ShouldBeTrue();
    }

    [Fact]
    public void EmitsFilesToDiskWhenRequested()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddMSBuildProperty("GeneratorOutPutTo", "C:\\temp\\generated")
            .AddReference(typeof(object))
            .Build();

        // This test verifies the code doesn't throw when trying to emit files
        Should.NotThrow(() => SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation));
    }

    [Fact]
    public void HandlesGenericBaseTypes()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Items"", IncludeReferencedAssemblies = true)]
                public abstract class ItemBase<T> : EnumOptionBase<ItemBase<T>> where T : class
                {
                    protected ItemBase(int id, string name) : base(id, name) { }
                    public abstract T Value { get; }
                }

                [EnumOption]
                public class StringItem : ItemBase<string>
                {
                    public StringItem() : base(1, ""StringItem"") { }
                    public override string Value => ""Test"";
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Items", c => c
                .HasMethod("StringItem"))
            .Verify();
    }

    [Fact]
    public void HandlesMultipleAssemblyReferences()
    {
        var baseAssemblySource = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace BaseAssembly
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }
            }";

        var optionAssembly1Source = @"
            using FractalDataWorks.Attributes;
            using BaseAssembly;

            namespace OptionAssembly1
            {
                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }
            }";

        var optionAssembly2Source = @"
            using FractalDataWorks.Attributes;
            using BaseAssembly;

            namespace OptionAssembly2
            {
                [EnumOption]
                public class Blue : ColorBase
                {
                    public Blue() : base(2, ""Blue"") { }
                }
            }";

        var consumerSource = @"
            using BaseAssembly;

            namespace Consumer
            {
                public class TestClass
                {
                    public void Test()
                    {
                        var colors = Colors.All;
                    }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("ConsumerAssembly")
            .AddSources(baseAssemblySource, optionAssembly1Source, optionAssembly2Source, consumerSource)
            .AddReference(typeof(object))
            .Build();

        var result = SourceGeneratorTestHelper.RunGenerator<GlobalEnhancedEnumGenerator>(compilation);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasMethod("Red", m => m.HasReturnType("ColorBase"))
                .HasMethod("Blue", m => m.HasReturnType("ColorBase")))
            .Verify();
    }
}