using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FractalDataWorks.EnhancedEnums.Models;

/// <summary>
/// An immutable, equatable array wrapper that provides value-based equality for use in incremental generators.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    /// <summary>
    /// Gets an empty array.
    /// </summary>
    public static EquatableArray<T> Empty { get; } = new(ImmutableArray<T>.Empty);

    private readonly ImmutableArray<T> _array;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquatableArray{T}"/> struct.
    /// </summary>
    /// <param name="array">The array to wrap.</param>
    public EquatableArray(ImmutableArray<T> array)
    {
        _array = array.IsDefault ? ImmutableArray<T>.Empty : array;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquatableArray{T}"/> struct.
    /// </summary>
    /// <param name="items">The items to create an array from.</param>
    public EquatableArray(IEnumerable<T> items)
        : this(items?.ToImmutableArray() ?? ImmutableArray<T>.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquatableArray{T}"/> struct.
    /// </summary>
    /// <param name="items">The items to create an array from.</param>
    public EquatableArray(params T[] items)
        : this(ImmutableArray.Create(items ?? Array.Empty<T>()))
    {
    }

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    public int Length => _array.Length;

    /// <summary>
    /// Gets a value indicating whether the array is empty.
    /// </summary>
    public bool IsEmpty => _array.IsEmpty;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The element at the specified index.</returns>
    public T this[int index] => _array[index];

    /// <summary>
    /// Returns an enumerator that iterates through the array.
    /// </summary>
    /// <returns>An enumerator for the array.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)_array).GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the array.
    /// </summary>
    /// <returns>An enumerator for the array.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Determines whether the specified <see cref="EquatableArray{T}"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The array to compare with the current instance.</param>
    /// <returns>true if the specified array is equal to the current instance; otherwise, false.</returns>
    public bool Equals(EquatableArray<T> other)
    {
        if (_array.Length != other._array.Length)
        {
            return false;
        }

        for (int i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> array && Equals(array);
    }

    /// <summary>
    /// Returns the hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var item in _array)
        {
            hash = hash * 31 + (item?.GetHashCode() ?? 0);
        }
        return hash;
    }

    /// <summary>
    /// Converts the array to an immutable array.
    /// </summary>
    /// <returns>An immutable array containing the same elements.</returns>
    public ImmutableArray<T> AsImmutableArray() => _array;

    /// <summary>
    /// Determines whether two specified arrays have the same value.
    /// </summary>
    /// <param name="left">The first array to compare.</param>
    /// <param name="right">The second array to compare.</param>
    /// <returns>true if the value of left is the same as the value of right; otherwise, false.</returns>
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two specified arrays have different values.
    /// </summary>
    /// <param name="left">The first array to compare.</param>
    /// <param name="right">The second array to compare.</param>
    /// <returns>true if the value of left is different from the value of right; otherwise, false.</returns>
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
    {
        return !left.Equals(right);
    }
}