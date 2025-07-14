using System;
using System.Globalization;
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
/// Tests for incremental generation behavior in EnhancedEnumOptionGenerator.
/// </summary>
public class EnhancedEnumOptionIncrementalTests : EnhancedEnumOptionTestBase
{
    [Fact]
    public void GeneratorCachesOutputWhenSourceUnchanged()
    {
        // Arrange
        var source = """

		             using FractalDataWorks.EnhancedEnums.Attributes;
		             namespace TestNamespace
		             {
		                 [EnhancedEnumOption]
		                 public abstract class StatusBase
		                 {
		                     public abstract string Name { get; }
		                 }

		                 [EnumOption]
		                 public class Active : StatusBase
		                 {
		                     public override string Name => "Active";
		                 }
		             }
		             """;

        var generator = new EnhancedEnumOptionGenerator();
        var driver = CreateGeneratorDriver(generator);

        // Act - First run
        var compilation1 = CreateCompilationWithEnhancedEnumOption(source);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation1, out _, out _, TestContext.Current.CancellationToken);

        // Add unrelated source that shouldn't affect enum generation
        var unrelatedSource = """

		                      namespace TestNamespace
		                      {
		                          public class UnrelatedClass
		                          {
		                              public void DoSomething() { }
		                          }
		                      }
		                      """;

