using System.IO;

namespace FractalDataWorks.SmartGenerators;

/// <summary>
/// Interface for models that support input change tracking for incremental generation.
/// </summary>
public interface IInputInfo
{
    /// <summary>
    /// Gets a hash representing the current state of the model.
    /// </summary>
    string InputHash { get; }

    /// <summary>
    /// Writes the model state to a TextWriter for hash calculation.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    void WriteToHash(TextWriter writer);
}
