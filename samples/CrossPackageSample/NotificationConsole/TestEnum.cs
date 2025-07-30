using FractalDataWorks;

namespace NotificationConsole;

[EnumCollection(CollectionName = "TestItems")]
public abstract class TestItemBase : EnumOptionBase<TestItemBase>
{
    protected TestItemBase(int id, string name) : base(id, name) { }
}

[EnumOption]
public class TestItem1 : TestItemBase
{
    public TestItem1() : base(1, "Test1") { }
}

[EnumOption]
public class TestItem2 : TestItemBase
{
    public TestItem2() : base(2, "Test2") { }
}