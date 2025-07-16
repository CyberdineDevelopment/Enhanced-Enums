using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[EnumOption]
public class Cancelled : OrderStatus
{
    public override string Name => "Cancelled";
    public override string Description => "Order has been cancelled";
    public override string Code => "CANC";
}