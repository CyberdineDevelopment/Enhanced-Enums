using System;
using Services.Notification;

namespace NotificationConsole;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Notification Services Demo");
        Console.WriteLine("==========================");
        Console.WriteLine();
        
        // Show all available notification services discovered across assemblies
        Console.WriteLine("Available notification services:");
        foreach (var service in NotificationServiceBases.All)
        {
            Console.WriteLine($"- {service.Name} (ID: {service.Id})");
        }
        Console.WriteLine();
        
        // Get specific services by name
        var sms = NotificationServiceBases.GetByName("SMS");
        var email = NotificationServiceBases.GetByName("Email");
        
        if (sms != null && email != null)
        {
            var recipient = "user@example.com";
            var message = "Hello from Enhanced Enums!";
            
            Console.WriteLine($"Sending via SMS: {sms.Send(recipient, message)}");
            Console.WriteLine($"Sending via Email: {email.Send(recipient, message)}");
        }
        else
        {
            Console.WriteLine("Error: Could not find SMS or Email services.");
        }
        
        Console.WriteLine();
        Console.WriteLine("Demonstrating polymorphic usage:");
        foreach (var service in NotificationServiceBases.All)
        {
            Console.WriteLine($"  {service.Name}: {service.Send("test@example.com", "Test message")}");
        }
    }
}