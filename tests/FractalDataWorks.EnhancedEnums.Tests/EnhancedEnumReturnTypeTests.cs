using System.Linq;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.EnhancedEnums.Tests.TestHelpers;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Tests for ReturnType functionality in EnhancedEnumGenerator.
/// </summary>
public class EnhancedEnumReturnTypeTests : EnhancedEnumOptionTestBase
{
    [Fact]
    public void GeneratorUsesSpecifiedReturnType()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums;
                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         public interface IMessage
                         {
                             string Code { get; }
                         }

                         [EnhancedEnumBase(ReturnType = "TestNamespace.IMessage")]
                         public abstract class MessageBase : IMessage
                         {
                             public abstract string Name { get; }
                             public abstract string Code { get; }
                         }

                         [EnumOption]
                         public class ErrorMessage : MessageBase
                         {
                             public override string Name => "Error";
                             public override string Code => "ERR001";
                         }

                         [EnumOption]
                         public class WarningMessage : MessageBase
                         {
                             public override string Name => "Warning";
                             public override string Code => "WRN001";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("MessageBases.g.cs").ShouldBeTrue();

        var generatedCode = result["MessageBases.g.cs"];
        LogGeneratedCode("MessageBases.g.cs", generatedCode);
        
        // Write generated code to file for debugging
        WriteGeneratedCodeToFile("GeneratorUsesSpecifiedReturnType_MessageBases.g.cs", generatedCode);

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("MessageBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("ErrorMessage", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IMessage"))
                    .HasProperty("WarningMessage", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IMessage"))
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.IMessage>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.IMessage?")
                        .HasParameter("name", param => param.HasType("string")))))
            .Assert();
    }

    [Fact]
    public void GeneratorAutoDetectsInterfaceReturnType()
    {
        // Arrange
        // Note: This test adds IEnhancedEnumOption to the test source to enable auto-detection
        var source = """

                     using FractalDataWorks.EnhancedEnums;
                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace FractalDataWorks
                     {
                         public interface IEnhancedEnumOption
                         {
                             int Id { get; }
                         }
                     }

                     namespace TestNamespace
                     {
                         public interface IStatus : FractalDataWorks.IEnhancedEnumOption
                         {
                             int Priority { get; }
                         }

                         [EnhancedEnumBase] // No explicit ReturnType
                         public abstract class StatusBase : IStatus
                         {
                             public abstract string Name { get; }
                             public abstract int Priority { get; }
                             public int Id => Name?.GetHashCode() ?? 0;
                         }

                         [EnumOption]
                         public class ActiveStatus : StatusBase
                         {
                             public override string Name => "Active";
                             public override int Priority => 1;
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("StatusBases.g.cs").ShouldBeTrue();

        var generatedCode = result["StatusBases.g.cs"];
        LogGeneratedCode("StatusBases.g.cs", generatedCode);

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("StatusBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("ActiveStatus", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IStatus"))
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.IStatus>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.IStatus?")
                        .HasParameter("name", param => param.HasType("string")))
                    // Should also generate GetById since StatusBase implements IEnhancedEnumOption
                    .HasMethod("GetById", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.IStatus?")
                        .HasParameter("id", param => param.HasType("int")))))
            .Assert();
    }

    [Fact]
    public void GeneratorUsesBaseTypeWhenNoInterfaceDetected()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         [EnhancedEnumBase] // No interface, no explicit ReturnType
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
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("ColorBases.g.cs").ShouldBeTrue();

        var generatedCode = result["ColorBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("ColorBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("Red", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.ColorBase"))
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.ColorBase>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.ColorBase?")
                        .HasParameter("name", param => param.HasType("string")))))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesLookupMethodReturnTypeOverride()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums;
                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         public interface IProduct
                         {
                             string Code { get; }
                         }

                         [EnhancedEnumBase(ReturnType = "TestNamespace.IProduct")]
                         public abstract class ProductBase : IProduct
                         {
                             public abstract string Name { get; }
                             
                             [EnumLookup(ReturnType = "TestNamespace.ProductBase")]
                             public abstract string Code { get; }
                             
                             [EnumLookup]
                             public abstract int CategoryId { get; }
                         }

                         [EnumOption]
                         public class Widget : ProductBase
                         {
                             public override string Name => "Widget";
                             public override string Code => "WDG001";
                             public override int CategoryId => 1;
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("ProductBases.g.cs").ShouldBeTrue();

        var generatedCode = result["ProductBases.g.cs"];
        LogGeneratedCode("ProductBases.g.cs", generatedCode);

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("ProductBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("Widget", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IProduct"))
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.IProduct>"))
                    .HasMethod("GetByCode", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.ProductBase?")
                        .HasParameter("code", param => param.HasType("string")))
                    .HasMethod("GetByCategoryId", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.IProduct?")
                        .HasParameter("categoryId", param => param.HasType("int")))))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesMultipleCollectionsWithReturnType()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums;
                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         public interface IUser { }

                         [EnhancedEnumBase("ActiveUsers", ReturnType = "TestNamespace.IUser")]
                         [EnhancedEnumBase("AdminUsers", ReturnType = "TestNamespace.UserBase")]
                         public abstract class UserBase : IUser
                         {
                             public abstract string Name { get; }
                         }

                         [EnumOption(CollectionName = "ActiveUsers")]
                         public class RegularUser : UserBase
                         {
                             public override string Name => "Regular";
                         }

                         [EnumOption(CollectionName = "AdminUsers")]
                         public class AdminUser : UserBase
                         {
                             public override string Name => "Admin";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        var activeUsersCode = result["ActiveUsers.g.cs"];
        var adminUsersCode = result["AdminUsers.g.cs"];

        // ActiveUsers uses IUser
        ExpectationsFactory.ExpectCode(activeUsersCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("ActiveUsers", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("RegularUser", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IUser"))
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.IUser>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.IUser?")
                        .HasParameter("name", param => param.HasType("string")))))
            .Assert();

        // AdminUsers uses UserBase
        ExpectationsFactory.ExpectCode(adminUsersCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("AdminUsers", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("AdminUser", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.UserBase"))
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.UserBase>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.UserBase?")
                        .HasParameter("name", param => param.HasType("string")))))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesMultipleReturnTypeFormats()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums;
                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace.SubNamespace
                     {
                         public interface IEntity { }

                         // Test various ways to specify return type
                         [EnhancedEnumBase(ReturnType = "IEntity")] // Without namespace
                         public abstract class EntityBase : IEntity
                         {
                             public abstract string Name { get; }
                         }

                         [EnumOption]
                         public class Entity1 : EntityBase
                         {
                             public override string Name => "Entity1";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("EntityBases.g.cs").ShouldBeTrue();

        var generatedCode = result["EntityBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace.SubNamespace", ns => ns
                .HasClass("EntityBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("Entity1", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("IEntity"))  // Should use the return type as specified
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<IEntity>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("IEntity?")
                        .HasParameter("name", param => param.HasType("string")))))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesFactoryPatternWithReturnType()
    {
        // Arrange
        var source = """

                     using System;
                     using FractalDataWorks.EnhancedEnums;
                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         public interface IShape
                         {
                             double Area { get; }
                         }

                         [EnhancedEnumBase(UseFactory = true, ReturnType = "TestNamespace.IShape")]
                         public abstract class ShapeBase : IShape
                         {
                             public abstract string Name { get; }
                             public abstract double Area { get; }
                             
                             public static ShapeBase Create(Type type)
                             {
                                 if (type == typeof(Circle))
                                     return new Circle(5.0);
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
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("ShapeBases.g.cs").ShouldBeTrue();

        var generatedCode = result["ShapeBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("ShapeBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("Circle", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IShape"))
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.IShape>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.IShape?")
                        .HasParameter("name", param => param.HasType("string")))))
            .Assert();
        
        // Should still use factory for instantiation
        generatedCode.ShouldContain(".Create(typeof(");
    }

    [Fact]
    public void GeneratorHandlesNullableReturnTypes()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums;
                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         public interface IConfig { }

                         [EnhancedEnumBase(ReturnType = "TestNamespace.IConfig?")]
                         public abstract class ConfigBase : IConfig
                         {
                             public abstract string Name { get; }
                         }

                         [EnumOption]
                         public class DefaultConfig : ConfigBase
                         {
                             public override string Name => "Default";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("ConfigBases.g.cs").ShouldBeTrue();

        var generatedCode = result["ConfigBases.g.cs"];
        
        // Write generated code to file for debugging
        WriteGeneratedCodeToFile("GeneratorHandlesNullableReturnTypes_ConfigBases.g.cs", generatedCode);

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("ConfigBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("DefaultConfig", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IConfig?"))
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.IConfig?>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.IConfig?")
                        .HasParameter("name", param => param.HasType("string")))))
            .Assert();
    }
}