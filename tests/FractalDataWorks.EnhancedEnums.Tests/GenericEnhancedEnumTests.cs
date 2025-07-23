using System.Linq;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.SmartGenerators.TestUtilities;
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
                
                [EnumOption]
                public class StringContainer : Container<string>
                {
                    public override string Name => ""String"";
                }
                
                [EnumOption]
                public class IntContainer : Container<int>
                {
                    public override string Name => ""Int"";
                }
            }";
        
        // Act
        var result = RunGenerator([source]);
        
        // Assert
        result.Diagnostics.ShouldBeEmpty();
        result.ContainsSource("Containers.g.cs").ShouldBeTrue();
        
        var generated = result["Containers.g.cs"];
        generated.ShouldContain("public static class Containers");
        generated.ShouldContain("StringContainer String");
        generated.ShouldContain("IntContainer Int");
    }

    [Fact]
    public void GeneratorExtractsNamespacesFromConstraints()
    {
        // Arrange
        var source = @"
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
        
        // Act
        var result = RunGenerator([source]);
        
        // Assert
        var generated = result["Services.g.cs"];
        generated.ShouldContain("using System;");
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
        
        // Assert
        result.Diagnostics.ShouldBeEmpty();
        result.ContainsSource("Pipelines.g.cs").ShouldBeTrue();
    }
}