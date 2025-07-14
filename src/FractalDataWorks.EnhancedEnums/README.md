# FractalDataWorks EnhancedEnumOption Library

A lightweight, type-safe enum implementation for C# that ensures compile-time type safety, extensibility via interface-first and class-first patterns, optional factory support, and optimized runtime lookups via generated lookup collections.

## Table of Contents

- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [Factory-Based Enums](#factory-based-enums)
- [Advanced Lookup Configuration](#advanced-lookup-configuration)
- [How It Works - Under the Hood](#how-it-works---under-the-hood)
- [Benefits Over Traditional C# Enums](#benefits-over-traditional-csharp-enums)
- [Performance Characteristics](#performance-characteristics)
- [Best Practices](#best-practices)
- [Customization Options](#customization-options)
- [Migration from Traditional Enums](#migration-from-traditional-enums)
- [Cross-Project Enum Collections](#cross-project-enum-collections)
- [Multiple Collections](#multiple-collections)
- [Advanced Patterns](#advanced-patterns)
- [Testing Your Enums](#testing-your-enums)
- [Integration with Other Libraries](#integration-with-other-libraries)
- [Extending the Library](#extending-the-library)
- [Attribute Reference](#attribute-reference)
- [License](#license)

## Installation

```bash
dotnet add package FractalDataWorks.SmartGenerators.EnhancedEnumOptions
```

## Basic Usage

Support both interface-first and class-first enum definitions.

### Interface-First

```csharp
using FractalDataWorks.EnhancedEnumOptions;
using FractalDataWorks.EnhancedEnumOptions.Attributes;

[EnhancedEnumOption("StatusOptions")]
public interface IStatusOption : IEnhancedEnumOption
{
    bool IsGood { get; }
    [EnumLookup] string Category { get; }
}
```

### Class-First

```csharp
using FractalDataWorks.EnhancedEnumOptions;
using FractalDataWorks.EnhancedEnumOptions.Attributes;

[EnhancedEnumOption("StatusOptions")]
public abstract class StatusOptionBase : IEnhancedEnumOption<StatusOptionBase>
{
    protected StatusOptionBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    protected StatusOptionBase() { } // Required for generator

    public int Id { get; }
    public string Name { get; }
    
    public abstract StatusOptionBase Empty();
}

[EnumOption(Name = "Activate")]
public class ActivateOption : StatusOptionBase
{
    public ActivateOption() : base(1, "Activate") { }
    
    public override StatusOptionBase Empty() => new EmptyStatusOption();
}
```

## Factory-Based Enums

Enable dynamic instance creation with `UseFactory = true`.

### Interface-First Factory

```csharp
[EnhancedEnumOption(CollectionName = "PaymentOptions", UseFactory = true)]
public interface IPaymentOption : IEnhancedEnumOption
{
    [EnumLookup] string Gateway { get; }
    [EnumLookup] string CountryCode { get; }
}
```

### Class-First Factory

```csharp
[EnhancedEnumOption(CollectionName = "PaymentOptions", UseFactory = true)]
public abstract class PaymentOptionBase : IEnhancedEnumOption<PaymentOptionBase>
{
    protected PaymentOptionBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }
    public string Name { get; init; }
    [EnumLookup] public abstract string Gateway { get; }
    [EnumLookup] public abstract string CountryCode { get; }
}

[EnumOption(Name = "CreditCard")]
public class CreditCardOption : PaymentOptionBase
{
    public CreditCardOption() : base(1, "Credit Card") { }
}
```

## Advanced Lookup Configuration

Customize lookup methods on any property.

### Interface-First Lookup

```csharp
[EnhancedEnumOption(NameComparison = StringComparison.InvariantCultureIgnoreCase)]
public interface IErrorCode : IEnhancedEnumOption
{
    [EnumLookup(MethodName = "ForCategory")] string Category { get; }
    [EnumLookup(AllowMultiple = true)] ErrorSeverity Severity { get; }
}
```

### Class-First Lookup

```csharp
[EnhancedEnumOption(NameComparison = StringComparison.Ordinal)]
public abstract class ErrorCodeBase : IEnhancedEnumOption<ErrorCodeBase>
{
    protected ErrorCodeBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }
    public string Name { get; init; }
    [EnumLookup] public abstract string Category { get; }
    [EnumLookup(AllowMultiple = true)] public abstract ErrorSeverity Severity { get; }
}
```

## How It Works - Under the Hood

The source generator emits static collection classes:

```csharp
public static class StatusOptions
{
    public static IStatusOption ByName(string name) => /* lookup */;
    public static bool TryGetById(int id, out IStatusOption opt) => /* ... */;
    public static IStatusOption Empty() => /* ... */;
}
```

## Benefits Over Traditional C# Enums

- Compile-time type safety
- Rich object-oriented behaviors
- Flexible lookup methods
- Optional factory-driven creation
- Cross-assembly aggregation

## Performance Characteristics

- **.NET 9+**: `Enum.TryGetAlternateLookup` optimized
- **< .NET 9**: Efficient dictionary fallback

## Best Practices

- **Naming**: Interfaces end with `Option`, base classes with `Base`, collections with `Options`.
- **UseFactory**: Enable for per-call instances.
- **XML Comments**: Document all enum types and members.
- **Extensions**: Provide helper methods for common operations.

## Customization Options

| Attribute          | Property         | Default                          | Description                     |
|--------------------|------------------|----------------------------------|---------------------------------|
| EnhancedEnumOption       | CollectionName   | `{ClassName}s`                   | Collection class name           |
|                    | UseFactory       | `false`                          | New instance per lookup         |
|                    | NameComparison   | `OrdinalIgnoreCase`              | Name matching strategy          |
| EnumLookup         | MethodName       | `By{PropertyName}`               | Lookup method name              |
|                    | AllowMultiple    | `false`                          | Multiple values per key         |
| EnumOption         | Name             | Class name                       | Custom display name             |

## Migration from Traditional Enums

### Before

```csharp
public enum OrderStatus { Pending = 1, Shipped = 2, Delivered = 3 }
```

### Interface-First

```csharp
[EnhancedEnumOption(CollectionName = "OrderStatuses")]
public interface IOrderStatus : IEnhancedEnumOption { }
```

### Class-First

```csharp
[EnhancedEnumOption(CollectionName = "OrderStatuses")]
public abstract class OrderStatusBase : IEnhancedEnumOption<OrderStatusBase> { }
```

## Cross-Project Enum Collections

Enable cross-assembly discovery:

```xml
<PropertyGroup>
  <EnableCrossAssemblyDiscovery>true</EnableCrossAssemblyDiscovery>
</PropertyGroup>
```

```csharp
public interface IConnectionOption : IEnhancedEnumOption { [EnumLookup] string Category { get; } }
public static class ConnectionOptions { /* aggregated lookups */ }
```

## Multiple Collections

Create multiple collections from the same base enum type for logical categorization.

### Defining Multiple Collections

```csharp
[EnhancedEnumOption("PositionPlayers")]
[EnhancedEnumOption("Pitchers")]
public abstract class BaseballPlayerBase
{
    public abstract string Name { get; }
    public abstract string Position { get; }
}
```

### Assigning Options to Collections

Each enum option must specify which collection(s) it belongs to:

```csharp
[EnumOption(CollectionName = "PositionPlayers")]
public class Catcher : BaseballPlayerBase
{
    public override string Name => "Catcher";
    public override string Position => "C";
}

[EnumOption(CollectionName = "Pitchers")]
public class StartingPitcher : BaseballPlayerBase
{
    public override string Name => "Starting Pitcher";
    public override string Position => "SP";
}

// Options can belong to multiple collections
[EnumOption(CollectionName = "Pitchers")]
[EnumOption(CollectionName = "PositionPlayers")]
public class TwoWayPlayer : BaseballPlayerBase
{
    public override string Name => "Two-Way Player";
    public override string Position => "P/DH";
}
```

### Using Multiple Collections

```csharp
// Access each collection independently
foreach (var player in PositionPlayers.All)
{
    Console.WriteLine($"Position Player: {player.Name}");
}

foreach (var pitcher in Pitchers.All)
{
    Console.WriteLine($"Pitcher: {pitcher.Name}");
}

// Each collection has its own lookup methods
var catcher = PositionPlayers.GetByName("Catcher");
var starter = Pitchers.GetByName("Starting Pitcher");
```

### Requirements for Multiple Collections

- When multiple `[EnhancedEnumOption]` attributes are used, all `[EnumOption]` classes **must** specify a `CollectionName`
- Options can belong to multiple collections using multiple `[EnumOption]` attributes
- Collection names must match those defined in the `[EnhancedEnumOption]` attributes

## Advanced Patterns

Implement state machines or discriminated unions via interfaces or base classes.

## Testing Your Enums

*(Tests will be added soon.)*

## Integration with Other Libraries

Use enhanced enums as value objects in ASP.NET, EF Core, gRPC, and JSON serializers.

## Extending the Library

Implement `IEnhancedEnumOptionStrategy` to add custom generation patterns.

## Attribute Reference

### EnhancedEnumOptionAttribute
```csharp
[EnhancedEnumOption("MyOptions", UseFactory = true, NameComparison = StringComparison.Ordinal)]
```
- `CollectionName` (string, required): Name of the generated collection class
- `UseFactory` (bool, default: false): Use factory pattern for instance creation
- `NameComparison` (StringComparison, default: OrdinalIgnoreCase): String comparison for lookups
- `IncludeReferencedAssemblies` (bool, default: false): Scan referenced assemblies for options

### EnumOptionAttribute
```csharp
[EnumOption(Name = "Display Name", Order = 1, CollectionName = "MyCollection")]
```
- `Name` (string, default: class name): Display name of the option
- `Order` (int, default: 0): Sort order in the collection
- `CollectionName` (string, default: null): Which collection this option belongs to (required for multiple collections)

### EnumLookupAttribute
```csharp
[EnumLookup(MethodName = "FindByCode", AllowMultiple = true)]
```
- `MethodName` (string, default: "GetBy{PropertyName}"): Custom lookup method name
- `AllowMultiple` (bool, default: false): Return collection instead of single instance

## License

MIT License.
