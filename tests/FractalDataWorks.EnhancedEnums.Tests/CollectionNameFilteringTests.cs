using System.Linq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests;

public class CollectionNameFilteringTests : EnhancedEnumOptionTestBase
{

    [Fact]
    public void GeneratorFiltersEnumOptionsByCollectionName()
    {
        var source = @"
using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace;

[EnhancedEnumBase(""Messages"")]
public abstract class MessageBase { }

[EnhancedEnumBase(""Errors"")]
public abstract class ErrorBase { }

[EnumOption(""Messages"")]
public class InfoMessage : MessageBase { }

[EnumOption(""Errors"")]
public class ErrorMessage : ErrorBase { }

[EnumOption(""Messages"")]
public class WarningMessage : MessageBase { }
";

        var result = RunGenerator(new[] { source });
        
        result.GeneratedSources.ShouldNotBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
        
        var output = string.Join("\n", result.GeneratedSources.Select(s => s.SourceText));
        
        // Write output for debugging
        WriteGeneratedCodeToFile("CollectionNameFilteringTest_FiltersEnumOptions.cs", output);
        
        // Check Messages collection only has InfoMessage and WarningMessage
        output.ShouldContain("public static TestNamespace.MessageBase InfoMessage =>");
        output.ShouldContain("public static TestNamespace.MessageBase WarningMessage =>");
        // ErrorMessage should only be in Errors collection, not Messages
        var messagesStart = output.IndexOf("public static class Messages");
        var errorsStart = output.IndexOf("public static class Errors");
        errorsStart.ShouldNotBe(-1);
        var messagesSection = output.Substring(messagesStart, errorsStart - messagesStart);
        messagesSection.ShouldNotContain("ErrorMessage");
        
        // Check that we have the Messages collection
        output.ShouldContain("public static class Messages");
    }

    [Fact]
    public void GeneratorHandlesMultipleCollectionsInSameFile()
    {
        var source = @"
using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace;

[EnhancedEnumBase(""ServiceMessages"")]
public abstract class ServiceMessageBase { }

[EnhancedEnumBase(""ConfigurationMessages"")]
public abstract class ConfigurationMessageBase { }

[EnumOption(""ServiceMessages"")]
public class ServiceStarted : ServiceMessageBase { }

[EnumOption(""ConfigurationMessages"")]
public class ConfigurationLoaded : ConfigurationMessageBase { }

[EnumOption(""ServiceMessages"")]
public class ServiceStopped : ServiceMessageBase { }

[EnumOption(""ConfigurationMessages"")]
public class ConfigurationError : ConfigurationMessageBase { }
";

        var result = RunGenerator(new[] { source });
        
        result.GeneratedSources.ShouldNotBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
        
        var output = string.Join("\n", result.GeneratedSources.Select(s => s.SourceText));
        
        // Check ServiceMessages collection
        output.ShouldContain("public static class ServiceMessages");
        output.ShouldContain("ServiceStarted =>");
        output.ShouldContain("ServiceStopped =>");
        
        // ServiceMessages should not contain Configuration options
        var serviceMessagesStart = output.IndexOf("public static class ServiceMessages");
        serviceMessagesStart.ShouldNotBe(-1);
        var configMessagesStart = output.IndexOf("public static class ConfigurationMessages");
        configMessagesStart.ShouldNotBe(-1);
        
        // Get the section for ServiceMessages (between ServiceMessages and ConfigurationMessages)
        string serviceMessagesSection;
        if (serviceMessagesStart < configMessagesStart)
        {
            serviceMessagesSection = output.Substring(serviceMessagesStart, configMessagesStart - serviceMessagesStart);
        }
        else
        {
            // ConfigurationMessages comes first
            serviceMessagesSection = output.Substring(serviceMessagesStart);
        }
        
        serviceMessagesSection.ShouldNotContain("ConfigurationLoaded");
        serviceMessagesSection.ShouldNotContain("ConfigurationError");
    }

    [Fact]
    public void GeneratorIgnoresEnumOptionsWithoutMatchingCollection()
    {
        var source = @"
using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace;

[EnhancedEnumBase(""ActiveMessages"")]
public abstract class MessageBase { }

[EnumOption(""InactiveMessages"")]
public class OrphanMessage : MessageBase { }

[EnumOption(""ActiveMessages"")]
public class ValidMessage : MessageBase { }
";

        var result = RunGenerator(new[] { source });
        
        result.GeneratedSources.ShouldNotBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
        
        var output = string.Join("\n", result.GeneratedSources.Select(s => s.SourceText));
        
        // Should only contain ValidMessage
        output.ShouldContain("ValidMessage =>");
        output.ShouldNotContain("OrphanMessage");
    }

