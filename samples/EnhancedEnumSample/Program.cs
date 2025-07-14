using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using System;

namespace EnhancedEnumSample;

 [EnhancedEnum("Colors", NameComparison = StringComparison.InvariantCultureIgnoreCase)]
    public abstract class ColorEnum : IEnhancedEnum
    {
        protected ColorEnum(int id, string name)
        {
            Id = id;
            Name = name;
        }

        // Parameterless constructor required for generator
        protected ColorEnum() 
        {
            Id = 0;
            Name = string.Empty;
        }

        public int Id { get; }
        public string Name { get; }
        
        public abstract string Hex { get; }
        
        public override string ToString() => Name;
    }

    [EnumOption(Name = "Red", Order = 1)]
    public class RedOption : ColorEnum
    {
        public RedOption() : base(1, "Red") { }
        public override string Hex => "#FF0000";
    }

    [EnumOption(Name = "Green", Order = 2)]
    public class GreenOption : ColorEnum
    {
        public GreenOption() : base(2, "Green") { }
        public override string Hex => "#00FF00";
    }

    [EnumOption(Name = "Blue", Order = 3)]
    public class BlueOption : ColorEnum
    {
        public BlueOption() : base(3, "Blue") { }
        public override string Hex => "#0000FF";
    }
    class Program
    {
        static void Main()
        {
            Console.WriteLine("All Colors:");
            foreach (var c in Colors.All)
                Console.WriteLine($"{c.Value}: {c.Name} ({c.Hex})");

            Console.WriteLine("Lookup by name (ignore case):");
            Console.WriteLine(Colors.ByName("green"));

            Console.WriteLine("Lookup by value:");
            Console.WriteLine(Colors.ByValue(3));
        }
    }


