using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[MarkdownExporter]
public class AllPropertyAllocationBenchmark
{
    private readonly List<SampleItem> _items;
    private readonly ImmutableArray<SampleItem> _cachedImmutableArray;
    private readonly SampleItem[] _cachedArray;
    private readonly IReadOnlyList<SampleItem> _readOnlyList;
    
    public AllPropertyAllocationBenchmark()
    {
        // Initialize with 20 items (typical enum size)
        _items = new List<SampleItem>();
        for (int i = 0; i < 20; i++)
        {
            _items.Add(new SampleItem { Id = i, Name = $"Item{i}" });
        }
        
        // Pre-cache different collection types
        _cachedImmutableArray = _items.ToImmutableArray();
        _cachedArray = _items.ToArray();
        _readOnlyList = _items.AsReadOnly();
    }
    
    // Current Enhanced Enums approach - allocates on every access
    [Benchmark(Baseline = true)]
    public ImmutableArray<SampleItem> ToImmutableArrayEveryTime()
    {
        return _items.ToImmutableArray();
    }
    
    // Cached immutable array - no allocation
    [Benchmark]
    public ImmutableArray<SampleItem> CachedImmutableArray()
    {
        return _cachedImmutableArray;
    }
    
    // Return cached regular array
    [Benchmark]
    public SampleItem[] CachedArray()
    {
        return _cachedArray;
    }
    
    // Return as IReadOnlyList - no allocation
    [Benchmark]
    public IReadOnlyList<SampleItem> AsReadOnlyList()
    {
        return _readOnlyList;
    }
    
    // Direct list access (not safe, but for comparison)
    [Benchmark]
    public IReadOnlyList<SampleItem> DirectListAccess()
    {
        return _items;
    }
    
    // Accessing first item - current approach
    [Benchmark]
    public SampleItem? FirstItemToImmutableArray()
    {
        var all = _items.ToImmutableArray();
        return all.Length > 0 ? all[0] : null;
    }
    
    // Accessing first item - cached
    [Benchmark]
    public SampleItem? FirstItemCached()
    {
        return _cachedImmutableArray.Length > 0 ? _cachedImmutableArray[0] : null;
    }
    
    // Iterating - current approach
    [Benchmark]
    public int IterateToImmutableArray()
    {
        int sum = 0;
        foreach (var item in _items.ToImmutableArray())
        {
            sum += item.Id;
        }
        return sum;
    }
    
    // Iterating - cached
    [Benchmark]
    public int IterateCached()
    {
        int sum = 0;
        foreach (var item in _cachedImmutableArray)
        {
            sum += item.Id;
        }
        return sum;
    }
    
    // Count property access - current
    [Benchmark]
    public int CountToImmutableArray()
    {
        return _items.ToImmutableArray().Length;
    }
    
    // Count property access - cached
    [Benchmark]
    public int CountCached()
    {
        return _cachedImmutableArray.Length;
    }
    
    // LINQ operations - current
    [Benchmark]
    public SampleItem? LinqWhereToImmutableArray()
    {
        return _items.ToImmutableArray().FirstOrDefault(x => x.Id == 10);
    }
    
    // LINQ operations - cached
    [Benchmark]
    public SampleItem? LinqWhereCached()
    {
        return _cachedImmutableArray.FirstOrDefault(x => x.Id == 10);
    }
    
    public class SampleItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}