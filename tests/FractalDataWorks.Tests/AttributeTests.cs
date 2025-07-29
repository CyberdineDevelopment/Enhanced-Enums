using System;
using FractalDataWorks;
using FractalDataWorks.Attributes;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tests;

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
        attribute.ReturnTypeNamespace.ShouldBeNull();
        attribute.DefaultGenericReturnType.ShouldBeNull();
        attribute.DefaultGenericReturnTypeNamespace.ShouldBeNull();
    }

    [Fact]
    public void EnumCollectionAttributeFullConstructorSetsAllProperties()
    {
        var attribute = new EnumCollectionAttribute(
            "TestCollection",
            "ITestType",
            false,
            StringComparison.OrdinalIgnoreCase,
            "TestNamespace",
            "ReturnTypeNamespace",
            "DefaultGeneric",
            "DefaultGenericNamespace");
        
        attribute.CollectionName.ShouldBe("TestCollection");
        attribute.ReturnType.ShouldBe("ITestType");
        attribute.GenerateFactoryMethods.ShouldBeFalse();
        attribute.NameComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
        attribute.Namespace.ShouldBe("TestNamespace");
        attribute.ReturnTypeNamespace.ShouldBe("ReturnTypeNamespace");
        attribute.DefaultGenericReturnType.ShouldBe("DefaultGeneric");
        attribute.DefaultGenericReturnTypeNamespace.ShouldBe("DefaultGenericNamespace");
    }

    [Fact]
    public void EnumOptionAttributeDefaultConstructorSetsExpectedDefaults()
    {
        var attribute = new EnumOptionAttribute();
        
        attribute.Name.ShouldBeNull();
        attribute.Order.ShouldBe(0);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.ReturnTypeNamespace.ShouldBeNull();
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
            "ITestReturn",
            "TestReturnNamespace",
            true,
            "TestMethod");
        
        attribute.Name.ShouldBe("TestName");
        attribute.Order.ShouldBe(5);
        attribute.CollectionName.ShouldBe("TestCollection");
        attribute.ReturnType.ShouldBe("ITestReturn");
        attribute.ReturnTypeNamespace.ShouldBe("TestReturnNamespace");
        attribute.GenerateFactoryMethod.ShouldBe(true);
        attribute.MethodName.ShouldBe("TestMethod");
    }

    [Fact]
    public void EnhancedEnumBaseAttributeDefaultConstructorSetsExpectedDefaults()
    {
        var attribute = new EnhancedEnumBaseAttribute();
        
        attribute.CollectionName.ShouldBe(string.Empty);
        attribute.UseFactory.ShouldBeFalse();
        attribute.NameComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
        attribute.IncludeReferencedAssemblies.ShouldBeFalse();
        attribute.ReturnType.ShouldBeNull();
        attribute.ReturnTypeNamespace.ShouldBeNull();
        attribute.DefaultGenericReturnType.ShouldBeNull();
        attribute.DefaultGenericReturnTypeNamespace.ShouldBeNull();
    }

    [Fact]
    public void EnhancedEnumBaseAttributeFullConstructorSetsAllProperties()
    {
        var attribute = new EnhancedEnumBaseAttribute(
            "TestCollection",
            true,
            StringComparison.Ordinal,
            true,
            "IReturnType",
            "ReturnNamespace",
            "DefaultGeneric",
            "DefaultNamespace");
        
        attribute.CollectionName.ShouldBe("TestCollection");
        attribute.UseFactory.ShouldBeTrue();
        attribute.NameComparison.ShouldBe(StringComparison.Ordinal);
        attribute.IncludeReferencedAssemblies.ShouldBeTrue();
        attribute.ReturnType.ShouldBe("IReturnType");
        attribute.ReturnTypeNamespace.ShouldBe("ReturnNamespace");
        attribute.DefaultGenericReturnType.ShouldBe("DefaultGeneric");
        attribute.DefaultGenericReturnTypeNamespace.ShouldBe("DefaultNamespace");
    }

    [Fact]
    public void EnumLookupAttributeConstructorSetsAllProperties()
    {
        var attribute = new EnumLookupAttribute("ByTestName", true, "ITestReturn");
        
        attribute.MethodName.ShouldBe("ByTestName");
        attribute.AllowMultiple.ShouldBeTrue();
        attribute.ReturnType.ShouldBe("ITestReturn");
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