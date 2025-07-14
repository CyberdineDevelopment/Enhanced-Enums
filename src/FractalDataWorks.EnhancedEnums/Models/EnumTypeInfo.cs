using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FractalDataWorks.SmartGenerators;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Models;

/// <summary>
/// Contains metadata about an enum type definition to be processed by the generator.
/// </summary>
public sealed class EnumTypeInfo : IInputInfo, IEquatable<EnumTypeInfo>
{
    // Note: ISymbol removed per Roslyn cookbook - symbols are never equatable
    // All needed information is extracted to other properties

    /// <summary>
    /// Gets or sets the namespace for the generated code.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class name of the enum type.
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fully qualified type name of the enum base type.
    /// </summary>
    public string FullTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the enum type is generic.
    /// </summary>
    public bool IsGenericType { get; set; }

    /// <summary>
    /// Gets or sets the name of the generated collection class.
    /// </summary>
    public string CollectionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether factory-based instance creation should be used.
    /// </summary>
    public bool UseFactory { get; set; }

    /// <summary>
    /// Gets or sets the generation strategy name to use.
    /// </summary>
    public string Strategy { get; set; } = "Default";

    /// <summary>
    /// Gets or sets the string comparison mode for name-based lookups.
    /// </summary>
    public StringComparison NameComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets or sets a value indicating whether to include enum options from referenced assemblies.
    /// </summary>
    public bool IncludeReferencedAssemblies { get; set; }

    /// <summary>
    /// Gets the list of properties that should be used for lookup methods.
    /// </summary>
    public EquatableArray<PropertyLookupInfo> LookupProperties { get; set; } = EquatableArray<PropertyLookupInfo>.Empty;

    /// <summary>
    /// Gets the list of concrete enum value types discovered during processing.
    /// </summary>
    public EquatableArray<EnumValueInfo> ConcreteTypes { get; set; } = EquatableArray<EnumValueInfo>.Empty;

    /// <summary>
    /// Gets the ID value for a specific enum value.
    /// </summary>
    /// <param name="value">The enum value to get the ID for.</param>
    /// <returns>The ID assigned to the enum value.</returns>
    public int GetIdFor(EnumValueInfo value)
    {
        for (int i = 0; i < ConcreteTypes.Length; i++)
        {
            if (ConcreteTypes[i].Equals(value))
            {
                return i + 1;
            }
        }
        return -1;
    }

    private string _inputHash = string.Empty;

    /// <summary>
    /// Gets a hash string representing the contents of this enum type info for incremental generation.
    /// </summary>
    public string InputHash
    {
        get
        {
            if (!string.IsNullOrEmpty(_inputHash)) return _inputHash;
            _inputHash = InputTracker.CalculateInputHash(this);
            return _inputHash;
        }
    }

    /// <summary>
    /// Writes the contents of this enum type info to a TextWriter for hash generation.
    /// </summary>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the writer is null.</exception>
    public void WriteToHash(TextWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer), "The TextWriter cannot be null.");
        }

        // Write basic properties
        writer.Write(Namespace);
        writer.Write(ClassName);
        writer.Write(FullTypeName);
        writer.Write(IsGenericType);
        writer.Write(CollectionName);
        writer.Write(UseFactory);
        writer.Write(Strategy);
        writer.Write(NameComparison.ToString());
        writer.Write(IncludeReferencedAssemblies);

        // Write lookup properties
        foreach (var lookup in LookupProperties.OrderBy(p => p.PropertyName, StringComparer.Ordinal))
        {
            writer.Write(lookup.PropertyName);
            writer.Write(lookup.PropertyType);
            writer.Write(lookup.StringComparison.ToString());
        }

        // Write concrete types
        foreach (var concreteType in ConcreteTypes.OrderBy(c => c.FullTypeName, StringComparer.Ordinal))
        {
            writer.Write(concreteType.InputHash);
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="EnumTypeInfo"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The EnumTypeInfo to compare with the current instance.</param>
    /// <returns>true if the specified EnumTypeInfo is equal to the current instance; otherwise, false.</returns>
    public bool Equals(EnumTypeInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // Compare by InputHash for efficient equality
        return InputHash == other.InputHash;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as EnumTypeInfo);

    /// <summary>
    /// Returns a hash code for this enum type info.
    /// </summary>
    /// <returns>A hash code for the current enum type info.</returns>
    public override int GetHashCode()
    {
        // Use the InputHash for consistent hash code
        return InputHash.GetHashCode();
    }
}
