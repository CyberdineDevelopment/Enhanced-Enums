using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[MarkdownExporter]
public class OptimizationStrategiesBenchmark
{
    // Test data
    private readonly List<TestItem> _list;
    private readonly Dictionary<string, TestItem> _dictionary;
    private readonly Dictionary<int, TestItem> _intDictionary;
    private readonly FrozenDictionary<string, TestItem> _frozenDictionary;
    private readonly FrozenDictionary<int, TestItem> _frozenIntDictionary;
    private readonly ImmutableArray<TestItem> _immutableArray;
    private readonly TestItem[] _array;
    
    // Test cases
    private readonly string[] _lookupNames;
    private readonly int[] _lookupIds;
    
    public OptimizationStrategiesBenchmark()
    {
        // Initialize test data with 50 items (medium-sized enum)
        var items = new List<TestItem>();
        for (int i = 0; i < 50; i++)
        {
            items.Add(new TestItem { Id = i + 1, Name = $"Item{i + 1}", Code = $"CODE{i + 1:D3}" });
        }
        
        _list = items;
        _array = items.ToArray();
        _immutableArray = items.ToImmutableArray();
        
        // Create dictionaries
        _dictionary = items.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _intDictionary = items.ToDictionary(x => x.Id);
        
        // Create frozen dictionaries (.NET 8+)
        _frozenDictionary = items.ToFrozenDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _frozenIntDictionary = items.ToFrozenDictionary(x => x.Id);
        
        // Prepare lookup test cases (mix of existing and non-existing)
        _lookupNames = new[] { "Item1", "Item25", "Item50", "NonExistent", "Item10", "Item40" };
        _lookupIds = new[] { 1, 25, 50, 999, 10, 40 };
    }
    
    // Current Enhanced Enums approach - Linear search
    [Benchmark(Baseline = true)]
    public TestItem? LinearSearchByName()
    {
        TestItem? result = null;
        foreach (var name in _lookupNames)
        {
            result = _list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        return result;
    }
    
    [Benchmark]
    public TestItem? LinearSearchById()
    {
        TestItem? result = null;
        foreach (var id in _lookupIds)
        {
            result = _list.FirstOrDefault(x => x.Id == id);
        }
        return result;
    }
    
    // Dictionary approach - O(1) lookup
    [Benchmark]
    public TestItem? DictionaryByName()
    {
        TestItem? result = null;
        foreach (var name in _lookupNames)
        {
            _dictionary.TryGetValue(name, out result);
        }
        return result;
    }
    
    [Benchmark]
    public TestItem? DictionaryById()
    {
        TestItem? result = null;
        foreach (var id in _lookupIds)
        {
            _intDictionary.TryGetValue(id, out result);
        }
        return result;
    }
    
    // Frozen Dictionary approach - Optimized for read-heavy scenarios
    [Benchmark]
    public TestItem? FrozenDictionaryByName()
    {
        TestItem? result = null;
        foreach (var name in _lookupNames)
        {
            _frozenDictionary.TryGetValue(name, out result);
        }
        return result;
    }
    
    [Benchmark]
    public TestItem? FrozenDictionaryById()
    {
        TestItem? result = null;
        foreach (var id in _lookupIds)
        {
            _frozenIntDictionary.TryGetValue(id, out result);
        }
        return result;
    }
    
    // Switch expression for small enums (simulated with first 10 items)
    [Benchmark]
    public TestItem? SwitchExpressionByName()
    {
        TestItem? result = null;
        foreach (var name in _lookupNames)
        {
            result = name switch
            {
                "Item1" => _array[0],
                "Item2" => _array[1],
                "Item3" => _array[2],
                "Item4" => _array[3],
                "Item5" => _array[4],
                "Item6" => _array[5],
                "Item7" => _array[6],
                "Item8" => _array[7],
                "Item9" => _array[8],
                "Item10" => _array[9],
                _ => _list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
            };
        }
        return result;
    }
    
    // All property access patterns
    [Benchmark]
    public int AllToImmutableArray()
    {
        // Current approach - creates new array each time
        return _list.ToImmutableArray().Length;
    }
    
    [Benchmark]
    public int AllCached()
    {
        // Cached approach - no allocation
        return _immutableArray.Length;
    }
    
    [Benchmark]
    public TestItem AllFirstItemToImmutableArray()
    {
        // Current approach with element access
        return _list.ToImmutableArray()[0];
    }
    
    [Benchmark]
    public TestItem AllFirstItemCached()
    {
        // Cached approach with element access
        return _immutableArray[0];
    }
    
    // Iteration patterns
    [Benchmark]
    public int IterateToImmutableArray()
    {
        int count = 0;
        foreach (var item in _list.ToImmutableArray())
        {
            count++;
        }
        return count;
    }
    
    [Benchmark]
    public int IterateCached()
    {
        int count = 0;
        foreach (var item in _immutableArray)
        {
            count++;
        }
        return count;
    }
    
    public class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}