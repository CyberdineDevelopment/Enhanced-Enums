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
        attribute.GenerateStaticCollection.ShouldBeTrue();
        attribute.Generic.ShouldBeFalse();
        attribute.NameComparison.ShouldBe(StringComparison.Ordinal);
        attribute.Namespace.ShouldBeNull();
        attribute.DefaultGenericReturnType.ShouldBeNull();
        attribute.IncludeReferencedAssemblies.ShouldBeFalse();
    }

    [Fact]
    public void EnumCollectionAttributeFullConstructorSetsAllProperties()
    {
        var attribute = new EnumCollectionAttribute(
            collectionName: "TestCollection",
            returnType: typeof(ITestType),
            generateFactoryMethods: false,
            generateStaticCollection: true,
            generic: false,
            nameComparison: StringComparison.OrdinalIgnoreCase,
            @namespace: "TestNamespace",
            defaultGenericReturnType: typeof(IDefaultGeneric),
            includeReferencedAssemblies: false);
        
        attribute.CollectionName.ShouldBe("TestCollection");
        attribute.ReturnType.ShouldBe(typeof(ITestType));
        attribute.GenerateFactoryMethods.ShouldBeFalse();
        attribute.GenerateStaticCollection.ShouldBeTrue();
        attribute.Generic.ShouldBeFalse();
        attribute.NameComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
        attribute.Namespace.ShouldBe("TestNamespace");
        attribute.DefaultGenericReturnType.ShouldBe(typeof(IDefaultGeneric));
        attribute.IncludeReferencedAssemblies.ShouldBeFalse();
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

    [Fact]
    public void EnumCollectionAttributeGenericPropertyCanBeSetIndependently()
    {
        var attribute = new EnumCollectionAttribute
        {
            Generic = true,
            GenerateStaticCollection = true // Should be ignored when Generic = true
        };

        attribute.Generic.ShouldBeTrue();
        attribute.GenerateStaticCollection.ShouldBeTrue(); // Property is still set but ignored in generation
    }

    [Fact]
    public void EnumCollectionAttributeInstanceCollectionGeneration()
    {
        var attribute = new EnumCollectionAttribute
        {
            GenerateStaticCollection = false,
            Generic = false
        };

        attribute.GenerateStaticCollection.ShouldBeFalse();
        attribute.Generic.ShouldBeFalse();
    }

    [Fact]
    public void EnumCollectionAttributeGenericWithDefaultReturnType()
    {
        var attribute = new EnumCollectionAttribute
        {
            Generic = true,
            DefaultGenericReturnType = typeof(IDefaultGeneric)
        };

        attribute.Generic.ShouldBeTrue();
        attribute.DefaultGenericReturnType.ShouldBe(typeof(IDefaultGeneric));
    }

    [Fact]
    public void EnumCollectionAttributeCrossAssemblyDiscovery()
    {
        var attribute = new EnumCollectionAttribute
        {
            IncludeReferencedAssemblies = true,
            CollectionName = "CrossAssemblyCollection"
        };

        attribute.IncludeReferencedAssemblies.ShouldBeTrue();
        attribute.CollectionName.ShouldBe("CrossAssemblyCollection");
    }

    [Fact]
    public void EnumCollectionAttributeSupportsDifferentStringComparisons()
    {
        var ordinalAttribute = new EnumCollectionAttribute { NameComparison = StringComparison.Ordinal };
        var ignoreCaseAttribute = new EnumCollectionAttribute { NameComparison = StringComparison.OrdinalIgnoreCase };
        var currentCultureAttribute = new EnumCollectionAttribute { NameComparison = StringComparison.CurrentCulture };

        ordinalAttribute.NameComparison.ShouldBe(StringComparison.Ordinal);
        ignoreCaseAttribute.NameComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
        currentCultureAttribute.NameComparison.ShouldBe(StringComparison.CurrentCulture);
    }

    [Fact]
    public void EnumCollectionAttributeNamespaceOverrideWorks()
    {
        var attribute = new EnumCollectionAttribute
        {
            Namespace = "CustomNamespace.ForGeneration"
        };

        attribute.Namespace.ShouldBe("CustomNamespace.ForGeneration");
    }

    [Fact]
    public void EnumOptionAttributeCustomMethodNameGeneration()
    {
        var attribute = new EnumOptionAttribute(
            generateFactoryMethod: true,
            methodName: "CreateCustom");

        attribute.GenerateFactoryMethod.ShouldBe(true);
        attribute.MethodName.ShouldBe("CreateCustom");
    }

    [Fact]
    public void EnumOptionAttributeCustomOrderAndNaming()
    {
        var attribute = new EnumOptionAttribute(
            name: "CustomDisplayName",
            order: 100);

        attribute.Name.ShouldBe("CustomDisplayName");
        attribute.Order.ShouldBe(100);
    }

    [Fact]
    public void EnumLookupAttributeMultipleReturnTypesSupported()
    {
        var singleAttribute = new EnumLookupAttribute("GetSingle", false, typeof(ITestReturn));
        var multipleAttribute = new EnumLookupAttribute("GetMultiple", true, typeof(ITestReturn));

        singleAttribute.AllowMultiple.ShouldBeFalse();
        singleAttribute.ReturnType.ShouldBe(typeof(ITestReturn));
        
        multipleAttribute.AllowMultiple.ShouldBeTrue();
        multipleAttribute.ReturnType.ShouldBe(typeof(ITestReturn));
    }
}