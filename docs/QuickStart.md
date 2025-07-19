# Quick Start Guide

This guide will help you get up and running with FractalDataWorks.EnhancedEnums in just a few minutes.

## Installation

### Option 1: NuGet Package (Recommended)

```bash
dotnet add package FractalDataWorks.EnhancedEnums
```

### Option 2: Project Reference

If you're referencing the source code directly (e.g., for local development):

```xml
<ItemGroup>
  <ProjectReference Include="path\to\FractalDataWorks.EnhancedEnums.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="true" />
</ItemGroup>
```

**Note:** You must also add the EnableAssemblyScanner attribute:

```csharp
// In AssemblyInfo.cs or any source file
using FractalDataWorks.SmartGenerators;

[assembly: EnableAssemblyScanner]
```

## Your First Enhanced Enum

### 1. Create the Base Class

```csharp
using FractalDataWorks.EnhancedEnums.Attributes;

[EnhancedEnumBase]
public abstract class TaskStatus
{
    protected TaskStatus(string name, bool isCompleted)
    {
        Name = name;
        IsCompleted = isCompleted;
    }
    
    public string Name { get; }
    public bool IsCompleted { get; }
}
```

### 2. Add Enum Options

```csharp
[EnumOption]
public class Todo : TaskStatus
{
    public Todo() : base("To Do", false) { }
}

[EnumOption]
public class InProgress : TaskStatus
{
    public InProgress() : base("In Progress", false) { }
}

[EnumOption]
public class Done : TaskStatus
{
    public Done() : base("Done", true) { }
}
```

**With C# 12 Primary Constructors (Preferred):**

```csharp
[EnumOption]
public class Todo() : TaskStatus("To Do", false);

[EnumOption]
public class InProgress() : TaskStatus("In Progress", false);

[EnumOption]
public class Done() : TaskStatus("Done", true);
```

### 3. Use Your Enhanced Enum

```csharp
class Program
{
    static void Main()
    {
        // Get all statuses
        Console.WriteLine("All Task Statuses:");
        foreach (var status in TaskStatuses.All)
        {
            Console.WriteLine($"- {status.Name} (Completed: {status.IsCompleted})");
        }

        // Direct access via static properties
        var todo = TaskStatuses.Todo;
        var done = TaskStatuses.Done;
        Console.WriteLine($"\nDirect access: {todo.Name} -> {done.Name}");

        // Find a specific status by name
        var inProgress = TaskStatuses.GetByName("In Progress");
        Console.WriteLine($"Found: {inProgress?.Name}");

        // Check completion status
        if (done.IsCompleted)
        {
            Console.WriteLine("This task is complete!");
        }
        
        // Handle "no selection" case
        var empty = TaskStatuses.Empty;
        Console.WriteLine($"Empty status: '{empty.Name}' (Completed: {empty.IsCompleted})");
    }
}
```

### 4. Build and Run

Build your project and run it. You should see output like:

```
All Task Statuses:
- To Do (Completed: False)
- In Progress (Completed: False)
- Done (Completed: True)

Direct access: To Do -> Done
Found: In Progress
This task is complete!
Empty status: '' (Completed: False)
```

## What Just Happened?

The source generator automatically created a static class called `TaskStatuses` with:

- **All property**: Returns all enum instances as an `ImmutableArray` (cached, zero allocations)
- **GetByName method**: Finds an enum by its name (O(1) dictionary lookup)
- **Static property accessors**: Direct access like `TaskStatuses.Todo`
- **Empty property**: Singleton representing "no selection"
- **Static initialization**: All enum instances are created once at startup

## Next Steps

- Learn about [lookup properties](Advanced.md#lookup-properties) for efficient searching
- Explore [custom collection names](Advanced.md#custom-collection-names)
- Check out [best practices](../README.md#best-practices)

## Common Patterns

### Business Status Enums

```csharp
[EnhancedEnumBase]
public abstract class OrderStatus
{
    protected OrderStatus(string name, string description, bool canBeCancelled)
    {
        Name = name;
        Description = description;
        CanBeCancelled = canBeCancelled;
    }
    
    public string Name { get; }
    public string Description { get; }
    public bool CanBeCancelled { get; }
}

// Traditional constructor approach
[EnumOption]
public class Pending : OrderStatus
{
    public Pending() : base("Pending", "Order is awaiting processing", true) { }
}

// C# 12 Primary constructor (cleaner!)
[EnumOption]
public class Shipped() : OrderStatus("Shipped", "Order has been shipped", false);

[EnumOption]
public class Processing() : OrderStatus("Processing", "Order is being processed", false);

[EnumOption]
public class Delivered() : OrderStatus("Delivered", "Order has been delivered", false);
```

### Configuration Enums

```csharp
[EnhancedEnumBase]
public abstract class LogLevel
{
    protected LogLevel(string name, int severity, ConsoleColor color)
    {
        Name = name;
        Severity = severity;
        Color = color;
    }
    
    public string Name { get; }
    public int Severity { get; }
    public ConsoleColor Color { get; }
}

// C# 12 Primary constructors - concise and clear!
[EnumOption]
public class Debug() : LogLevel("Debug", 0, ConsoleColor.Gray);

[EnumOption]
public class Info() : LogLevel("Info", 1, ConsoleColor.White);

[EnumOption]
public class Warning() : LogLevel("Warning", 2, ConsoleColor.Yellow);

[EnumOption]
public class Error() : LogLevel("Error", 3, ConsoleColor.Red);

[EnumOption]
public class Fatal() : LogLevel("Fatal", 4, ConsoleColor.DarkRed);
```

**Benefits of Constructor Approach:**
- No need to override properties or add XML documentation
- Properties are immutable by default
- Less boilerplate code
- Primary constructors make enum options extremely concise
- IntelliSense shows constructor parameters clearly

## Troubleshooting

### Build Errors

If you see build errors:

1. **Missing namespace**: Make sure to add `using FractalDataWorks.EnhancedEnums.Attributes;`
2. **Abstract class**: The base class must be `abstract`
3. **Concrete options**: Enum options must be concrete classes, not abstract
4. **Name property**: Make sure your base class has a `Name` property

### Runtime Issues

If lookups return null:

1. **Case sensitivity**: Name lookups are case-insensitive by default
2. **Exact match**: Make sure the name matches exactly (excluding case)
3. **Enum registered**: Ensure the enum option class is marked with `[EnumOption]`

## Getting Help

- Check the [Advanced Guide](Advanced.md) for more features
- Review the [API Reference](API.md) for complete documentation
- Look at the [examples](../samples/) for real-world usage patterns