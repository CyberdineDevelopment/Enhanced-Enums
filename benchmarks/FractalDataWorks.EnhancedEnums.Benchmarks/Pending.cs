using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[EnumOption]
public class Pending : OrderStatus
{
    public override string Name => "Pending";
    public override string Description => "Order is pending processing";
    public override string Code => "PEND";
}