using System;
using FractalDataWorks;
using FractalDataWorks.Attributes;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tests;

// Test interfaces for type parameter tests
public interface ITestType { }
public interface IDefaultGeneric { }
public interface ITestReturn { }

public class AttributeTests
{
    [Fact]
    public void EnumCollectionAttributeDefaultConstructorSetsExpectedDefaults()
    {
        var attribute = new EnumCollectionAttribute();
        
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethods.ShouldBeTrue();
        attribute.NameComparison.ShouldBe(StringComparison.Ordinal);
        attribute.Namespace.ShouldBeNull();
        attribute.DefaultGenericReturnType.ShouldBeNull();
    }

    [Fact]
    public void EnumCollectionAttributeFullConstructorSetsAllProperties()
    {
        var attribute = new EnumCollectionAttribute(
            "TestCollection",
            typeof(ITestType),
            false,
            StringComparison.OrdinalIgnoreCase,
            "TestNamespace",
            typeof(IDefaultGeneric));
        
        attribute.CollectionName.ShouldBe("TestCollection");
        attribute.ReturnType.ShouldBe(typeof(ITestType));
        attribute.GenerateFactoryMethods.ShouldBeFalse();
        attribute.NameComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
        attribute.Namespace.ShouldBe("TestNamespace");
        attribute.DefaultGenericReturnType.ShouldBe(typeof(IDefaultGeneric));
    }

    [Fact]
    public void EnumOptionAttributeDefaultConstructorSetsExpectedDefaults()
    {
        var attribute = new EnumOptionAttribute();
        
        attribute.Name.ShouldBeNull();
        attribute.Order.ShouldBe(0);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethod.ShouldBeNull();
        attribute.MethodName.ShouldBeNull();
    }

    [Fact]
    public void EnumOptionAttributeFullConstructorSetsAllProperties()
    {
        var attribute = new EnumOptionAttribute(
            "TestName",
            5,
            "TestCollection",
            typeof(ITestReturn),
            true,
            "TestMethod");
        
        attribute.Name.ShouldBe("TestName");
        attribute.Order.ShouldBe(5);
        attribute.CollectionName.ShouldBe("TestCollection");
        attribute.ReturnType.ShouldBe(typeof(ITestReturn));
        attribute.GenerateFactoryMethod.ShouldBe(true);
        attribute.MethodName.ShouldBe("TestMethod");
    }


    [Fact]
    public void EnumLookupAttributeConstructorSetsAllProperties()
    {
        var attribute = new EnumLookupAttribute("ByTestName", true, typeof(ITestReturn));
        
        attribute.MethodName.ShouldBe("ByTestName");
        attribute.AllowMultiple.ShouldBeTrue();
        attribute.ReturnType.ShouldBe(typeof(ITestReturn));
    }

    [Fact]
    public void EnumLookupAttributeWithDefaultParametersSetsExpectedValues()
    {
        var attribute = new EnumLookupAttribute();
        
        attribute.MethodName.ShouldBe("");
        attribute.AllowMultiple.ShouldBeFalse();
        attribute.ReturnType.ShouldBeNull();
    }
}