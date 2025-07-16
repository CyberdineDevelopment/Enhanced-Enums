using BenchmarkDotNet.Running;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

static class Program
{
    static void Main(string[] args)
    {
        // Run simple benchmark
        BenchmarkRunner.Run<SimpleBenchmark>();
    }
}
