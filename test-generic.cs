using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace
{
    [EnhancedEnumBase]
    public abstract class Container<T>
    {
        public abstract string Name { get; }
    }
    
    [EnumOption]
    public class StringContainer : Container<string>
    {
        public override string Name => "String";
    }
    
    [EnumOption]
    public class IntContainer : Container<int>
    {
        public override string Name => "Int";
    }
}