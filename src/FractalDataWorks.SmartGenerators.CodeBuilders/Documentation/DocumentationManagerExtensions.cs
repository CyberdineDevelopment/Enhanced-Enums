namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

/// <summary>
/// Extension methods for working with DocumentationManager in code builders.
/// </summary>
public static class DocumentationManagerExtensions
{
    /// <summary>
    /// Gets the DocumentationManager from a code builder.
    /// </summary>
    /// <typeparam name="T">The type of the code builder.</typeparam>
    /// <param name="builder">The code builder.</param>
    /// <returns>The documentation manager or null if not set.</returns>
    public static DocumentationManager? GetDocumentationManager<T>(this T builder)
        where T : ICodeBuilder
    {
        if (builder as IDocumentationManagerProvider is { } provider)
        {
            return provider.DocumentationManager;
        }

        return null;
    }
}
