namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Defines an interface for builders that can generate code via Build().
/// </summary>
public interface IBuildable
{
    /// <summary>
    /// Builds and returns the generated code as a string.
    /// </summary>
    /// <returns>The generated code.</returns>
    string Build();
}
