# Performance Guide

This guide covers performance characteristics, optimization strategies, and benchmarking for FractalDataWorks.EnhancedEnums.

## Table of Contents

- [Current Performance Characteristics](#current-performance-characteristics)
- [Performance Implications](#performance-implications)
- [Optimization Strategies](#optimization-strategies)
- [Benchmarking](#benchmarking)
- [Memory Usage](#memory-usage)
- [Best Practices](#best-practices)

## Current Performance Characteristics

### Time Complexity

| Operation | Complexity | Description |
|-----------|------------|-------------|
| Static initialization | O(n) | All enum instances created during static constructor |
| `All` property access | O(n) | Creates new `ImmutableArray` on each access |
| `GetByName()` lookup | O(n) | Linear search using `FirstOrDefault()` |
| `GetBy[Property]()` lookup | O(n) | Linear search using `FirstOrDefault()` |
| Instance access | O(1) | Direct property access on instances |

### Generated Code Structure

```csharp
public static class OrderStatuses
{
    private static readonly List<OrderStatus> _all = new List<OrderStatus>();
    
    static OrderStatuses()
    {
        // O(n) initialization
        _all.Add(new Pending());
        _all.Add(new Processing());
        _all.Add(new Shipped());
    }
    
    // O(n) on each access - creates new ImmutableArray
    public static ImmutableArray<OrderStatus> All => _all.ToImmutableArray();
    
    // O(n) linear search
    public static OrderStatus? GetByName(string name)
    {
        return _all.FirstOrDefault(x => 
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
```

## Performance Implications

### Small Enums (1-10 items)
- **Acceptable**: Linear search performance is negligible
- **Memory**: Minimal impact
- **Recommendation**: No optimization needed

### Medium Enums (10-50 items)
- **Noticeable**: Linear search may show in profiling
- **Memory**: Moderate impact
- **Recommendation**: Consider caching for frequent lookups

### Large Enums (50+ items)
- **Significant**: Linear search becomes expensive
- **Memory**: All instances remain in memory
- **Recommendation**: Implement optimization strategies

## Optimization Strategies

### 1. Dictionary Caching

Cache lookup results in dictionaries for O(1) access:

```csharp
// Create cached lookups for frequently used enums
public static class OptimizedOrderStatuses
{
    private static readonly Dictionary<string, OrderStatus> _nameCache = 
        OrderStatuses.All.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    
    private static readonly Dictionary<string, OrderStatus> _codeCache = 
        OrderStatuses.All.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase);
    
    public static OrderStatus? GetByNameFast(string name)
    {
        return _nameCache.TryGetValue(name, out var status) ? status : null;
    }
    
    public static OrderStatus? GetByCodeFast(string code)
    {
        return _codeCache.TryGetValue(code, out var status) ? status : null;
    }
}
```

### 2. FrozenDictionary (.NET 8+)

Use `FrozenDictionary<TKey, TValue>` for even better read performance:

```csharp
using System.Collections.Frozen;

public static class FrozenOrderStatuses
{
    private static readonly FrozenDictionary<string, OrderStatus> _nameCache = 
        OrderStatuses.All.ToFrozenDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    
    public static OrderStatus? GetByNameFrozen(string name)
    {
        return _nameCache.TryGetValue(name, out var status) ? status : null;
    }
}
```

### 3. Switch Expression Optimization

For small, well-known enums, switch expressions can be faster:

```csharp
public static OrderStatus? GetByCodeSwitch(string code) => code.ToUpperInvariant() switch
{
    "PEND" => new Pending(),
    "PROC" => new Processing(), 
    "SHIP" => new Shipped(),
    _ => null
};
```

### 4. Lazy Initialization

Delay expensive operations until needed:

```csharp
public static class LazyOrderStatuses
{
    private static readonly Lazy<Dictionary<string, OrderStatus>> _nameCache = 
        new(() => OrderStatuses.All.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase));
    
    public static OrderStatus? GetByNameLazy(string name)
    {
        return _nameCache.Value.TryGetValue(name, out var status) ? status : null;
    }
}
```

## Benchmarking

### Setting Up Benchmarks

Use BenchmarkDotNet to measure performance:

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
public class EnumLookupBenchmarks
{
    private const string TestName = "Processing";
    private const string TestCode = "PROC";
    
    [Benchmark(Baseline = true)]
    public OrderStatus? GetByName_Linear()
    {
        return OrderStatuses.GetByName(TestName);
    }
    
    [Benchmark]
    public OrderStatus? GetByName_Dictionary()
    {
        return OptimizedOrderStatuses.GetByNameFast(TestName);
    }
    
    [Benchmark]
    public OrderStatus? GetByName_FrozenDictionary()
    {
        return FrozenOrderStatuses.GetByNameFrozen(TestName);
    }
    
    [Benchmark]
    public OrderStatus? GetByCode_Switch()
    {
        return GetByCodeSwitch(TestCode);
    }
}

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<EnumLookupBenchmarks>();
    }
}
```

### Expected Results

Typical performance characteristics for 50 enum items:

| Method | Mean | Ratio | Allocated |
|--------|------|-------|----------|
| Linear Search | 500.0 ns | 1.00 | 0 B |
| Dictionary | 15.0 ns | 0.03 | 0 B |
| FrozenDictionary | 8.0 ns | 0.02 | 0 B |
| Switch Expression | 2.0 ns | 0.004 | 0 B |

## Memory Usage

### Current Implementation

```csharp
// Memory usage breakdown:
// 1. Static List<T> holding all instances
// 2. Individual enum instances (persistent)
// 3. ImmutableArray allocation on each All access

private static readonly List<OrderStatus> _all = new List<OrderStatus>();
public static ImmutableArray<OrderStatus> All => _all.ToImmutableArray(); // New allocation!
```

### Memory Optimization

```csharp
public static class MemoryOptimizedStatuses
{
    // Cache the ImmutableArray to avoid repeated allocations
    private static readonly Lazy<ImmutableArray<OrderStatus>> _allCached = 
        new(() => OrderStatuses.All.ToImmutableArray());
    
    public static ImmutableArray<OrderStatus> All => _allCached.Value;
}
```

### Memory Profiling

For a typical enum with 20 items:

- **Base instances**: ~2KB (depends on property complexity)
- **List overhead**: ~200 bytes
- **Dictionary cache**: ~1KB additional
- **ImmutableArray**: ~160 bytes per access (if not cached)

## Best Practices

### When to Optimize

1. **Profile first**: Use a profiler to identify actual bottlenecks
2. **Measure impact**: Use benchmarks to verify improvements
3. **Consider frequency**: Optimize frequently accessed lookups
4. **Balance complexity**: Don't over-optimize rarely used code

### Optimization Guidelines

```csharp
// ✅ Good: Cache frequently used lookups
if (lookupFrequency > 1000_per_second)
{
    // Use dictionary caching
}

// ✅ Good: Use appropriate data structures
if (enumSize > 50)
{
    // Consider FrozenDictionary or custom optimization
}

// ❌ Avoid: Premature optimization
if (enumSize < 10 && lookupFrequency < 100_per_second)
{
    // Linear search is fine
}
```

### Code Generation Improvements

Future versions may include:

1. **Automatic dictionary generation** for large enums
2. **Switch expression generation** for small, stable enums
3. **Cached All property** to avoid repeated allocations
4. **Configurable optimization strategies**

### Integration with Dependency Injection

```csharp
// Register optimized lookups as singletons
services.AddSingleton<IOrderStatusLookup, OptimizedOrderStatusLookup>();

public interface IOrderStatusLookup
{
    OrderStatus? GetByName(string name);
    OrderStatus? GetByCode(string code);
}

public class OptimizedOrderStatusLookup : IOrderStatusLookup
{
    private readonly FrozenDictionary<string, OrderStatus> _nameCache;
    private readonly FrozenDictionary<string, OrderStatus> _codeCache;
    
    public OptimizedOrderStatusLookup()
    {
        _nameCache = OrderStatuses.All.ToFrozenDictionary(
            s => s.Name, StringComparer.OrdinalIgnoreCase);
        _codeCache = OrderStatuses.All.ToFrozenDictionary(
            s => s.Code, StringComparer.OrdinalIgnoreCase);
    }
    
    public OrderStatus? GetByName(string name) => 
        _nameCache.TryGetValue(name, out var status) ? status : null;
        
    public OrderStatus? GetByCode(string code) => 
        _codeCache.TryGetValue(code, out var status) ? status : null;
}
```

## Monitoring and Metrics

### Application Performance Monitoring

```csharp
// Track lookup performance
public static class EnumMetrics
{
    private static readonly Counter LookupCounter = 
        Metrics.CreateCounter("enum_lookups_total", "Total enum lookups");
        
    private static readonly Histogram LookupDuration = 
        Metrics.CreateHistogram("enum_lookup_duration_seconds", "Enum lookup duration");
    
    public static OrderStatus? GetByNameWithMetrics(string name)
    {
        using var timer = LookupDuration.NewTimer();
        LookupCounter.Inc();
        
        return OrderStatuses.GetByName(name);
    }
}
```

### Performance Testing

```csharp
[Fact]
public void Lookup_Performance_ShouldBeFastEnough()
{
    var stopwatch = Stopwatch.StartNew();
    
    for (int i = 0; i < 10_000; i++)
    {
        var status = OrderStatuses.GetByName("Processing");
        Assert.NotNull(status);
    }
    
    stopwatch.Stop();
    Assert.True(stopwatch.ElapsedMilliseconds < 100, 
        $"10,000 lookups took {stopwatch.ElapsedMilliseconds}ms");
}
```

Remember: **Measure, don't guess!** Always profile your specific use case before implementing optimizations.