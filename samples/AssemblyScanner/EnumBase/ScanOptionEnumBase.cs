using System;
using FractalDataWorks;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace AssemblyScannerSample.EnumBase;

[EnhancedEnum("Options", IncludeReferencedAssemblies = true)]
public abstract class ScanOptionEnumBase : EnumBase<ScanOptionEnumBase>, IEnhancedEnum<ScanOptionEnumBase>
{
    private readonly int _id;
    private readonly string _name = string.Empty;

    /// <summary>Creates a new instance with specified id and name.</summary>
    protected ScanOptionEnumBase(int id, string name)
    {
        _id = id;
        _name = name;
    }

    /// <summary>Parameterless constructor for generated empty subclass.</summary>
    protected ScanOptionEnumBase() { }

    /// <summary>Gets the string value representation.</summary>
    public virtual string Value => Name;

    /// <inheritdoc/>        
    public virtual int Id => _id;

    /// <inheritdoc/>
    public virtual string Name => _name;

    /// <summary>Indicates whether this is the empty value.</summary>
    public virtual bool IsEmpty => Id == 0 && string.IsNullOrEmpty(Name);
}
