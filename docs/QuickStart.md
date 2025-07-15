# Quick Start Guide

This guide will help you get up and running with FractalDataWorks.EnhancedEnums in just a few minutes.

## Installation

Add the package to your project:

```bash
dotnet add package FractalDataWorks.EnhancedEnums
```

## Your First Enhanced Enum

### 1. Create the Base Class

```csharp
using FractalDataWorks.EnhancedEnums.Attributes;

[EnhancedEnumOption]
public abstract class TaskStatus
{
    public abstract string Name { get; }
    public abstract bool IsCompleted { get; }
}
```

### 2. Add Enum Options

```csharp
[EnumOption]
public class Todo : TaskStatus
{
    public override string Name => "To Do";
    public override bool IsCompleted => false;
}

[EnumOption]
public class InProgress : TaskStatus
{
    public override string Name => "In Progress";
    public override bool IsCompleted => false;
}

[EnumOption]
public class Done : TaskStatus
{
    public override string Name => "Done";
    public override bool IsCompleted => true;
}
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

        // Find a specific status
        var inProgress = TaskStatuses.GetByName("In Progress");
        Console.WriteLine($"\nFound: {inProgress?.Name}");

        // Check completion status
        var done = TaskStatuses.GetByName("Done");
        if (done?.IsCompleted == true)
        {
            Console.WriteLine("This task is complete!");
        }
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

Found: In Progress
This task is complete!
```

## What Just Happened?

The source generator automatically created a static class called `TaskStatuses` with:

- **All property**: Returns all enum instances as an `ImmutableArray`
- **GetByName method**: Finds an enum by its name
- **Static initialization**: All enum instances are created once at startup

## Next Steps

- Learn about [lookup properties](Advanced.md#lookup-properties) for efficient searching
- Explore [custom collection names](Advanced.md#custom-collection-names)
- Read about [performance characteristics](Performance.md)
- Check out [best practices](../README.md#best-practices)

## Common Patterns

### Business Status Enums

```csharp
[EnhancedEnumOption]
public abstract class OrderStatus
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract bool CanBeCancelled { get; }
}

[EnumOption]
public class Pending : OrderStatus
{
    public override string Name => "Pending";
    public override string Description => "Order is awaiting processing";
    public override bool CanBeCancelled => true;
}

[EnumOption]
public class Shipped : OrderStatus
{
    public override string Name => "Shipped";
    public override string Description => "Order has been shipped";
    public override bool CanBeCancelled => false;
}
```

### Configuration Enums

```csharp
[EnhancedEnumOption]
public abstract class LogLevel
{
    public abstract string Name { get; }
    public abstract int Severity { get; }
    public abstract ConsoleColor Color { get; }
}

[EnumOption]
public class Info : LogLevel
{
    public override string Name => "Info";
    public override int Severity => 1;
    public override ConsoleColor Color => ConsoleColor.White;
}

[EnumOption]
public class Warning : LogLevel
{
    public override string Name => "Warning";
    public override int Severity => 2;
    public override ConsoleColor Color => ConsoleColor.Yellow;
}

[EnumOption]
public class Error : LogLevel
{
    public override string Name => "Error";
    public override int Severity => 3;
    public override ConsoleColor Color => ConsoleColor.Red;
}
```

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