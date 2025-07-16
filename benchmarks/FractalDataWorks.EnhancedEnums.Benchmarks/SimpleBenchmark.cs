using BenchmarkDotNet.Attributes;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class SimpleBenchmark
{
    private const string TestStatusName = "Processing";
    private const string TestStatusCode = "PROC";

    [Benchmark(Baseline = true)]
    public OrderStatus? GetByNameGenerated()
    {
        return OrderStatuses.GetByName(TestStatusName);
    }

    [Benchmark]
    public OrderStatus? GetByCodeGenerated()
    {
        return OrderStatuses.GetByCode(TestStatusCode);
    }

    [Benchmark]
    public int GetAllCount()
    {
        return OrderStatuses.All.Length;
    }

    [Benchmark]
    public OrderStatus GetAllFirstItem()
    {
        return OrderStatuses.All[0];
    }
}