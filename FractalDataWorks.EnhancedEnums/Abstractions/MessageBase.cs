using System;

namespace FractalDataWorks.EnhancedEnums.Abstractions;

/// &lt;summary&gt;
/// Base implementation for all FDW messages using Enhanced Enums pattern.
/// &lt;/summary&gt;
public abstract class MessageBase : IFdwMessage
{
    /// &lt;summary&gt;
    /// Initializes a new instance of the &lt;see cref="MessageBase"/&gt; class.
    /// &lt;/summary&gt;
    /// &lt;param name="code"&gt;The unique code for this message.&lt;/param&gt;
    /// &lt;param name="message"&gt;The message template text.&lt;/param&gt;
    /// &lt;param name="severity"&gt;The severity level of this message.&lt;/param&gt;
    protected MessageBase(string code, string message, MessageSeverity severity = MessageSeverity.Information)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Severity = severity;
    }

    /// &lt;inheritdoc/&gt;
    public string Code { get; }

    /// &lt;inheritdoc/&gt;
    public string Message { get; }

    /// &lt;inheritdoc/&gt;
    public MessageSeverity Severity { get; private set; }

    /// &lt;inheritdoc/&gt;
    public virtual string Format(params object[] args) 
        =&gt; args?.Length &gt; 0 ? string.Format(Message, args) : Message;

    /// &lt;inheritdoc/&gt;
    public virtual IFdwMessage WithSeverity(MessageSeverity severity)
    {
        var clone = (MessageBase)MemberwiseClone();
        clone.Severity = severity;
        return clone;
    }

    /// &lt;summary&gt;
    /// Returns a string representation of this message.
    /// &lt;/summary&gt;
    /// &lt;returns&gt;A string containing the code and message.&lt;/returns&gt;
    public override string ToString() =&gt; $"[{Code}] {Message}";
}
