using FractalDataWorks;

namespace Services.Notification;

[EnumCollection(CollectionName = "NotificationServiceBases")]
public abstract class NotificationServiceBase : EnumOptionBase<NotificationServiceBase>
{
    protected NotificationServiceBase(int id, string name) : base(id, name) { }
    
    public abstract string Send(string recipient, string message);
}