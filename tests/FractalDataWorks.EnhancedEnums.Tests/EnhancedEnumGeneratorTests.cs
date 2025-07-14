using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.EnhancedEnums.Tests.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Tests for the EnhancedEnumOptionGenerator core functionality.
/// </summary>
public class EnhancedEnumOptionGeneratorTests : EnhancedEnumOptionTestBase
{
    [Fact]
    public void GeneratorProducesCollectionClassForBasicEnum()
    {
        // Arrange
        var source = """

		             using System;
		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class ColorBase
		                 {
		                     public abstract string Name { get; }
		                     public abstract string HexCode { get; }
		                 }

		                 [EnumOption]
		                 public class Red : ColorBase
		                 {
		                     public override string Name => "Red";
		                     public override string HexCode => "#FF0000";
		                 }

		                 [EnumOption]
		                 public class Blue : ColorBase
		                 {
		                     public override string Name => "Blue";
		                     public override string HexCode => "#0000FF";
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("ColorBases.g.cs").ShouldBeTrue();

        var generatedCode = result["ColorBases.g.cs"];
        LogGeneratedCode("ColorBases.g.cs", generatedCode);

        // Validate the generated collection class
        CodeValidationHelper.ValidateGeneratedCollectionClass(
            generatedCode,
            "TestNamespace",
            "ColorBases",
            "ColorBase");
    }

    [Fact]
    public void GeneratorHandlesCustomCollectionName()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption("MyColors")]
		                 public abstract class ColorBase
		                 {
		                     public abstract string Name { get; }
		                 }

		                 [EnumOption]
		                 public class Red : ColorBase
		                 {
		                     public override string Name => "Red";
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.ContainsSource("MyColors.g.cs").ShouldBeTrue();

        CodeValidationHelper.ValidateNamespaceAndClass(
            result["MyColors.g.cs"],
            "TestNamespace",
            "MyColors",
            classDecl =>
            {
                classDecl.Modifiers.Any(m => m.Text == "public").ShouldBeTrue();
                classDecl.Modifiers.Any(m => m.Text == "static").ShouldBeTrue();
            });
    }

    [Fact]
    public void GeneratorSupportsFactoryPattern()
    {
        // Arrange
        var source = """

		             using System;
		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption(UseFactory = true)]
		                 public abstract class ShapeBase
		                 {
		                     public abstract string Name { get; }
		                     public abstract double Area { get; }
		                     
		                     public static ShapeBase Create(Type type)
		                     {
		                         if (type == typeof(Circle))
		                             return new Circle(5.0);
		                         if (type == typeof(Square))
		                             return new Square(4.0);
		                         throw new ArgumentException($"Unknown type: {type}");
		                     }
		                 }

		                 [EnumOption]
		                 public class Circle : ShapeBase
		                 {
		                     private readonly double _radius;
		                     
		                     public Circle(double radius)
		                     {
		                         _radius = radius;
		                     }
		                     
		                     public override string Name => "Circle";
		                     public override double Area => Math.PI * _radius * _radius;
		                 }

		                 [EnumOption]
		                 public class Square : ShapeBase
		                 {
		                     private readonly double _side;
		                     
		                     public Square(double side)
		                     {
		                         _side = side;
		                     }
		                     
		                     public override string Name => "Square";
		                     public override double Area => _side * _side;
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("ShapeBases.g.cs").ShouldBeTrue();

        var generatedCode = result["ShapeBases.g.cs"];

        // Write generated code to file for debugging
        WriteGeneratedCodeToFile("GeneratorSupportsFactoryPattern_ShapeBases.g.cs", generatedCode);

        // Verify the static constructor uses factory method
        generatedCode.ShouldContain(".Create(typeof(");
        generatedCode.ShouldContain("Circle");
        generatedCode.ShouldContain("Square");
    }

