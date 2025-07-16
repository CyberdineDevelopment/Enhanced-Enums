using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[EnumOption]
public class Delivered : OrderStatus
{
    public override string Name => "Delivered";
    public override string Description => "Order has been delivered";
    public override string Code => "DELV";
}