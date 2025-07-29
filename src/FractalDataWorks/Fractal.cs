using System;

namespace FractalDataWorks;

/// <summary>
/// Represents a unit type for operations that don't return a value.
/// </summary>
public struct Fractal : IEquatable<Fractal>
{
    /// <summary>
    /// Gets the default fractal value.
    /// </summary>
    public static readonly Fractal Value;

    /// <summary>
    /// Determines whether this instance is equal to another Fractal.
    /// </summary>
    /// <param name="other">The other Fractal instance to compare.</param>
    /// <returns>Always returns true as all Fractal instances are equal.</returns>
    public bool Equals(Fractal other) => true;

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>true if obj is a Fractal; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Fractal;

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Always returns 0 as all Fractal instances are equal.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns a string representation of this instance.
    /// </summary>
    /// <returns>A string representation of the Fractal unit type.</returns>
    public override string ToString() => "()";

    /// <summary>
    /// Determines whether two Fractal instances are equal.
    /// </summary>
    /// <param name="left">The first Fractal instance.</param>
    /// <param name="right">The second Fractal instance.</param>
    /// <returns>Always returns true as all Fractal instances are equal.</returns>
    public static bool operator ==(Fractal left, Fractal right) => true;

    /// <summary>
    /// Determines whether two Fractal instances are not equal.
    /// </summary>
    /// <param name="left">The first Fractal instance.</param>
    /// <param name="right">The second Fractal instance.</param>
    /// <returns>Always returns false as all Fractal instances are equal.</returns>
    public static bool operator !=(Fractal left, Fractal right) => false;
}