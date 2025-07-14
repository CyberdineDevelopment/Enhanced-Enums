# EnhancedEnumOptions Testing Patterns and Best Practices

This document outlines the testing patterns, methodologies, and best practices for testing the EnhancedEnumOptions source generator using SmartGenerators testing utilities.

## Core Testing Philosophy

### Structure Over Text
The primary principle is to test the **structure** of generated code rather than its exact text representation. This makes tests:
- More resilient to formatting changes
- Clearer in their intent
- Easier to maintain
- Better focused on semantic correctness

### Semantic Validation
Use the `ExpectationsFactory` API to validate:
- Namespace organization
- Type declarations (classes, interfaces, enums)
- Member signatures (methods, properties, fields)
- Access modifiers and type modifiers
- Inheritance and interface implementation

## SmartGenerators Testing Framework

### Key Components

#### 1. SourceGeneratorTestHelper
The main utility for running generators and capturing output:

```csharp
public static class SourceGeneratorTestHelper
{
    // Run generator and get generated files
    public static Dictionary<string, string> RunGenerator(
        IIncrementalGenerator generator,
        string[] sources,
        out ImmutableArray<Diagnostic> diagnostics,
        params MetadataReference[] additionalReferences);

    // Run generator and compile the result
    public static (Compilation, GeneratorRunResult) RunGeneratorAndCompile(
        IIncrementalGenerator generator,
        string[] sources,
        params MetadataReference[] additionalReferences);

    // Get syntax tree from generated output
    public static SyntaxTree GetSyntaxTree(
        Dictionary<string, string> generatedOutput,
        string hintName);
}
```

#### 2. ExpectationsFactory
The fluent API for structural assertions:

```csharp
public static class ExpectationsFactory
{
    // Create expectations from code string
    public static SyntaxTreeExpectations ExpectCode(string code);
    
    // Create expectations from generated output
    public static SyntaxTreeExpectations ExpectFile(
        Dictionary<string, string> output,
        string fileName);
}
```

#### 3. Expectation Classes
- `SyntaxTreeExpectations`: Root expectations for a syntax tree
- `NamespaceExpectations`: Validate namespace structure
- `ClassExpectations`: Validate class declarations
- `InterfaceExpectations`: Validate interface declarations
- `EnumExpectations`: Validate enum declarations
- `MethodExpectations`: Validate method signatures
- `PropertyExpectations`: Validate property declarations
- `FieldExpectations`: Validate field declarations
- `ParameterExpectations`: Validate parameter definitions

### Testing Workflow

1. **Arrange**: Create source code input
2. **Act**: Run the generator
3. **Assert**: Validate structure using ExpectationsFactory

## EnhancedEnumOptions-Specific Patterns

### Pattern 1: Basic Enum Generation
```csharp
[Fact]
public void BasicEnumGeneration()
{
    // Arrange
    var source = @"
        using FractalDataWorks.EnhancedEnumOptions.Attributes;
        using FractalDataWorks.SmartGenerators.AssemblyScanning;

        [assembly: EnableAssemblyScanner]

        namespace TestNamespace
        {
            [EnhancedEnumOption]
            public abstract class StatusBase
            {
                public abstract string Name { get; }
            }

            [EnumOption(typeof(StatusBase))]
            public class Active : StatusBase
            {
                public override string Name => ""Active"";
            }

            [EnumOption(typeof(StatusBase))]
            public class Inactive : StatusBase
            {
                public override string Name => ""Inactive"";
            }
        }";

    // Act
    var generator = new EnhancedEnumOptionGenerator();
    var output = SourceGeneratorTestHelper.RunGenerator(
        generator,
        new[] { source },
        out var diagnostics);

    // Assert - No errors
    diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
        .ShouldBeEmpty();

    // Assert - File generated
    output.ShouldContainKey("Statuses.g.cs");

    // Assert - Structure
    ExpectationsFactory.ExpectCode(output["Statuses.g.cs"])
        .HasNamespace("TestNamespace", ns => ns
            .HasClass("Statuses", c => c
                .IsPublic()
                .IsStatic()
                .HasProperty("All", p => p
                    .IsPublic()
                    .IsStatic()
                    .HasType("IReadOnlyList<StatusBase>")
                    .HasGetter())
                .HasMethod("GetByName", m => m
                    .IsPublic()
                    .IsStatic()
                    .HasReturnType("StatusBase?")
                    .HasParameter("name", p => p.HasType("string")))
                .HasStaticConstructor()))
        .Assert();
}
```

