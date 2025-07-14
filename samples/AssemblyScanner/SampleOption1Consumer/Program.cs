using System;
using System.Collections.Generic;
using System.Linq;
using EnumLibrary1;
using AssemblyScannerSample.EnumBase;

namespace SampleOption1Consumer
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Single Option Demo:");
            var alpha = new OptionAlpha();
            Console.WriteLine($"{alpha.Id} - {alpha.Name} - {alpha.Value}");
        }
    }
}
