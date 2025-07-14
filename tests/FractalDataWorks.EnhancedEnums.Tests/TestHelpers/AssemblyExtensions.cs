using System.Reflection;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Tests.TestHelpers;

internal static class AssemblyExtensions
{
    public static MetadataReference ToMetadataReference(this Assembly assembly)
    {
        return MetadataReference.CreateFromFile(assembly.Location);
    }

    public static MetadataReference ToMetadataReference(this Compilation compilation)
    {
        using var stream = new System.IO.MemoryStream();
        var emitResult = compilation.Emit(stream);

        if (!emitResult.Success)
        {
            throw new System.InvalidOperationException(
                $"Compilation failed: {string.Join("\n", emitResult.Diagnostics)}");
        }

        stream.Seek(0, System.IO.SeekOrigin.Begin);
        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
