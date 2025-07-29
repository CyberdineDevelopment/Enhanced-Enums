using System;

namespace FractalDataWorks;

/// <summary>
/// Represents a result of an operation that can either succeed or fail.
/// </summary>
public interface IGenericResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Message on a successful result.</exception>
    string Message { get; }
}

/// <summary>
/// Represents a result of an operation that can either succeed with a value or fail with an error.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public interface IGenericResult<T> : IGenericResult
{
    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Value on a failed result.</exception>
    T Value { get; }

    /// <summary>
    /// Maps the value of a successful result to a new type.
    /// </summary>
    /// <typeparam name="TNew">The new type to map to.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new result with the mapped value or the original error.</returns>
    IGenericResult<TNew> Map<TNew>(Func<T, TNew> mapper);

    /// <summary>
    /// Executes one of two functions depending on the result state.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="success">Function to execute if successful.</param>
    /// <param name="failure">Function to execute if failed.</param>
    /// <returns>The result of the executed function.</returns>
    TResult Match<TResult>(Func<T, TResult> success, Func<string, TResult> failure);
}