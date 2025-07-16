using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

static class Program
{
    static void Main(string[] args)
    {
        // The project targets .NET 10, so benchmarks will run on .NET 10
        // Memory diagnostics are enabled via [MemoryDiagnoser] attribute on each benchmark class
        var config = DefaultConfig.Instance
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        // Run specific benchmark based on command line argument
        if (args.Length > 0)
        {
            switch (args[0].ToLowerInvariant())
            {
                case "simple":
                    BenchmarkRunner.Run<SimpleBenchmark>(config);
                    break;
                case "optimization":
                    BenchmarkRunner.Run<OptimizationStrategiesBenchmark>(config);
                    break;
                case "small":
                    BenchmarkRunner.Run<SmallEnumOptimizationBenchmark>(config);
                    break;
                case "allocation":
                    BenchmarkRunner.Run<AllPropertyAllocationBenchmark>(config);
                    break;
                default:
                    // Run all benchmarks
                    BenchmarkRunner.Run(new[] 
                    { 
                        typeof(SimpleBenchmark),
                        typeof(OptimizationStrategiesBenchmark),
                        typeof(SmallEnumOptimizationBenchmark),
                        typeof(AllPropertyAllocationBenchmark)
                    }, config);
                    break;
            }
        }
        else
        {
            // Default to optimization strategies benchmark to show the improvements
            BenchmarkRunner.Run<OptimizationStrategiesBenchmark>(config);
        }
    }
}