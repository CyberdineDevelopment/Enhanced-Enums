using System;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

/// <summary>
/// Provides extension methods for connecting code builders with XML documentation providers.
/// </summary>
public static class XmlDocumentationExtensions
{
    /// <summary>
    /// Creates and attaches a class documentation provider to a ClassBuilder.
    /// </summary>
    /// <param name="builder">The class builder.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ClassBuilder WithAutoDocumentation(this ClassBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var provider = new ClassDocumentationProvider(builder.Name);
        var manager = new DocumentationManager(provider);
        return builder.WithDocumentationManager(manager);
    }

    /// <summary>
    /// Creates and attaches a method documentation provider to a MethodBuilder.
    /// </summary>
    /// <param name="builder">The method builder.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static MethodBuilder WithAutoDocumentation(this MethodBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var provider = new MethodDocumentationProvider(builder.Name, builder.TypeName);
        var manager = new DocumentationManager(provider);
        return builder.WithDocumentationManager(manager);
    }

    /// <summary>
    /// Creates and attaches a property documentation provider to a PropertyBuilder.
    /// </summary>
    /// <param name="builder">The property builder.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static PropertyBuilder WithAutoDocumentation(this PropertyBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var provider = new PropertyDocumentationProvider(builder.Name, builder.TypeName);
        var manager = new DocumentationManager(provider);
        return builder.WithDocumentationManager(manager);
    }

    /// <summary>
    /// Creates and attaches a field documentation provider to a FieldBuilder.
    /// </summary>
    /// <param name="builder">The field builder.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static FieldBuilder WithAutoDocumentation(this FieldBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var provider = new FieldDocumentationProvider(builder.Name, builder.TypeName);
        var manager = new DocumentationManager(provider);
        return builder.WithDocumentationManager(manager);
    }
}
