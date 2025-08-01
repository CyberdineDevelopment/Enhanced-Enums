using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.SourceGenerator.Generators;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.SourceGenerator.Tests.Generators;

public class EnhancedEnumGeneratorTests
{
    [Fact]
    public void GeneratesStaticCollectionForBasicEnum()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"")]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .IsStatic()
                .IsPublic()
                .HasProperty("All", p => p
                    .IsStatic()
                    .HasReturnType("ImmutableArray<ColorBase>")
                    .HasXmlDocSummary("Gets all enum values."))
                .HasProperty("Empty", p => p
                    .IsStatic()
                    .HasReturnType("ColorBase")
                    .HasXmlDocSummary("Gets an empty/null enum value."))
                .HasMethod("GetByName", m => m
                    .IsStatic()
                    .HasReturnType("ColorBase")
                    .HasParameter("string", "name")
                    .HasXmlDocSummary("Gets an enum value by its name."))
                .HasMethod("TryGetByName", m => m
                    .IsStatic()
                    .HasReturnType("bool")
                    .HasParameter("string", "name")
                    .HasParameter("out ColorBase?", "value"))
                .HasMethod("Red", m => m
                    .IsStatic()
                    .HasReturnType("ColorBase")
                    .HasXmlDocSummary("Creates a new instance of Red."))
                .HasMethod("Blue", m => m
                    .IsStatic()
                    .HasReturnType("ColorBase")
                    .HasXmlDocSummary("Creates a new instance of Blue.")))
            .HasClass("EmptyColorOption", c => c
                .IsPublic()
                .IsSealed()
                .HasBaseType("ColorBase"))
            .Verify();
    }

    [Fact]
    public void GeneratesGenericCollectionWhenRequested()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", Generic = true)]
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .IsNotStatic() // Generic classes cannot be static
                .IsPublic()
                .HasGenericParameter("T")
                .HasGenericConstraint("where T : ColorBase")
                .HasProperty("All", p => p
                    .IsNotStatic()
                    .HasReturnType("ImmutableArray<T>"))
                .HasProperty("Empty", p => p
                    .IsNotStatic()
                    .HasReturnType("T"))
                .HasMethod("GetByName", m => m
                    .IsNotStatic()
                    .HasReturnType("T")
                    .HasParameter("string", "name"))
                .HasMethod("Red", m => m
                    .IsNotStatic()
                    .HasReturnType("T")))
            .Verify();
    }

    [Fact]
    public void GeneratesInstanceCollectionWhenRequested()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", GenerateStaticCollection = false)]
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .IsNotStatic()
                .IsPublic()
                .HasProperty("All", p => p
                    .IsNotStatic()
                    .HasReturnType("ImmutableArray<ColorBase>"))
                .HasProperty("Empty", p => p
                    .IsNotStatic()
                    .HasReturnType("ColorBase"))
                .HasMethod("GetByName", m => m
                    .IsNotStatic()
                    .HasReturnType("ColorBase"))
                .HasMethod("Red", m => m
                    .IsNotStatic()
                    .HasReturnType("ColorBase")))
            .Verify();
    }

    [Fact]
    public void GeneratesLookupMethodsForEnumLookupAttributes()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"")]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    [EnumLookup(""GetByHex"")]
                    public abstract string Hex { get; }

                    protected ColorBase(int id, string name, string hex) : base(id, name)
                    {
                        Hex = hex;
                    }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"", ""#FF0000"") { }
                    public override string Hex => ""#FF0000"";
                }
            }";

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasMethod("GetByHex", m => m
                    .IsStatic()
                    .HasReturnType("ColorBase?")
                    .HasParameter("string", "hex")
                    .HasXmlDocSummary("Gets the enum value with the specified Hex.")))
            .Verify();
    }

    [Fact]
    public void GeneratesMultiValueLookupMethods()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"")]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    [EnumLookup(""GetByCategory"", AllowMultiple = true)]
                    public abstract string Category { get; }

                    protected ColorBase(int id, string name, string category) : base(id, name)
                    {
                        Category = category;
                    }
                }

                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"", ""Primary"") { }
                    public override string Category => ""Primary"";
                }

                [EnumOption]
                public class Blue : ColorBase
                {
                    public Blue() : base(2, ""Blue"", ""Primary"") { }
                    public override string Category => ""Primary"";
                }
            }";

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasMethod("GetByCategory", m => m
                    .IsStatic()
                    .HasReturnType("ImmutableArray<ColorBase>")
                    .HasParameter("string", "category")
                    .HasXmlDocSummary("Gets all enum values with the specified Category.")))
            .Verify();
    }

    [Fact]
    public void DoesNotGenerateFactoryMethodsWhenDisabled()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", GenerateFactoryMethods = false)]
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .DoesNotHaveMethod("Red"))
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

                [EnumCollection(CollectionName = ""Colors"", ReturnType = typeof(IColor))]
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasProperty("All", p => p.HasReturnType("ImmutableArray<IColor>"))
                .HasProperty("Empty", p => p.HasReturnType("IColor"))
                .HasMethod("GetByName", m => m.HasReturnType("IColor"))
                .HasMethod("Red", m => m.HasReturnType("IColor")))
            .Verify();
    }

    [Fact]
    public void HandlesStringComparisonOptions()
    {
        var source = @"
            using System;
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"", NameComparison = StringComparison.OrdinalIgnoreCase)]
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        result.ShouldContain("StringComparer.OrdinalIgnoreCase");
    }

    [Fact]
    public void GeneratesCorrectNamespaceForGeneratedCode()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace CustomNamespace.Deep.Nested
            {
                [EnumCollection(CollectionName = ""Colors"")]
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        result.ShouldContain("namespace CustomNamespace.Deep.Nested");
    }

    [Fact]
    public void HandlesEmptyEnumCollections()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"")]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }
            }";

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c
                .HasProperty("All", p => p.HasReturnType("ImmutableArray<ColorBase>"))
                .HasProperty("Empty", p => p.HasReturnType("ColorBase"))
                .HasMethod("GetByName", m => m.HasReturnType("ColorBase")))
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
                [EnumCollection(CollectionName = ""Shapes"")]
                public abstract class ShapeBase : EnumOptionBase<ShapeBase>
                {
                    public abstract int Sides { get; }
                    protected ShapeBase(int id, string name, int sides) : base(id, name)
                    {
                        Sides = sides;
                    }
                }

                public abstract class PolygonBase : ShapeBase
                {
                    protected PolygonBase(int id, string name, int sides) : base(id, name, sides) { }
                }

                [EnumOption]
                public class Triangle : PolygonBase
                {
                    public Triangle() : base(1, ""Triangle"", 3) { }
                    public override int Sides => 3;
                }

                [EnumOption]
                public class Square : PolygonBase
                {
                    public Square() : base(2, ""Square"", 4) { }
                    public override int Sides => 4;
                }
            }";

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Shapes", c => c
                .HasMethod("Triangle", m => m.HasReturnType("ShapeBase"))
                .HasMethod("Square", m => m.HasReturnType("ShapeBase")))
            .Verify();
    }

    [Fact]
    public void HandlesUnicodeCharactersInNames()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colores"")]
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colores", c => c
                .HasMethod("Rojo", m => m.HasReturnType("ColorBase"))
                .HasMethod("Azul", m => m.HasReturnType("ColorBase")))
            .Verify();
    }

    [Fact]
    public void DoesNotGenerateForClassesWithoutEnumCollectionAttribute()
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void HandlesErrorsGracefully()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = """")]  // Empty collection name
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }
            }";

        Should.NotThrow(() => SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source));
    }

    [Fact]
    public void HandlesMultipleEnumCollectionsInSameNamespace()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Colors"")]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                [EnumCollection(CollectionName = ""Shapes"")]
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

        var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

        ExpectationsFactory.ExpectCode(result)
            .HasClass("Colors", c => c.HasMethod("Red"))
            .HasClass("Shapes", c => c.HasMethod("Circle"))
            .Verify();
    }
}