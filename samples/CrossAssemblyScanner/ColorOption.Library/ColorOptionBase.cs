using System;
using FractalDataWorks;
using FractalDataWorks.Attributes;

namespace ColorOption.Library;

[EnumCollection(CollectionName = "Colors", NameComparison = StringComparison.OrdinalIgnoreCase, GenerateFactoryMethods = true, IncludeReferencedAssemblies = true)]
public abstract class ColorOptionBase : EnumOptionBase<ColorOptionBase>
{
    public string Hex { get; }
    
    [EnumLookup("GetByValue")]
    public int Value { get; }
    
    // Constructor must include all abstract properties as parameters
    protected ColorOptionBase(int id, string name, string hex, int value) : base(id, name)
    {
        Hex = hex;
        Value = value;
    }
}