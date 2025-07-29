using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FractalDataWorks.SmartGenerators;

/// <summary>
/// Provides utilities for tracking input changes and calculating hashes for incremental generation.
/// </summary>
public static class InputTracker
{
    /// <summary>
    /// Calculates a hash for the input information to determine if code needs to be regenerated.
    /// </summary>
    /// <typeparam name="T">The type of input information.</typeparam>
    /// <param name="info">The input information.</param>
    /// <returns>A string representation of the hash.</returns>
    public static string CalculateInputHash<T>(T info)
        where T : IInputInfo
    {
        if (Equals(info, default(T)))
        {
            throw new ArgumentNullException(nameof(info));
        }

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        // Write model-specific data through the interface
        info.WriteToHash(writer);

        writer.Flush();
        stream.Position = 0;

        // Compute hash
        using var hashAlgorithm = SHA256.Create();
        var hash = hashAlgorithm.ComputeHash(stream);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Determines if two input objects have different hashes.
    /// </summary>
    /// <typeparam name="T">The type of input information.</typeparam>
    /// <param name="oldInfo">The previous input information.</param>
    /// <param name="newInfo">The new input information.</param>
    /// <returns><see langword="true"/> if the inputs have different hashes; otherwise, <c>false</c>.</returns>
    public static bool TrackedInputHasChanged<T>(T oldInfo, T newInfo)
        where T : IInputInfo
    {
        if (Equals(oldInfo, default(T)))
        {
            throw new ArgumentNullException(nameof(oldInfo));
        }

        if (Equals(newInfo, default(T)))
        {
            throw new ArgumentNullException(nameof(newInfo));
        }

        // If we already have hash values cached, just compare them
        var oldHash = oldInfo.InputHash;
        var newHash = newInfo.InputHash;

        return HasChanged(oldHash, newHash);
    }

    /// <summary>
    /// Determines if two input hashes are different.
    /// </summary>
    /// <param name="oldHash">The previous hash.</param>
    /// <param name="newHash">The new hash.</param>
    /// <returns><see langword="true"/> if the hashes are different; otherwise, <c>false</c>.</returns>
    public static bool HasChanged(string? oldHash, string newHash) => string.IsNullOrEmpty(oldHash) || !string.Equals(oldHash, newHash, StringComparison.Ordinal);
}
