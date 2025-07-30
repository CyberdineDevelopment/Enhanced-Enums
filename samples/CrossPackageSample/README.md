# Cross-Package Enhanced Enum Sample

This sample demonstrates how Enhanced Enums can discover enum options across multiple assemblies using the Service Type Pattern.

## Project Structure

- **Services.Notification** - Base project containing `NotificationServiceBase` with `[EnumCollection]` attribute
- **Services.Notification.Sms** - SMS implementation with `[EnumOption]` attribute
- **Services.Notification.Email** - Email implementation with `[EnumOption]` attribute  
- **NotificationConsole** - Console app that references EnhancedEnums and uses the services

## Key Points

1. **Only the console app needs EnhancedEnums** - The service projects only need the FractalDataWorks attributes
2. **Opt-in discovery** - Service projects must include `<IncludeInEnhancedEnumAssemblies>true</IncludeInEnhancedEnumAssemblies>` in their .csproj
3. **Compile-time generation** - The `NotificationServiceBases` collection is generated at compile time, not runtime

## How It Works

When you build the console app:
1. EnhancedEnums generator finds `NotificationServiceBase` with `[EnumCollection]`
2. It scans referenced assemblies that have opted in
3. It discovers `SmsNotificationService` and `EmailNotificationService` with `[EnumOption]`
4. It generates a `NotificationServiceBases` static class with all discovered services

## Running the Sample

```bash
cd samples/CrossPackageSample/NotificationConsole
dotnet run
```

The sample currently simulates what would happen - it creates the services manually to show the expected output.