namespace FractalDataWorks.EnhancedEnums.Abstractions;

/// &lt;summary&gt;
/// Defines the severity levels for messages.
/// &lt;/summary&gt;
public enum MessageSeverity
{
    /// &lt;summary&gt;
    /// Trace level - most detailed information.
    /// &lt;/summary&gt;
    Trace = 0,
    
    /// &lt;summary&gt;
    /// Debug level - debugging information.
    /// &lt;/summary&gt;
    Debug = 1,
    
    /// &lt;summary&gt;
    /// Information level - general informational messages.
    /// &lt;/summary&gt;
    Information = 2,
    
    /// &lt;summary&gt;
    /// Warning level - potentially harmful situations.
    /// &lt;/summary&gt;
    Warning = 3,
    
    /// &lt;summary&gt;
    /// Error level - error events that might still allow the application to continue.
    /// &lt;/summary&gt;
    Error = 4,
    
    /// &lt;summary&gt;
    /// Critical level - very severe error events that will presumably lead the application to abort.
    /// &lt;/summary&gt;
    Critical = 5
}
