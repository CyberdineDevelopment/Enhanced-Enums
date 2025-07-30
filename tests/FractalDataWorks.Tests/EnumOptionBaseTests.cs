using FractalDataWorks;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tests;

public class EnumOptionBaseTests
{
    private class TestEnumOption : EnumOptionBase<TestEnumOption>
    {
        public TestEnumOption(int id, string name) : base(id, name) { }
    }

    [Fact]
    public void ConstructorSetsIdAndNameProperties()
    {
        var testEnum = new TestEnumOption(42, "TestValue");
        
        testEnum.Id.ShouldBe(42);
        testEnum.Name.ShouldBe("TestValue");
    }

    [Fact]
    public void ImplementsIEnhancedEnumOptionInterface()
    {
        var testEnum = new TestEnumOption(1, "Test");
        
        testEnum.ShouldBeAssignableTo<IEnumOption>();
    }

    [Fact]
    public void PropertiesAreReadOnly()
    {
        var testEnum = new TestEnumOption(5, "ReadOnlyTest");
        
        var idProperty = typeof(TestEnumOption).GetProperty(nameof(TestEnumOption.Id));
        var nameProperty = typeof(TestEnumOption).GetProperty(nameof(TestEnumOption.Name));
        
        idProperty.ShouldNotBeNull();
        nameProperty.ShouldNotBeNull();
        idProperty.CanWrite.ShouldBeFalse();
        nameProperty.CanWrite.ShouldBeFalse();
    }
}