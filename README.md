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
- **Efficient lookup capabilities** for value retrieval
- **Rich metadata** support through attributes
- **Cross-assembly support** for shared enum definitions

## Installation

```bash
dotnet add package FractalDataWorks.EnhancedEnums
```

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

// Handle not found cases
var unknown = OrderStatuses.GetByName("Unknown"); // Returns null
```

### Generated Code Example

The generator produces code like this:

```csharp
public static class OrderStatuses
{
    private static readonly List<OrderStatus> _all = new List<OrderStatus>();
    
    static OrderStatuses()
    {
        _all.Add(new Pending());
        _all.Add(new Processing());
        _all.Add(new Shipped());
    }
    
    /// <summary>
    /// Gets all available OrderStatus values.
    /// </summary>
    public static ImmutableArray<OrderStatus> All => _all.ToImmutableArray();
    
    /// <summary>
    /// Gets the OrderStatus with the specified name.
    /// </summary>
    public static OrderStatus? GetByName(string name)
    {
        return _all.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Gets the OrderStatus with the specified Code.
    /// </summary>
    public static OrderStatus? GetByCode(string code)
    {
        return _all.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
    }
}
```

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

## Performance Characteristics

- **Initialization**: O(n) during static constructor
- **All property access**: O(n) array allocation on each access
- **Name lookups**: O(n) linear search through collection
- **Property lookups**: O(n) linear search through collection
- **Memory usage**: Stores all instances in memory

## Best Practices

1. **Use descriptive names** for enum options and properties
2. **Keep enums focused** - avoid too many responsibilities
3. **Consider lookup patterns** - mark frequently searched properties with `[EnumLookup]`
4. **Use factory pattern** sparingly - only when you need fresh instances
5. **Document your enums** with XML comments

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

## Contributing

See our [Contributing Guide](CONTRIBUTING.md) for details on how to contribute to this project.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.