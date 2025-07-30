
using System;
using ColorOption.Library;

namespace ConsoleApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Cross Assembly Scanner Sample");
        Console.WriteLine("============================");
        
        Console.WriteLine("\nAll Colors:");
        foreach (var color in Colors.All)
        {
            Console.WriteLine($"{color.Value}: {color.Name} ({color.Hex})");
        }

        Console.WriteLine("\nLookup by name (case insensitive):");
        var green = Colors.GetByName("green");
        Console.WriteLine($"Found: {green?.Name} - {green?.Hex}");

        Console.WriteLine("\nLookup by value:");
        // var greenByValue = Colors.GetByValue(2);  // TODO: EnumLookup attribute processing
        Console.WriteLine("GetByValue method not yet implemented");

        Console.WriteLine("\nLookup not found:");
        var purple = Colors.GetByName("Purple");
        Console.WriteLine($"Purple: {purple?.Name ?? "Not found"}");
        
        Console.WriteLine("\nFactory methods:");
        // Note: Green factory method is not available because no options were discovered
        Console.WriteLine("No factory methods available - no enum options discovered");
        
        Console.WriteLine("\nSingleton lookups:");
        var greenSingleton = Colors.GetByName("Green");
        Console.WriteLine($"Green singleton: {greenSingleton?.Name} - {greenSingleton?.Hex}");
        
        Console.WriteLine("\nEmpty value:");
        var empty = Colors.Empty;
        Console.WriteLine($"Empty: Id={empty.Id}, Name='{empty.Name}', Hex='{empty.Hex}', Value={empty.Value}");
        
        Console.WriteLine("\nTotal colors: " + Colors.All.Length);
    }
}