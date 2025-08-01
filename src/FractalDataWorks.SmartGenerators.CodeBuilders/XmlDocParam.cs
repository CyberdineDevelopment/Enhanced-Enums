using System;
using System.Collections.Generic;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Represents an XML documentation parameter.
/// </summary>
public class XmlDocParam
{
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the parameter.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Extension methods for <see cref="MethodBuilder"/> to add XML documentation.
/// </summary>
public static class MethodBuilderExtensions
{
    private static readonly Dictionary<MethodBuilder, List<XmlDocParam>> _methodParams = new();
    private static readonly Dictionary<MethodBuilder, string> _methodReturns = new();
    private static readonly Dictionary<MethodBuilder, List<XmlDocExceptionInformation>> _methodExceptions = new();

    /// <summary>
    /// Adds an XML documentation parameter tag for the method.
    /// </summary>
    /// <param name="builder">The method builder.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="description">The description of the parameter.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static MethodBuilder WithXmlDocParam(this MethodBuilder builder, string paramName, string description)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrWhiteSpace(paramName))
        {
            throw new ArgumentException("Parameter name cannot be null or whitespace.", nameof(paramName));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be null or whitespace.", nameof(description));
        }

        if (!_methodParams.TryGetValue(builder, out var parameters))
        {
            parameters = new List<XmlDocParam>();
            _methodParams[builder] = parameters;
        }

        parameters.Add(new XmlDocParam { Name = paramName, Description = description });
        return builder;
    }

    /// <summary>
    /// Adds an XML documentation returns tag for the method.
    /// </summary>
    /// <param name="builder">The method builder.</param>
    /// <param name="description">The description of the return value.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static MethodBuilder WithXmlDocReturns(this MethodBuilder builder, string description)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be null or whitespace.", nameof(description));
        }

        _methodReturns[builder] = description;
        return builder;
    }

    /// <summary>
    /// Adds an XML documentation exception tag for the method.
    /// </summary>
    /// <param name="builder">The method builder.</param>
    /// <param name="exceptionType">The type of the exception.</param>
    /// <param name="description">The description of when the exception is thrown.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static MethodBuilder WithXmlDocException(this MethodBuilder builder, string exceptionType, string description)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrWhiteSpace(exceptionType))
        {
            throw new ArgumentException("Exception type cannot be null or whitespace.", nameof(exceptionType));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be null or whitespace.", nameof(description));
        }

        if (!_methodExceptions.TryGetValue(builder, out var exceptions))
        {
            exceptions = new List<XmlDocExceptionInformation>();
            _methodExceptions[builder] = exceptions;
        }

        exceptions.Add(new XmlDocExceptionInformation { Type = exceptionType, Description = description });
        return builder;
    }

    /// <summary>
    /// Gets the XML documentation parameters for a method.
    /// </summary>
    /// <param name="builder">The method builder.</param>
    /// <returns>The list of XML documentation parameters.</returns>
    public static IReadOnlyList<XmlDocParam> GetXmlDocParams(this MethodBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (_methodParams.TryGetValue(builder, out var parameters))
        {
            return parameters;
        }

        return Array.Empty<XmlDocParam>();
    }

    /// <summary>
    /// Gets the XML documentation returns description for a method.
    /// </summary>
    /// <param name="builder">The method builder.</param>
    /// <returns>The returns description, or empty string if not set.</returns>
    public static string GetXmlDocReturns(this MethodBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (_methodReturns.TryGetValue(builder, out var returns))
        {
            return returns;
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the XML documentation exceptions for a method.
    /// </summary>
    /// <param name="builder">The method builder.</param>
    /// <returns>The list of XML documentation exceptions.</returns>
    public static IReadOnlyList<XmlDocExceptionInformation> GetXmlDocExceptions(this MethodBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (_methodExceptions.TryGetValue(builder, out var exceptions))
        {
            return exceptions;
        }

        return Array.Empty<XmlDocExceptionInformation>();
    }
}
