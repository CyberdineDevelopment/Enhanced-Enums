using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.Benchmarks;

[EnumOption]
public class Shipped : OrderStatus
{
    public override string Name => "Shipped";
    public override string Description => "Order has been shipped";
    public override string Code => "SHIP";
}