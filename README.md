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
- **Cross-assembly support** for shared enum definitions
- **Lookup capabilities** for efficient value retrieval
- **Rich metadata** support through attributes

## Installation

```bash
# Core interfaces (required dependency)
dotnet add package FractalDataWorks

# Source generators and analyzers
dotnet add package FractalDataWorks.EnhancedEnumOptions
```

## Quick Start

### Define an Enhanced Enum

```csharp
using FractalDataWorks;
using FractalDataWorks.EnhancedEnumOptions.Attributes;

[EnhancedEnumOption]
public partial class OrderStatus : IEnhancedEnumOption<OrderStatus>
{
    [EnumOption(1, "Pending")]
    public static readonly OrderStatus Pending = new();
    
    [EnumOption(2, "Processing")]
    public static readonly OrderStatus Processing = new();
    
    [EnumOption(3, "Shipped")]
    public static readonly OrderStatus Shipped = new();
    
    [EnumOption(4, "Delivered")]
    public static readonly OrderStatus Delivered = new();
}
```

### Use the Enhanced Enum

```csharp
// Get by ID
var status = OrderStatus.GetById(2); // Returns Processing

// Get by name
var pending = OrderStatus.GetByName("Pending");

// Iterate all values
foreach (var s in OrderStatus.GetAll())
{
    Console.WriteLine($"{s.Id}: {s.Name}");
}

// Empty/default value
var empty = OrderStatus.Empty();
```

### Lookup Properties

```csharp
[EnhancedEnumOption]
public partial class Country : IEnhancedEnumOption<Country>
{
    [EnumOption(1, "United States")]
    [EnumLookup("Code", "US")]
    [EnumLookup("Currency", "USD")]
    public static readonly Country UnitedStates = new();
    
    [EnumOption(2, "Canada")]
    [EnumLookup("Code", "CA")]
    [EnumLookup("Currency", "CAD")]
    public static readonly Country Canada = new();
}

// Lookup by property
var country = Country.GetByCode("US");
var currency = country.Currency; // "USD"
```

## Features

### Cross-Assembly Support

Enhanced enums can be discovered and used across assembly boundaries:

```csharp
// In Assembly A
[EnhancedEnumOption]
public partial class SharedStatus : IEnhancedEnumOption<SharedStatus>
{
    // ...
}

// In Assembly B - automatically discovered
var status = SharedStatus.GetById(1);
```

### Validation

The source generator provides compile-time validation for:
- Duplicate IDs
- Duplicate names
- Missing attributes
- Invalid configurations

### Performance

- O(1) lookup by ID using dictionary
- Cached reflection for property lookups
- Minimal runtime overhead

## Documentation

- [Developer Guide](docs/DeveloperGuide.md)
- [API Reference](docs/ApiReference.md)
- [Migration Guide](docs/MigrationGuide.md)
- [Performance Guide](docs/PerformanceGuide.md)

## Requirements

- .NET Standard 2.0 or higher
- C# 9.0 or higher for source generators
- Visual Studio 2022 or VS Code with C# extension

## Contributing

See our [Contributing Guide](CONTRIBUTING.md) for details on how to contribute to this project.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.