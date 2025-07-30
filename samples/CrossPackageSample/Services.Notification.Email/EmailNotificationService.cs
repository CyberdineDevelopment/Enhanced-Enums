using FractalDataWorks;
using Services.Notification;

namespace Services.Notification.Email;

[EnumOption]
public class EmailNotificationService : NotificationServiceBase
{
    public EmailNotificationService() : base(2, "Email") { }
    
    public override string Send(string recipient, string message)
    {
        return $"Message emailed to {recipient}";
    }
}