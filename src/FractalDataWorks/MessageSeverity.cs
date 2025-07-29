namespace FractalDataWorks;

/// <summary>
/// Defines the severity levels for messages.
/// </summary>
public enum MessageSeverity
{
    /// <summary>
    /// Trace level - most detailed information.
    /// </summary>
    Trace = 0,
    
    /// <summary>
    /// Debug level - debugging information.
    /// </summary>
    Debug = 1,
    
    /// <summary>
    /// Information level - general informational messages.
    /// </summary>
    Information = 2,
    
    /// <summary>
    /// Warning level - potentially harmful situations.
    /// </summary>
    Warning = 3,
    
    /// <summary>
    /// Message level - error events that might still allow the application to continue.
    /// </summary>
    Error = 4,
    
    /// <summary>
    /// Critical level - very severe error events that will presumably lead the application to abort.
    /// </summary>
    Critical = 5
}