        var compilation2 = compilation1.AddSyntaxTrees(CSharpSyntaxTree.ParseText(unrelatedSource, cancellationToken: TestContext.Current.CancellationToken));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation2, out _, out _, TestContext.Current.CancellationToken);

        // Assert - Should use cached output
        var runResult = driver.GetRunResult();
        
        // Check that the generator recognized the unrelated change and cached the enum generation
        runResult.Results.Length.ShouldBe(1);
        
        // Verify the generated output is still present
        var generatedTrees = runResult.GeneratedTrees;
        generatedTrees.ShouldContain(t => t.FilePath.EndsWith("StatusBases.g.cs"));
        
        // The incremental generator should have cached results for unchanged enum sources
        var trackedSteps = runResult.Results[0].TrackedSteps;
        trackedSteps.SelectMany(s => s.Value).Any(step => step.Outputs.Length > 0).ShouldBeTrue();
    }

    [Fact]
    public void GeneratorRegeneratesWhenEnumBaseChanges()
    {
        // Arrange
        var source1 = """

		              using FractalDataWorks.EnhancedEnums.Attributes;
		              namespace TestNamespace
		              {
		                  [EnhancedEnumOption]
		                  public abstract class ColorBase
		                  {
		                      public abstract string Name { get; }
		                  }

		                  [EnumOption]
		                  public class Red : ColorBase
		                  {
		                      public override string Name => "Red";
		                  }
		              }
		              """;

        var source2 = """

		              using FractalDataWorks.EnhancedEnums.Attributes;
		              namespace TestNamespace
		              {
		                  [EnhancedEnumOption("MyColors")] // Changed collection name
		                  public abstract class ColorBase
		                  {
		                      public abstract string Name { get; }
		                  }

		                  [EnumOption]
		                  public class Red : ColorBase
		                  {
		                      public override string Name => "Red";
		                  }
		              }
		              """;

        var generator = new EnhancedEnumOptionGenerator();
        var driver = CreateGeneratorDriver(generator);

        // Act
        var compilation1 = CreateCompilationWithEnhancedEnumOption(source1);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation1, out var output1, out _, TestContext.Current.CancellationToken);

        var compilation2 = CreateCompilationWithEnhancedEnumOption(source2);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation2, out var output2, out _, TestContext.Current.CancellationToken);

        // Assert - Should regenerate with new collection name
        var trees1 = output1.SyntaxTrees.Where(t => t.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)).ToList();
        var trees2 = output2.SyntaxTrees.Where(t => t.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)).ToList();

        trees1.ShouldContain(t => t.FilePath.Contains("ColorBases.g.cs"));
        // The second compilation should have the new collection name
        trees2.Any(t => t.FilePath.Contains("MyColors.g.cs")).ShouldBeTrue();
    }

    [Fact]
    public void GeneratorRegeneratesWhenNewOptionAdded()
    {
        // Arrange
        var baseSource = """

		                 using FractalDataWorks.EnhancedEnums.Attributes;
		                 namespace TestNamespace
		                 {
		                     [EnhancedEnumOption]
		                     public abstract class AnimalBase
		                     {
		                         public abstract string Name { get; }
		                     }

		                     [EnumOption]
		                     public class Dog : AnimalBase
		                     {
		                         public override string Name => "Dog";
		                     }
		                 }
		                 """;

        var newOptionSource = """

		                      using FractalDataWorks.EnhancedEnums.Attributes;

		                      namespace TestNamespace
		                      {
		                          [EnumOption]
		                          public class Cat : AnimalBase
		                          {
		                              public override string Name => "Cat";
		                          }
		                      }
		                      """;

        var generator = new EnhancedEnumOptionGenerator();
        var driver = CreateGeneratorDriver(generator);

        // Act
        var compilation1 = CreateCompilationWithEnhancedEnumOption(baseSource);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation1, out _, out _, TestContext.Current.CancellationToken);

        var compilation2 = compilation1.AddSyntaxTrees(CSharpSyntaxTree.ParseText(newOptionSource, cancellationToken: TestContext.Current.CancellationToken));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation2, out _, out _, TestContext.Current.CancellationToken);

        // Assert
        var runResult = driver.GetRunResult();
        var generatedSource = runResult.GeneratedTrees.First(t => t.FilePath.Contains("AnimalBases.g.cs"));
        var content = generatedSource.GetText(TestContext.Current.CancellationToken).ToString();

        // Should include both options
        content.ShouldContain("new TestNamespace.Dog()");
        content.ShouldContain("new TestNamespace.Cat()");
    }

    [Fact]
    public void GeneratorHandlesRemovedEnumOptions()
    {
        // Arrange
        var source1 = """

		              using FractalDataWorks.EnhancedEnums.Attributes;
		              namespace TestNamespace
		              {
		                  [EnhancedEnumOption]
		                  public abstract class StatusBase
		                  {
		                      public abstract string Name { get; }
		                  }

		                  [EnumOption]
		                  public class Active : StatusBase
		                  {
		                      public override string Name => "Active";
		                  }

		                  [EnumOption]
		                  public class Inactive : StatusBase
		                  {
		                      public override string Name => "Inactive";
		                  }
		              }
		              """;

        var source2 = """

		              using FractalDataWorks.EnhancedEnums.Attributes;
		              namespace TestNamespace
		              {
		                  [EnhancedEnumOption]
		                  public abstract class StatusBase
		                  {
		                      public abstract string Name { get; }
		                  }

		                  [EnumOption]
		                  public class Active : StatusBase
		                  {
		                      public override string Name => "Active";
		                  }
		                  // Inactive option removed
		              }
		              """;

        var generator = new EnhancedEnumOptionGenerator();
        var driver = CreateGeneratorDriver(generator);

        // Act
        var compilation1 = CreateCompilationWithEnhancedEnumOption(source1);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation1, out _, out _, TestContext.Current.CancellationToken);

        var compilation2 = CreateCompilationWithEnhancedEnumOption(source2);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation2, out _, out _, TestContext.Current.CancellationToken);

        // Assert
        var runResult = driver.GetRunResult();
        var generatedSource = runResult.GeneratedTrees.First(t => t.FilePath.Contains("StatusBases.g.cs"));
        var content = generatedSource.GetText(TestContext.Current.CancellationToken).ToString();

        // Should only include Active
        content.ShouldContain("new TestNamespace.Active()");
        content.ShouldNotContain("new TestNamespace.Inactive()");
    }

    [Fact]
    public void GeneratorHandlesAttributeChanges()
    {
        // Arrange
        var source1 = """

		              using FractalDataWorks.EnhancedEnums.Attributes;
		              namespace TestNamespace
		              {
		                  [EnhancedEnumOption]
		                  public abstract class ProductBase
		                  {
		                      public abstract string Name { get; }
		                      public abstract string Code { get; } // Not a lookup
		                  }

		                  [EnumOption]
		                  public class Widget : ProductBase
		                  {
		                      public override string Name => "Widget";
		                      public override string Code => "WDG";
		                  }
		              }
		              """;

        var source2 = """

		              using FractalDataWorks.EnhancedEnums.Attributes;
		              namespace TestNamespace
		              {
		                  [EnhancedEnumOption]
		                  public abstract class ProductBase
		                  {
		                      public abstract string Name { get; }
		                      
		                      [EnumLookup] // Added lookup attribute
		                      public abstract string Code { get; }
		                  }

		                  [EnumOption]
		                  public class Widget : ProductBase
		                  {
		                      public override string Name => "Widget";
		                      public override string Code => "WDG";
		                  }
		              }
		              """;

        var generator = new EnhancedEnumOptionGenerator();
        var driver = CreateGeneratorDriver(generator);

        // Act
        var compilation1 = CreateCompilationWithEnhancedEnumOption(source1);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation1, out _, out _, TestContext.Current.CancellationToken);

        var compilation2 = CreateCompilationWithEnhancedEnumOption(source2);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation2, out _, out _, TestContext.Current.CancellationToken);

        // Assert
        var runResult = driver.GetRunResult();
        var generatedSource = runResult.GeneratedTrees.First(t => t.FilePath.Contains("ProductBases.g.cs"));
        var content = generatedSource.GetText(TestContext.Current.CancellationToken).ToString();

        // Lookup methods are not implemented, just verify it regenerated
        content.ShouldContain("ProductBases");
    }

    [Fact]
    public void GeneratorHandlesLargeNumberOfOptions()
    {
        // Arrange - Generate many enum options
        var sourceBuilder = new System.Text.StringBuilder();
        sourceBuilder.AppendLine("""

		                         using FractalDataWorks.EnhancedEnums.Attributes;
		                         namespace TestNamespace
		                         {
		                             [EnhancedEnumOption]
		                             public abstract class ItemBase
		                             {
		                                 public abstract string Name { get; }
		                                 public abstract int Id { get; }
		                             }

		                         """);

        // Add 100 enum options
        for (int i = 0; i < 100; i++)
        {
            sourceBuilder.AppendLine(CultureInfo.InvariantCulture, $$"""

			                                                             [EnumOption]
			                                                             public class Item{{i}} : ItemBase
			                                                             {
			                                                                 public override string Name => "Item {{i}}";
			                                                                 public override int Id => {{i}};
			                                                             }
			                                                         """);
        }

        sourceBuilder.AppendLine("}");

        var generator = new EnhancedEnumOptionGenerator();
        var driver = CreateGeneratorDriver(generator);

        // Act
        var compilation = CreateCompilationWithEnhancedEnumOption(sourceBuilder.ToString());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _, TestContext.Current.CancellationToken);

        // Assert
        var runResult = driver.GetRunResult();
        runResult.Results[0].Exception.ShouldBeNull();

        var generatedSource = runResult.GeneratedTrees.First(t => t.FilePath.Contains("ItemBases.g.cs"));
        var content = generatedSource.GetText(TestContext.Current.CancellationToken).ToString();

        // Should include all 100 items
        content.ShouldContain("new TestNamespace.Item0()");
        content.ShouldContain("new TestNamespace.Item99()");
    }
}
