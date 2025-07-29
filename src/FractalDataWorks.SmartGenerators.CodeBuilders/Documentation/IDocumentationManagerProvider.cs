namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

/// <summary>
/// Interface for code elements that provide a documentation manager.
/// </summary>
public interface IDocumentationManagerProvider
{
    /// <summary>
    /// Gets the documentation manager for this element.
    /// </summary>
    DocumentationManager? DocumentationManager { get; }
}
