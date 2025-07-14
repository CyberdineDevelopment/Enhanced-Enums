using AssemblyScannerSample.EnumBase;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace EnumLibrary1
{
    [EnumOption(Name = "Alpha", Order = 1)]
    public class OptionAlpha : ScanOptionEnumBase
    {
        public OptionAlpha() : base(1, "Alpha") { }
        public override string Value => "Alpha";
    }
}
