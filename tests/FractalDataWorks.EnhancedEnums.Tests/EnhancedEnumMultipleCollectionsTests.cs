using System.Linq;
using FractalDataWorks.EnhancedEnums.Tests.TestHelpers;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests;

/// <summary>
/// Tests for multiple collections feature in EnhancedEnumOptionGenerator.
/// </summary>
public class EnhancedEnumOptionMultipleCollectionsTests : EnhancedEnumOptionTestBase
{
    [Fact]
    public void GeneratorCreatesMultipleCollectionsFromSameBase()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         [EnhancedEnumOption("PositionPlayers")]
                         [EnhancedEnumOption("Pitchers")]
                         public abstract class BaseballPlayerBase
                         {
                             public abstract string Name { get; }
                             public abstract string Position { get; }
                         }

                         [EnumOption(CollectionName = "PositionPlayers")]
                         public class Catcher : BaseballPlayerBase
                         {
                             public override string Name => "Catcher";
                             public override string Position => "C";
                         }

                         [EnumOption(CollectionName = "Pitchers")]
                         public class StartingPitcher : BaseballPlayerBase
                         {
                             public override string Name => "Starting Pitcher";
                             public override string Position => "SP";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        // Should generate separate collection files
        result.GeneratedSources.ShouldContainKey("PositionPlayers.g.cs");
        result.GeneratedSources.ShouldContainKey("Pitchers.g.cs");

        var positionPlayersCode = result.GeneratedSources["PositionPlayers.g.cs"];
        var pitchersCode = result.GeneratedSources["Pitchers.g.cs"];

        // Write generated code to files for debugging
        WriteGeneratedCodeToFile("MultipleCollections_PositionPlayers.g.cs", positionPlayersCode);
        WriteGeneratedCodeToFile("MultipleCollections_Pitchers.g.cs", pitchersCode);

        // PositionPlayers should contain Catcher but not StartingPitcher
        positionPlayersCode.ShouldContain("Catcher");
        positionPlayersCode.ShouldNotContain("StartingPitcher");

        // Pitchers should contain StartingPitcher but not Catcher
        pitchersCode.ShouldContain("StartingPitcher");
        pitchersCode.ShouldNotContain("Catcher");
    }

