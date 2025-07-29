using FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Defines the interface for a member builder.
/// </summary>
/// <typeparam name="T">The type of builder for fluent API chaining.</typeparam>
public interface IMemberBuilder<out T> : IBuildable
{
    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Sets the access modifier for this member.
    /// </summary>
    /// <param name="accessModifier">The access modifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    T WithAccessModifier(AccessModifier accessModifier);

    /// <summary>
    /// Adds a modifier to this member.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    T WithModifier(Modifiers modifier);

    /// <summary>
    /// Adds XML documentation summary for the member.
    /// </summary>
    /// <param name="summary">The documentation summary.</param>
    /// <returns>The builder instance for chaining.</returns>
    T WithXmlDocSummary(string summary);
}
