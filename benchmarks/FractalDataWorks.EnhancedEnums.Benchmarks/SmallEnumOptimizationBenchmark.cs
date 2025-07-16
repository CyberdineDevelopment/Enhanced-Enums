using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class SmallEnumOptimizationBenchmark
{
    // Small enum with only 5 items
    private readonly List<SmallEnum> _list;
    private readonly Dictionary<string, SmallEnum> _dictionary;
    private readonly FrozenDictionary<string, SmallEnum> _frozenDictionary;
    private readonly SmallEnum[] _array;
    
    // Test cases
    private readonly string[] _lookupNames;
    
    public SmallEnumOptimizationBenchmark()
    {
        // Initialize small enum
        _array = new[]
        {
            new SmallEnum { Id = 1, Name = "Active", Code = "ACT" },
            new SmallEnum { Id = 2, Name = "Inactive", Code = "INA" },
            new SmallEnum { Id = 3, Name = "Pending", Code = "PEN" },
            new SmallEnum { Id = 4, Name = "Suspended", Code = "SUS" },
            new SmallEnum { Id = 5, Name = "Deleted", Code = "DEL" }
        };
        
        _list = _array.ToList();
        _dictionary = _array.ToDictionary(x => x.Name);
        _frozenDictionary = _array.ToFrozenDictionary(x => x.Name);
        
        // Test cases - mix of all values plus some non-existent
        _lookupNames = new[] { "Active", "Pending", "Deleted", "Unknown", "Inactive", "Invalid" };
    }
    
    [Benchmark(Baseline = true)]
    public SmallEnum? LinearSearch()
    {
        SmallEnum? result = null;
        foreach (var name in _lookupNames)
        {
            result = _list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        return result;
    }
    
    [Benchmark]
    public SmallEnum? Dictionary()
    {
        SmallEnum? result = null;
        foreach (var name in _lookupNames)
        {
            _dictionary.TryGetValue(name, out result);
        }
        return result;
    }
    
    [Benchmark]
    public SmallEnum? FrozenDictionary()
    {
        SmallEnum? result = null;
        foreach (var name in _lookupNames)
        {
            _frozenDictionary.TryGetValue(name, out result);
        }
        return result;
    }
    
    [Benchmark]
    public SmallEnum? SwitchExpression()
    {
        SmallEnum? result = null;
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
    public SmallEnum? SwitchExpression_CaseInsensitive()
    {
        SmallEnum? result = null;
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
    public SmallEnum DirectArrayAccess()
    {
        // Simulating enum value access like StatusEnum.Active
        return _array[0]; // Active
    }
    
    // Comparison with if-else chain
    [Benchmark]
    public SmallEnum? IfElseChain()
    {
        SmallEnum? result = null;
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
    
    public class SmallEnum
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}