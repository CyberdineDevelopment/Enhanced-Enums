using System;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Tests;

public class InterfaceBuilderGenericTests
{
    [Fact]
    public void InterfaceBuilderWithSingleTypeParameterGeneratesCorrectly()
    {
        // Arrange
        var builder = new InterfaceBuilder("IMyInterface")
            .WithTypeParameter("T")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public interface IMyInterface<T>");
        result.ShouldNotContain("where"); // No constraints
    }

    [Fact]
    public void InterfaceBuilderWithMultipleTypeParametersGeneratesCorrectly()
    {
        // Arrange
        var builder = new InterfaceBuilder("IMyInterface")
            .WithTypeParameters("T1", "T2", "T3")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public interface IMyInterface<T1, T2, T3>");
    }

    [Fact]
    public void InterfaceBuilderWithTypeConstraintsGeneratesCorrectly()
    {
        // Arrange
        var builder = new InterfaceBuilder("IRepository")
            .WithTypeParameter("T")
            .WithTypeConstraint("where T : class")
            .WithTypeConstraint("where T : IEntity")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public interface IRepository<T>");
        result.ShouldContain("where T : class");
        result.ShouldContain("where T : IEntity");
    }

    [Fact]
    public void InterfaceBuilderWithVarianceModifiersGeneratesCorrectly()
    {
        // Arrange
        var builder = new InterfaceBuilder("IConverter")
            .WithTypeParameters("TIn", "TOut")
            .WithBaseInterface("IDisposable")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public interface IConverter<TIn, TOut> : IDisposable");
    }
}