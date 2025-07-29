using System;
using System.Runtime.Serialization;

namespace FractalDataWorks.SmartGenerators.TestUtilities;

/// <summary>
/// Exception thrown when expectations are not met in test assertions.
/// </summary>
[Serializable]
public class ExpectationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectationException"/> class.
    /// </summary>
    public ExpectationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ExpectationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectationException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ExpectationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectationException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
    protected ExpectationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
