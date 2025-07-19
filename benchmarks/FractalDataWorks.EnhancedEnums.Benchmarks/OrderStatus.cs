using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

// Small enum for basic testing
[EnhancedEnumBase("OrderStatuses")]
public abstract class OrderStatus
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    [EnumLookup]
    public abstract string Code { get; }
}