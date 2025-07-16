namespace FractalDataWorks.EnhancedEnums.Benchmarks;

// Test class to verify generation
public static class TestGeneration
{
    public static void Test()
    {
        // If this compiles, the source generator worked
        var all = SimpleStatuses.All;
        var option = SimpleStatuses.GetByName("Option1");

        // System.Console.WriteLine($"Found {all.Length} options");
        // System.Console.WriteLine($"Option1: {option?.Name}");
    }
}