### Pattern 2: Factory Pattern Support
```csharp
[Fact]
public void FactoryPatternGeneration()
{
    // Test UseFactory = true configuration
    var source = @"
        [EnhancedEnumOption(UseFactory = true)]
        public abstract class WidgetBase
        {
            public abstract void Process();
        }";

    var output = RunGenerator(source);

    ExpectationsFactory.ExpectCode(output["Widgets.g.cs"])
        .HasNamespace("TestNamespace", ns => ns
            .HasInterface("IWidgetFactory", i => i
                .IsPublic()
                .HasMethod("Create", m => m
                    .HasReturnType("WidgetBase[]")))
            .HasClass("Widgets", c => c
                .HasMethod("RegisterFactory", m => m
                    .IsPublic()
                    .IsStatic()
                    .HasReturnType("void")
                    .HasParameter("factory", p => p
                        .HasType("IWidgetFactory")))))
        .Assert();
}
```

### Pattern 3: Lookup Method Generation
```csharp
[Fact]
public void LookupMethodGeneration()
{
    var source = @"
        [EnhancedEnumOption]
        public abstract class ProductBase
        {
            [EnumLookup]
            public abstract string Sku { get; }
            
            [EnumLookup(AllowMultiple = true)]
            public abstract string[] Categories { get; }
        }";

    var output = RunGenerator(source);

    ExpectationsFactory.ExpectCode(output["Products.g.cs"])
        .HasClass("Products", c => c
            .HasMethod("GetBySku", m => m
                .HasReturnType("ProductBase?")
                .HasParameter("sku", p => p.HasType("string")))
            .HasMethod("GetByCategories", m => m
                .HasReturnType("IEnumerable<ProductBase>")
                .HasParameter("categories", p => p.HasType("string"))))
        .Assert();
}
```

### Pattern 4: Cross-Assembly Testing
```csharp
[Fact]
public void CrossAssemblyEnumOptions()
{
    // Step 1: Create base assembly
    var baseAssemblySource = @"
        namespace BaseAssembly
        {
            [EnhancedEnumOption]
            public abstract class PluginBase
            {
                public abstract string Name { get; }
            }
        }";
    
    var baseCompilation = CreateCompilation(baseAssemblySource);
    
    // Step 2: Create extension assembly
    var extensionSource = @"
        using BaseAssembly;
        
        namespace ExtensionAssembly
        {
            [EnumOption(typeof(PluginBase))]
            public class MyPlugin : PluginBase
            {
                public override string Name => ""MyPlugin"";
            }
        }";
    
    var extensionCompilation = CreateCompilation(
        extensionSource,
        baseCompilation.ToMetadataReference());
    
    // Step 3: Test generation in main assembly
    var mainSource = @"
        using BaseAssembly;
        
        namespace MainApp
        {
            public class PluginManager
            {
                public void LoadPlugins()
                {
                    var plugins = Plugins.All;
                }
            }
        }";
    
    var output = RunGeneratorWithReferences(
        new[] { mainSource },
        baseCompilation.ToMetadataReference(),
        extensionCompilation.ToMetadataReference());
    
    output.ShouldContainKey("Plugins.g.cs");
}
```

## Error Scenario Testing

### Pattern 5: Missing Assembly Scanner
```csharp
[Fact]
public void MissingAssemblyScanner_ProducesWarning()
{
    var source = @"
        // Note: No [assembly: EnableAssemblyScanner]
        
        [EnhancedEnumOption]
        public abstract class TypeBase { }";
    
    var output = RunGenerator(source, out var diagnostics);
    
    diagnostics.ShouldContain(d => 
        d.Id == "EE001" && 
        d.Severity == DiagnosticSeverity.Warning &&
        d.GetMessage().Contains("EnableAssemblyScanner"));
}
```

### Pattern 6: Invalid Configuration
```csharp
[Theory]
[InlineData("")] // Empty collection name
[InlineData(null)] // Null collection name
[InlineData("123Invalid")] // Invalid identifier
public void InvalidCollectionName_UsesDefault(string? collectionName)
{
    var source = $@"
        [EnhancedEnumOption(CollectionName = ""{collectionName}"")]
        public abstract class ItemBase {{ }}";
    
    var output = RunGenerator(source);
    
    // Should generate with default name
    output.ShouldContainKey("Items.g.cs");
}
```

## Incremental Generation Testing

