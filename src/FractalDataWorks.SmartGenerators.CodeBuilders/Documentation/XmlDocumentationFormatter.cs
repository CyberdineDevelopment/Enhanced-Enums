using System.Text;
using System.Text.RegularExpressions;

namespace FractalDataWorks.SmartGenerators.CodeBuilders.Documentation;

#pragma warning disable CA1308 // Normalize strings to uppercase

/// <summary>
/// Utility class for formatting and generating XML documentation comments.
/// </summary>
public static class XmlDocumentationFormatter
{
    private static readonly Regex _pascalCaseRegex = new(@"(?<!^)(?=[A-Z])", RegexOptions.Compiled);

    /// <summary>
    /// Formats a documentation string by wrapping it in XML summary tags.
    /// </summary>
    /// <param name="summary">The summary content.</param>
    /// <returns>The formatted documentation string.</returns>
    public static string FormatSummary(string summary)
    {
        var sb = new StringBuilder()
            .AppendLine("/// <summary>")
            .AppendLine($"/// {summary}")
            .AppendLine("/// </summary>");
        return sb.ToString();
    }

    /// <summary>
    /// Formats a parameter documentation string by wrapping it in XML param tags.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="description">The description of the parameter.</param>
    /// <returns>The formatted parameter documentation string.</returns>
    public static string FormatParam(string paramName, string description) => $"/// <param name=\"{paramName}\">{description}</param>";

    /// <summary>
    /// Formats a return documentation string by wrapping it in XML returns tags.
    /// </summary>
    /// <param name="description">The description of the return value.</param>
    /// <returns>The formatted return documentation string.</returns>
    public static string FormatReturns(string description) => $"/// <returns>{description}</returns>";

    /// <summary>
    /// Formats an exception documentation string by wrapping it in XML exception tags.
    /// </summary>
    /// <param name="exceptionType">The type of the exception.</param>
    /// <param name="description">The description of when the exception is thrown.</param>
    /// <returns>The formatted exception documentation string.</returns>
    public static string FormatException(string exceptionType, string description) => $"/// <exception cref=\"{exceptionType}\">{description}</exception>";

    /// <summary>
    /// Splits a PascalCase or camelCase string into separate words with spaces between them.
    /// </summary>
    /// <param name="text">The camelCase or PascalCase text to split.</param>
    /// <returns>A human-readable string with spaces between words.</returns>
    public static string SplitPascalCase(string text)
    {
        return !string.IsNullOrEmpty(text) ? _pascalCaseRegex.Replace(text, " ") : string.Empty;
    }

    /// <summary>
    /// Converts the first letter of a string to lowercase.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>The converted text.</returns>
    public static string ToLowerCaseFirst(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text.Length != 1 ? char.ToLowerInvariant(text[0]) + text.Substring(1) : text.ToLowerInvariant();
    }

    /// <summary>
    /// Capitalizes the first letter of a string.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>The converted text with first letter capitalized.</returns>
    public static string CapitalizeFirst(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length == 1 ? text.ToUpperInvariant() : char.ToUpperInvariant(text[0]) + text.Substring(1);
    }
}

#pragma warning restore CA1308
