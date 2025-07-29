namespace FractalDataWorks;

/// <summary>
/// Contract for all FDW messages - enables polymorphism and testing.
/// </summary>
public interface IFdwMessage
{
    /// <summary>
    /// Gets the unique code for this message.
    /// </summary>
    string Code { get; }
    
    /// <summary>
    /// Gets the message template text.
    /// </summary>
    string Message { get; }
    
    /// <summary>
    /// Gets the severity level of this message.
    /// </summary>
    MessageSeverity Severity { get; }
    
    /// <summary>
    /// Formats the message with the provided parameters.
    /// </summary>
    /// <param name="args">The parameters to format the message with.</param>
    /// <returns>The formatted message.</returns>
    string Format(params object[] args);
    
    /// <summary>
    /// Creates a copy of this message with a different severity level.
    /// </summary>
    /// <param name="severity">The new severity level.</param>
    /// <returns>A new instance with the updated severity.</returns>
    IFdwMessage WithSeverity(MessageSeverity severity);
}

