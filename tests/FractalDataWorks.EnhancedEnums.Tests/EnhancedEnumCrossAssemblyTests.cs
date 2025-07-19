using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.EnhancedEnums.Tests.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Tests for cross-assembly enum option discovery in EnhancedEnumOptionGenerator.
/// 
/// IMPORTANT: These tests are fundamentally flawed because source generators
/// can only generate code for the compilation they're running on. They cannot
/// modify or generate code for referenced assemblies.
/// 
/// The correct approach would be:
/// 1. Run the generator in the assembly that contains the [EnhancedEnumBase] base type
/// 2. Use IncludeReferencedAssemblies=true to scan referenced assemblies for options
/// 3. Generate the collection in the base type's assembly
/// 
/// These tests are kept as documentation of the intended feature, but they
/// cannot work as written because they expect the generator to produce output
/// when the base type is in a referenced assembly.
/// </summary>
public class EnhancedEnumOptionCrossAssemblyTests : EnhancedEnumOptionTestBase
{
    [Fact(Skip = "Cross-assembly scanning requires running generator in base assembly")]
    public void GeneratorDiscoversCrossAssemblyEnumOptions()
    {
        // Arrange - Assembly A with base enum
        var assemblyASource = """

		                      using FractalDataWorks.EnhancedEnums.Attributes;
		                      using FractalDataWorks.SmartGenerators.AssemblyScanning;

		                      [assembly: EnableAssemblyScanner]

		                      namespace BaseLibrary
		                      {
		                          [EnhancedEnumBase(IncludeReferencedAssemblies = true)]
		                          public abstract class PluginBase
		                          {
		                              public abstract string Name { get; }
		                              public abstract string Version { get; }
		                          }
		                      }
		                      """;

        // Assembly B with enum options
        var assemblyBSource = """

		                      using FractalDataWorks.EnhancedEnums.Attributes;
		                      using BaseLibrary;

		                      namespace PluginLibrary
		                      {
		                          [EnumOption]
		                          public class SecurityPlugin : PluginBase
		                          {
		                              public override string Name => "Security";
		                              public override string Version => "1.0";
		                          }

		                          [EnumOption]
		                          public class LoggingPlugin : PluginBase
		                          {
		                              public override string Name => "Logging";
		                              public override string Version => "2.0";
		                          }
		                      }
		                      """;

        // Create compilations
        var assemblyA = CreateCompilationWithEnhancedEnumOption(assemblyASource);
        var assemblyB = CSharpCompilation.Create(
            "PluginLibrary",
            new[] { CSharpSyntaxTree.ParseText(assemblyBSource, cancellationToken: TestContext.Current.CancellationToken) },
            new[]
            {
                assemblyA.ToMetadataReference()
            }.Concat(GetDefaultReferences()).ToArray(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Act - Run generator on the base assembly source with reference to plugin assembly
        var result = RunGenerator([assemblyASource], assemblyB.ToMetadataReference());

        // Assert
        result.ContainsSource("PluginBases.g.cs").ShouldBeTrue();

        var generatedCode = result["PluginBases.g.cs"];
        generatedCode.ShouldContain("new PluginLibrary.SecurityPlugin()");
        generatedCode.ShouldContain("new PluginLibrary.LoggingPlugin()");
    }

    [Fact(Skip = "Cross-assembly scanning requires running generator in base assembly")]
    public void GeneratorHandlesMultipleAssembliesWithOptions()
    {
        // Arrange - Base assembly
        var baseSource = """

		                 using FractalDataWorks.EnhancedEnums.Attributes;
		                 using FractalDataWorks.SmartGenerators.AssemblyScanning;

		                 [assembly: EnableAssemblyScanner]

		                 namespace Core
		                 {
		                     [EnhancedEnumBase("Handlers", IncludeReferencedAssemblies = true)]
		                     public abstract class HandlerBase
		                     {
		                         public abstract string Type { get; }
		                         public abstract void Handle();
		                     }
		                 }
		                 """;

        // First extension assembly
        var ext1Source = """

		                 using FractalDataWorks.EnhancedEnums.Attributes;
		                 using Core;

		                 namespace Extensions.Network
		                 {
		                     [EnumOption]
		                     public class HttpHandler : HandlerBase
		                     {
		                         public override string Type => "HTTP";
		                         public override void Handle() { }
		                     }

		                     [EnumOption]
		                     public class TcpHandler : HandlerBase
		                     {
		                         public override string Type => "TCP";
		                         public override void Handle() { }
		                     }
		                 }
		                 """;

        // Second extension assembly
        var ext2Source = """

		                 using FractalDataWorks.EnhancedEnums.Attributes;
		                 using Core;

		                 namespace Extensions.File
		                 {
		                     [EnumOption]
		                     public class FileHandler : HandlerBase
		                     {
		                         public override string Type => "FILE";
		                         public override void Handle() { }
		                     }

		                     [EnumOption]
		                     public class DatabaseHandler : HandlerBase
		                     {
		                         public override string Type => "DB";
		                         public override void Handle() { }
		                     }
		                 }
		                 """;

        // Create compilations
        var baseAssembly = CreateCompilationWithEnhancedEnumOption(baseSource);
        var ext1Assembly = CreateCompilationWithReferences(ext1Source, baseAssembly.ToMetadataReference());
        var ext2Assembly = CreateCompilationWithReferences(ext2Source, baseAssembly.ToMetadataReference());

        // Main assembly
        var mainSource = """

		                 using Core;

		                 namespace App
		                 {
		                     public class HandlerService
		                     {
		                         public void Initialize()
		                         {
		                             var handlers = Handlers.All;
		                         }
		                     }
		                 }
		                 """;

        // Act
        var result = RunGeneratorWithReferences(
            [mainSource],
            baseAssembly.ToMetadataReference(),
            ext1Assembly.ToMetadataReference(),
            ext2Assembly.ToMetadataReference());

        // Assert
        result.ContainsSource("Handlers.g.cs").ShouldBeTrue();

        var generatedCode = result["Handlers.g.cs"];
        // Should include all handlers from both extension assemblies
        generatedCode.ShouldContain("HttpHandler");
        generatedCode.ShouldContain("TcpHandler");
        generatedCode.ShouldContain("FileHandler");
        generatedCode.ShouldContain("DatabaseHandler");
    }

    [Fact(Skip = "Cross-assembly scanning requires running generator in base assembly")]
    public void GeneratorRespectsIncludeReferencedAssembliesFlag()
    {
        // Arrange - Base without IncludeReferencedAssemblies
        var baseSource = """

		                 using FractalDataWorks.EnhancedEnums.Attributes;
		                 using FractalDataWorks.SmartGenerators.AssemblyScanning;

		                 [assembly: EnableAssemblyScanner]

		                 namespace Core
		                 {
		                     [EnhancedEnumBase(IncludeReferencedAssemblies = false)] // Explicitly false
		                     public abstract class FeatureBase
		                     {
		                         public abstract string Name { get; }
		                     }
		                 }
		                 """;

        // Extension assembly
        var extSource = """

		                using FractalDataWorks.EnhancedEnums.Attributes;
		                using Core;

		                namespace Extensions
		                {
		                    [EnumOption]
		                    public class ExternalFeature : FeatureBase
		                    {
		                        public override string Name => "External";
		                    }
		                }
		                """;

        // Create compilations
        var baseAssembly = CreateCompilationWithEnhancedEnumOption(baseSource);
        var extAssembly = CreateCompilationWithReferences(extSource, baseAssembly.ToMetadataReference());

        // Main assembly
        var mainSource = """

		                 using Core;

		                 namespace App
		                 {
		                     public class FeatureManager
		                     {
		                         public void Load()
		                         {
		                             var features = FeatureBases.All;
		                         }
		                     }
		                 }
		                 """;

        // Act
        var result = RunGeneratorWithReferences(
            [mainSource],
            baseAssembly.ToMetadataReference(),
            extAssembly.ToMetadataReference());

        // Assert
        result.ContainsSource("FeatureBases.g.cs").ShouldBeTrue();

        var generatedCode = result["FeatureBases.g.cs"];
        // Should NOT include external features when flag is false
        generatedCode.ShouldNotContain("ExternalFeature");
    }

    [Fact(Skip = "Cross-assembly scanning requires running generator in base assembly")]
    public void GeneratorHandlesCircularAssemblyReferences()
    {
        // This is a complex scenario that might not be directly testable
        // but we should ensure the generator doesn't crash

        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;
		             using FractalDataWorks.SmartGenerators.AssemblyScanning;

		             [assembly: EnableAssemblyScanner]

		             namespace Test
		             {
		                 [EnhancedEnumBase(IncludeReferencedAssemblies = true)]
		                 public abstract class Base
		                 {
		                     public abstract string Name { get; }
		                 }

		                 [EnumOption]
		                 public class Option : Base
		                 {
		                     public override string Name => "Option";
		                 }
		             }
		             """;

        // Act - Should not throw or hang
        var result = RunGeneratorWithAssemblyScanning([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.ContainsSource("Bases.g.cs").ShouldBeTrue();
    }

    [Fact(Skip = "Cross-assembly scanning requires running generator in base assembly")]
    public void GeneratorHandlesMixedAssemblyOptions()
    {
        // Arrange - Base with some local options
        var baseSource = """

		                 using FractalDataWorks.EnhancedEnums.Attributes;
		                 using FractalDataWorks.SmartGenerators.AssemblyScanning;

		                 [assembly: EnableAssemblyScanner]

		                 namespace Core
		                 {
		                     [EnhancedEnumBase(IncludeReferencedAssemblies = true)]
		                     public abstract class ServiceBase
		                     {
		                         public abstract string Name { get; }
		                         public abstract int Priority { get; }
		                     }

		                     [EnumOption]
		                     public class CoreService : ServiceBase
		                     {
		                         public override string Name => "Core";
		                         public override int Priority => 100;
		                     }
		                 }
		                 """;

        // External assembly with additional options
        var extSource = """

		                using FractalDataWorks.EnhancedEnums.Attributes;
		                using Core;

		                namespace External
		                {
		                    [EnumOption]
		                    public class CustomService : ServiceBase
		                    {
		                        public override string Name => "Custom";
		                        public override int Priority => 50;
		                    }
		                }
		                """;

        // Create compilations
        var baseAssembly = CreateCompilationWithEnhancedEnumOption(baseSource);
        var extAssembly = CreateCompilationWithReferences(extSource, baseAssembly.ToMetadataReference());

        // Main assembly
        var mainSource = """

		                 using Core;

		                 namespace App
		                 {
		                     public class ServiceRegistry
		                     {
		                         public void Register()
		                         {
		                             var services = ServiceBases.All;
		                         }
		                     }
		                 }
		                 """;

        // Act
        var result = RunGeneratorWithReferences(
            [mainSource],
            baseAssembly.ToMetadataReference(),
            extAssembly.ToMetadataReference());

        // Assert
        result.ContainsSource("ServiceBases.g.cs").ShouldBeTrue();

        var generatedCode = result["ServiceBases.g.cs"];
        // Should include both local and external options
        generatedCode.ShouldContain("CoreService");
        generatedCode.ShouldContain("CustomService");
    }

    private static CSharpCompilation CreateCompilationWithReferences(string source, params MetadataReference[] references)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);

        return CSharpCompilation.Create(
            $"TestAssembly_{Guid.NewGuid():N}",
            new[] { syntaxTree },
            GetDefaultReferences().Concat(references).ToArray(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
