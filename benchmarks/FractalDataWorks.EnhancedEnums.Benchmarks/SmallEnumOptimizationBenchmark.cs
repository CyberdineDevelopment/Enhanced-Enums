using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[MarkdownExporter]
public class SmallEnumOptimizationBenchmark
{
    // Small enum with only 5 items
    private readonly List<SmallItem> _list;
    private readonly Dictionary<string, SmallItem> _dictionary;
    private readonly FrozenDictionary<string, SmallItem> _frozenDictionary;
    private readonly SmallItem[] _array;
    
    // Test cases
    private readonly string[] _lookupNames;
    
    public SmallEnumOptimizationBenchmark()
    {
        // Initialize small enum
        _array = new[]
        {
            new SmallItem { Id = 1, Name = "Active", Code = "ACT" },
            new SmallItem { Id = 2, Name = "Inactive", Code = "INA" },
            new SmallItem { Id = 3, Name = "Pending", Code = "PEN" },
            new SmallItem { Id = 4, Name = "Suspended", Code = "SUS" },
            new SmallItem { Id = 5, Name = "Deleted", Code = "DEL" }
        };
        
        _list = _array.ToList();
        _dictionary = _array.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _frozenDictionary = _array.ToFrozenDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        
        // Test cases - mix of all values plus some non-existent
        _lookupNames = new[] { "Active", "Pending", "Deleted", "Unknown", "Inactive", "Invalid" };
    }
    
    [Benchmark(Baseline = true)]
    public SmallItem? LinearSearch()
    {
        SmallItem? result = null;
        foreach (var name in _lookupNames)
        {
            result = _list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        return result;
    }
    
    [Benchmark]
    public SmallItem? Dictionary()
    {
        SmallItem? result = null;
        foreach (var name in _lookupNames)
        {
            _dictionary.TryGetValue(name, out result);
        }
        return result;
    }
    
    [Benchmark]
    public SmallItem? FrozenDictionary()
    {
        SmallItem? result = null;
        foreach (var name in _lookupNames)
        {
            _frozenDictionary.TryGetValue(name, out result);
        }
        return result;
    }
    
    [Benchmark]
    public SmallItem? SwitchExpression()
    {
        SmallItem? result = null;
        foreach (var name in _lookupNames)
        {
            result = name switch
            {
                "Active" => _array[0],
                "Inactive" => _array[1],
                "Pending" => _array[2],
                "Suspended" => _array[3],
                "Deleted" => _array[4],
                _ => null
            };
        }
        return result;
    }
    
    [Benchmark]
    public SmallItem? SwitchExpressionCaseInsensitive()
    {
        SmallItem? result = null;
        foreach (var name in _lookupNames)
        {
            result = name?.ToUpperInvariant() switch
            {
                "ACTIVE" => _array[0],
                "INACTIVE" => _array[1],
                "PENDING" => _array[2],
                "SUSPENDED" => _array[3],
                "DELETED" => _array[4],
                _ => null
            };
        }
        return result;
    }
    
    // Direct array indexing (when you know the index)
    [Benchmark]
    public SmallItem DirectArrayAccess()
    {
        // Simulating enum value access like StatusEnum.Active
        return _array[0]; // Active
    }
    
    // Comparison with if-else chain
    [Benchmark]
    public SmallItem? IfElseChain()
    {
        SmallItem? result = null;
        foreach (var name in _lookupNames)
        {
            if (string.Equals(name, "Active", StringComparison.OrdinalIgnoreCase))
                result = _array[0];
            else if (string.Equals(name, "Inactive", StringComparison.OrdinalIgnoreCase))
                result = _array[1];
            else if (string.Equals(name, "Pending", StringComparison.OrdinalIgnoreCase))
                result = _array[2];
            else if (string.Equals(name, "Suspended", StringComparison.OrdinalIgnoreCase))
                result = _array[3];
            else if (string.Equals(name, "Deleted", StringComparison.OrdinalIgnoreCase))
                result = _array[4];
            else
                result = null;
        }
        return result;
    }
    
    public class SmallItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}