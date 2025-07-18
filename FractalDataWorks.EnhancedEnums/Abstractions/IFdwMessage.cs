using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.Abstractions;

/// &lt;summary&gt;
/// Contract for all FDW messages - enables polymorphism and testing.
/// &lt;/summary&gt;
public interface IFdwMessage
{
    /// &lt;summary&gt;
    /// Gets the unique code for this message.
    /// &lt;/summary&gt;
    string Code { get; }
    
    /// &lt;summary&gt;
    /// Gets the message template text.
    /// &lt;/summary&gt;
    string Message { get; }
    
    /// &lt;summary&gt;
    /// Gets the severity level of this message.
    /// &lt;/summary&gt;
    MessageSeverity Severity { get; }
    
    /// &lt;summary&gt;
    /// Formats the message with the provided parameters.
    /// &lt;/summary&gt;
    /// &lt;param name="args"&gt;The parameters to format the message with.&lt;/param&gt;
    /// &lt;returns&gt;The formatted message.&lt;/returns&gt;
    string Format(params object[] args);
    
    /// &lt;summary&gt;
    /// Creates a copy of this message with a different severity level.
    /// &lt;/summary&gt;
    /// &lt;param name="severity"&gt;The new severity level.&lt;/param&gt;
    /// &lt;returns&gt;A new instance with the updated severity.&lt;/returns&gt;
    IFdwMessage WithSeverity(MessageSeverity severity);
}
