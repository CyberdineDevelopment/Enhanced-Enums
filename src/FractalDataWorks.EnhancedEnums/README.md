# FractalDataWorks.EnhancedEnums

A source generator library that provides type-safe, extensible enum implementations for C#.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [Advanced Features](#advanced-features)
- [Generated Code](#generated-code)
- [Performance Characteristics](#performance-characteristics)
- [Best Practices](#best-practices)
- [Attribute Reference](#attribute-reference)
- [How It Works](#how-it-works)

## Overview

FractalDataWorks.EnhancedEnums is a source generator that creates type-safe, object-oriented alternatives to traditional C# enums. It generates static collection classes with efficient lookup methods for enum-like types.

### Key Benefits

- **Type Safety**: Compile-time validation of enum definitions
- **Rich Behavior**: Object-oriented enum instances with methods and properties
- **Efficient Lookups**: Generated lookup methods for fast value retrieval
- **Extensibility**: Easy to add new enum values without breaking existing code
- **Source Generation**: Zero runtime overhead, everything is generated at compile time

## Installation

```bash
dotnet add package FractalDataWorks.EnhancedEnums
```

## Debugging Generated Files

To see the generated source files on disk for debugging purposes, add this to your project file:

```xml
<PropertyGroup>
  <EmitGeneratorFiles>true</EmitGeneratorFiles>
</PropertyGroup>
```

This will automatically:
- Set `EmitCompilerGeneratedFiles` to true
- Set `CompilerGeneratedFilesOutputPath` to "GeneratedFiles" (unless already specified)
- Exclude the generated files from compilation to prevent double compilation

The generated files will appear in the `GeneratedFiles` folder in your project directory.

## Basic Usage

### 1. Define an Enhanced Enum

```csharp
using FractalDataWorks.EnhancedEnums.Attributes;

[EnhancedEnumOption]
public abstract class Priority
{
    public abstract string Name { get; }
    public abstract int Level { get; }
    public abstract string Description { get; }
}

[EnumOption]
public class High : Priority
{
    public override string Name => "High";
    public override int Level => 1;
    public override string Description => "High priority item";
}

[EnumOption]
public class Medium : Priority
{
    public override string Name => "Medium";
    public override int Level => 2;
    public override string Description => "Medium priority item";
}

[EnumOption]
public class Low : Priority
{
    public override string Name => "Low";
    public override int Level => 3;
    public override string Description => "Low priority item";
}
```

### 2. Use the Generated Collection

```csharp
// Access all values
foreach (var priority in Priorities.All)
{
    Console.WriteLine($"{priority.Name}: {priority.Description}");
}

// Lookup by name
var high = Priorities.GetByName("High");
Console.WriteLine($"Level: {high?.Level}");

// Check if value exists
var unknown = Priorities.GetByName("Unknown");
if (unknown == null)
{
    Console.WriteLine("Priority not found");
}
```

## Advanced Features

### Lookup Properties

Mark properties with `[EnumLookup]` to generate dedicated lookup methods:

```csharp
[EnhancedEnumOption]
public abstract class HttpStatusCode
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract int Code { get; }
    
    [EnumLookup]
    public abstract string Category { get; }
}

[EnumOption]
public class Ok : HttpStatusCode
{
    public override string Name => "OK";
    public override int Code => 200;
    public override string Category => "Success";
}

[EnumOption]
public class NotFound : HttpStatusCode
{
    public override string Name => "Not Found";
    public override int Code => 404;
    public override string Category => "Client Error";
}

// Usage:
var ok = HttpStatusCodes.GetByCode(200);
var clientErrors = HttpStatusCodes.GetByCategory("Client Error");
```

### Custom Collection Names

```csharp
[EnhancedEnumOption("MyCustomStatuses")]
public abstract class Status
{
    public abstract string Name { get; }
}

// Generates: MyCustomStatuses.All, MyCustomStatuses.GetByName()
```

### String Comparison Options

```csharp
[EnhancedEnumOption(NameComparison = StringComparison.Ordinal)]
public abstract class CaseSensitiveEnum
{
    public abstract string Name { get; }
}
```

### Factory Pattern Support

```csharp
[EnhancedEnumOption(UseFactory = true)]
public abstract class DatabaseConnection
{
    public abstract string Name { get; }
    public abstract string ConnectionString { get; }
    
    // Factory method must be defined in base class
    public static DatabaseConnection Create(Type type)
    {
        return (DatabaseConnection)Activator.CreateInstance(type);
    }
}
```

### Custom Lookup Method Names

```csharp
[EnhancedEnumOption]
public abstract class User
{
    public abstract string Name { get; }
    
    [EnumLookup(MethodName = "FindByRole")]
    public abstract string Role { get; }
}

// Generates: FindByRole() instead of GetByRole()
```

## Generated Code

The source generator creates static collection classes with the following structure:

```csharp
public static class [EnumName]s
{
    private static readonly List<[EnumType]> _all = new List<[EnumType]>();
    
    static [EnumName]s()
    {
        // Initialize all enum instances
        _all.Add(new [EnumOption1]());
        _all.Add(new [EnumOption2]());
        // ...
    }
    
    /// <summary>
    /// Gets all available [EnumType] values.
    /// </summary>
    public static ImmutableArray<[EnumType]> All => _all.ToImmutableArray();
    
    /// <summary>
    /// Gets the [EnumType] with the specified name.
    /// </summary>
    public static [EnumType]? GetByName(string name)
    {
        return _all.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
    
    // Additional lookup methods for [EnumLookup] properties
    public static [EnumType]? GetBy[PropertyName]([PropertyType] [paramName])
    {
        return _all.FirstOrDefault(x => /* comparison logic */);
    }
}
```

## Performance Characteristics

### Current Implementation

- **Initialization**: O(n) - All enum instances created during static constructor
- **All Property Access**: O(n) - Creates new `ImmutableArray` on each access
- **Name Lookups**: O(n) - Linear search through collection using `FirstOrDefault`
- **Property Lookups**: O(n) - Linear search through collection using `FirstOrDefault`
- **Memory Usage**: All instances stored in memory permanently

### Performance Considerations

- **Small Enums (< 10 items)**: Performance is generally acceptable
- **Large Enums (> 50 items)**: Consider the performance impact of linear searches
- **Frequent Lookups**: Cache results if performing many repeated lookups
- **Memory Constraints**: All enum instances remain in memory for the application lifetime

## Best Practices

### 1. Naming Conventions

```csharp
// Good: Descriptive base class name
[EnhancedEnumOption]
public abstract class OrderStatus { }

// Good: Descriptive option names
[EnumOption]
public class AwaitingPayment : OrderStatus { }
```

### 2. Keep Enums Focused

```csharp
// Good: Single responsibility
[EnhancedEnumOption]
public abstract class PaymentMethod
{
    public abstract string Name { get; }
    public abstract bool RequiresVerification { get; }
}

// Avoid: Too many responsibilities
[EnhancedEnumOption]
public abstract class EverythingEnum
{
    public abstract string PaymentMethod { get; }
    public abstract string UserRole { get; }
    public abstract string OrderStatus { get; }
}
```

### 3. Use Lookup Properties Strategically

```csharp
[EnhancedEnumOption]
public abstract class Country
{
    public abstract string Name { get; }
    
    // Good: Frequently searched properties
    [EnumLookup]
    public abstract string IsoCode { get; }
    
    // Consider: Only add lookup if needed
    public abstract string Capital { get; }
}
```

### 4. Document Your Enums

```csharp
/// <summary>
/// Represents the status of an order in the system.
/// </summary>
[EnhancedEnumOption]
public abstract class OrderStatus
{
    /// <summary>
    /// Gets the display name of the status.
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// Gets the internal status code.
    /// </summary>
    [EnumLookup]
    public abstract string Code { get; }
}
```

## Attribute Reference

### EnhancedEnumOptionAttribute

Marks a class or interface for enhanced enum generation.

```csharp
[EnhancedEnumOption(
    CollectionName = "MyCollection",      // Optional: Custom collection name
    UseFactory = true,                    // Optional: Use factory pattern
    NameComparison = StringComparison.Ordinal,  // Optional: String comparison method
    IncludeReferencedAssemblies = true    // Optional: Scan referenced assemblies
)]
```

**Properties:**
- `CollectionName` (string): Name of the generated collection class. Default: `{ClassName}s`
- `UseFactory` (bool): Whether to use factory pattern for instance creation. Default: `false`
- `NameComparison` (StringComparison): String comparison method for name lookups. Default: `OrdinalIgnoreCase`
- `IncludeReferencedAssemblies` (bool): Whether to scan referenced assemblies. Default: `false`

### EnumOptionAttribute

Marks a class as an option for an enhanced enum.

```csharp
[EnumOption]
public class MyOption : MyEnumBase { }
```

### EnumLookupAttribute

Marks a property to generate a lookup method.

```csharp
[EnumLookup(
    MethodName = "FindByCode",    // Optional: Custom method name
    AllowMultiple = true          // Optional: Return multiple results
)]
public abstract string Code { get; }
```

**Properties:**
- `MethodName` (string): Name of the generated lookup method. Default: `GetBy{PropertyName}`
- `AllowMultiple` (bool): Whether to return multiple results. Default: `false`

## How It Works

The FractalDataWorks.EnhancedEnums source generator:

1. **Scans** your code for classes marked with `[EnhancedEnumOption]`
2. **Finds** all classes marked with `[EnumOption]` that inherit from the base class
3. **Analyzes** properties marked with `[EnumLookup]` to generate lookup methods
4. **Generates** static collection classes with optimized lookup methods
5. **Compiles** the generated code as part of your build process

The generated code is included in your assembly and provides compile-time safety with runtime efficiency.

## License

This library is part of the FractalDataWorks toolkit and is licensed under the Apache License 2.0.