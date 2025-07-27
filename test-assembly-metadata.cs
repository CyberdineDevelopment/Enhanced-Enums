using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        // Check the SMS assembly
        var smsAssembly = Assembly.LoadFrom(@"C:\development\fractaldataworks\developer-kit\samples\services\ServiceRegistration\FractalDataWorks.Services.Notifications.SMS\bin\Debug\net10.0\FractalDataWorks.Services.Notifications.SMS.dll");
        Console.WriteLine("SMS Assembly: " + smsAssembly.FullName);
        
        var smsMetadata = smsAssembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        foreach (var attr in smsMetadata)
        {
            Console.WriteLine($"  Metadata: {attr.Key} = {attr.Value}");
        }
        
        // Check the Email assembly
        var emailAssembly = Assembly.LoadFrom(@"C:\development\fractaldataworks\developer-kit\samples\services\ServiceRegistration\FractalDataWorks.Services.Notifications.Email\bin\Debug\net10.0\FractalDataWorks.Services.Notifications.Email.dll");
        Console.WriteLine("\nEmail Assembly: " + emailAssembly.FullName);
        
        var emailMetadata = emailAssembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        foreach (var attr in emailMetadata)
        {
            Console.WriteLine($"  Metadata: {attr.Key} = {attr.Value}");
        }
    }
}