    [Fact]
    public void GeneratorSupportsOptionsInMultipleCollections()
    {
        // Arrange - Two-way player belongs to both collections
        var source = """

                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         [EnhancedEnumOption("PositionPlayers")]
                         [EnhancedEnumOption("Pitchers")]
                         public abstract class BaseballPlayerBase
                         {
                             public abstract string Name { get; }
                         }

                         [EnumOption(CollectionName = "PositionPlayers")]
                         public class FirstBaseman : BaseballPlayerBase
                         {
                             public override string Name => "First Baseman";
                         }

                         [EnumOption(CollectionName = "Pitchers")]
                         public class ClosingPitcher : BaseballPlayerBase
                         {
                             public override string Name => "Closing Pitcher";
                         }

                         [EnumOption(CollectionName = "Pitchers")]
                         [EnumOption(CollectionName = "PositionPlayers")]
                         public class TwoWayPlayer : BaseballPlayerBase
                         {
                             public override string Name => "Two-Way Player";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        result.GeneratedSources.ShouldContainKey("PositionPlayers.g.cs");
        result.GeneratedSources.ShouldContainKey("Pitchers.g.cs");

        var positionPlayersCode = result.GeneratedSources["PositionPlayers.g.cs"];
        var pitchersCode = result.GeneratedSources["Pitchers.g.cs"];

        // PositionPlayers should contain FirstBaseman and TwoWayPlayer
        positionPlayersCode.ShouldContain("FirstBaseman");
        positionPlayersCode.ShouldContain("TwoWayPlayer");
        positionPlayersCode.ShouldNotContain("ClosingPitcher");

        // Pitchers should contain ClosingPitcher and TwoWayPlayer
        pitchersCode.ShouldContain("ClosingPitcher");
        pitchersCode.ShouldContain("TwoWayPlayer");
        pitchersCode.ShouldNotContain("FirstBaseman");
    }

    [Fact]
    public void GeneratorCreatesIndependentLookupMethodsForEachCollection()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         [EnhancedEnumOption("ActiveUsers")]
                         [EnhancedEnumOption("AdminUsers")]
                         public abstract class UserBase
                         {
                             public abstract string Name { get; }
                             [EnumLookup] public abstract string Department { get; }
                         }

                         [EnumOption(CollectionName = "ActiveUsers")]
                         public class RegularUser : UserBase
                         {
                             public override string Name => "Regular User";
                             public override string Department => "General";
                         }

                         [EnumOption(CollectionName = "AdminUsers")]
                         public class SuperAdmin : UserBase
                         {
                             public override string Name => "Super Admin";
                             public override string Department => "IT";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        var activeUsersCode = result.GeneratedSources["ActiveUsers.g.cs"];
        var adminUsersCode = result.GeneratedSources["AdminUsers.g.cs"];

        // Both collections should have their own lookup methods
        activeUsersCode.ShouldContain("GetByDepartment");
        adminUsersCode.ShouldContain("GetByDepartment");

        // Both should have GetByName methods (default)
        activeUsersCode.ShouldContain("GetByName");
        adminUsersCode.ShouldContain("GetByName");
    }

    [Fact]
    public void GeneratorHandlesMultipleCollectionsWithFactoryPattern()
    {
        // Arrange
        var source = """

                     using System;
                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         [EnhancedEnumOption("Circles", UseFactory = true)]
                         [EnhancedEnumOption("Squares", UseFactory = true)]
                         public abstract class ShapeBase
                         {
                             public abstract string Name { get; }
                             
                             public static ShapeBase Create(Type type)
                             {
                                 if (type.Name == "Circle")
                                     return new Circle();
                                 if (type.Name == "Square")
                                     return new Square();
                                 throw new ArgumentException($"Unknown type: {type}");
                             }
                         }

                         [EnumOption(CollectionName = "Circles")]
                         public class Circle : ShapeBase
                         {
                             public override string Name => "Circle";
                         }

                         [EnumOption(CollectionName = "Squares")]
                         public class Square : ShapeBase
                         {
                             public override string Name => "Square";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        var circlesCode = result.GeneratedSources["Circles.g.cs"];
        var squaresCode = result.GeneratedSources["Squares.g.cs"];

        WriteGeneratedCodeToFile("MultipleCollectionsFactory_Circles.g.cs", circlesCode);
        WriteGeneratedCodeToFile("MultipleCollectionsFactory_Squares.g.cs", squaresCode);

        // Both collections should use factory pattern
        circlesCode.ShouldContain(".Create(typeof(");
        circlesCode.ShouldContain("Circle");
        circlesCode.ShouldNotContain("Square");

        squaresCode.ShouldContain(".Create(typeof(");
        squaresCode.ShouldContain("Square");
        squaresCode.ShouldNotContain("Circle");
    }

    [Fact]
    public void GeneratorHandlesThreeOrMoreCollections()
    {
        // Arrange
        var source = """

                     using FractalDataWorks.EnhancedEnums.Attributes;

                     namespace TestNamespace
                     {
                         [EnhancedEnumOption("Small")]
                         [EnhancedEnumOption("Medium")]
                         [EnhancedEnumOption("Large")]
                         public abstract class SizeBase
                         {
                             public abstract string Name { get; }
                         }

                         [EnumOption(CollectionName = "Small")]
                         public class TinySize : SizeBase
                         {
                             public override string Name => "Tiny";
                         }

                         [EnumOption(CollectionName = "Medium")]
                         public class RegularSize : SizeBase
                         {
                             public override string Name => "Regular";
                         }

                         [EnumOption(CollectionName = "Large")]
                         public class HugeSize : SizeBase
                         {
                             public override string Name => "Huge";
                         }
                     }
                     """;

        // Act
        var result = RunGenerator([source]);

        // Assert
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        // Should generate three separate collections
        result.GeneratedSources.ShouldContainKey("Small.g.cs");
        result.GeneratedSources.ShouldContainKey("Medium.g.cs");
        result.GeneratedSources.ShouldContainKey("Large.g.cs");

        var smallCode = result.GeneratedSources["Small.g.cs"];
        var mediumCode = result.GeneratedSources["Medium.g.cs"];
        var largeCode = result.GeneratedSources["Large.g.cs"];

        // Each collection should contain only its own options
        smallCode.ShouldContain("TinySize");
        smallCode.ShouldNotContain("RegularSize");
        smallCode.ShouldNotContain("HugeSize");

        mediumCode.ShouldContain("RegularSize");
        mediumCode.ShouldNotContain("TinySize");
        mediumCode.ShouldNotContain("HugeSize");

        largeCode.ShouldContain("HugeSize");
        largeCode.ShouldNotContain("TinySize");
        largeCode.ShouldNotContain("RegularSize");
    }
}
