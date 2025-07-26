using System;
using System.IO;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.SmartGenerators.TestUtilities;

var source = @"
using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace
{
    [EnhancedEnumBase]
    public abstract class Container<T>
    {
        public abstract string Name { get; }
    }
    
    [EnumOption(Name = ""String"")]
    public class StringContainer : Container<string>
    {
        public override string Name => ""String"";
    }
    
    [EnumOption(Name = ""Int"")]
    public class IntContainer : Container<int>
    {
        public override string Name => ""Int"";
    }
}";

var result = SourceGeneratorTestHelper.RunGenerator<EnhancedEnumOptionGenerator>(source);

foreach (var kvp in result)
{
    Console.WriteLine($"File: {kvp.Key}");
    Console.WriteLine(kvp.Value);
    Console.WriteLine("---");
}