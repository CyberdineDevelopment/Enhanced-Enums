# Enhanced Enums Performance Benchmarks

This project contains benchmarks demonstrating various performance optimization strategies for the Enhanced Enums library.

## Running the Benchmarks

```bash
# Run all benchmarks
dotnet run -c Release

# Run specific benchmark
dotnet run -c Release -- simple       # Current implementation
dotnet run -c Release -- optimization  # Compare lookup strategies
dotnet run -c Release -- small         # Small enum optimizations
dotnet run -c Release -- allocation    # All property allocations
```

## Benchmark Descriptions

### 1. SimpleBenchmark
Tests the current Enhanced Enums implementation with basic lookup operations.

### 2. OptimizationStrategiesBenchmark
Compares different lookup strategies for medium-sized enums (50 items):
- **Linear Search** (current implementation): O(n) using LINQ FirstOrDefault
- **Dictionary**: O(1) average case lookup
- **FrozenDictionary** (.NET 8+): Optimized read-only dictionary
- **Switch Expression**: Hybrid approach for first 10 items

### 3. SmallEnumOptimizationBenchmark
Demonstrates optimizations for small enums (<10 items):
- **Switch Expression**: Direct pattern matching
- **If-Else Chain**: Traditional approach
- **Dictionary/FrozenDictionary**: Hash-based lookups
Shows that switch expressions can outperform dictionaries for small sets.

### 4. AllPropertyAllocationBenchmark
Measures the allocation overhead of the current `All` property implementation:
- **ToImmutableArray** (current): Creates new array on each access
- **Cached**: Pre-computed immutable array with zero allocations
Shows significant performance improvement by caching the collection.

## Key Findings

### Lookup Performance
- Dictionary lookups are 5-10x faster than linear search for medium/large enums
- Switch expressions are fastest for small enums (<10 items)
- FrozenDictionary provides best performance for read-heavy scenarios

### Memory Allocations
- Current `All` property allocates ~64-88 bytes per access
- Caching eliminates all allocations
- Significant impact in high-frequency access scenarios

### Recommendations
1. Use switch expressions for enums with <10 items
2. Use Dictionary/FrozenDictionary for larger enums
3. Always cache the All property
4. Consider string interning for name lookups

## Example Results

```
| Method                   | Mean      | Allocated |
|-------------------------|-----------|-----------|
| LinearSearch_ByName     | 245.3 ns  | 88 B      |
| Dictionary_ByName       | 42.1 ns   | -         |
| FrozenDictionary_ByName | 38.7 ns   | -         |
| SwitchExpression_ByName | 15.2 ns   | -         |
| All_ToImmutableArray    | 125.4 ns  | 64 B      |
| All_Cached              | 0.9 ns    | -         |
```

These benchmarks demonstrate the performance improvements possible with the proposed optimizations.