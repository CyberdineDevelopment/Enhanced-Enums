# EnhancedEnumOptions Testing Guide

This document provides a comprehensive guide for testing the EnhancedEnumOptions source generator using SmartGenerators testing utilities.

## Overview

The EnhancedEnumOptions generator creates collection classes for enum-like hierarchies. Testing focuses on validating:
1. Correct code generation for various enum configurations
2. Proper handling of edge cases and errors
3. Cross-assembly scenario support
4. Incremental generation performance

## Test Structure

### Project Organization

```
FractalDataWorks.EnhancedEnumOptions.Tests/
├── EnhancedEnumOptionTestBase.cs          # Base class with common test infrastructure
├── EnhancedEnumOptionGeneratorTests.cs    # Core functionality tests
├── EnhancedEnumOptionErrorScenarioTests.cs # Error handling and edge cases
├── SmokeTests.cs                    # Basic verification tests
└── TestHelpers/
    └── AssemblyExtensions.cs        # Extension methods for assembly manipulation
```

### Key Dependencies

- **FractalDataWorks.SmartGenerators.TestUtilities**: Provides `SourceGeneratorTestHelper` and `ExpectationsFactory`
- **Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing.XUnit**: Roslyn testing infrastructure
- **Shouldly**: Fluent assertion library
- **XUnit v3**: Test framework (preview version)

## Testing Patterns

### 1. Basic Generator Test Pattern

```csharp
[Fact]
public void GeneratorShouldGenerateCollectionClassForBasicEnum()
{
    // Arrange - Define source code with EnhancedEnumOption
    var sources = new[]
    {
        @"
        using FractalDataWorks.EnhancedEnumOptions.Attributes;
        using FractalDataWorks.SmartGenerators.AssemblyScanning;

        [assembly: EnableAssemblyScanner]

        namespace TestNamespace
        {
            [EnhancedEnumOption]
            public abstract class ColorBase
            {
                public abstract string Name { get; }
                public abstract string HexCode { get; }
            }

            [EnumOption(typeof(ColorBase))]
            public class Red : ColorBase
            {
                public override string Name => ""Red"";
                public override string HexCode => ""#FF0000"";
            }
        }"
    };

    // Act - Run the generator
    var result = RunGeneratorWithAssemblyScanning(sources);

    // Assert - Validate generated code structure
    result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    result.GeneratedSources.ShouldContainKey("Colors.g.cs");

    ExpectationsFactory.ExpectCode(result.GeneratedSources["Colors.g.cs"])
        .HasNamespace("TestNamespace", ns => ns
            .HasClass("Colors", c => c
                .IsPublic()
                .IsStatic()
                .HasField("_all", f => f
                    .IsPrivate()
                    .IsStatic()
                    .IsReadOnly()
                    .HasType("List<ColorBase>"))
                .HasProperty("All", p => p
                    .IsPublic()
                    .IsStatic()
                    .HasType("IReadOnlyList<ColorBase>")
                    .HasGetter())
                .HasMethod("GetByName", m => m
                    .IsPublic()
                    .IsStatic()
                    .HasReturnType("ColorBase?")
                    .HasParameter("name", param => param.HasType("string")))))
        .Assert();
}
```

### 2. Using the Test Base Class

The `EnhancedEnumOptionTestBase` provides common functionality:

```csharp
public abstract class EnhancedEnumOptionTestBase : IDisposable
{
    // Run generator with assembly scanning enabled
    protected GeneratorRunResult RunGeneratorWithAssemblyScanning(
        string[] sources,
        params MetadataReference[] additionalReferences);

    // Run generator without assembly scanning (for error testing)
    protected GeneratorRunResult RunGenerator(
        string[] sources,
        params MetadataReference[] additionalReferences);

    // Create a compilation for cross-assembly testing
    protected Compilation CreateCompilationWithEnhancedEnumOption(string source);

    // Get default references including EnhancedEnumOptions assemblies
    protected MetadataReference[] GetDefaultReferences();
}
```

### 3. Testing Error Scenarios

```csharp
[Fact]
public void GeneratorShouldReportDiagnosticWhenAssemblyScannerNotEnabled()
{
    // Arrange - Source without EnableAssemblyScanner
    var sources = new[]
    {
        @"
        using FractalDataWorks.EnhancedEnumOptions.Attributes;
        
        // NOTE: No [assembly: EnableAssemblyScanner] attribute

        namespace TestNamespace
        {
            [EnhancedEnumOption]
            public abstract class StatusBase
            {
                public abstract string Name { get; }
            }
        }"
    };

    // Act
    var result = RunGenerator(sources);

    // Assert - Should have warning diagnostic
    result.Diagnostics.ShouldContain(d => 
        d.Id == "EE001" && 
        d.Severity == DiagnosticSeverity.Warning);
}
```

