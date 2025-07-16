# FractalDataWorks Enhanced Enums

Part of the FractalDataWorks toolkit.

## Build Status

[![Master Build](https://github.com/CyberdineDevelopment/Enhanced-Enums/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/CyberdineDevelopment/Enhanced-Enums/actions/workflows/ci.yml)
[![Develop Build](https://github.com/CyberdineDevelopment/Enhanced-Enums/actions/workflows/ci.yml/badge.svg?branch=develop)](https://github.com/CyberdineDevelopment/Enhanced-Enums/actions/workflows/ci.yml)

## Release Status

![GitHub release (latest by date)](https://img.shields.io/github/v/release/CyberdineDevelopment/Enhanced-Enums)
![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/CyberdineDevelopment/Enhanced-Enums?include_prereleases&label=pre-release)

## Package Status

![Nuget](https://img.shields.io/nuget/v/FractalDataWorks.EnhancedEnums)
![GitHub Packages](https://img.shields.io/badge/github%20packages-available-blue)

## License

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Advanced enumeration system with source generation for .NET applications.

## Overview

FractalDataWorks Enhanced Enums provides a powerful alternative to standard C# enums, offering:

- **Type-safe enumeration patterns** with compile-time validation
- **Source generation** for boilerplate code reduction
- **High-performance lookups** with zero allocations and dictionary-based O(1) access
- **Rich metadata** support through attributes
- **Cross-assembly support** for shared enum definitions
- **Empty value pattern** for representing "no selection" scenarios
- **Static property accessors** for direct enum value access (e.g., `OrderStatuses.Pending`)

## Installation

### NuGet Package
```bash
dotnet add package FractalDataWorks.EnhancedEnums
```

### Project Reference
When referencing the source generator as a project (e.g., for local development or benchmarks), you must configure it properly:

```xml
<ItemGroup>
  <ProjectReference Include="path\to\FractalDataWorks.EnhancedEnums.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="true" />
</ItemGroup>
```

**Important:** 
- `OutputItemType="Analyzer"` - Registers the project as a Roslyn analyzer/source generator
- `ReferenceOutputAssembly="true"` - Ensures the attributes are available at runtime

Without proper configuration, the source generator won't run and the collection classes won't be generated.

## Quick Start

### Define an Enhanced Enum

```csharp
using FractalDataWorks.EnhancedEnums.Attributes;

[EnhancedEnumOption]
public abstract class OrderStatus
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    
    [EnumLookup]
    public abstract string Code { get; }
}

[EnumOption]
public class Pending : OrderStatus
{
    public override string Name => "Pending";
    public override string Description => "Order is pending processing";
    public override string Code => "PEND";
}

[EnumOption]
public class Processing : OrderStatus
{
    public override string Name => "Processing";
    public override string Description => "Order is being processed";
    public override string Code => "PROC";
}

[EnumOption]
public class Shipped : OrderStatus
{
    public override string Name => "Shipped";
    public override string Description => "Order has been shipped";
    public override string Code => "SHIP";
}
```

### Use the Generated Collection

The source generator creates a static collection class with lookup methods:

```csharp
// Get all values
foreach (var status in OrderStatuses.All)
{
    Console.WriteLine($"{status.Name}: {status.Description}");
}

// Lookup by name
var pending = OrderStatuses.GetByName("Pending");

// Lookup by custom property (marked with [EnumLookup])
var shipped = OrderStatuses.GetByCode("SHIP");

// Static property accessors for common values
var pending = OrderStatuses.Pending;
var shipped = OrderStatuses.Shipped;

// Handle not found cases
var unknown = OrderStatuses.GetByName("Unknown"); // Returns null

// Get empty/none value
var empty = OrderStatuses.Empty; // Singleton with default values
```

### Generated Code Example

The generator produces optimized code like this:

```csharp
public static class OrderStatuses
{
    private static readonly List<OrderStatus> _all = new List<OrderStatus>();
    private static readonly ImmutableArray<OrderStatus> _cachedAll;
    private static readonly Dictionary<string, OrderStatus> _nameDict;
    private static readonly Dictionary<string, OrderStatus> _codeDict;
    private static readonly EmptyValue _empty = new EmptyValue();
    
    static OrderStatuses()
    {
        _all.Add(new Pending());
        _all.Add(new Processing());
        _all.Add(new Shipped());
        
        // Cache for zero-allocation access
        _cachedAll = _all.ToImmutableArray();
        
        // Build dictionaries for O(1) lookups
        _nameDict = _all.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _codeDict = _all.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Gets all available OrderStatus values.
    /// </summary>
    public static ImmutableArray<OrderStatus> All => _cachedAll; // Zero allocations!
    
    /// <summary>
    /// Gets an empty instance representing no selection.
    /// </summary>
    public static OrderStatus Empty => _empty;
    
    /// <summary>
    /// Gets the OrderStatus with the specified name.
    /// </summary>
    public static OrderStatus? GetByName(string name)
    {
        _nameDict.TryGetValue(name, out var result);
        return result; // O(1) lookup, zero allocations
    }
    
    /// <summary>
    /// Gets the OrderStatus with the specified Code.
    /// </summary>
    public static OrderStatus? GetByCode(string code)
    {
        _codeDict.TryGetValue(code, out var result);
        return result; // O(1) lookup, zero allocations
    }
    
    // Static property accessors
    public static OrderStatus Pending => _cachedAll[0];
    public static OrderStatus Processing => _cachedAll[1];
    public static OrderStatus Shipped => _cachedAll[2];
    
    private sealed class EmptyValue : OrderStatus
    {
        public override string Name => string.Empty;
        public override string Description => string.Empty;
        public override string Code => string.Empty;
    }
}
```

## Performance

Enhanced Enums are optimized for high-performance scenarios:

- **Zero allocations** for all lookup operations
- **O(1) dictionary-based lookups** instead of O(n) linear search
- **Cached All property** eliminates repeated array allocations
- **FrozenDictionary support** on .NET 8+ for additional 35% performance improvement
- **8-10x faster lookups** compared to traditional LINQ-based searches

Benchmark results show:
- Name lookups: ~850ns → ~93ns (9x faster)
- All property access: ~57ns + 424B allocation → ~0.06ns + 0B allocation (950x faster)
- Memory usage: 528B per lookup → 0B per lookup

## Features

### Lookup Properties

Mark properties with `[EnumLookup]` to generate lookup methods:

```csharp
[EnhancedEnumOption]
public abstract class Country
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract string IsoCode { get; }
    
    [EnumLookup]
    public abstract string Currency { get; }
}

[EnumOption]
public class UnitedStates : Country
{
    public override string Name => "United States";
    public override string IsoCode => "US";
    public override string Currency => "USD";
}

// Usage:
var country = Countries.GetByIsoCode("US");
var byCurrency = Countries.GetByCurrency("USD");
```

### Custom Collection Names

```csharp
[EnhancedEnumOption("MyStatuses")]
public abstract class StatusBase
{
    public abstract string Name { get; }
}

// Generates: MyStatuses.All, MyStatuses.GetByName()
```

### String Comparison Options

```csharp
[EnhancedEnumOption(NameComparison = StringComparison.Ordinal)]
public abstract class CaseSensitive
{
    public abstract string Name { get; }
}
```

### Factory Pattern Support

```csharp
[EnhancedEnumOption(UseFactory = true)]
public abstract class ConnectionType
{
    public abstract string Name { get; }
    
    // Factory method must be defined in base class
    public static ConnectionType Create(Type type)
    {
        return (ConnectionType)Activator.CreateInstance(type);
    }
}
```

### Multiple Lookup Properties

```csharp
[EnhancedEnumOption]
public abstract class Currency
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract string Code { get; }
    
    [EnumLookup]
    public abstract string Symbol { get; }
    
    [EnumLookup]
    public abstract int NumericCode { get; }
}

// Generates: GetByCode(), GetBySymbol(), GetByNumericCode()
```

### Empty Value Pattern

Every generated collection includes an Empty singleton for representing "no selection":

```csharp
// Get the empty value
var none = OrderStatuses.Empty;

// Check if a value is empty
if (status == OrderStatuses.Empty)
{
    Console.WriteLine("No status selected");
}

// Empty returns appropriate defaults:
// - string properties return string.Empty
// - numeric properties return 0
// - DateTime returns DateTime.MinValue
// - Guid returns Guid.Empty
// - nullable types return null
```

### Static Property Accessors

Generated collections include static properties for direct access to enum values:

```csharp
// Direct access via static properties
var pending = OrderStatuses.Pending;
var shipped = OrderStatuses.Shipped;

// Properties are named based on the enum value's Name
// Use [EnumOption(Name = "CustomName")] to control the property name
```

### Custom Lookup Method Names

```csharp
[EnhancedEnumOption]
public abstract class UserRole
{
    public abstract string Name { get; }
    
    [EnumLookup(MethodName = "FindByLevel")]
    public abstract int PermissionLevel { get; }
}

// Generates: FindByLevel() instead of GetByPermissionLevel()
```

## Best Practices

1. **Use descriptive names** for enum options and properties
2. **Keep enums focused** - avoid too many responsibilities
3. **Consider lookup patterns** - mark frequently searched properties with `[EnumLookup]`
4. **Use factory pattern** sparingly - only when you need fresh instances
5. **Document your enums** with XML comments
6. **Leverage static properties** for commonly accessed values to improve readability
7. **Use Empty value** instead of null for representing "no selection"
8. **Target .NET 8+** when possible for FrozenDictionary performance benefits

## Requirements

- **.NET Standard 2.0** or higher
- **C# 8.0** or higher for nullable reference types
- **Visual Studio 2022** or VS Code with C# extension for best experience

## Migration from Traditional Enums

### Before (Traditional Enum)
```csharp
public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4
}
```

### After (Enhanced Enum)
```csharp
[EnhancedEnumOption]
public abstract class OrderStatus
{
    public abstract string Name { get; }
    public abstract int Value { get; }
}

[EnumOption]
public class Pending : OrderStatus
{
    public override string Name => "Pending";
    public override int Value => 1;
}

[EnumOption]
public class Processing : OrderStatus
{
    public override string Name => "Processing";
    public override int Value => 2;
}
```

## Advanced Usage

### Cross-Assembly Support

Enhanced enums can be used across assembly boundaries by referencing the generated collection classes.

### Nullable Properties

```csharp
[EnhancedEnumOption]
public abstract class Config
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract string? OptionalKey { get; }
}
```

### Complex Property Types

```csharp
[EnhancedEnumOption]
public abstract class DateRange
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract DateTime StartDate { get; }
    
    [EnumLookup]
    public abstract Guid Id { get; }
}
```

## Troubleshooting

### Source Generator Not Running

If your enhanced enum collection classes are not being generated:

1. **Check Project Reference Configuration**
   ```xml
   <!-- Correct -->
   <ProjectReference Include="..\..\src\FractalDataWorks.EnhancedEnums\FractalDataWorks.EnhancedEnums.csproj" 
                     OutputItemType="Analyzer" 
                     ReferenceOutputAssembly="true" />
   
   <!-- Incorrect - Generator won't run -->
   <ProjectReference Include="..\..\src\FractalDataWorks.EnhancedEnums\FractalDataWorks.EnhancedEnums.csproj" />
   ```

2. **Enable Assembly Scanner**
   Add to your AssemblyInfo.cs or any source file:
   ```csharp
   using FractalDataWorks.SmartGenerators;
   
   [assembly: EnableAssemblyScanner]
   ```

3. **Check Build Output**
   Look for warnings like:
   - `CS8032: An instance of analyzer ... cannot be created`
   - `CS0103: The name 'YourEnums' does not exist in the current context`

### Common Issues

- **Missing Dependencies**: The generator requires SmartGenerators dependencies. When using PackageReference, these are included automatically. With ProjectReference, you may need to ensure dependencies are available.
- **Case Sensitivity**: By default, name lookups are case-insensitive. Use `NameComparison` attribute property to change this.
- **Nullable Reference Types**: Generated code may produce warnings in projects with nullable reference types enabled. This is a known issue that doesn't affect functionality.

## Contributing

See our [Contributing Guide](CONTRIBUTING.md) for details on how to contribute to this project.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.