using FractalDataWorks;

namespace NotificationConsole;

[EnumCollection(CollectionName = "TestItems")]
public abstract class TestItemBase : EnumOptionBase<TestItemBase>
{
    protected TestItemBase(int id, string name) : base(id, name) { }
}