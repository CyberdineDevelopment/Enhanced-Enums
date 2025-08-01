using System;

namespace EnhancedEnumSample;

static class Program
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