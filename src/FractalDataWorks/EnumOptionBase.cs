using System;

namespace FractalDataWorks;

/// <summary>
/// Base class for enhanced enum types that enforces Id and Name properties through constructor initialization.
/// This base class uses auto properties instead of abstract properties for cleaner code.
/// </summary>
/// <typeparam name="T">The derived enum type.</typeparam>
public abstract class EnumOptionBase<T> : IEnumOption where T : EnumOptionBase<T>
{
    /// <summary>
    /// Gets the unique identifier for this enum value.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this enum value.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOptionBase{T}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this enum value.</param>
    /// <param name="name">The name of this enum value.</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    protected EnumOptionBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

}