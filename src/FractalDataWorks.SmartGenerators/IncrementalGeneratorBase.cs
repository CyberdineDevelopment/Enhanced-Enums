using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FractalDataWorks.SmartGenerators;

/// <summary>
/// Base class for incremental source generators that provides common functionality.
/// </summary>
/// <typeparam name="TInputInfo">The type of input info processed by this generator. Must implement IInputInfo.</typeparam>
public abstract class IncrementalGeneratorBase<TInputInfo> : IIncrementalGenerator
    where TInputInfo : class, IInputInfo
{
    /// <summary>
    /// Gets a dictionary of attribute source names and content to be registered.
    /// </summary>
    protected virtual IDictionary<string, string> AttributeSources => new Dictionary<string, string>(System.StringComparer.Ordinal);

    /// <summary>
    /// Initializes the generator by setting up the syntax provider pipeline and registering for source output.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register attribute sources if needed
        if (AttributeSources.Count > 0)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                foreach (var pair in AttributeSources)
                {
                    ctx.AddSource(pair.Key, SourceText.From(pair.Value, Encoding.UTF8));
                }
            });
        }

        // Set up the syntax provider pipeline
        var syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntax, _) => IsRelevantSyntax(syntax),
                transform: (ctx, cancellationToken) => TransformSyntax(ctx))
            .Where(model => model != null);

        // Register source output using the derived class implementation
        RegisterSourceOutput(context, syntaxProvider);
    }

    /// <summary>
    /// Determines whether a syntax node is relevant for this generator.
    /// </summary>
    /// <param name="syntaxNode">The syntax node to check.</param>
    /// <returns>True if the syntax node is relevant, false otherwise.</returns>
    protected abstract bool IsRelevantSyntax(SyntaxNode syntaxNode);

    /// <summary>
    /// Transforms a syntax context into an input model.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>The transformed input model, or null if the syntax is not relevant.</returns>
    protected abstract TInputInfo? TransformSyntax(GeneratorSyntaxContext context);

    /// <summary>
    /// Registers source output for this generator.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    /// <param name="syntaxProvider">The syntax provider.</param>
    protected abstract void RegisterSourceOutput(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<TInputInfo?> syntaxProvider);
}

/// <summary>
/// Non-generic base class for incremental source generators that provides common functionality.
/// Use the generic version for better type safety.
/// </summary>
public abstract class IncrementalGeneratorBase : IncrementalGeneratorBase<IInputInfo>
{
}
