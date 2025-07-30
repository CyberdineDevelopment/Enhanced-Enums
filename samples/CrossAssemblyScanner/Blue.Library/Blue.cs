using FractalDataWorks;
using ColorOption.Library;

namespace Blue.Library;

[EnumOption]
public class Blue : ColorOptionBase
{
    public Blue() : base(3, "Blue", "#0000FF", 3) { }
}