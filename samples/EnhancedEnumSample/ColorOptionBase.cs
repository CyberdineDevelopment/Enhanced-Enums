using FractalDataWorks;
using FractalDataWorks.Attributes;
using System;

namespace EnhancedEnumSample;

[EnumCollection(CollectionName = "Colors", NameComparison = StringComparison.OrdinalIgnoreCase, GenerateFactoryMethods = true)]
public abstract class ColorOptionBase : EnumOptionBase<ColorOptionBase>
{
    public string Hex { get; }
    
    [EnumLookup]
    public int Value { get; }
    
    // Constructor must include all abstract properties as parameters
    protected ColorOptionBase(int id, string name, string hex, int value) : base(id, name)
    {
        Hex = hex;
        Value = value;
    }
}