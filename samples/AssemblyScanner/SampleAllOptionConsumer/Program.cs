using System;
using AssemblyScannerSample.EnumBase;
using AssemblyScannerSample.SampleAllOptionConsumer;

Console.WriteLine("All Options Demo:");
foreach(var o in Options.All)
    Console.WriteLine($"{o.Id} - {o.Name} - {o.Value}");
