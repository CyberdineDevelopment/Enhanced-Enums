using FractalDataWorks.EnhancedEnums.Attributes;
using System;

namespace EnhancedEnumSample;

[EnhancedEnumOption("ColorEnums", NameComparison = StringComparison.OrdinalIgnoreCase)]
public abstract class ColorEnum
{
    public abstract string Name { get; }
    public abstract string Hex { get; }
    
    [EnumLookup]
    public abstract int Value { get; }
}

[EnumOption]
public class Red : ColorEnum
{
    public override string Name => "Red";
    public override string Hex => "#FF0000";
    public override int Value => 1;
}

[EnumOption]
public class Green : ColorEnum
{
    public override string Name => "Green";
    public override string Hex => "#00FF00";
    public override int Value => 2;
}

[EnumOption]
public class Blue : ColorEnum
{
    public override string Name => "Blue";
    public override string Hex => "#0000FF";
    public override int Value => 3;
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Enhanced Enum Sample");
        Console.WriteLine("===================");
        
        Console.WriteLine("\nAll Colors:");
        foreach (var color in ColorEnums.All)
        {
            Console.WriteLine($"{color.Value}: {color.Name} ({color.Hex})");
        }

        Console.WriteLine("\nLookup by name (case insensitive):");
        var green = ColorEnums.GetByName("green");
        Console.WriteLine($"Found: {green?.Name} - {green?.Hex}");

        Console.WriteLine("\nLookup by value:");
        var blue = ColorEnums.GetByValue(3);
        Console.WriteLine($"Found: {blue?.Name} - {blue?.Hex}");

        Console.WriteLine("\nLookup not found:");
        var purple = ColorEnums.GetByName("Purple");
        Console.WriteLine($"Purple: {purple?.Name ?? "Not found"}");
        
        Console.WriteLine("\nStatic property accessors:");
        Console.WriteLine($"Red: {ColorEnums.Red.Name} - {ColorEnums.Red.Hex}");
        Console.WriteLine($"Green: {ColorEnums.Green.Name} - {ColorEnums.Green.Hex}");
        Console.WriteLine($"Blue: {ColorEnums.Blue.Name} - {ColorEnums.Blue.Hex}");
        
        Console.WriteLine("\nTotal colors: " + ColorEnums.All.Length);
    }
}