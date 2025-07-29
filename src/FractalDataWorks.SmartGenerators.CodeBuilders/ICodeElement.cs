namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Defines the interface for code elements that can be generated.
/// </summary>
public interface ICodeElement
{
    /// <summary>
    /// Generates code for this element.
    /// </summary>
    /// <param name="codeBuilder">The code builder to use.</param>
    void Generate(ICodeBuilder codeBuilder);
}