### 4. Testing Cross-Assembly Scenarios

```csharp
[Fact]
public void GeneratorShouldHandleCrossAssemblyEnumOptions()
{
    // Create base enum in Assembly A
    var assemblyA = CreateCompilationWithEnhancedEnumOption(@"
        namespace AssemblyA
        {
            [EnhancedEnumOption]
            public abstract class PaymentMethodBase
            {
                public abstract string Name { get; }
            }
        }");

    // Create enum options in Assembly B
    var assemblyB = CreateCompilationWithEnhancedEnumOption(@"
        namespace AssemblyB
        {
            [EnumOption(typeof(PaymentMethodBase))]
            public class CreditCard : PaymentMethodBase
            {
                public override string Name => ""Credit Card"";
            }
        }");

    // Test generation in main assembly
    var mainSource = @"
        using AssemblyA;
        namespace TestApp
        {
            public class PaymentService
            {
                public void ProcessPayment(PaymentMethodBase method)
                {
                    var allMethods = PaymentMethods.All;
                }
            }
        }";

    // Act
    var result = RunGeneratorWithReferences(
        new[] { mainSource },
        assemblyA.ToMetadataReference(),
        assemblyB.ToMetadataReference());

    // Assert
    result.GeneratedSources.ShouldContainKey("PaymentMethods.g.cs");
}
```

### 5. Testing Factory Pattern Support

```csharp
[Fact]
public void GeneratorShouldSupportFactoryPatternWhenEnabled()
{
    var sources = new[]
    {
        @"
        [EnhancedEnumOption(UseFactory = true)]
        public abstract class ShapeBase
        {
            public abstract double Area { get; }
        }

        [EnumOption(typeof(ShapeBase))]
        public class Circle : ShapeBase
        {
            private readonly double _radius;
            public Circle(double radius) => _radius = radius;
            public override double Area => Math.PI * _radius * _radius;
        }"
    };

    var result = RunGeneratorWithAssemblyScanning(sources);

    ExpectationsFactory.ExpectCode(result.GeneratedSources["Shapes.g.cs"])
        .HasNamespace("TestNamespace", ns => ns
            .HasInterface("IShapeFactory", i => i
                .IsPublic()
                .HasMethod("Create", m => m.HasReturnType("ShapeBase[]")))
            .HasClass("Shapes", c => c
                .HasMethod("RegisterFactory", m => m
                    .IsPublic()
                    .IsStatic()
                    .HasParameter("factory", p => p.HasType("IShapeFactory")))))
        .Assert();
}
```

## ExpectationsFactory API Reference

### Namespace Expectations
```csharp
ExpectationsFactory.ExpectCode(code)
    .HasNamespace("NamespaceName", ns => ns
        .HasUsing("System")
        .HasUsing("System.Collections.Generic")
        .HasClass("ClassName", classExpectations)
        .HasInterface("IInterfaceName", interfaceExpectations)
        .HasEnum("EnumName", enumExpectations));
```

### Class Expectations
```csharp
.HasClass("ClassName", c => c
    .IsPublic()
    .IsStatic()
    .IsPartial()
    .IsSealed()
    .IsAbstract()
    .HasBaseType("BaseClassName")
    .ImplementsInterface("IInterfaceName")
    .HasConstructor(ctorExpectations)
    .HasMethod("MethodName", methodExpectations)
    .HasProperty("PropertyName", propertyExpectations)
    .HasField("_fieldName", fieldExpectations)
    .HasStaticConstructor());
```

### Method Expectations
```csharp
.HasMethod("MethodName", m => m
    .IsPublic()
    .IsPrivate()
    .IsProtected()
    .IsInternal()
    .IsStatic()
    .IsOverride()
    .IsVirtual()
    .IsAsync()
    .HasReturnType("ReturnType")
    .HasParameter("paramName", p => p
        .HasType("string")
        .HasDefaultValue("\"default\""))
    .HasNoParameters()
    .HasBody()
    .HasGenericParameter("T", g => g
        .HasConstraint("class")));
```

### Property Expectations
```csharp
.HasProperty("PropertyName", p => p
    .IsPublic()
    .IsStatic()
    .IsOverride()
    .HasType("string")
    .HasGetter()
    .HasSetter()
    .HasInitSetter()
    .IsAutoProperty()
    .IsReadOnly());
```

