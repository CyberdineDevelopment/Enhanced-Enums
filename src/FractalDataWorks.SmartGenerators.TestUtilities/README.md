# FractalDataWorks.SmartGenerators.TestUtilities

Comprehensive testing framework for Roslyn source generators with compilation helpers, assertions, and expectations API.

## Installation

```bash
dotnet add package FractalDataWorks.SmartGenerators.TestUtilities
```

## Features

- **Generator Testing** - Run generators and verify output
- **Compilation Building** - Create test compilations easily
- **Expectations API** - Assert code structure and content
- **Syntax Assertions** - Verify generated syntax trees
- **Multi-Generator Support** - Test multiple generators together

## Usage

### Basic Generator Testing

```csharp
using FractalDataWorks.SmartGenerators.TestUtilities;
using Xunit;
using Shouldly;

public class MyGeneratorTests
{
    [Fact]
    public void Generator_ProducesExpectedOutput()
    {
        // Arrange
        var source = @"
            [GenerateBuilder]
            public class Person
            {
                public string Name { get; set; }
                public int Age { get; set; }
            }";

        // Act
        var output = SourceGeneratorTestHelper.RunGenerator<MyGenerator>(source);
        
        // Assert
        output.ShouldNotBeNull();
        output.ShouldContain("public class PersonBuilder");
        output.ShouldContain("public PersonBuilder WithName(string name)");
    }
}
```

### Using Expectations API

```csharp
[Fact]
public void Generator_CreatesCorrectStructure()
{
    var source = GetTestSource();
    var output = SourceGeneratorTestHelper.RunGenerator<MyGenerator>(source);
    
    // Parse and verify structure
    var tree = CSharpSyntaxTree.ParseText(output);
    var expectations = new SyntaxTreeExpectations(tree);
    
    expectations
        .HasNamespace("MyApp.Builders")
        .HasClass("PersonBuilder", c => c
            .IsPublic()
            .IsPartial()
            .HasMethod("WithName", m => m
                .IsPublic()
                .HasParameter("name", p => p.HasType("string"))
                .ReturnsType("PersonBuilder"))
            .HasMethod("Build", m => m
                .ReturnsType("Person")))
        .Verify();
}
```

### Building Test Compilations

```csharp
[Fact]
public void Generator_HandlesComplexScenarios()
{
    // Build compilation with references
    var compilation = AssemblyCompilationBuilder.Create()
        .WithAssemblyName("TestAssembly")
        .AddSource(@"
            namespace TestNamespace
            {
                public interface IService { }
                public class Service : IService { }
            }")
        .AddReference<object>()
        .AddReference(typeof(IEnumerable<>).Assembly)
        .AddMetadataReference("System.Runtime")
        .Build();
    
    // Run generator
    var result = SourceGeneratorTestHelper.RunGenerator<MyGenerator>(compilation);
    
    // Verify compilation succeeds
    result.Compilation.GetDiagnostics()
        .Where(d => d.Severity == DiagnosticSeverity.Error)
        .ShouldBeEmpty();
}
```

### Testing Multiple Generators

```csharp
[Fact]
public void MultipleGenerators_WorkTogether()
{
    var source = @"
        [GenerateBuilder]
        [GenerateDto]
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }";
    
    var results = SourceGeneratorTestHelper.RunGenerators(
        new[] { source },
        typeof(BuilderGenerator),
        typeof(DtoGenerator));
    
    results.ShouldContainKey("Product.Builder.g.cs");
    results.ShouldContainKey("Product.Dto.g.cs");
}
```

### Generator Pipeline Testing

```csharp
[Fact]
public void Generator_Pipeline_ProducesExpectedOutput()
{
    var result = GeneratorPipelineBuilder.Create()
        .WithSource(@"
            [MyAttribute]
            public class TestClass { }")
        .AddGenerator<MyGenerator>()
        .AddReference<object>()
        .WithAssemblyName("TestAssembly")
        .Build()
        .Run();
    
    result.GeneratedSources.ShouldHaveSingleItem();
    result.Diagnostics.ShouldBeEmpty();
}
```

## Assertion Helpers

### Syntax Tree Assertions

```csharp
var tree = CSharpSyntaxTree.ParseText(generatedCode);

// Class assertions
var classDecl = tree.GetRoot()
    .DescendantNodes()
    .OfType<ClassDeclarationSyntax>()
    .ShouldHaveSingleItem();

classDecl.Identifier.Text.ShouldBe("MyClass");
classDecl.Modifiers.ShouldContain(m => m.IsKind(SyntaxKind.PublicKeyword));

// Method assertions
var methods = classDecl.Members.OfType<MethodDeclarationSyntax>();
methods.ShouldContain(m => m.Identifier.Text == "MyMethod");
```

### Compilation Assertions

```csharp
var compilation = CreateCompilation(source, generatedCode);

// No errors
compilation.GetDiagnostics()
    .Where(d => d.Severity == DiagnosticSeverity.Error)
    .ShouldBeEmpty();

// Type exists
var type = compilation.GetTypeByMetadataName("MyNamespace.MyClass");
type.ShouldNotBeNull();
type.GetMembers("MyMethod").ShouldNotBeEmpty();
```

## Test Helpers

### Creating Test Sources

```csharp
public class TestSources
{
    public static string SimpleClass => @"
        namespace Test
        {
            public class SimpleClass
            {
                public int Id { get; set; }
            }
        }";
    
    public static string WithAttribute(string attributeName) => $@"
        [{attributeName}]
        public class TestClass {{ }}";
}
```

### Compilation Verification

```csharp
public static class CompilationVerifier
{
    public static void VerifyNoErrors(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error);
        
        if (errors.Any())
        {
            var errorMessages = string.Join("\n", errors.Select(e => e.ToString()));
            throw new Exception($"Compilation failed:\n{errorMessages}");
        }
    }
}
```

### Mock Types for Testing

```csharp
// Create mock types for testing
var mockAttribute = TestMocks.CreateAttributeSymbol("TestAttribute");
var mockClass = TestMocks.CreateNamedTypeSymbol("TestClass");
var mockMethod = TestMocks.CreateMethodSymbol("TestMethod");
```

## Best Practices

1. **Test in Isolation** - Each test should be independent
2. **Use Meaningful Names** - Test names should describe what they verify
3. **Test Edge Cases** - Include tests for error scenarios
4. **Verify Compilation** - Always check that generated code compiles
5. **Use Expectations** - Leverage the expectations API for maintainable tests

## Advanced Scenarios

### Testing Incremental Generation

```csharp
[Fact]
public void Generator_IsIncremental()
{
    var source1 = "public class Class1 { }";
    var source2 = "[Generate] public class Class2 { }";
    
    // First run
    var driver = CSharpGeneratorDriver.Create(new MyGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output1, out _);
    
    // Second run with additional source
    var newCompilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source2));
    driver = driver.RunGeneratorsAndUpdateCompilation(newCompilation, out var output2, out _);
    
    // Verify incremental behavior
    output2.SyntaxTrees.Count().ShouldBeGreaterThan(output1.SyntaxTrees.Count());
}
```

### Testing Diagnostics

```csharp
[Fact]
public void Generator_ReportsDiagnostics()
{
    var source = @"
        [Invalid]
        public class TestClass { }";
    
    var (output, diagnostics) = SourceGeneratorTestHelper.RunGeneratorWithDiagnostics<MyGenerator>(source);
    
    diagnostics.ShouldContain(d => 
        d.Id == "GEN001" && 
        d.Severity == DiagnosticSeverity.Warning);
}
```

## License

Apache License 2.0