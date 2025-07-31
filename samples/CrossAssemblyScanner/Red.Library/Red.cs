using FractalDataWorks;
using ColorOption.Library;

namespace Red.Library;

[EnumOption]
public class Red : ColorOptionBase
{
    public Red() : base(1, "Red", "#FF0000", 1) { }
}