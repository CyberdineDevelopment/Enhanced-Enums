using System;
using System.IO;
using System.Linq;

// Test if the generated code includes System.IO namespace
var sourceCode = @"
using FractalDataWorks.EnhancedEnums.Attributes;
using System;

namespace TestNamespace
{
    [EnhancedEnumBase]
    public abstract class Service<T> where T : IDisposable
    {
        public abstract string Name { get; }
    }
    
    [EnumOption]
    public class FileService : Service<System.IO.FileStream>
    {
        public override string Name => ""File"";
    }
}";

// Read the generated file from the test output
var projectDir = @"C:\development\fractaldataworks\enhanced-enums";
var generatedDir = Path.Combine(projectDir, @"tests\FractalDataWorks.EnhancedEnums.Tests\bin\Release\net10.0\Generated\FractalDataWorks.EnhancedEnums\FractalDataWorks.EnhancedEnums.Generators.EnhancedEnumOptionGenerator");

if (Directory.Exists(generatedDir))
{
    Console.WriteLine($"Looking in: {generatedDir}");
    var files = Directory.GetFiles(generatedDir, "*.g.cs", SearchOption.AllDirectories);
    
    foreach (var file in files)
    {
        Console.WriteLine($"\nFile: {file}");
        var content = File.ReadAllText(file);
        
        if (content.Contains("class Services"))
        {
            Console.WriteLine("Found Services class!");
            var lines = content.Split('\n').Take(30);
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
            
            Console.WriteLine($"\nContains 'using System.IO;': {content.Contains("using System.IO;")}");
        }
    }
}
else
{
    Console.WriteLine($"Directory not found: {generatedDir}");
}