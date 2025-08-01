using FractalDataWorks;

namespace EnhancedEnumSample;

[EnumOption]
public class Blue : ColorOptionBase
{
    public Blue() : base(3, "Blue", "#0000FF", 3) { }
}