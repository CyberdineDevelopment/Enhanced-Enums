using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

// Simple test to see if source generator works
[EnhancedEnumBase("SimpleStatuses")]
public abstract class SimpleStatus
{
    public abstract string Name { get; }
}