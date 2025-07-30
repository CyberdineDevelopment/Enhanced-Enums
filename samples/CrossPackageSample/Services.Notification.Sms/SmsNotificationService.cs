using FractalDataWorks;
using Services.Notification;

namespace Services.Notification.Sms;

[EnumOption]
public class SmsNotificationService : NotificationServiceBase
{
    public SmsNotificationService() : base(1, "SMS") { }
    
    public override string Send(string recipient, string message)
    {
        return $"Message texted to {recipient}";
    }
}