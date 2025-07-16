using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

static class Program
{
    static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        // Check if a specific benchmark is requested via command line
        if (args.Length > 0)
        {
            switch (args[0].ToLower())
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
                    RunAllBenchmarks(config);
                    break;
            }
        }
        else
        {
            // Run all benchmarks if no argument provided
            RunAllBenchmarks(config);
        }
    }

    static void RunAllBenchmarks(IConfig config)
    {
        BenchmarkRunner.Run(new[] 
        { 
            typeof(SimpleBenchmark),
            typeof(OptimizationStrategiesBenchmark),
            typeof(SmallEnumOptimizationBenchmark),
            typeof(AllPropertyAllocationBenchmark)
        }, config);
    }
}