### Pattern 7: Testing Incremental Behavior
```csharp
[Fact]
public void IncrementalGeneration_CachesUnchangedOutput()
{
    var source = @"
        [EnhancedEnumOption]
        public abstract class StateBase { }";
    
    var generator = new EnhancedEnumOptionGenerator();
    var driver = CSharpGeneratorDriver.Create(generator);
    
    // First run
    var compilation1 = CreateCompilation(source);
    driver = driver.RunGeneratorsAndUpdateCompilation(
        compilation1, out _, out _);
    
    // Second run with unrelated change
    var unrelatedSource = "public class Unrelated { }";
    var compilation2 = compilation1.AddSyntaxTrees(
        CSharpSyntaxTree.ParseText(unrelatedSource));
    
    driver = driver.RunGeneratorsAndUpdateCompilation(
        compilation2, out _, out _);
    
    // Verify caching
    var runResult = driver.GetRunResult();
    runResult.Results[0].TrackedSteps
        .SelectMany(s => s.Value)
        .ShouldContain(step => 
            step.Outputs.Any(o => 
                o.Reason == IncrementalStepRunReason.Cached));
}
```

## Test Organization Patterns

### Pattern 8: Test Fixture Organization
```csharp
public class EnhancedEnumOptionGeneratorTests : EnhancedEnumOptionTestBase
{
    // Group by feature
    #region Basic Generation Tests
    [Fact]
    public void GeneratesCollectionClass() { }
    
    [Fact]
    public void GeneratesAllProperty() { }
    #endregion
    
    #region Factory Pattern Tests
    [Fact]
    public void GeneratesFactoryInterface() { }
    
    [Fact]
    public void GeneratesRegisterFactoryMethod() { }
    #endregion
    
    #region Lookup Method Tests
    [Fact]
    public void GeneratesSimpleLookupMethod() { }
    
    [Fact]
    public void GeneratesMultiValueLookupMethod() { }
    #endregion
}
```

### Pattern 9: Parameterized Testing
```csharp
[Theory]
[MemberData(nameof(GetEnumConfigurations))]
public void GeneratesCorrectOutput_ForVariousConfigurations(
    string enumName,
    bool useFactory,
    string expectedFileName)
{
    var source = $@"
        [EnhancedEnumOption(UseFactory = {useFactory.ToString().ToLower()})]
        public abstract class {enumName}Base {{ }}";
    
    var output = RunGenerator(source);
    
    output.ShouldContainKey(expectedFileName);
}

public static IEnumerable<object[]> GetEnumConfigurations()
{
    yield return new object[] { "Status", false, "Statuses.g.cs" };
    yield return new object[] { "Category", true, "Categories.g.cs" };
    yield return new object[] { "Type", false, "Types.g.cs" };
}
```

## Advanced Testing Patterns

### Pattern 10: Compilation and Runtime Testing
```csharp
[Fact]
public void GeneratedCode_CompilesAndExecutes()
{
    var source = @"
        [EnhancedEnumOption]
        public abstract class ColorBase
        {
            public abstract string Name { get; }
        }
        
        [EnumOption(typeof(ColorBase))]
        public class Red : ColorBase
        {
            public override string Name => ""Red"";
        }";
    
    // Generate and compile
    var (compilation, diagnostics) = CompileWithGenerator(source);
    
    diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
        .ShouldBeEmpty();
    
    // Load and execute
    using var ms = new MemoryStream();
    var emitResult = compilation.Emit(ms);
    emitResult.Success.ShouldBeTrue();
    
    ms.Seek(0, SeekOrigin.Begin);
    var assembly = Assembly.Load(ms.ToArray());
    
    // Test runtime behavior
    var colorsType = assembly.GetType("TestNamespace.Colors");
    var allProperty = colorsType.GetProperty("All");
    var allColors = allProperty.GetValue(null) as IEnumerable;
    
    allColors.ShouldNotBeNull();
    allColors.Cast<object>().Count().ShouldBe(1);
}
```

### Pattern 11: Diagnostic Message Testing
```csharp
[Fact]
public void InvalidEnumOption_ProducesHelpfulDiagnostic()
{
    var source = @"
        [EnumOption(typeof(NonExistentBase))]
        public class InvalidOption { }";
    
    var output = RunGenerator(source, out var diagnostics);
    
    var diagnostic = diagnostics.Single(d => d.Id == "EE002");
    diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
    diagnostic.GetMessage().ShouldContain("NonExistentBase");
    diagnostic.Location.ShouldNotBeNull();
}
```

## Testing Best Practices