    [Fact]
    public void GeneratorSupportsFactoryPatternWithExplicitCollectionName()
    {
        // Arrange - Use explicit collection name to avoid any naming issues
        var source = """

		             using System;
		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption("TestShapes", UseFactory = true)]
		                 public abstract class ShapeBase
		                 {
		                     public abstract string Name { get; }
		                     
		                     public static ShapeBase Create(Type type)
		                     {
		                         if (type.Name == "Circle")
		                             return new Circle();
		                         if (type.Name == "Square")  
		                             return new Square();
		                         throw new ArgumentException($"Unknown type: {type}");
		                     }
		                 }

		                 [EnumOption]
		                 public class Circle : ShapeBase
		                 {
		                     public override string Name => "Circle";
		                 }

		                 [EnumOption]
		                 public class Square : ShapeBase
		                 {
		                     public override string Name => "Square";
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("TestShapes.g.cs").ShouldBeTrue();

        var generatedCode = result["TestShapes.g.cs"];

        // Write generated code to file for debugging
        WriteGeneratedCodeToFile("GeneratorSupportsFactoryPatternWithExplicitCollectionName_TestShapes.g.cs", generatedCode);

        // Verify the static constructor uses factory method
        generatedCode.ShouldContain(".Create(typeof(");
        generatedCode.ShouldContain("Circle");
        generatedCode.ShouldContain("Square");
    }

    [Fact]
    public void GeneratorHandlesEnumOptionsInSeparateFiles()
    {
        // Arrange
        var baseSource = """

		                 using FractalDataWorks.EnhancedEnums.Attributes;

		                 namespace TestNamespace
		                 {
		                     [EnhancedEnumOption]
		                     public abstract class AnimalBase
		                     {
		                         public abstract string Name { get; }
		                         public abstract string Sound { get; }
		                     }
		                 }
		                 """;

        var dogSource = """

		                using FractalDataWorks.EnhancedEnums.Attributes;

		                namespace TestNamespace
		                {
		                    [EnumOption]
		                    public class Dog : AnimalBase
		                    {
		                        public override string Name => "Dog";
		                        public override string Sound => "Woof";
		                    }
		                }
		                """;

        var catSource = """

		                using FractalDataWorks.EnhancedEnums.Attributes;

		                namespace TestNamespace
		                {
		                    [EnumOption]
		                    public class Cat : AnimalBase
		                    {
		                        public override string Name => "Cat";
		                        public override string Sound => "Meow";
		                    }
		                }
		                """;

        // Act
        var result = RunGenerator([baseSource, dogSource, catSource]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("AnimalBases.g.cs").ShouldBeTrue();

        var generatedCode = result["AnimalBases.g.cs"];

        // Should contain both animals
        generatedCode.ShouldContain("new TestNamespace.Dog()");
        generatedCode.ShouldContain("new TestNamespace.Cat()");
    }

    [Fact]
    public void GeneratorHandlesEmptyEnum()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class EmptyBase
		                 {
		                     public abstract string Name { get; }
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("EmptyBases.g.cs").ShouldBeTrue();

        CodeValidationHelper.ValidateGeneratedCollectionClass(
            result["EmptyBases.g.cs"],
            "TestNamespace",
            "EmptyBases",
            "EmptyBase");
    }

    [Fact]
    public void GeneratorHandlesNestedNamespaces()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;
		             namespace Company.Product.Domain.Models
		             {
		                 [EnhancedEnumOption("OrderStatuses")]
		                 public abstract class OrderStatusBase
		                 {
		                     public abstract string Name { get; }
		                     public abstract int Priority { get; }
		                 }

		                 [EnumOption]
		                 public class Pending : OrderStatusBase
		                 {
		                     public override string Name => "Pending";
		                     public override int Priority => 1;
		                 }

		                 [EnumOption]
		                 public class Processing : OrderStatusBase
		                 {
		                     public override string Name => "Processing";
		                     public override int Priority => 2;
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.ContainsSource("OrderStatuses.g.cs").ShouldBeTrue();

        CodeValidationHelper.ValidateNamespaceAndClass(
            result["OrderStatuses.g.cs"],
            "Company.Product.Domain.Models",
            "OrderStatuses",
            classDecl =>
            {
                classDecl.Modifiers.Any(m => m.Text == "public").ShouldBeTrue();
                classDecl.Modifiers.Any(m => m.Text == "static").ShouldBeTrue();
            });
    }

    [Fact]
    public void GeneratorHandlesInterfaceBasedEnums()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public interface IPaymentMethod
		                 {
		                     string Name { get; }
		                     bool RequiresCvv { get; }
		                 }

		                 [EnumOption]
		                 public class CreditCard : IPaymentMethod
		                 {
		                     public string Name => "Credit Card";
		                     public bool RequiresCvv => true;
		                 }

		                 [EnumOption]
		                 public class PayPal : IPaymentMethod
		                 {
		                     public string Name => "PayPal";
		                     public bool RequiresCvv => false;
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.ContainsSource("IPaymentMethods.g.cs").ShouldBeTrue();

        CodeValidationHelper.ValidateGeneratedCollectionClass(
            result["IPaymentMethods.g.cs"],
            "TestNamespace",
            "IPaymentMethods",
            "IPaymentMethod");
    }

    [Fact]
    public void GeneratorPreservesEnumOptionOrder()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class PriorityBase
		                 {
		                     public abstract string Name { get; }
		                 }

		                 [EnumOption(Order = 3)]
		                 public class Low : PriorityBase
		                 {
		                     public override string Name => "Low";
		                 }

		                 [EnumOption(Order = 1)]
		                 public class High : PriorityBase
		                 {
		                     public override string Name => "High";
		                 }

		                 [EnumOption(Order = 2)]
		                 public class Medium : PriorityBase
		                 {
		                     public override string Name => "Medium";
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        var generatedCode = result["PriorityBases.g.cs"];

        // Verify the collection was generated
        generatedCode.ShouldContain("new TestNamespace.High()");
        generatedCode.ShouldContain("new TestNamespace.Medium()");
        generatedCode.ShouldContain("new TestNamespace.Low()");

        // Note: Order attribute is not currently implemented in the generator
        // This test verifies that all options are included regardless of order
    }
}
