using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Discovery;

/// <summary>
/// Interface for discovering types across assembly boundaries.
/// </summary>
public interface ICrossAssemblyTypeDiscoveryService
{
    /// <summary>
    /// Gets the list of assembly names that should be included in cross-assembly discovery.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>A collection of assembly names to include, or null/empty to include all referenced assemblies.</returns>
    IEnumerable<string> GetIncludedAssemblies(Compilation compilation);

    /// <summary>
    /// Finds all types in the compilation and referenced assemblies that derive from the specified base type.
    /// </summary>
    /// <param name="baseType">The base type to search for derived types.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>A collection of named type symbols that derive from the base type.</returns>
    IEnumerable<INamedTypeSymbol> FindDerivedTypes(INamedTypeSymbol baseType, Compilation compilation);

    /// <summary>
    /// Finds all types in the compilation and referenced assemblies that have the specified attribute.
    /// </summary>
    /// <param name="attributeType">The attribute type to search for.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>A collection of named type symbols that have the specified attribute.</returns>
    IEnumerable<INamedTypeSymbol> FindTypesWithAttribute(INamedTypeSymbol attributeType, Compilation compilation);

    /// <summary>
    /// Finds all types in the compilation and referenced assemblies that have an attribute with the specified name.
    /// </summary>
    /// <param name="attributeName">The name of the attribute to search for (e.g., "EnumOption" for [EnumOption]).</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>A collection of named type symbols that have the specified attribute.</returns>
    IEnumerable<INamedTypeSymbol> FindTypesWithAttributeName(string attributeName, Compilation compilation);
}