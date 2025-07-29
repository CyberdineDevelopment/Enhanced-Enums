using System;
using FractalDataWorks.SmartGenerators.CodeBuilders;
using Shouldly;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Tests;

public class ClassBuilderGenericTests
{
    [Fact]
    public void ClassBuilderWithSingleTypeParameterGeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass")
            .WithTypeParameter("T")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public class MyClass<T>");
        result.ShouldNotContain("where"); // No constraints
    }

    [Fact]
    public void ClassBuilderWithMultipleTypeParametersGeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass")
            .WithTypeParameters("T1", "T2", "T3")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public class MyClass<T1, T2, T3>");
    }

    [Fact]
    public void ClassBuilderWithTypeConstraintsGeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass")
            .WithTypeParameter("T")
            .WithTypeConstraint("where T : class")
            .WithTypeConstraint("where T : IDisposable")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public class MyClass<T>");
        result.ShouldContain("where T : class");
        result.ShouldContain("where T : IDisposable");
    }

    [Fact]
    public void ClassBuilderWithGenericBaseTypeGeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass")
            .WithTypeParameter("T")
            .WithBaseType("BaseClass<T>")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public class MyClass<T> : BaseClass<T>");
    }

    [Fact]
    public void ClassBuilderWithGenericInterfacesGeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassBuilder("MyClass")
            .WithTypeParameter("T")
            .ImplementsInterface("IService<T>")
            .ImplementsInterface("IDisposable")
            .MakePublic();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public class MyClass<T> : IService<T>, IDisposable");
    }

    [Fact]
    public void ClassBuilderWithComplexGenericsGeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassBuilder("ServiceFactory")
            .WithTypeParameters("TService", "TConfig")
            .WithBaseType("FactoryBase<TService>")
            .ImplementsInterface("IFactory<TService>")
            .WithTypeConstraint("where TService : IService<TConfig>")
            .WithTypeConstraint("where TConfig : IConfiguration")
            .MakePublic()
            .MakeAbstract();
        
        // Act
        var result = builder.Build();
        
        // Assert
        result.ShouldContain("public abstract class ServiceFactory<TService, TConfig> : FactoryBase<TService>, IFactory<TService>");
        result.ShouldContain("where TService : IService<TConfig>");
        result.ShouldContain("where TConfig : IConfiguration");
    }
}