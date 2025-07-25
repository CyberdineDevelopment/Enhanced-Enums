using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace InterfaceBasedDiscovery;

// Define a base interface for services
public interface INotificationService
{
    string Name { get; }
    Task SendAsync(string message);
}

// Base class that can be in a base package
public abstract class NotificationServiceBase : INotificationService
{
    public abstract string Name { get; }
    public abstract int Id { get; }
    public abstract Task SendAsync(string message);
}

// Implementation 1 - Email (could be in separate package)
[EnumOption(Name = "Email")]
public class EmailNotificationService : NotificationServiceBase, IEnhancedEnumOptionAlt<NotificationServiceBase>
{
    public override string Name => "Email";
    public override int Id => 1;
    
    public override Task SendAsync(string message)
    {
        Console.WriteLine($"Sending email: {message}");
        return Task.CompletedTask;
    }
}

// Implementation 2 - SMS (could be in separate package)
[EnumOption(Name = "SMS")]
public class SmsNotificationService : NotificationServiceBase, IEnhancedEnumOptionAlt<NotificationServiceBase>
{
    public override string Name => "SMS";
    public override int Id => 2;
    
    public override Task SendAsync(string message)
    {
        Console.WriteLine($"Sending SMS: {message}");
        return Task.CompletedTask;
    }
}

// Implementation 3 - Push (could be in separate package)
[EnumOption(Name = "Push")]
public class PushNotificationService : NotificationServiceBase, IEnhancedEnumOptionAlt<NotificationServiceBase>
{
    public override string Name => "Push";
    public override int Id => 3;
    
    public override Task SendAsync(string message)
    {
        Console.WriteLine($"Sending push notification: {message}");
        return Task.CompletedTask;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Interface-Based Discovery");
        Console.WriteLine("=================================");
        
        // The generator should create NotificationServiceBaseCollection
        // Let's see if it works by trying to use it
        
        try
        {
            // This should be generated
            Console.WriteLine($"All services: {NotificationServiceBaseCollection.All.Count}");
            
            foreach (var service in NotificationServiceBaseCollection.All)
            {
                Console.WriteLine($"- {service.Name} (ID: {service.Id})");
            }
            
            // Test GetByName
            var emailService = NotificationServiceBaseCollection.GetByName("Email");
            if (emailService != null)
            {
                await emailService.SendAsync("Test message via Email");
            }
            
            // Test GetById
            var smsService = NotificationServiceBaseCollection.GetById(2);
            if (smsService != null)
            {
                await smsService.SendAsync("Test message via SMS");
            }
            
            // Test static properties
            await NotificationServiceBaseCollection.Push.SendAsync("Test via static property");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine("The collection might not have been generated.");
        }
    }
}