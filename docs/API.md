# API Reference

Complete API documentation for FractalDataWorks.EnhancedEnums.

## Table of Contents

- [Attributes](#attributes)
- [Generated Classes](#generated-classes)
- [Interfaces](#interfaces)
- [Extension Methods](#extension-methods)
- [Type Constraints](#type-constraints)

## Attributes

### EnhancedEnumBaseAttribute

Marks a class or interface for enhanced enum generation.

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class EnhancedEnumBaseAttribute : Attribute
```

#### Constructors

##### EnhancedEnumBaseAttribute()

Initializes a new instance with default settings.

```csharp
public EnhancedEnumBaseAttribute()
```

##### EnhancedEnumBaseAttribute(string)

Initializes a new instance with a custom collection name.

```csharp
public EnhancedEnumBaseAttribute(string collectionName)
```

**Parameters:**
- `collectionName` (string): The name of the generated collection class.

#### Properties

##### CollectionName

Gets or sets the name of the generated collection class.

```csharp
public string? CollectionName { get; set; }
```

**Default Value:** `null` (uses `{ClassName}s`)

**Example:**
```csharp
[EnhancedEnumBase("MyCustomCollection")]
public abstract class MyEnum { }

// Generates: MyCustomCollection.All, MyCustomCollection.GetByName()
```

##### UseFactory

Gets or sets whether to use the factory pattern for instance creation.

```csharp
public bool UseFactory { get; set; }
```

**Default Value:** `false`

**When `true`:**
- Each lookup creates a new instance using the factory method
- Base class must implement a static `Create(Type type)` method
- Useful for stateful objects

**When `false`:**
- Singleton pattern - same instance returned for each lookup
- Better performance for stateless objects

**Example:**
```csharp
[EnhancedEnumBase(UseFactory = true)]
public abstract class Connection
{
    public abstract string Name { get; }
    
    public static Connection Create(Type type)
    {
        return (Connection)Activator.CreateInstance(type);
    }
}
```

##### NameComparison

Gets or sets the string comparison method for name lookups.

```csharp
public StringComparison NameComparison { get; set; }
```

**Default Value:** `StringComparison.OrdinalIgnoreCase`

**Available Options:**
- `StringComparison.Ordinal` - Case-sensitive, culture-invariant
- `StringComparison.OrdinalIgnoreCase` - Case-insensitive, culture-invariant (default)
- `StringComparison.CurrentCulture` - Case-sensitive, culture-specific
- `StringComparison.CurrentCultureIgnoreCase` - Case-insensitive, culture-specific
- `StringComparison.InvariantCulture` - Case-sensitive, invariant culture
- `StringComparison.InvariantCultureIgnoreCase` - Case-insensitive, invariant culture

**Example:**
```csharp
[EnhancedEnumOption(NameComparison = StringComparison.Ordinal)]
public abstract class CaseSensitive
{
    public abstract string Name { get; }
}

// "test" != "Test" when using Ordinal comparison
```

##### IncludeReferencedAssemblies

Gets or sets whether to scan referenced assemblies for enum options.

```csharp
public bool IncludeReferencedAssemblies { get; set; }
```

**Default Value:** `false`

**When `true`:**
- Scans all referenced assemblies for `[EnumOption]` classes
- Allows cross-assembly enum definitions
- May impact build performance

**When `false`:**
- Only scans the current assembly
- Faster build times
- Most common scenario

##### ReturnType

Gets or sets the return type for generated static properties and methods.

```csharp
public string? ReturnType { get; set; }
```

**Default Value:** `null` (auto-detects or uses base type)

**Auto-detection:** If not specified, the generator will:
1. Look for interfaces that extend `IEnhancedEnumOption`
2. Use the first matching interface found
3. Fall back to the concrete base class type

**Example:**
```csharp
// Explicit return type
[EnhancedEnumBase(ReturnType = "IMyInterface")]
public abstract class MyEnum : IMyInterface { }

// Auto-detection
public interface IMyEnum : IEnhancedEnumOption { }

[EnhancedEnumBase] // Will auto-detect IMyEnum as return type
public abstract class MyEnum : IMyEnum { }
```

### EnumOptionAttribute

Marks a class as an option for an enhanced enum.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class EnumOptionAttribute : Attribute
```

#### Constructors

##### EnumOptionAttribute()

Initializes a new instance.

```csharp
public EnumOptionAttribute()
```

#### Usage

```csharp
[EnumOption]
public class MyOption : MyEnumBase
{
    public override string Name => "My Option";
    // Additional properties...
}
```

#### Requirements

- Must be applied to a concrete (non-abstract) class
- Class must inherit from a class marked with `[EnhancedEnumOption]`
- Class must be public or internal
- Class should have a parameterless constructor

### EnumLookupAttribute

Marks a property to generate a lookup method.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public sealed class EnumLookupAttribute : Attribute
```

#### Constructors

##### EnumLookupAttribute()

Initializes a new instance with default settings.

```csharp
public EnumLookupAttribute()
```

#### Properties

##### MethodName

Gets or sets the name of the generated lookup method.

```csharp
public string? MethodName { get; set; }
```

**Default Value:** `null` (uses `GetBy{PropertyName}`)

**Example:**
```csharp
[EnumLookup(MethodName = "FindByRole")]
public abstract string Role { get; }

// Generates: FindByRole(string role) instead of GetByRole(string role)
```

##### AllowMultiple

Gets or sets whether the lookup can return multiple results.

```csharp
public bool AllowMultiple { get; set; }
```

**Default Value:** `false`

**When `true`:**
- Returns `IEnumerable<T>` containing all matching items
- Uses `Where()` instead of `FirstOrDefault()`

**When `false`:**
- Returns `T?` with the first matching item
- Uses `FirstOrDefault()`

**Example:**
```csharp
[EnumLookup(AllowMultiple = true)]
public abstract string Category { get; }

// Generates: IEnumerable<MyEnum> GetByCategory(string category)
```

##### ReturnType

Gets or sets the return type for this specific lookup method.

```csharp
public string? ReturnType { get; set; }
```

**Default Value:** `null` (inherits from `EnhancedEnumBaseAttribute.ReturnType`)

**Example:**
```csharp
[EnhancedEnumBase(ReturnType = "IMyInterface")]
public abstract class MyEnum : IMyInterface 
{
    // This lookup returns the base return type (IMyInterface)
    [EnumLookup]
    public abstract string Code { get; }
    
    // This lookup overrides to return a specific type
    [EnumLookup(ReturnType = "ISpecialInterface")]
    public abstract string SpecialProperty { get; }
}
```

#### Usage Requirements

- Must be applied to an abstract property
- Property must be in a class marked with `[EnhancedEnumOption]`
- Property type must support comparison operations
- Property should be implemented in all `[EnumOption]` classes

## Generated Classes

### Collection Class Structure

For each enhanced enum, a static collection class is generated:

```csharp
public static class {EnumName}s // or custom CollectionName
{
    // Private backing storage
    private static readonly List<{EnumType}> _all = new List<{EnumType}>();
    private static readonly ImmutableArray<{EnumType}> _cachedAll;
    private static readonly Dictionary<string, {EnumType}> _nameDict;
    private static readonly EmptyValue _empty = new EmptyValue();
    
    // Dictionary for each lookup property
    private static readonly Dictionary<{PropertyType}, {EnumType}> _{propertyName}Dict;
    
    // Static constructor for initialization
    static {EnumName}s() 
    { 
        // Populate _all collection
        // Cache immutable array
        // Build lookup dictionaries
        // FrozenDictionary on .NET 8+
    }
    
    // Public API
    public static ImmutableArray<{EnumType}> All { get; } // Zero allocations
    public static {EnumType} Empty { get; } // Singleton empty value
    public static {EnumType}? GetByName(string name) { /* O(1) lookup */ }
    
    // Static property accessors
    public static {EnumType} {ValueName} { get; } // Direct access
    
    // Generated lookup methods
    public static {EnumType}? GetBy{PropertyName}({PropertyType} value) { /* O(1) lookup */ }
}
```

### All Property

Returns all enum instances as an immutable array.

```csharp
public static ImmutableArray<{EnumType}> All { get; }
```

**Returns:** An `ImmutableArray<T>` containing all enum instances.

**Performance:** O(1) - Returns cached array with zero allocations.

**Example:**
```csharp
foreach (var status in OrderStatuses.All)
{
    Console.WriteLine(status.Name);
}
```

### Empty Property

Returns a singleton instance representing "no selection".

```csharp
public static {EnumType} Empty { get; }
```

**Returns:** A singleton instance with default values:
- String properties return `string.Empty`
- Numeric properties return `0`
- `DateTime` returns `DateTime.MinValue`
- `Guid` returns `Guid.Empty`
- `bool` returns `false`
- Nullable types return `null`

**Performance:** O(1) - Returns cached singleton.

**Example:**
```csharp
var none = OrderStatuses.Empty;
if (currentStatus == OrderStatuses.Empty)
{
    Console.WriteLine("No status selected");
}
```

### Static Property Accessors

Direct access to specific enum values.

```csharp
public static {EnumType} {ValueName} { get; }
```

**Returns:** The specific enum instance.

**Performance:** O(1) - Direct array index access.

**Example:**
```csharp
var pending = OrderStatuses.Pending;
var shipped = OrderStatuses.Shipped;

// No need for string lookups!
if (order.Status == OrderStatuses.Processing)
{
    // Handle processing orders
}
```

### GetByName Method

Finds an enum instance by its name. Always generated.

```csharp
public static {EnumType}? GetByName(string name)
```

**Parameters:**
- `name` (string): The name to search for.

**Returns:** The matching enum instance, or `null` if not found.

**Performance:** O(1) - Dictionary lookup with zero allocations.

**Comparison:** Uses the `NameComparison` setting from `[EnhancedEnumBase]`.

**Example:**
```csharp
var pending = OrderStatuses.GetByName("Pending");
if (pending != null)
{
    Console.WriteLine($"Found: {pending.Description}");
}
```

### GetById Method

Finds an enum instance by its Id. Automatically generated when the base class implements `IEnhancedEnumOption`.

```csharp
public static {EnumType}? GetById(int id)
```

**Parameters:**
- `id` (int): The id to search for.

**Returns:** The matching enum instance, or `null` if not found.

**Performance:** O(1) - Dictionary lookup with zero allocations.

**Requirements:** Base class must implement `FractalDataWorks.IEnhancedEnumOption`.

**Example:**
```csharp
var order = OrderStatuses.GetById(123);
if (order != null)
{
    Console.WriteLine($"Found: {order.Name}");
}
```

### Generated Lookup Methods

For each property marked with `[EnumLookup]`, a lookup method is generated:

```csharp
public static {EnumType}? GetBy{PropertyName}({PropertyType} value)
```

**Parameters:**
- `value` ({PropertyType}): The value to search for.

**Returns:** 
- `{EnumType}?` when `AllowMultiple = false` (default)
- `IEnumerable<{EnumType}>` when `AllowMultiple = true`

**Performance:** O(1) - Dictionary lookup with zero allocations.

**Comparison Logic:**
- **String properties**: Uses configured `StringComparison` (default: `OrdinalIgnoreCase`)
- **Value types**: Uses `Equals()` method for dictionary key comparison
- **Reference types**: Uses `Equals()` method with null handling

**Example:**
```csharp
// Single result
var status = OrderStatuses.GetByCode("PEND");

// Multiple results (when AllowMultiple = true)
var electronics = Products.GetByCategory("Electronics"); // IEnumerable<Product>
```

## Interfaces

### IEnhancedEnumOption

All enhanced enum base classes must implement this interface from the FractalDataWorks core package:

```csharp
namespace FractalDataWorks
{
    /// <summary>
    /// Represents an enhanced enumeration type that provides additional functionality beyond standard enums.
    /// This interface enables strongly-typed enumerations with identifiers, names, and the ability to represent an empty state.
    /// </summary>
    public interface IEnhancedEnumOption
    {
        /// <summary>
        /// Gets the unique identifier for this enum value.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the display name or string representation of this enum value.
        /// </summary>
        string Name { get; }
    }
}
```

**Purpose:**
- Provides a standard contract for all enhanced enums
- Enables automatic generation of `GetById` and `GetByName` methods
- Supports interface-based return types and polymorphism

**Implementation Requirements:**
- All base classes marked with `[EnhancedEnumBase]` must implement this interface
- The `Id` property should return a unique identifier for each enum value
- The `Name` property should return a meaningful display name

## Extension Methods

The library doesn't provide extension methods, but you can create your own:

```csharp
public static class EnhancedEnumExtensions
{
    public static bool IsOneOf<T>(this T item, params T[] values) 
        where T : class
    {
        return values.Contains(item);
    }
    
    public static TEnum? ParseOrDefault<TEnum>(string name, 
        Func<string, TEnum?> parser) where TEnum : class
    {
        try
        {
            return parser(name);
        }
        catch
        {
            return null;
        }
    }
}

// Usage:
var pending = OrderStatuses.GetByName("Pending");
if (pending.IsOneOf(pendingStatus, processingStatus))
{
    // Handle active statuses
}
```

## Type Constraints

### Enhanced Enum Base Classes

```csharp
// Must be abstract
[EnhancedEnumOption]
public abstract class MyEnum
{
    // Must have Name property
    public abstract string Name { get; }
    
    // Other properties are optional
    public abstract string Description { get; }
}
```

### Enum Option Classes

```csharp
// Must be concrete (non-abstract)
[EnumOption]
public class MyOption : MyEnum
{
    // Must override all abstract properties
    public override string Name => "My Option";
    public override string Description => "Description";
    
    // Must have parameterless constructor (implicit or explicit)
    public MyOption() { }
}
```

### Lookup Properties

```csharp
[EnhancedEnumOption]
public abstract class MyEnum
{
    public abstract string Name { get; }
    
    // Lookup properties must be abstract
    [EnumLookup]
    public abstract string Code { get; }
    
    // Supported types for lookup properties:
    [EnumLookup] public abstract int IntValue { get; }
    [EnumLookup] public abstract DateTime DateValue { get; }
    [EnumLookup] public abstract Guid GuidValue { get; }
    [EnumLookup] public abstract TimeSpan TimeValue { get; }
    [EnumLookup] public abstract decimal DecimalValue { get; }
    [EnumLookup] public abstract bool BoolValue { get; }
    [EnumLookup] public abstract DayOfWeek EnumValue { get; }
    [EnumLookup] public abstract string? NullableString { get; }
    [EnumLookup] public abstract int? NullableInt { get; }
}
```

## Error Handling

### Compilation Errors

The source generator validates your code and produces compilation errors for:

1. **Missing Name property**: Base class must have a `Name` property
2. **Non-abstract base class**: Base class must be abstract
3. **Abstract enum option**: Option classes must be concrete
4. **Missing EnumOption attribute**: Option classes must have `[EnumOption]`
5. **Invalid factory method**: When `UseFactory = true`, must have valid `Create()` method

### Runtime Behavior

- **Null inputs**: Lookup methods handle null inputs gracefully and return null
- **Not found**: Lookup methods return null (or empty sequence for multiple results)
- **Case sensitivity**: Controlled by `NameComparison` setting
- **Thread safety**: Generated classes are thread-safe for read operations

## Versioning and Compatibility

### Source Generator Versioning

- **Major version changes**: Breaking changes to generated API
- **Minor version changes**: New features, backward compatible
- **Patch version changes**: Bug fixes, no API changes

### Generated Code Stability

- Generated code structure is stable within major versions
- New features are added as optional attributes
- Existing functionality remains unchanged

### .NET Compatibility

- **Minimum**: .NET Standard 2.0
- **Recommended**: .NET 6.0 or higher
- **Nullable reference types**: Supported with C# 8.0+
- **Performance optimizations**:
  - .NET Standard 2.0 - .NET 7: Uses `Dictionary<TKey, TValue>`
  - .NET 8.0+: Uses `FrozenDictionary<TKey, TValue>` for additional 35% performance improvement

## Source Generator Configuration

### Package Reference

When using the NuGet package, the source generator is automatically configured:

```xml
<PackageReference Include="FractalDataWorks.EnhancedEnums" Version="*" />
```

### Project Reference

When referencing the project directly, proper configuration is required:

```xml
<ItemGroup>
  <!-- Correct: Registers as analyzer and includes runtime components -->
  <ProjectReference Include="path\to\FractalDataWorks.EnhancedEnums.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="true" />
</ItemGroup>
```

#### Configuration Options

- **OutputItemType="Analyzer"**: Required to register the project as a Roslyn analyzer/source generator
- **ReferenceOutputAssembly="true"**: Required to access attributes at runtime
- **ReferenceOutputAssembly="false"**: Use only if attributes are provided separately

### Assembly Scanner Requirement

The source generator requires the assembly scanner to be enabled:

```csharp
using FractalDataWorks.SmartGenerators;

[assembly: EnableAssemblyScanner]
```

Without this attribute, the generator may not discover all enum options, especially in cross-assembly scenarios.