using FractalDataWorks.EnhancedEnums.Attributes;

namespace InterfaceBasedDiscovery.Test;

// Test the original generator to see if it works
[EnhancedEnumBase("TestOptions")]
public abstract class TestOption
{
    public abstract string Name { get; }
}

[EnumOption]
public class OptionA : TestOption
{
    public override string Name => "A";
}

[EnumOption]
public class OptionB : TestOption
{
    public override string Name => "B";
}