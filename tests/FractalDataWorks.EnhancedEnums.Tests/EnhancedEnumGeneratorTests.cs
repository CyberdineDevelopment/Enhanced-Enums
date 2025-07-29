using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests;

public class EnhancedEnumGeneratorTests
{
    [Fact]
    public void GeneratorCreatesCollectionFromDocumentedPattern()
    {
        // Arrange - Use the exact pattern from the README
        var source = """
            using FractalDataWorks;

            namespace TestNamespace
            {
                [EnumCollection]
                public abstract class OrderStatus : EnhancedEnumBase<OrderStatus>
                {
                    public abstract bool CanCancel { get; }
                    
                    protected OrderStatus(int id, string name) : base(id, name) { }
                }

                [EnumOption]
                public class Pending : OrderStatus
                {
                    public Pending() : base(1, "Pending") { }
                    public override bool CanCancel => true;
                }
            }
            """;

        // Act
        var compilation = CSharpCompilation.Create("TestAssembly",
            [CSharpSyntaxTree.ParseText(source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Collections.Immutable.ImmutableArray).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(FractalDataWorks.EnhancedEnumBase<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(FractalDataWorks.EnumCollectionAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new EnhancedEnumOptionGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert - Should generate OrderStatuses.g.cs
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        
        System.Console.WriteLine($"Generated {result.GeneratedTrees.Length} files");
        foreach (var tree in result.GeneratedTrees)
        {
            System.Console.WriteLine($"File: {tree.FilePath}");
        }
        
        result.GeneratedTrees.ShouldNotBeEmpty();
        
        var generatedSource = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.EndsWith("OrderStatuses.g.cs"));
        generatedSource.ShouldNotBeNull();
        
        var generatedCode = generatedSource.ToString();
        
        // Debug output to see what's generated
        System.Console.WriteLine("=== GENERATED CODE ===");
        System.Console.WriteLine(generatedCode);
        System.Console.WriteLine("===============");
        
        // Check diagnostics
        System.Console.WriteLine("=== DIAGNOSTICS ===");
        foreach (var diag in result.Diagnostics)
        {
            System.Console.WriteLine($"{diag.Severity}: {diag.GetMessage()}");
        }
        System.Console.WriteLine("===============");
        
        // Use ExpectationsFactory to validate the generated structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("OrderStatuses", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("All")
                    .HasMethod("GetByName")
                    .HasMethod("Pending"))
                .HasClass("EmptyOrderStatusOption", cls => cls
                    .IsPublic()
                    .IsSealed()))
            .Assert();
    }
}