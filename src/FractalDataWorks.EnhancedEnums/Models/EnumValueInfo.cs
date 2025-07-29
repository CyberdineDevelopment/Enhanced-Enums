using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FractalDataWorks.SmartGenerators;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Models;

/// <summary>
/// Contains metadata about an enum value.
/// </summary>
public sealed class EnumValueInfo : IInputInfo, IEquatable<EnumValueInfo>
{
    private string _inputHash = string.Empty;
    private readonly Dictionary<string, string> _properties = new Dictionary<string, string>(StringComparer.Ordinal);
    private readonly HashSet<string> _categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the fully qualified type name for this enum value.
    /// </summary>
    public string FullTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the short type name (without namespace) for this enum value.
    /// </summary>
    public string ShortTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for this enum value.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this enum value should be included in the collection.
    /// </summary>
    public bool Include { get; set; } = true;

    /// <summary>
    /// Gets or sets the ordering of this enum value within the collection.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets an optional description for this enum value.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets a dictionary of additional properties for this enum value.
    /// </summary>
    public Dictionary<string, string> Properties => _properties;

    /// <summary>
    /// Gets a set of categories associated with this enum value.
    /// </summary>
    public HashSet<string> Categories => _categories;

    // Note: ISymbol removed per Roslyn cookbook - symbols are never equatable
    // Extract needed information to other properties instead

    /// <summary>
    /// Gets a hash string representing the contents of this enum value info for incremental generation.
    /// </summary>
    public string InputHash
    {
        get
        {
            if (string.IsNullOrEmpty(_inputHash))
            {
                _inputHash = InputTracker.CalculateInputHash(this);
            }

            return _inputHash;
        }
    }

    /// <summary>
    /// Writes the contents of this enum value info to a TextWriter for hash generation.
    /// </summary>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the writer is null.</exception>
    public void WriteToHash(TextWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer), "The TextWriter cannot be null.");
        }

        writer.Write(FullTypeName);
        writer.Write(ShortTypeName);
        writer.Write(Name);
        writer.Write(Include);
        writer.Write(Order);
        writer.Write(Description ?? string.Empty);

        foreach (var kv in Properties.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            writer.Write(kv.Key);
            writer.Write(kv.Value);
        }

        foreach (var cat in Categories.OrderBy(c => c, StringComparer.Ordinal))
        {
            writer.Write(cat);
        }
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as EnumValueInfo);

    /// <summary>
    /// Determines whether the specified EnumValueInfo is equal to the current EnumValueInfo.
    /// </summary>
    /// <param name="other">The EnumValueInfo to compare with the current EnumValueInfo.</param>
    /// <returns>true if the specified EnumValueInfo is equal to the current EnumValueInfo; otherwise, false.</returns>
    public bool Equals(EnumValueInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        bool basic = string.Equals(FullTypeName, other.FullTypeName, StringComparison.Ordinal) &&
string.Equals(ShortTypeName, other.ShortTypeName, StringComparison.Ordinal) &&
string.Equals(Name, other.Name, StringComparison.Ordinal) &&
                     Include == other.Include &&
                     Order == other.Order &&
string.Equals(Description, other.Description, StringComparison.Ordinal);
        if (!basic)
        {
            return false;
        }

        if (Properties.Count != other.Properties.Count)
        {
            return false;
        }

        foreach (var kv in Properties)
        {
            if (!other.Properties.TryGetValue(kv.Key, out var val) || !string.Equals(kv.Value, val, StringComparison.Ordinal))
            {
                return false;
            }
        }

        if (Categories.Count != other.Categories.Count)
        {
            return false;
        }

        return Categories.All(cat => other.Categories.Contains(cat));
    }

    /// <summary>
    /// Returns a hash code for this enum value info.
    /// </summary>
    /// <returns>A hash code for the current enum value info.</returns>
    public override int GetHashCode()
    {
        // Use the InputHash for consistent hash code
        return StringComparer.Ordinal.GetHashCode(InputHash);
    }
}
