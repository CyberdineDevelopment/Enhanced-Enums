using System;
using System.IO;
using FractalDataWorks.SmartGenerators;



namespace FractalDataWorks.EnhancedEnums.Models;

/// <summary>
/// Contains information about a property that should be used for lookups in the enhanced enum collection.
/// </summary>
public sealed class PropertyLookupInfo : IInputInfo, IEquatable<PropertyLookupInfo>
{
    private string _inputHash = string.Empty;

    /// <summary>
    /// Gets or sets the name of the property to create lookup methods for.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the property as a string.
    /// </summary>
    public string PropertyType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the generated lookup method.
    /// </summary>
    public string LookupMethodName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to generate a TryGet method in addition to the direct lookup method.
    /// </summary>
    public bool GenerateTryGet { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to allow multiple matches for this property.
    /// </summary>
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the default lookup property.
    /// </summary>
    public bool IsDefaultProperty { get; set; }

    /// <summary>
    /// Gets or sets the string comparison mode to use for string property lookups.
    /// </summary>
    public StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets or sets the name of a custom comparer to use for property comparisons.
    /// </summary>
    public string? Comparer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property is nullable.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the return type for this specific lookup method.
    /// If null, inherits from the EnumTypeInfo.ReturnType.
    /// </summary>
    public string? ReturnType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property is abstract or virtual and needs to be overridden in the Empty class.
    /// </summary>
    public bool RequiresOverride { get; set; }

    /// <summary>
    /// Gets a hash string representing the contents of this property lookup info for incremental generation.
    /// </summary>
    public string InputHash
    {
        get
        {
            if (!string.IsNullOrEmpty(_inputHash))
            {
                return _inputHash;
            }

            _inputHash = InputTracker.CalculateInputHash(this);
            return _inputHash;
        }
    }

    /// <summary>
    /// Gets a string representation of the default value for this property type.
    /// </summary>
    /// <returns>A string that represents the default value for the property type.</returns>
#pragma warning disable CA1024
    public string GetDefaultValueString()
#pragma warning restore CA1024
    {
        if (IsNullable)
        {
            return "null";
        }

        return PropertyType switch
        {
            "string" => "string.Empty",
            "int" => "0",
            "long" => "0L",
            "double" => "0.0",
            "decimal" => "0m",
            "bool" => "false",
            "Guid" => "Guid.Empty",
            _ => "default",
        };
    }

    /// <summary>
    /// Writes the contents of this property lookup info to a TextWriter for hash generation.
    /// </summary>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the writer is null.</exception>
    public void WriteToHash(TextWriter writer)
    {
        if (writer != null)
        {
            writer.Write(PropertyName);
            writer.Write(PropertyType);
            writer.Write(LookupMethodName);
            writer.Write(GenerateTryGet);
            writer.Write(AllowMultiple);
            writer.Write(IsDefaultProperty);
            writer.Write((int)StringComparison);
            writer.Write(Comparer ?? string.Empty);
            writer.Write(IsNullable);
            writer.Write(ReturnType ?? string.Empty);
            writer.Write(RequiresOverride);
        }
        else
        {
            throw new ArgumentNullException(nameof(writer), "The TextWriter cannot be null.");
        }
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as PropertyLookupInfo);

    /// <summary>
    /// Determines whether the specified PropertyLookupInfo is equal to the current PropertyLookupInfo.
    /// </summary>
    /// <param name="other">The PropertyLookupInfo to compare with the current PropertyLookupInfo.</param>
    /// <returns>true if the specified PropertyLookupInfo is equal to the current PropertyLookupInfo; otherwise, false.</returns>
    public bool Equals(PropertyLookupInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(PropertyName, other.PropertyName, StringComparison.Ordinal) &&
string.Equals(PropertyType, other.PropertyType, StringComparison.Ordinal) &&
string.Equals(LookupMethodName, other.LookupMethodName, StringComparison.Ordinal) &&
               GenerateTryGet == other.GenerateTryGet &&
               AllowMultiple == other.AllowMultiple &&
               IsDefaultProperty == other.IsDefaultProperty &&
               StringComparison == other.StringComparison &&
string.Equals(Comparer, other.Comparer, StringComparison.Ordinal) &&
               IsNullable == other.IsNullable &&
string.Equals(ReturnType, other.ReturnType, StringComparison.Ordinal) &&
               RequiresOverride == other.RequiresOverride;
    }

    /// <summary>
    /// Returns a hash code for this property lookup info.
    /// </summary>
    /// <returns>A hash code for the current property lookup info.</returns>
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(InputHash);
}
