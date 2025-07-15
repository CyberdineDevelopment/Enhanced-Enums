# Advanced Usage Guide

This guide covers advanced features and patterns for FractalDataWorks.EnhancedEnums.

## Table of Contents

- [Lookup Properties](#lookup-properties)
- [Custom Collection Names](#custom-collection-names)
- [String Comparison Options](#string-comparison-options)
- [Factory Pattern](#factory-pattern)
- [Custom Lookup Method Names](#custom-lookup-method-names)
- [Complex Property Types](#complex-property-types)
- [Multiple Lookup Properties](#multiple-lookup-properties)
- [Cross-Assembly Usage](#cross-assembly-usage)
- [Integration Patterns](#integration-patterns)

## Lookup Properties

Use `[EnumLookup]` to generate efficient lookup methods for any property:

```csharp
[EnhancedEnumOption]
public abstract class Country
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract string IsoCode { get; }
    
    [EnumLookup]
    public abstract string Currency { get; }
    
    [EnumLookup]
    public abstract int Population { get; }
}

[EnumOption]
public class UnitedStates : Country
{
    public override string Name => "United States";
    public override string IsoCode => "US";
    public override string Currency => "USD";
    public override int Population => 331_000_000;
}

[EnumOption]
public class Canada : Country
{
    public override string Name => "Canada";
    public override string IsoCode => "CA";
    public override string Currency => "CAD";
    public override int Population => 38_000_000;
}
```

This generates:

```csharp
public static class Countries
{
    public static Country? GetByName(string name) { ... }
    public static Country? GetByIsoCode(string isoCode) { ... }
    public static Country? GetByCurrency(string currency) { ... }
    public static Country? GetByPopulation(int population) { ... }
}
```

Usage:

```csharp
var us = Countries.GetByIsoCode("US");
var usdCountry = Countries.GetByCurrency("USD");
var populous = Countries.GetByPopulation(331_000_000);
```

## Custom Collection Names

Override the default collection name:

```csharp
[EnhancedEnumOption("ApplicationStates")]
public abstract class AppState
{
    public abstract string Name { get; }
    public abstract bool IsActive { get; }
}

[EnumOption]
public class Starting : AppState
{
    public override string Name => "Starting";
    public override bool IsActive => false;
}

[EnumOption]
public class Running : AppState
{
    public override string Name => "Running";
    public override bool IsActive => true;
}

// Usage: ApplicationStates.All, ApplicationStates.GetByName()
```

## String Comparison Options

Control how string comparisons are performed:

```csharp
[EnhancedEnumOption(NameComparison = StringComparison.Ordinal)]
public abstract class CaseSensitive
{
    public abstract string Name { get; }
}

[EnhancedEnumOption(NameComparison = StringComparison.OrdinalIgnoreCase)] // Default
public abstract class CaseInsensitive
{
    public abstract string Name { get; }
}
```

Available options:
- `StringComparison.Ordinal` - Case-sensitive, culture-invariant
- `StringComparison.OrdinalIgnoreCase` - Case-insensitive, culture-invariant (default)
- `StringComparison.CurrentCulture` - Case-sensitive, culture-specific
- `StringComparison.CurrentCultureIgnoreCase` - Case-insensitive, culture-specific

## Factory Pattern

Use the factory pattern for fresh instances on each lookup:

```csharp
[EnhancedEnumOption(UseFactory = true)]
public abstract class DatabaseConnection
{
    public abstract string Name { get; }
    public abstract string ConnectionString { get; }
    public abstract int TimeoutSeconds { get; }
    
    // Factory method must be defined in base class
    public static DatabaseConnection Create(Type type)
    {
        return (DatabaseConnection)Activator.CreateInstance(type);
    }
}

[EnumOption]
public class SqlServer : DatabaseConnection
{
    public override string Name => "SQL Server";
    public override string ConnectionString => "Server=localhost;Database=MyDB;";
    public override int TimeoutSeconds => 30;
}

[EnumOption]
public class PostgreSql : DatabaseConnection
{
    public override string Name => "PostgreSQL";
    public override string ConnectionString => "Host=localhost;Database=mydb;";
    public override int TimeoutSeconds => 60;
}
```

With factory pattern:
- Each lookup call creates a new instance
- Useful for stateful objects
- Slightly more overhead than singleton pattern

## Custom Lookup Method Names

Customize the generated lookup method names:

```csharp
[EnhancedEnumOption]
public abstract class User
{
    public abstract string Name { get; }
    
    [EnumLookup(MethodName = "FindByRole")]
    public abstract string Role { get; }
    
    [EnumLookup(MethodName = "GetUserWithId")]
    public abstract int UserId { get; }
}

// Generates:
// - FindByRole(string role)
// - GetUserWithId(int userId)
```

## Complex Property Types

Lookup properties can be any type:

```csharp
[EnhancedEnumOption]
public abstract class Holiday
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract DateTime Date { get; }
    
    [EnumLookup]
    public abstract Guid Id { get; }
    
    [EnumLookup]
    public abstract TimeSpan Duration { get; }
    
    [EnumLookup]
    public abstract DayOfWeek DayOfWeek { get; }
}

[EnumOption]
public class Christmas : Holiday
{
    public override string Name => "Christmas";
    public override DateTime Date => new DateTime(2023, 12, 25);
    public override Guid Id => Guid.Parse("12345678-1234-1234-1234-123456789012");
    public override TimeSpan Duration => TimeSpan.FromDays(1);
    public override DayOfWeek DayOfWeek => DayOfWeek.Monday;
}

// Usage:
var christmas = Holidays.GetByDate(new DateTime(2023, 12, 25));
var holiday = Holidays.GetById(Guid.Parse("12345678-1234-1234-1234-123456789012"));
var mondayHoliday = Holidays.GetByDayOfWeek(DayOfWeek.Monday);
```

## Multiple Lookup Properties

Generate multiple lookup methods for different search patterns:

```csharp
[EnhancedEnumOption]
public abstract class Product
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract string Sku { get; }
    
    [EnumLookup]
    public abstract string Category { get; }
    
    [EnumLookup]
    public abstract decimal Price { get; }
    
    [EnumLookup]
    public abstract string Brand { get; }
}

[EnumOption]
public class Laptop : Product
{
    public override string Name => "Gaming Laptop";
    public override string Sku => "LAPTOP-001";
    public override string Category => "Electronics";
    public override decimal Price => 1299.99m;
    public override string Brand => "TechCorp";
}

// Usage:
var laptop = Products.GetBySku("LAPTOP-001");
var electronics = Products.GetByCategory("Electronics");
var expensive = Products.GetByPrice(1299.99m);
var techCorp = Products.GetByBrand("TechCorp");
```

## Cross-Assembly Usage

Enhanced enums can be used across assembly boundaries:

### Assembly A (Shared Library)

```csharp
[EnhancedEnumOption]
public abstract class ApiEndpoint
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract string Route { get; }
    
    [EnumLookup]
    public abstract string Method { get; }
}

[EnumOption]
public class GetUsers : ApiEndpoint
{
    public override string Name => "Get Users";
    public override string Route => "/api/users";
    public override string Method => "GET";
}
```

### Assembly B (Consuming Application)

```csharp
// Reference the shared assembly
using SharedLib;

class Program
{
    static void Main()
    {
        var endpoint = ApiEndpoints.GetByRoute("/api/users");
        Console.WriteLine($"Found: {endpoint?.Name}");
    }
}
```

## Integration Patterns

### ASP.NET Core Integration

```csharp
[EnhancedEnumOption]
public abstract class ResponseStatus
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract int StatusCode { get; }
    
    public abstract string Message { get; }
}

[EnumOption]
public class Success : ResponseStatus
{
    public override string Name => "Success";
    public override int StatusCode => 200;
    public override string Message => "Operation completed successfully";
}

[EnumOption]
public class NotFound : ResponseStatus
{
    public override string Name => "Not Found";
    public override int StatusCode => 404;
    public override string Message => "Resource not found";
}

// Usage in controller:
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        var user = userService.GetUser(id);
        if (user == null)
        {
            var status = ResponseStatuses.GetByStatusCode(404);
            return StatusCode(status.StatusCode, status.Message);
        }
        
        return Ok(user);
    }
}
```

### Entity Framework Integration

```csharp
[EnhancedEnumOption]
public abstract class OrderStatus
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract string Code { get; }
    
    public abstract bool IsCompleted { get; }
}

[EnumOption]
public class Pending : OrderStatus
{
    public override string Name => "Pending";
    public override string Code => "PEND";
    public override bool IsCompleted => false;
}

// Entity class:
public class Order
{
    public int Id { get; set; }
    public string StatusCode { get; set; }
    
    [NotMapped]
    public OrderStatus Status => OrderStatuses.GetByCode(StatusCode);
}
```

### JSON Serialization

```csharp
[EnhancedEnumOption]
public abstract class Priority
{
    public abstract string Name { get; }
    
    [EnumLookup]
    public abstract int Value { get; }
    
    public abstract string Color { get; }
}

[EnumOption]
public class High : Priority
{
    public override string Name => "High";
    public override int Value => 1;
    public override string Color => "red";
}

// Custom JSON converter:
public class PriorityJsonConverter : JsonConverter<Priority>
{
    public override Priority Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var name = reader.GetString();
        return Priorities.GetByName(name);
    }

    public override void Write(Utf8JsonWriter writer, Priority value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}
```

## Performance Considerations

### Lookup Performance

Current implementation uses linear search (O(n)):

```csharp
// This performs O(n) search for each lookup
var status = OrderStatuses.GetByName("Pending");
```

For better performance with large enums, cache results:

```csharp
private static readonly Dictionary<string, OrderStatus> _cachedStatuses = 
    OrderStatuses.All.ToDictionary(s => s.Name);

public static OrderStatus GetStatusFast(string name)
{
    return _cachedStatuses.TryGetValue(name, out var status) ? status : null;
}
```

### Memory Usage

All enum instances are created at startup and kept in memory:

```csharp
// All instances created during static initialization
static OrderStatuses()
{
    _all.Add(new Pending());
    _all.Add(new Processing());
    _all.Add(new Shipped());
}
```

Consider the memory impact for large enums with many instances.

## Best Practices

1. **Use descriptive names** for enum options
2. **Keep enums focused** on a single responsibility
3. **Add lookup properties** only for frequently searched fields
4. **Consider performance** for large enums with many lookups
5. **Document your enums** with XML comments
6. **Use factory pattern** only when you need fresh instances

## Troubleshooting

### Common Issues

1. **Lookup returns null**: Check the exact name and case sensitivity settings
2. **Build errors**: Ensure base class is abstract and options are concrete
3. **Missing lookups**: Verify `[EnumLookup]` attribute is applied
4. **Performance issues**: Consider caching for frequently accessed lookups

### Debug Tips

1. **Check generated code**: Look in `obj/Debug/net*/generated/` folder
2. **Enable detailed logging**: Add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to see warnings
3. **Use debugger**: Set breakpoints in static constructors to verify initialization