using System;

namespace FractalDataWorks.SmartGenerators.TestUtilities.Expectations;

/// <summary>
/// Exception thrown when an expectation validation fails.
/// </summary>
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
}
