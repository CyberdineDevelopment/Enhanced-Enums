# Migration Guide

This guide helps you migrate from traditional C# enums to FractalDataWorks.EnhancedEnums.

## Table of Contents

- [Why Migrate?](#why-migrate)
- [Migration Strategies](#migration-strategies)
- [Step-by-Step Migration](#step-by-step-migration)
- [Common Patterns](#common-patterns)
- [Breaking Changes](#breaking-changes)
- [Coexistence Strategies](#coexistence-strategies)
- [Performance Considerations](#performance-considerations)

## Why Migrate?

### Limitations of Traditional Enums

```csharp
// Traditional enum limitations:
public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4
}

// Problems:
// 1. No additional properties (description, metadata)
// 2. Limited to primitive types
// 3. No type safety for lookups
// 4. Hard to extend without breaking changes
// 5. No behavior or methods
```

### Benefits of Enhanced Enums

```csharp
// Enhanced enum advantages:
[EnhancedEnumOption]
public abstract class OrderStatus
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract bool CanBeCancelled { get; }
    public abstract TimeSpan EstimatedDuration { get; }
    
    [EnumLookup]
    public abstract string Code { get; }
    
    // Custom behavior
    public virtual bool IsCompleted => false;
    
    public virtual void OnStatusChanged()
    {
        // Custom logic
    }
}

// Benefits:
// 1. Rich metadata and properties
// 2. Type-safe lookups
// 3. Custom behavior and methods
// 4. Easy to extend
// 5. Better maintainability
```

## Migration Strategies

### 1. Gradual Migration (Recommended)

Migrate one enum at a time while maintaining backward compatibility:

```csharp
// Phase 1: Keep existing enum
public enum LegacyOrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3
}

// Phase 2: Create enhanced enum
[EnhancedEnumOption]
public abstract class OrderStatus
{
    public abstract string Name { get; }
    public abstract int LegacyValue { get; } // For compatibility
    
    // Conversion methods
    public static OrderStatus FromLegacy(LegacyOrderStatus legacy) => legacy switch
    {
        LegacyOrderStatus.Pending => OrderStatuses.GetByName("Pending")!,
        LegacyOrderStatus.Processing => OrderStatuses.GetByName("Processing")!,
        LegacyOrderStatus.Shipped => OrderStatuses.GetByName("Shipped")!,
        _ => throw new ArgumentException($"Unknown status: {legacy}")
    };
    
    public LegacyOrderStatus ToLegacy() => (LegacyOrderStatus)LegacyValue;
}

// Phase 3: Update consumers gradually
// Phase 4: Remove legacy enum
```

### 2. Big Bang Migration

Replace all usages at once (suitable for smaller codebases):

```csharp
// Before: Traditional enum
public enum Priority { Low = 1, Medium = 2, High = 3 }

// After: Enhanced enum
[EnhancedEnumOption]
public abstract class Priority
{
    public abstract string Name { get; }
    public abstract int Level { get; }
    public abstract string Color { get; }
}

[EnumOption]
public class Low : Priority
{
    public override string Name => "Low";
    public override int Level => 1;
    public override string Color => "green";
}
```

### 3. Wrapper Migration

Create enhanced enums that wrap existing enums:

```csharp
// Existing enum (can't change)
public enum HttpStatusCode
{
    OK = 200,
    NotFound = 404,
    InternalServerError = 500
}

// Enhanced wrapper
[EnhancedEnumOption]
public abstract class HttpStatus
{
    public abstract string Name { get; }
    public abstract HttpStatusCode Code { get; }
    public abstract string Description { get; }
    public abstract bool IsError { get; }
    
    [EnumLookup]
    public abstract int StatusCode { get; }
}

[EnumOption]
public class OK : HttpStatus
{
    public override string Name => "OK";
    public override HttpStatusCode Code => HttpStatusCode.OK;
    public override string Description => "Request successful";
    public override bool IsError => false;
    public override int StatusCode => 200;
}
```

## Step-by-Step Migration

### Step 1: Analyze Current Usage

Identify how the enum is currently used:

```csharp
// Current usage patterns to identify:

// 1. Direct comparisons
if (order.Status == OrderStatus.Pending) { }

// 2. Switch statements
switch (order.Status)
{
    case OrderStatus.Pending:
        // ...
        break;
}

// 3. String conversions
var statusName = order.Status.ToString();

// 4. Parsing from strings
var status = Enum.Parse<OrderStatus>("Pending");

// 5. Getting all values
var allStatuses = Enum.GetValues<OrderStatus>();

// 6. Serialization/Deserialization
[JsonConverter(typeof(StringEnumConverter))]
public OrderStatus Status { get; set; }
```

### Step 2: Create Enhanced Enum

Design the enhanced enum based on requirements:

```csharp
[EnhancedEnumOption]
public abstract class OrderStatus
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    
    // For backward compatibility
    public abstract int LegacyId { get; }
    
    // New functionality
    public abstract bool CanBeCancelled { get; }
    public abstract string DisplayColor { get; }
    
    [EnumLookup]
    public abstract string Code { get; }
}

[EnumOption]
public class Pending : OrderStatus
{
    public override string Name => "Pending";
    public override string Description => "Order is awaiting processing";
    public override int LegacyId => 1; // Maps to old enum value
    public override bool CanBeCancelled => true;
    public override string DisplayColor => "orange";
    public override string Code => "PEND";
}
```

### Step 3: Create Conversion Utilities

```csharp
public static class OrderStatusConverter
{
    private static readonly Dictionary<LegacyOrderStatus, OrderStatus> _legacyToNew = 
        new Dictionary<LegacyOrderStatus, OrderStatus>
        {
            { LegacyOrderStatus.Pending, new Pending() },
            { LegacyOrderStatus.Processing, new Processing() },
            { LegacyOrderStatus.Shipped, new Shipped() }
        };
    
    private static readonly Dictionary<int, OrderStatus> _idToNew = 
        OrderStatuses.All.ToDictionary(s => s.LegacyId);
    
    public static OrderStatus FromLegacy(LegacyOrderStatus legacy)
    {
        return _legacyToNew.TryGetValue(legacy, out var status) 
            ? status 
            : throw new ArgumentException($"Unknown legacy status: {legacy}");
    }
    
    public static OrderStatus FromId(int id)
    {
        return _idToNew.TryGetValue(id, out var status) 
            ? status 
            : throw new ArgumentException($"Unknown status ID: {id}");
    }
    
    public static LegacyOrderStatus ToLegacy(OrderStatus status)
    {
        return (LegacyOrderStatus)status.LegacyId;
    }
}
```

### Step 4: Update Usage Patterns

```csharp
// Before: Direct comparisons
if (order.Status == OrderStatus.Pending) { }

// After: Reference equality (since instances are singletons)
if (order.Status == OrderStatuses.GetByName("Pending")) { }
// Or better:
var pending = OrderStatuses.GetByName("Pending");
if (order.Status == pending) { }

// Before: Switch statements
switch (order.Status)
{
    case OrderStatus.Pending:
        // ...
        break;
}

// After: Pattern matching or method calls
switch (order.Status.Name)
{
    case "Pending":
        // ...
        break;
}
// Or use the enhanced functionality:
if (order.Status.CanBeCancelled)
{
    // Handle cancellable statuses
}

// Before: String conversions
var statusName = order.Status.ToString();

// After: Use Name property
var statusName = order.Status.Name;

// Before: Parsing from strings
var status = Enum.Parse<OrderStatus>("Pending");

// After: Use lookup method
var status = OrderStatuses.GetByName("Pending");

// Before: Getting all values
var allStatuses = Enum.GetValues<OrderStatus>();

// After: Use All property
var allStatuses = OrderStatuses.All;
```

### Step 5: Update Serialization

```csharp
// Custom JSON converter for enhanced enums
public class OrderStatusJsonConverter : JsonConverter<OrderStatus>
{
    public override OrderStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var name = reader.GetString();
        return OrderStatuses.GetByName(name) 
            ?? throw new JsonException($"Unknown order status: {name}");
    }

    public override void Write(Utf8JsonWriter writer, OrderStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}

// Usage:
[JsonConverter(typeof(OrderStatusJsonConverter))]
public OrderStatus Status { get; set; }
```

### Step 6: Update Database Layer

```csharp
// Entity Framework mapping
public class Order
{
    public int Id { get; set; }
    
    // Store as string or int in database
    public string StatusName { get; set; }
    
    // Map to enhanced enum
    [NotMapped]
    public OrderStatus Status 
    {
        get => OrderStatuses.GetByName(StatusName);
        set => StatusName = value.Name;
    }
}

// Or use EF value converter
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>()
        .Property(e => e.Status)
        .HasConversion(
            status => status.Name,
            name => OrderStatuses.GetByName(name));
}
```

## Common Patterns

### Flags Enum Migration

```csharp
// Before: Flags enum
[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Admin = Read | Write | Delete
}

// After: Enhanced enum with composition
[EnhancedEnumOption]
public abstract class Permission
{
    public abstract string Name { get; }
    public abstract int Value { get; }
    public abstract IReadOnlyList<Permission> Includes { get; }
    
    public bool HasPermission(Permission permission)
    {
        return this == permission || Includes.Contains(permission);
    }
}

[EnumOption]
public class Admin : Permission
{
    public override string Name => "Admin";
    public override int Value => 7; // Read | Write | Delete
    public override IReadOnlyList<Permission> Includes => new[] 
    { 
        new Read(), new Write(), new Delete() 
    };
}
```

### Enum with Attributes Migration

```csharp
// Before: Enum with attributes
public enum Color
{
    [Description("Red color")]
    [HexValue("#FF0000")]
    Red,
    
    [Description("Green color")]
    [HexValue("#00FF00")]
    Green
}

// After: Enhanced enum with properties
[EnhancedEnumOption]
public abstract class Color
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    
    [EnumLookup]
    public abstract string HexValue { get; }
}

[EnumOption]
public class Red : Color
{
    public override string Name => "Red";
    public override string Description => "Red color";
    public override string HexValue => "#FF0000";
}
```

## Breaking Changes

### API Changes

| Traditional Enum | Enhanced Enum | Migration |
|------------------|---------------|----------|
| `Status.Pending` | `OrderStatuses.GetByName("Pending")` | Use lookup method |
| `Enum.Parse<T>(string)` | `T.GetByName(string)` | Use generated method |
| `Enum.GetValues<T>()` | `T.All` | Use All property |
| `status.ToString()` | `status.Name` | Use Name property |
| `status == other` | `status == other` | Reference equality works |
| `(int)status` | `status.LegacyId` | Add compatibility property |

### Type System Changes

```csharp
// Before: Value type
OrderStatus status = OrderStatus.Pending;
status = default; // Compiles, equals OrderStatus.Pending (0)

// After: Reference type
OrderStatus status = OrderStatuses.GetByName("Pending");
status = default; // Compiles, equals null
status = null; // Now possible, need null checks
```

### Serialization Changes

```csharp
// Before: Automatic enum serialization
{
  "status": "Pending" // or 1 for numeric
}

// After: Requires custom converter
{
  "status": "Pending" // Same format with converter
}
```

## Coexistence Strategies

### Adapter Pattern

```csharp
public class OrderStatusAdapter
{
    private readonly LegacyOrderStatus? _legacy;
    private readonly OrderStatus? _enhanced;
    
    public OrderStatusAdapter(LegacyOrderStatus legacy)
    {
        _legacy = legacy;
        _enhanced = OrderStatusConverter.FromLegacy(legacy);
    }
    
    public OrderStatusAdapter(OrderStatus enhanced)
    {
        _enhanced = enhanced;
        _legacy = OrderStatusConverter.ToLegacy(enhanced);
    }
    
    public LegacyOrderStatus Legacy => _legacy ?? OrderStatusConverter.ToLegacy(_enhanced!);
    public OrderStatus Enhanced => _enhanced ?? OrderStatusConverter.FromLegacy(_legacy!.Value);
    
    public string Name => Enhanced.Name;
    public string Description => Enhanced.Description;
}
```

### Interface Abstraction

```csharp
public interface IOrderStatus
{
    string Name { get; }
    string Description { get; }
    bool CanBeCancelled { get; }
}

// Implement on both types
public static class LegacyOrderStatusExtensions
{
    public static IOrderStatus ToInterface(this LegacyOrderStatus status)
    {
        return OrderStatusConverter.FromLegacy(status);
    }
}

// Enhanced enum already implements via inheritance
public abstract class OrderStatus : IOrderStatus
{
    // ...
}
```

## Performance Considerations

### Migration Impact

| Aspect | Traditional Enum | Enhanced Enum | Impact |
|--------|------------------|---------------|--------|
| Memory | Value type (4-8 bytes) | Reference type (~24+ bytes) | Higher memory usage |
| Lookup | O(1) cast | O(n) linear search | Slower lookups |
| Comparison | Direct value | Reference equality | Similar performance |
| Serialization | Built-in | Custom converter | Slightly slower |

### Optimization Strategies

```csharp
// Cache frequently used lookups
public static class CachedOrderStatuses
{
    private static readonly Dictionary<string, OrderStatus> _nameCache = 
        OrderStatuses.All.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    
    public static OrderStatus? GetByNameFast(string name)
    {
        return _nameCache.TryGetValue(name, out var status) ? status : null;
    }
}

// Pre-compute common comparisons
public static class OrderStatusSets
{
    public static readonly ISet<OrderStatus> CancellableStatuses = 
        OrderStatuses.All.Where(s => s.CanBeCancelled).ToHashSet();
    
    public static readonly ISet<OrderStatus> CompletedStatuses = 
        OrderStatuses.All.Where(s => s.IsCompleted).ToHashSet();
}

// Usage:
if (OrderStatusSets.CancellableStatuses.Contains(order.Status))
{
    // Fast set lookup instead of property access
}
```

## Testing Migration

### Unit Tests

```csharp
[Test]
public void Migration_ConvertsLegacyToEnhanced_Successfully()
{
    // Arrange
    var legacyStatus = LegacyOrderStatus.Pending;
    
    // Act
    var enhancedStatus = OrderStatusConverter.FromLegacy(legacyStatus);
    
    // Assert
    Assert.That(enhancedStatus.Name, Is.EqualTo("Pending"));
    Assert.That(enhancedStatus.LegacyId, Is.EqualTo((int)legacyStatus));
}

[Test]
public void Migration_ConvertsEnhancedToLegacy_Successfully()
{
    // Arrange
    var enhancedStatus = OrderStatuses.GetByName("Pending");
    
    // Act
    var legacyStatus = OrderStatusConverter.ToLegacy(enhancedStatus);
    
    // Assert
    Assert.That(legacyStatus, Is.EqualTo(LegacyOrderStatus.Pending));
}
```

### Integration Tests

```csharp
[Test]
public void Serialization_RoundTrip_PreservesData()
{
    // Arrange
    var original = new Order { Status = OrderStatuses.GetByName("Pending") };
    
    // Act
    var json = JsonSerializer.Serialize(original);
    var deserialized = JsonSerializer.Deserialize<Order>(json);
    
    // Assert
    Assert.That(deserialized.Status.Name, Is.EqualTo(original.Status.Name));
}
```

## Migration Checklist

- [ ] Analyze current enum usage patterns
- [ ] Design enhanced enum with backward compatibility
- [ ] Create conversion utilities
- [ ] Update serialization/deserialization
- [ ] Update database mappings
- [ ] Update unit tests
- [ ] Update integration tests
- [ ] Update documentation
- [ ] Performance test critical paths
- [ ] Plan rollback strategy
- [ ] Gradual deployment
- [ ] Monitor for issues
- [ ] Remove legacy code after validation