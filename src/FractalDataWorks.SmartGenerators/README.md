# FractalDataWorks.SmartGenerators

Core framework for building incremental source generators with best practices and utilities.

## Installation

```bash
dotnet add package FractalDataWorks.SmartGenerators
```

## Features

- **IncrementalGeneratorBase<T>** - Simplified base class for incremental generators
- **Diagnostic Reporting** - Built-in diagnostic helpers
- **Compilation Helpers** - Extension methods for working with compilations
- **Attribute Helpers** - Utilities for attribute-based generation

## Usage

### Create an Incremental Generator

```csharp
using FractalDataWorks.SmartGenerators;
using Microsoft.CodeAnalysis;

[Generator]
public class MyGenerator : IncrementalGeneratorBase<MyModel>
{
    protected override bool IsRelevantSyntax(SyntaxNode node)
    {
        // Filter to relevant syntax nodes
        return node is ClassDeclarationSyntax cds &&
               cds.AttributeLists.Any();
    }

    protected override MyModel? TransformSyntax(GeneratorSyntaxContext context)
    {
        // Transform syntax to your model
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        
        if (symbol == null) return null;
        
        return new MyModel
        {
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace.ToDisplayString()
        };
    }

    protected override void Execute(SourceProductionContext context, MyModel model)
    {
        // Generate code from your model
        var code = GenerateCode(model);
        context.AddSource($"{model.Name}.g.cs", code);
    }
}
```

### Report Diagnostics

```csharp
using FractalDataWorks.SmartGenerators;

// In your generator
var reporter = new DiagnosticReporter(context);

reporter.ReportInfo("Processing class: {0}", className);
reporter.ReportWarning(location, "Deprecated pattern detected");
reporter.ReportError(location, "Invalid configuration");
```

### Custom Generator Strategy

```csharp
public class MyGenerationStrategy : IGenerationStrategy
{
    public void Execute(SourceProductionContext context, IInputInfo input)
    {
        // Custom generation logic
    }
}
```

## API Reference

### IncrementalGeneratorBase<T>

Base class for creating incremental generators with a simplified pattern.

**Methods:**
- `IsRelevantSyntax(SyntaxNode node)` - Filter syntax nodes
- `TransformSyntax(GeneratorSyntaxContext context)` - Transform to model
- `Execute(SourceProductionContext context, T model)` - Generate code
- `RegisterSourceOutput(...)` - Override for custom registration

### DiagnosticReporter

Helper for reporting diagnostics during generation.

**Methods:**
- `ReportInfo(string message, params object[] args)`
- `ReportWarning(Location location, string message, params object[] args)`
- `ReportError(Location location, string message, params object[] args)`

### Extension Methods

**GeneratorExecutionContextExtensions:**
- `GetMSBuildProperty(string name)` - Get MSBuild property value
- `TryGetGlobalOption(string key, out T value)` - Get global analyzer option

## Best Practices

1. **Filter Early** - Use `IsRelevantSyntax` to filter out irrelevant nodes
2. **Pure Transformations** - Keep `TransformSyntax` side-effect free
3. **Handle Nulls** - Always check for null symbols and models
4. **Report Progress** - Use diagnostics to report generation progress
5. **Cache When Possible** - Leverage incremental generation caching

## License

Apache License 2.0