using FractalDataWorks;

namespace EnhancedEnumSample;

[EnumOption]
public class Red : ColorOptionBase
{
    public Red() : base(1, "Red", "#FF0000", 1) { }
}