### 1. Test Naming Conventions
```csharp
// Pattern: Method_Scenario_ExpectedOutcome
[Fact]
public void GenerateCollection_WithEmptyEnum_GeneratesEmptyCollection() { }

[Fact]
public void GetByName_WithDuplicateNames_ReturnsFirstMatch() { }
```

### 2. Test Data Builders
```csharp
public class EnhancedEnumOptionSourceBuilder
{
    private string _namespace = "TestNamespace";
    private string _baseName = "TestBase";
    private bool _useFactory = false;
    private List<string> _options = new();
    
    public EnhancedEnumOptionSourceBuilder WithNamespace(string ns)
    {
        _namespace = ns;
        return this;
    }
    
    public EnhancedEnumOptionSourceBuilder WithFactory()
    {
        _useFactory = true;
        return this;
    }
    
    public EnhancedEnumOptionSourceBuilder WithOption(string name)
    {
        _options.Add(name);
        return this;
    }
    
    public string Build()
    {
        // Build source code
    }
}

// Usage
var source = new EnhancedEnumOptionSourceBuilder()
    .WithNamespace("MyApp")
    .WithFactory()
    .WithOption("Option1")
    .WithOption("Option2")
    .Build();
```

### 3. Assertion Helpers
```csharp
public static class EnhancedEnumOptionAssertions
{
    public static void ShouldHaveStandardCollectionStructure(
        this Dictionary<string, string> output,
        string fileName,
        string className,
        string itemType)
    {
        ExpectationsFactory.ExpectFile(output, fileName)
            .HasClass(className, c => c
                .IsPublic()
                .IsStatic()
                .HasProperty("All", p => p
                    .HasType($"IReadOnlyList<{itemType}>"))
                .HasMethod("GetByName", m => m
                    .HasReturnType($"{itemType}?")
                    .HasParameter("name", p => p.HasType("string"))))
            .Assert();
    }
}
```

### 4. Common Test Constants
```csharp
public static class TestSources
{
    public const string AssemblyScanner = 
        "[assembly: EnableAssemblyScanner]";
    
    public const string StandardUsings = @"
        using System;
        using System.Collections.Generic;
        using FractalDataWorks.EnhancedEnumOptions.Attributes;
        using FractalDataWorks.SmartGenerators.AssemblyScanning;";
    
    public static string BasicEnum(string name) => $@"
        {StandardUsings}
        {AssemblyScanner}
        
        namespace Test
        {{
            [EnhancedEnumOption]
            public abstract class {name}Base
            {{
                public abstract string Name {{ get; }}
            }}
        }}";
}
```

## Debugging Tips

### 1. Capture Generated Output
```csharp
[Fact]
public void Debug_CaptureGeneratedOutput()
{
    var source = "...";
    var output = RunGenerator(source);
    
    foreach (var (fileName, content) in output)
    {
        Console.WriteLine($"=== {fileName} ===");
        Console.WriteLine(content);
        Console.WriteLine("================");
    }
}
```

### 2. Diagnostic Analysis
```csharp
[Fact]
public void Debug_AnalyzeDiagnostics()
{
    var source = "...";
    var output = RunGenerator(source, out var diagnostics);
    
    foreach (var diagnostic in diagnostics)
    {
        Console.WriteLine($"[{diagnostic.Severity}] {diagnostic.Id}: {diagnostic.GetMessage()}");
        Console.WriteLine($"Location: {diagnostic.Location}");
    }
}
```

### 3. Incremental Pipeline Inspection
```csharp
[Fact]
public void Debug_InspectPipeline()
{
    var generator = new EnhancedEnumOptionGenerator();
    var driver = CSharpGeneratorDriver.Create(
        generator,
        driverOptions: new GeneratorDriverOptions(
            IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true));
    
    // Run and inspect
    var result = driver.GetRunResult();
    var steps = result.Results[0].TrackedSteps;
    
    foreach (var (name, stepResults) in steps)
    {
        Console.WriteLine($"Step: {name}");
        foreach (var step in stepResults)
        {
            Console.WriteLine($"  - Outputs: {step.Outputs.Length}");
        }
    }
}
```

## Summary

These patterns provide a comprehensive approach to testing the EnhancedEnumOptions generator:

1. **Structure-based validation** using ExpectationsFactory
2. **Comprehensive scenario coverage** including edge cases
3. **Cross-assembly testing** for distributed enum options
4. **Incremental generation verification** for performance
5. **Clear test organization** by feature and scenario
6. **Helpful debugging techniques** for troubleshooting

Following these patterns ensures robust, maintainable tests that effectively validate the generator's behavior while remaining resilient to implementation changes.