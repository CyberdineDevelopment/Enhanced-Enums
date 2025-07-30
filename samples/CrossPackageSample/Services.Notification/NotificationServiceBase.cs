using FractalDataWorks;

namespace Services.Notification;

[EnumCollection]
public abstract class NotificationServiceBase : EnhancedEnumBase<NotificationServiceBase>
{
    protected NotificationServiceBase(int id, string name) : base(id, name) { }
    
    public abstract string Send(string recipient, string message);
}