using System;
using System.Security.Cryptography;
using System.Text;

namespace FractalDataWorks.EnhancedEnums.Models;

/// <summary>
/// Represents parameter information for a constructor.
/// </summary>
public sealed class ParameterInfo : IEquatable<ParameterInfo>
{
    /// <summary>
    /// Gets or sets the type name of the parameter.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the default value string representation if any.
    /// </summary>
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// Gets or sets whether this parameter has a default value.
    /// </summary>
    public bool HasDefaultValue { get; set; }
    
    /// <summary>
    /// Gets or sets the namespace of the parameter type for imports.
    /// </summary>
    public string? Namespace { get; set; }
    
    /// <summary>
    /// Writes this parameter information to a hash for change detection.
    /// </summary>
    public void WriteToHash(SHA256 sha256)
    {
        var bytes = Encoding.UTF8.GetBytes($"Param:{TypeName}:{Name}:{HasDefaultValue}:{DefaultValue ?? "null"}:{Namespace ?? "null"}");
        sha256.TransformBlock(bytes, 0, bytes.Length, null, 0);
    }
    
    /// <inheritdoc/>
    public bool Equals(ParameterInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return string.Equals(TypeName, other.TypeName, StringComparison.Ordinal) &&
               string.Equals(Name, other.Name, StringComparison.Ordinal) &&
               string.Equals(DefaultValue, other.DefaultValue, StringComparison.Ordinal) &&
               HasDefaultValue == other.HasDefaultValue &&
               string.Equals(Namespace, other.Namespace, StringComparison.Ordinal);
    }
    
    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ParameterInfo);
    
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(TypeName, Name, DefaultValue, HasDefaultValue, Namespace);
    }
}