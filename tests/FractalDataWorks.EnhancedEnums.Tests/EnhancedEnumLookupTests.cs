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
/// Tests for lookup method generation in EnhancedEnumOptionGenerator.
/// </summary>
public class EnhancedEnumOptionLookupTests : EnhancedEnumOptionTestBase
{
    [Fact]
    public void GeneratorCreatesLookupMethodForMarkedProperty()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class CountryBase
		                 {
		                     public abstract string Name { get; }
		                     
		                     [EnumLookup]
		                     public abstract string IsoCode { get; }
		                 }

		                 [EnumOption]
		                 public class UnitedStates : CountryBase
		                 {
		                     public override string Name => "United States";
		                     public override string IsoCode => "US";
		                 }

		                 [EnumOption]
		                 public class France : CountryBase
		                 {
		                     public override string Name => "France";
		                     public override string IsoCode => "FR";
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        var generatedCode = result["CountryBases.g.cs"];

        // Debug: Write generated code to help diagnose issues
        WriteGeneratedCodeToFile("GeneratorCreatesLookupMethodForMarkedProperty_CountryBases.g.cs", generatedCode);

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("CountryBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("GetByIsoCode", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.CountryBase?")
                        .HasParameter("isoCode", param => param.HasType("string")))))
            .Assert();
    }

    [Fact]
    public void GeneratorCreatesMultiValueLookupMethod()
    {
        // Arrange
        var source = """

		             using System;
		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class ProductBase
		                 {
		                     public abstract string Name { get; }
		                     
		                     [EnumLookup(AllowMultiple = true)]
		                     public abstract string[] Categories { get; }
		                 }

		                 [EnumOption]
		                 public class Laptop : ProductBase
		                 {
		                     public override string Name => "Laptop";
		                     public override string[] Categories => new[] { "Electronics", "Computers" };
		                 }

		                 [EnumOption]
		                 public class Phone : ProductBase
		                 {
		                     public override string Name => "Phone";
		                     public override string[] Categories => new[] { "Electronics", "Mobile" };
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        var generatedCode = result["ProductBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("ProductBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("GetByCategories", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("IEnumerable<TestNamespace.ProductBase>")
                        .HasParameter("categories", param => param.HasType("string[]")))))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesCustomLookupMethodName()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class UserRoleBase
		                 {
		                     public abstract string Name { get; }
		                     
		                     [EnumLookup(MethodName = "FindByPermissionLevel")]
		                     public abstract int PermissionLevel { get; }
		                 }

		                 [EnumOption]
		                 public class Admin : UserRoleBase
		                 {
		                     public override string Name => "Admin";
		                     public override int PermissionLevel => 100;
		                 }

		                 [EnumOption]
		                 public class User : UserRoleBase
		                 {
		                     public override string Name => "User";
		                     public override int PermissionLevel => 10;
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        var generatedCode = result["UserRoleBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("UserRoleBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("FindByPermissionLevel", method => method
                        .IsPublic()
                        .IsStatic())))
            .Assert();
        
        // Verify that the default method name is not used
        generatedCode.ShouldNotContain("GetByPermissionLevel");
    }

    [Fact]
    public void GeneratorHandlesMultipleLookupProperties()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class CurrencyBase
		                 {
		                     public abstract string Name { get; }
		                     
		                     [EnumLookup]
		                     public abstract string Code { get; }
		                     
		                     [EnumLookup]
		                     public abstract string Symbol { get; }
		                     
		                     [EnumLookup]
		                     public abstract int NumericCode { get; }
		                 }

		                 [EnumOption]
		                 public class USDollar : CurrencyBase
		                 {
		                     public override string Name => "US Dollar";
		                     public override string Code => "USD";
		                     public override string Symbol => "$";
		                     public override int NumericCode => 840;
		                 }

		                 [EnumOption]
		                 public class Euro : CurrencyBase
		                 {
		                     public override string Name => "Euro";
		                     public override string Code => "EUR";
		                     public override string Symbol => "€";
		                     public override int NumericCode => 978;
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        var generatedCode = result["CurrencyBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("CurrencyBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("GetByCode", method => method
                        .IsPublic()
                        .IsStatic())
                    .HasMethod("GetBySymbol", method => method
                        .IsPublic()
                        .IsStatic())
                    .HasMethod("GetByNumericCode", method => method
                        .IsPublic()
                        .IsStatic())))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesNullablePropertyLookup()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class ConfigBase
		                 {
		                     public abstract string Name { get; }
		                     
		                     [EnumLookup]
		                     public abstract string? OptionalKey { get; }
		                 }

		                 [EnumOption]
		                 public class DefaultConfig : ConfigBase
		                 {
		                     public override string Name => "Default";
		                     public override string? OptionalKey => null;
		                 }

		                 [EnumOption]
		                 public class CustomConfig : ConfigBase
		                 {
		                     public override string Name => "Custom";
		                     public override string? OptionalKey => "custom-key";
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        var generatedCode = result["ConfigBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("ConfigBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("GetByOptionalKey", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasParameter("optionalKey", param => param.HasType("string?")))))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesComplexPropertyTypeLookup()
    {
        // Arrange
        var source = """

		             using System;
		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class DateRangeBase
		                 {
		                     public abstract string Name { get; }
		                     
		                     [EnumLookup]
		                     public abstract DateTime StartDate { get; }
		                     
		                     [EnumLookup]
		                     public abstract Guid Id { get; }
		                 }

		                 [EnumOption]
		                 public class Q1_2024 : DateRangeBase
		                 {
		                     public override string Name => "Q1 2024";
		                     public override DateTime StartDate => new DateTime(2024, 1, 1);
		                     public override Guid Id => Guid.Parse("12345678-1234-1234-1234-123456789012");
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        var generatedCode = result["DateRangeBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("DateRangeBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("GetByStartDate", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasParameter("startDate", param => param.HasType("DateTime")))
                    .HasMethod("GetById", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasParameter("id", param => param.HasType("Guid")))))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesEnumWithOnlyLookupProperties()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class LookupOnlyBase
		                 {
		                     [EnumLookup]
		                     public abstract int Id { get; }
		                     
		                     [EnumLookup]
		                     public abstract string Code { get; }
		                 }

		                 [EnumOption]
		                 public class Option1 : LookupOnlyBase
		                 {
		                     public override int Id => 1;
		                     public override string Code => "OPT1";
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        var generatedCode = result["LookupOnlyBases.g.cs"];
        
        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("LookupOnlyBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("GetById", method => method
                        .IsPublic()
                        .IsStatic())
                    .HasMethod("GetByCode", method => method
                        .IsPublic()
                        .IsStatic())))
            .Assert();
    }

    [Fact]
    public void GeneratorHandlesNameComparison()
    {
        // Arrange
        var source = """

		             using System;
		             using FractalDataWorks.EnhancedEnums.Attributes;

		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption(NameComparison = StringComparison.Ordinal)]
		                 public abstract class CaseSensitiveBase
		                 {
		                     public abstract string Name { get; }
		                 }

		                 [EnumOption]
		                 public class OptionA : CaseSensitiveBase
		                 {
		                     public override string Name => "OptionA";
		                 }

		                 [EnumOption]
		                 public class Optiona : CaseSensitiveBase
		                 {
		                     public override string Name => "optiona";
		                 }
		             }
		             """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        var generatedCode = result["CaseSensitiveBases.g.cs"];

        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("CaseSensitiveBases", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic())))
            .Assert();
        
        // GetByName should use specified comparison
        generatedCode.ShouldContain("StringComparison.Ordinal");
    }
}
