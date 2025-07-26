using System.Linq;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests;

public class GenericEnhancedEnumTests : EnhancedEnumOptionTestBase
{
    [Fact]
    public void GeneratorWithGenericBaseGeneratesCollection()
    {
        // Arrange
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
        
        // Act
        var result = RunGenerator([source]);
        
        // Assert - We expect a warning about missing DefaultGenericReturnType
        result.Diagnostics.Length.ShouldBe(1);
        result.Diagnostics[0].Id.ShouldBe("ENH003");
        result.ContainsSource("Containers.g.cs").ShouldBeTrue();
        
        var generated = result["Containers.g.cs"];
        
        // Debug: Let's see what's actually generated
        System.Console.WriteLine("=== GENERATED CODE ===");
        System.Console.WriteLine(generated);
        System.Console.WriteLine("=== END GENERATED CODE ===");
        
        var tree = CSharpSyntaxTree.ParseText(generated);
        var expectations = new SyntaxTreeExpectations(tree);
        
        expectations
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("Containers", c => c
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("All", p => p.IsPublic().IsStatic())
                    .HasMethod("GetByName", m => m.IsPublic().IsStatic())))
            .Verify();
    }

    [Fact]
    public void GeneratorExtractsNamespacesFromConstraints()
    {
        // Arrange
        var source = @"
            using FractalDataWorks.EnhancedEnums.Attributes;
            using System;
            using System.IO;

            namespace TestNamespace
            {
                [EnhancedEnumBase]
                public abstract class Service<T> where T : IDisposable
                {
                    public abstract string Name { get; }
                }
                
                [EnumOption]
                public class FileService : Service<FileStream>
                {
                    public override string Name => ""File"";
                }
            }";
        
        // Act
        var result = RunGenerator([source]);
        
        // Assert
        result.GeneratedSources.ShouldNotBeEmpty();
        var generatedSource = result.GeneratedSources.FirstOrDefault();
        generatedSource.ShouldNotBeNull();
        
        var generated = result["Services.g.cs"];
        
        // Check if the generated code has all expected parts
        generated.ShouldContain("using System;");
        
        // Print full generated code to debug
        System.Console.WriteLine("=== FULL GENERATED CODE ===");
        System.Console.WriteLine(generated);
        System.Console.WriteLine("=== END GENERATED CODE ===");
        
        // The generator should include System.IO because FileStream is used in the option type
        generated.ShouldContain("using System.IO;");
    }

    [Fact]
    public void GeneratorUsesDefaultGenericReturnType()
    {
        // Arrange
        var source = @"
            using FractalDataWorks.EnhancedEnums.Attributes;
            
            namespace TestNamespace
            {
                public interface IService { }
                
                [EnhancedEnumBase(DefaultGenericReturnType = ""IService"")]
                public abstract class ServiceType<T> where T : IService
                {
                    public abstract string Name { get; }
                }
                
                [EnumOption]
                public class DataService : ServiceType<IDataService>
                {
                    public override string Name => ""Data"";
                }
                
                public interface IDataService : IService { }
            }";
        
        // Act
        var result = RunGenerator([source]);
        
        // Assert
        var generated = result["ServiceTypes.g.cs"];
        generated.ShouldContain("ImmutableArray<IService>");
        generated.ShouldContain("Dictionary<string, IService>");
    }

    [Fact]
    public void GeneratorHandlesMultipleTypeParameters()
    {
        // Arrange
        var source = @"
            using FractalDataWorks.EnhancedEnums.Attributes;
            
            namespace TestNamespace
            {
                [EnhancedEnumBase]
                public abstract class Pipeline<TInput, TOutput>
                {
                    public abstract string Name { get; }
                }
                
                [EnumOption]
                public class StringToIntPipeline : Pipeline<string, int>
                {
                    public override string Name => ""StringToInt"";
                }
            }";
        
        // Act
        var result = RunGenerator([source]);
        
        // Assert - We expect a warning about missing DefaultGenericReturnType
        result.Diagnostics.Length.ShouldBe(1);
        result.Diagnostics[0].Id.ShouldBe("ENH003");
        result.ContainsSource("Pipelines.g.cs").ShouldBeTrue();
    }
}