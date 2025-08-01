using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Discovery;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.CrossAssembly.Tests.Services;

public class TypeDiscoveryServiceTests
{
    [Fact]
    public void DiscoversEnumCollectionTypesAcrossAssemblies()
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

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(baseSource)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var types = service.DiscoverEnumCollectionTypes(compilation);

        types.ShouldHaveSingleItem();
        types.First().Name.ShouldBe("ColorBase");
        types.First().ContainingNamespace.ToDisplayString().ShouldBe("BasePackage");
    }

    [Fact]
    public void DiscoversOptionTypesForSpecificBase()
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
                public class Blue : ColorBase
                {
                    public Blue() : base(2, ""Blue"") { }
                }

                [EnumOption]
                public class Circle : ShapeBase
                {
                    public Circle() : base(1, ""Circle"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var enumCollectionTypes = service.DiscoverEnumCollectionTypes(compilation);
        var colorBase = enumCollectionTypes.First(t => t.Name == "ColorBase");
        
        var optionTypes = service.DiscoverOptionTypesForBase(compilation, colorBase);

        optionTypes.Count.ShouldBe(2);
        optionTypes.Any(t => t.Name == "Red").ShouldBeTrue();
        optionTypes.Any(t => t.Name == "Blue").ShouldBeTrue();
        optionTypes.Any(t => t.Name == "Circle").ShouldBeFalse();
    }

    [Fact]
    public void IgnoresAbstractOptionTypes()
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
                public abstract class AbstractColor : ColorBase
                {
                    protected AbstractColor(int id, string name) : base(id, name) { }
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

        var service = new TypeDiscoveryService();
        var enumCollectionTypes = service.DiscoverEnumCollectionTypes(compilation);
        var colorBase = enumCollectionTypes.First();
        
        var optionTypes = service.DiscoverOptionTypesForBase(compilation, colorBase);

        optionTypes.Count.ShouldBe(1);
        optionTypes.First().Name.ShouldBe("Red");
        optionTypes.Any(t => t.Name == "AbstractColor").ShouldBeFalse();
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

                    public static class Variants
                    {
                        [EnumOption]
                        public class DarkRed : ColorBase
                        {
                            public DarkRed() : base(2, ""DarkRed"") { }
                        }
                    }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var enumCollectionTypes = service.DiscoverEnumCollectionTypes(compilation);
        var colorBase = enumCollectionTypes.First();
        
        var optionTypes = service.DiscoverOptionTypesForBase(compilation, colorBase);

        optionTypes.Count.ShouldBe(2);
        optionTypes.Any(t => t.Name == "Red").ShouldBeTrue();
        optionTypes.Any(t => t.Name == "DarkRed").ShouldBeTrue();
    }

    [Fact]
    public void HandlesMultipleNamespaces()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace Base.Namespace
            {
                [EnumCollection(CollectionName = ""Colors"", IncludeReferencedAssemblies = true)]
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }
            }

            namespace Options.Red
            {
                using Base.Namespace;
                
                [EnumOption]
                public class Red : ColorBase
                {
                    public Red() : base(1, ""Red"") { }
                }
            }

            namespace Options.Blue
            {
                using Base.Namespace;
                
                [EnumOption]
                public class Blue : ColorBase
                {
                    public Blue() : base(2, ""Blue"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var enumCollectionTypes = service.DiscoverEnumCollectionTypes(compilation);
        var colorBase = enumCollectionTypes.First();
        
        var optionTypes = service.DiscoverOptionTypesForBase(compilation, colorBase);

        optionTypes.Count.ShouldBe(2);
        optionTypes.Any(t => t.Name == "Red").ShouldBeTrue();
        optionTypes.Any(t => t.Name == "Blue").ShouldBeTrue();
    }

    [Fact]
    public void IgnoresTypesWithoutEnumCollectionAttribute()
    {
        var source = @"
            using FractalDataWorks;

            namespace TestNamespace
            {
                public abstract class ColorBase : EnumOptionBase<ColorBase>
                {
                    protected ColorBase(int id, string name) : base(id, name) { }
                }

                public abstract class ShapeBase : EnumOptionBase<ShapeBase>
                {
                    protected ShapeBase(int id, string name) : base(id, name) { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var types = service.DiscoverEnumCollectionTypes(compilation);

        types.ShouldBeEmpty();
    }

    [Fact]
    public void IgnoresTypesWithoutEnumOptionAttribute()
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

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var enumCollectionTypes = service.DiscoverEnumCollectionTypes(compilation);
        var colorBase = enumCollectionTypes.First();
        
        var optionTypes = service.DiscoverOptionTypesForBase(compilation, colorBase);

        optionTypes.Count.ShouldBe(1);
        optionTypes.First().Name.ShouldBe("Blue");
        optionTypes.Any(t => t.Name == "Red").ShouldBeFalse();
    }

    [Fact]
    public void HandlesCacheEfficiently()
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

        var service = new TypeDiscoveryService();
        
        // First call should discover and cache
        var types1 = service.DiscoverEnumCollectionTypes(compilation);
        
        // Second call should use cache
        var types2 = service.DiscoverEnumCollectionTypes(compilation);

        types1.Count.ShouldBe(types2.Count);
        types1.First().Name.ShouldBe(types2.First().Name);
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
                public abstract class ItemBase<T> : EnumOptionBase<ItemBase<T>>
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

                [EnumOption]
                public class IntItem : ItemBase<int>
                {
                    public IntItem() : base(2, ""IntItem"") { }
                    public override int Value => 42;
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var enumCollectionTypes = service.DiscoverEnumCollectionTypes(compilation);

        enumCollectionTypes.ShouldHaveSingleItem();
        enumCollectionTypes.First().Name.ShouldBe("ItemBase");
        enumCollectionTypes.First().IsGenericType.ShouldBeTrue();
    }

    [Fact]
    public void HandlesComplexInheritanceChains()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""Shapes"", IncludeReferencedAssemblies = true)]
                public abstract class ShapeBase : EnumOptionBase<ShapeBase>
                {
                    protected ShapeBase(int id, string name) : base(id, name) { }
                }

                public abstract class PolygonBase : ShapeBase
                {
                    protected PolygonBase(int id, string name) : base(id, name) { }
                }

                public abstract class RegularPolygonBase : PolygonBase
                {
                    protected RegularPolygonBase(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Triangle : PolygonBase
                {
                    public Triangle() : base(1, ""Triangle"") { }
                }

                [EnumOption]
                public class Square : RegularPolygonBase
                {
                    public Square() : base(2, ""Square"") { }
                }

                [EnumOption]
                public class Circle : ShapeBase
                {
                    public Circle() : base(3, ""Circle"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var enumCollectionTypes = service.DiscoverEnumCollectionTypes(compilation);
        var shapeBase = enumCollectionTypes.First();
        
        var optionTypes = service.DiscoverOptionTypesForBase(compilation, shapeBase);

        optionTypes.Count.ShouldBe(3);
        optionTypes.Any(t => t.Name == "Triangle").ShouldBeTrue();
        optionTypes.Any(t => t.Name == "Square").ShouldBeTrue();
        optionTypes.Any(t => t.Name == "Circle").ShouldBeTrue();
    }

    [Fact]
    public void HandlesUnicodeInTypeNames()
    {
        var source = @"
            using FractalDataWorks;
            using FractalDataWorks.Attributes;

            namespace TestNamespace
            {
                [EnumCollection(CollectionName = ""数据Types"", IncludeReferencedAssemblies = true)]
                public abstract class 数据Base : EnumOptionBase<数据Base>
                {
                    protected 数据Base(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Español_Ñiño : 数据Base
                {
                    public Español_Ñiño() : base(1, ""Spanish"") { }
                }

                [EnumOption]
                public class 中文_数据 : 数据Base
                {
                    public 中文_数据() : base(2, ""Chinese"") { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var enumCollectionTypes = service.DiscoverEnumCollectionTypes(compilation);
        var baseType = enumCollectionTypes.First();
        
        var optionTypes = service.DiscoverOptionTypesForBase(compilation, baseType);

        enumCollectionTypes.ShouldHaveSingleItem();
        baseType.Name.ShouldBe("数据Base");
        optionTypes.Count.ShouldBe(2);
        optionTypes.Any(t => t.Name == "Español_Ñiño").ShouldBeTrue();
        optionTypes.Any(t => t.Name == "中文_数据").ShouldBeTrue();
    }

    [Fact]
    public void HandlesEmptyAssemblies()
    {
        var source = @"
            namespace TestNamespace
            {
                public class EmptyClass
                {
                    public void DoNothing() { }
                }
            }";

        var compilation = AssemblyCompilationBuilder.Create("TestAssembly")
            .AddSource(source)
            .AddReference(typeof(object))
            .Build();

        var service = new TypeDiscoveryService();
        var types = service.DiscoverEnumCollectionTypes(compilation);

        types.ShouldBeEmpty();
    }
}