using FractalDataWorks;

namespace EnhancedEnumSample;

[EnumOption]
public class Green : ColorOptionBase
{
    public Green() : base(2, "Green", "#00FF00", 2) { }
}