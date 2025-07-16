using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[EnumOption]
public class Processing : OrderStatus
{
    public override string Name => "Processing";
    public override string Description => "Order is being processed";
    public override string Code => "PROC";
}