    [Fact]
    public void GeneratorHandlesNullCollectionName()
    {
        var source = @"
using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace;

[EnhancedEnumBase(""DefaultCollection"")]
public abstract class MessageBase { }

[EnumOption()]
public class DefaultMessage : MessageBase { }

[EnumOption(""DefaultCollection"")]
public class ExplicitMessage : MessageBase { }
";

        var result = RunGenerator(new[] { source });
        
        result.GeneratedSources.ShouldNotBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
        
        var output = string.Join("\n", result.GeneratedSources.Select(s => s.SourceText));
        
        // When only one collection exists, null collection name should match it
        output.ShouldContain("DefaultMessage =>");
        output.ShouldContain("ExplicitMessage =>");
    }

    [Fact]
    public void GeneratorRespectsOrderProperty()
    {
        var source = @"
using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace;

[EnhancedEnumBase(""OrderedMessages"")]
public abstract class MessageBase { }

[EnumOption(""OrderedMessages"", order: 3)]
public class ThirdMessage : MessageBase { }

[EnumOption(""OrderedMessages"", order: 1)]
public class FirstMessage : MessageBase { }

[EnumOption(""OrderedMessages"", order: 2)]
public class SecondMessage : MessageBase { }
";

        var result = RunGenerator(new[] { source });
        
        result.GeneratedSources.ShouldNotBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
        
        var output = string.Join("\n", result.GeneratedSources.Select(s => s.SourceText));
        
        // Check order in _all list initialization
        var firstIdx = output.IndexOf("new TestNamespace.FirstMessage()");
        var secondIdx = output.IndexOf("new TestNamespace.SecondMessage()");
        var thirdIdx = output.IndexOf("new TestNamespace.ThirdMessage()");
        
        // In the _all list initialization, they should appear in order
        firstIdx.ShouldBeLessThan(secondIdx);
        secondIdx.ShouldBeLessThan(thirdIdx);
    }

    [Fact]
    public void GeneratorHandlesConstructorParameters()
    {
        var source = @"
using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace;

[EnhancedEnumBase(""TestMessages"")]
public abstract class MessageBase { }

[EnumOption(""TestMessages"", ""Custom Name"", 5)]
public class CustomMessage : MessageBase { }

[EnumOption(""TestMessages"", ""Another Name"", 1, ""string"", ""System"")]
public class TypedMessage : MessageBase { }
";

        var result = RunGenerator(new[] { source });
        
        result.GeneratedSources.ShouldNotBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
        
        var output = string.Join("\n", result.GeneratedSources.Select(s => s.SourceText));
        
        // Write output for debugging
        WriteGeneratedCodeToFile("CollectionNameFilteringTest_ConstructorParameters.cs", output);
        
        output.ShouldContain("public static class TestMessages");
        // The properties use the custom names from the constructor
        output.ShouldContain("public static TestNamespace.MessageBase AnotherName =>");
        output.ShouldContain("public static TestNamespace.MessageBase CustomName =>");
        
        // Verify TypedMessage appears before CustomMessage due to order
        var typedIdx = output.IndexOf("new TestNamespace.TypedMessage()");
        var customIdx = output.IndexOf("new TestNamespace.CustomMessage()");
        typedIdx.ShouldBeLessThan(customIdx);
    }

    [Fact]
    public void GeneratorHandlesBackwardCompatibility()
    {
        var source = @"
using FractalDataWorks.EnhancedEnums.Attributes;

namespace TestNamespace;

[EnhancedEnumBase(""BackCompatMessages"")]
public abstract class MessageBase { }

[EnumOption(CollectionName = ""BackCompatMessages"", Name = ""Legacy"", Order = 2)]
public class LegacyMessage : MessageBase { }

[EnumOption(""BackCompatMessages"", ""Modern"", 1)]
public class ModernMessage : MessageBase { }
";

        var result = RunGenerator(new[] { source });
        
        result.GeneratedSources.ShouldNotBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
        
        var output = string.Join("\n", result.GeneratedSources.Select(s => s.SourceText));
        
        // Write output for debugging
        WriteGeneratedCodeToFile("CollectionNameFilteringTest_BackwardCompatibility.cs", output);
        
        // Both styles should work
        output.ShouldContain("public static class BackCompatMessages");
        // The properties use the custom names
        output.ShouldContain("public static TestNamespace.MessageBase Legacy =>");
        output.ShouldContain("public static TestNamespace.MessageBase Modern =>");
        
        // Modern should come first due to order
        var modernIdx = output.IndexOf("new TestNamespace.ModernMessage()");
        var legacyIdx = output.IndexOf("new TestNamespace.LegacyMessage()");
        modernIdx.ShouldBeLessThan(legacyIdx);
    }
}