using System;
using System.Linq;
using System.Reflection;
using FractalDataWorks;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tests;

public class CompilationTests
{
    [Fact]
    public void AssemblyShouldCompile()
    {
        var assembly = typeof(IEnhancedEnumOption).Assembly;
        assembly.ShouldNotBeNull();
    }

    [Fact]
    public void IEnhancedEnumShouldBePublic()
    {
        var type = typeof(IEnhancedEnumOption);
        type.IsPublic.ShouldBeTrue();
        type.IsInterface.ShouldBeTrue();
    }

    [Fact]
    public void EnhancedEnumGenericShouldHaveCorrectConstraints()
    {
        var type = typeof(IEnhancedEnumOption<>);
        var genericArgument = type.GetGenericArguments().First();
        var constraints = genericArgument.GetGenericParameterConstraints();
        
        constraints.ShouldContain(c => c == typeof(IEnhancedEnumOption<>).MakeGenericType(genericArgument));
    }

    [Fact]
    public void PublicTypesShouldFollowExpectedPattern()
    {
        var assembly = typeof(IEnhancedEnumOption).Assembly;
        var types = assembly.GetTypes().Where(t => t.IsPublic && !t.IsEnum);
        
        foreach (var type in types)
        {
            var isValidType = type.IsInterface || 
                             type.IsSubclassOf(typeof(Attribute)) || 
                             type == typeof(Fractal) ||
                             (type.IsClass && type.Name.Contains("EnhancedEnum"));
            
            isValidType.ShouldBeTrue($"{type.Name} should be an interface, attribute, Fractal struct, or EnhancedEnum class");
        }
    }

    [Fact]
    public void EnhancedEnumShouldHaveExpectedMembers()
    {
        var type = typeof(IEnhancedEnumOption);
        
        // Check for Id property
        var idProperty = type.GetProperty("Id");
        idProperty.ShouldNotBeNull();
        idProperty.PropertyType.ShouldBe(typeof(int));
        
        // Check for Name property
        var nameProperty = type.GetProperty("Name");
        nameProperty.ShouldNotBeNull();
        nameProperty.PropertyType.ShouldBe(typeof(string));
    }

    [Fact]
    public void EnhancedEnumGenericShouldHaveEmptyMethod()
    {
        var type = typeof(IEnhancedEnumOption<>);
        var emptyMethod = type.GetMethod("Empty");
        
        emptyMethod.ShouldNotBeNull();
        emptyMethod.GetParameters().Length.ShouldBe(0);
    }
}