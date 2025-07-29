using FractalDataWorks;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tests;

public class EnhancedEnumBaseTests
{
    private class TestEnhancedEnum : EnhancedEnumBase<TestEnhancedEnum>
    {
        public TestEnhancedEnum(int id, string name) : base(id, name) { }
    }

    [Fact]
    public void ConstructorSetsIdAndNameProperties()
    {
        var testEnum = new TestEnhancedEnum(42, "TestValue");
        
        testEnum.Id.ShouldBe(42);
        testEnum.Name.ShouldBe("TestValue");
    }

    [Fact]
    public void ImplementsIEnhancedEnumOptionInterface()
    {
        var testEnum = new TestEnhancedEnum(1, "Test");
        
        testEnum.ShouldBeAssignableTo<IEnhancedEnumOption>();
    }

    [Fact]
    public void PropertiesAreReadOnly()
    {
        var testEnum = new TestEnhancedEnum(5, "ReadOnlyTest");
        
        var idProperty = typeof(TestEnhancedEnum).GetProperty(nameof(TestEnhancedEnum.Id));
        var nameProperty = typeof(TestEnhancedEnum).GetProperty(nameof(TestEnhancedEnum.Name));
        
        idProperty.ShouldNotBeNull();
        nameProperty.ShouldNotBeNull();
        idProperty.CanWrite.ShouldBeFalse();
        nameProperty.CanWrite.ShouldBeFalse();
    }
}