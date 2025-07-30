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

[EnumOption]
public class Red : ColorOptionBase
{
    public Red() : base(1, "Red", "#FF0000", 1) { }
}

[EnumOption]
public class Green : ColorOptionBase
{
    public Green() : base(2, "Green", "#00FF00", 2) { }
}

[EnumOption]
public class Blue : ColorOptionBase
{
    public Blue() : base(3, "Blue", "#0000FF", 3) { }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Enhanced Enum Sample");
        Console.WriteLine("===================");
        
        Console.WriteLine("\nAll Colors:");
        foreach (var color in Colors.All)
        {
            Console.WriteLine($"{color.Value}: {color.Name} ({color.Hex})");
        }

        Console.WriteLine("\nLookup by name (case insensitive):");
        var green = Colors.GetByName("green");
        Console.WriteLine($"Found: {green?.Name} - {green?.Hex}");

        Console.WriteLine("\nLookup by value:");
        var blue = Colors.GetByValue(3);
        Console.WriteLine($"Found: {blue?.Name} - {blue?.Hex}");

        Console.WriteLine("\nLookup not found:");
        var purple = Colors.GetByName("Purple");
        Console.WriteLine($"Purple: {purple?.Name ?? "Not found"}");
        
        Console.WriteLine("\nFactory methods:");
        var redInstance = Colors.Red();
        var greenInstance = Colors.Green();
        var blueInstance = Colors.Blue();
        Console.WriteLine($"Red: {redInstance.Name} - {redInstance.Hex}");
        Console.WriteLine($"Green: {greenInstance.Name} - {greenInstance.Hex}");
        Console.WriteLine($"Blue: {blueInstance.Name} - {blueInstance.Hex}");
        
        Console.WriteLine("\nSingleton lookups:");
        var redSingleton = Colors.GetByName("Red");
        var greenSingleton = Colors.GetByName("Green");
        Console.WriteLine($"Red singleton: {redSingleton?.Name} - {redSingleton?.Hex}");
        Console.WriteLine($"Green singleton: {greenSingleton?.Name} - {greenSingleton?.Hex}");
        
        Console.WriteLine("\nEmpty value:");
        var empty = Colors.Empty;
        Console.WriteLine($"Empty: Id={empty.Id}, Name='{empty.Name}', Hex='{empty.Hex}', Value={empty.Value}");
        
        Console.WriteLine("\nTotal colors: " + Colors.All.Length);
    }
}