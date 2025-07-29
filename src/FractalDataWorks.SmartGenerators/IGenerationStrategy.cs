using System.Text;

namespace FractalDataWorks.SmartGenerators;

/// <summary>
/// Interface for a code generation strategy.
/// </summary>
/// <typeparam name="TModel">The type of model to generate code for.</typeparam>
public interface IGenerationStrategy<in TModel>
{
    /// <summary>
    /// Generates code for the given model.
    /// </summary>
    /// <param name="model">The model to generate code for.</param>
    /// <param name="builder">The string builder to append to.</param>
    void Generate(TModel model, StringBuilder builder);
}
