using AssemblyScannerSample.EnumBase;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace EnumLibrary2;

[EnumOption(Name = "Beta", Order = 2)]
public class OptionBeta : ScanOptionEnumBase
{
    public OptionBeta() : base(2, "Beta") { }
    public override string Value => "Beta";
}