### Field Expectations
```csharp
.HasField("_fieldName", f => f
    .IsPrivate()
    .IsPublic()
    .IsStatic()
    .IsReadOnly()
    .HasType("int")
    .HasInitializer()
    .HasInitializer("42"));
```

## Common Test Scenarios

### 1. Testing Lookup Methods
```csharp
[Fact]
public void GeneratorShouldGenerateLookupMethodsForMarkedProperties()
{
    var sources = new[]
    {
        @"
        [EnhancedEnumOption]
        public abstract class CountryBase
        {
            [EnumLookup]
            public abstract string IsoCode { get; }
            
            [EnumLookup(AllowMultiple = true)]
            public abstract string[] Languages { get; }
        }"
    };

    var result = RunGeneratorWithAssemblyScanning(sources);

    ExpectationsFactory.ExpectCode(result.GeneratedSources["Countries.g.cs"])
        .HasNamespace("TestNamespace", ns => ns
            .HasClass("Countries", c => c
                .HasMethod("GetByIsoCode", m => m
                    .HasReturnType("CountryBase?")
                    .HasParameter("isoCode", p => p.HasType("string")))
                .HasMethod("GetByLanguages", m => m
                    .HasReturnType("IEnumerable<CountryBase>")
                    .HasParameter("languages", p => p.HasType("string")))))
        .Assert();
}
```

### 2. Testing Invalid Collection Names
```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public void GeneratorShouldHandleInvalidCollectionNames(string? collectionName)
{
    var sources = new[]
    {
        $@"
        [EnhancedEnumOption(CollectionName = ""{collectionName}"")]
        public abstract class ItemBase {{ }}"
    };

    var result = RunGeneratorWithAssemblyScanning(sources);

    // Should use default collection name
    result.GeneratedSources.ShouldContainKey("Items.g.cs");
}
```

### 3. Testing Nested Types
```csharp
[Fact]
public void GeneratorShouldHandleNestedNamespacesAndTypes()
{
    var sources = new[]
    {
        @"
        namespace Company.Product.Domain.Models
        {
            [EnhancedEnumOption]
            public abstract class OrderStatusBase { }

            public static class OrderStatuses
            {
                [EnumOption(typeof(OrderStatusBase))]
                public class Pending : OrderStatusBase { }
            }
        }"
    };

    var result = RunGeneratorWithAssemblyScanning(sources);

    result.GeneratedSources.ShouldContainKey("OrderStatuses_1.g.cs");
}
```

## Debugging Tests

### 1. Enable Generated File Output
Add to test project:
```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

### 2. Log Generated Code
```csharp
protected void LogGeneratedCode(string fileName, string code)
{
    System.Diagnostics.Debug.WriteLine($"=== Generated: {fileName} ===");
    System.Diagnostics.Debug.WriteLine(code);
    System.Diagnostics.Debug.WriteLine("=== End ===");
}
```

### 3. Check Diagnostics
```csharp
// Log all diagnostics for debugging
foreach (var diagnostic in result.Diagnostics)
{
    Console.WriteLine($"{diagnostic.Severity}: {diagnostic.GetMessage()}");
}
```

## Best Practices

1. **Use Assembly Scanning**: Always include `[assembly: EnableAssemblyScanner]` in test sources
2. **Test Structure, Not Text**: Use ExpectationsFactory instead of string comparisons
3. **Validate Diagnostics**: Always check that no error diagnostics are produced
4. **Test Edge Cases**: Empty enums, no namespace, invalid configurations
5. **Use Theory Tests**: For testing multiple similar scenarios
6. **Keep Tests Focused**: Each test should validate one specific aspect
7. **Use Descriptive Names**: Test names should clearly indicate what they test

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~GeneratorShouldGenerateCollectionClass"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Troubleshooting

### Common Issues

1. **"Type or namespace does not exist"**
   - Ensure generator project is referenced correctly
   - Check that all dependencies are included

2. **Tests fail with "No source was generated"**
   - Verify `[assembly: EnableAssemblyScanner]` is present
   - Check that input source code is valid

3. **ExpectationsFactory assertions fail**
   - Use `LogGeneratedCode` to see actual output
   - Check for typos in expected names/types

4. **Compilation errors in tests**
   - Ensure all required references are included
   - Check namespace imports

### Getting Help

For issues or questions:
1. Check the SmartGenerators documentation
2. Review existing tests for examples
3. Enable diagnostic output in the generator
4. Use the debugger to step through generator execution