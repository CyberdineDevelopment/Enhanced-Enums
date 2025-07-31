using System;
using System.Linq;
using System.Text;
using FractalDataWorks.EnhancedEnums.Models;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for building the final source code with headers and using statements.
/// </summary>
public static class SourceCodeBuilder
{
    /// <summary>
    /// Builds the complete source code with headers and using statements.
    /// </summary>
    public static string BuildSourceCode(EnumTypeInfo def, string effectiveReturnType, string generatedClassCode)
    {
        var sourceCode = new StringBuilder();
        
        AddHeaders(sourceCode);
        AddUsingStatements(sourceCode, def, effectiveReturnType);
        sourceCode.Append(generatedClassCode);
        
        return sourceCode.ToString();
    }

    private static void AddHeaders(StringBuilder sourceCode)
    {
        sourceCode.AppendLine("#nullable enable");
        sourceCode.AppendLine();
    }

    private static void AddUsingStatements(StringBuilder sourceCode, EnumTypeInfo def, string effectiveReturnType)
    {
        // Add standard using statements
        sourceCode.AppendLine("using System;");
        sourceCode.AppendLine("using System.Linq;");
        sourceCode.AppendLine("using System.Collections.Generic;");
        sourceCode.AppendLine("using System.Collections.Immutable;");
        sourceCode.AppendLine("#if NET8_0_OR_GREATER");
        sourceCode.AppendLine("using System.Collections.Frozen;");
        sourceCode.AppendLine("#endif");
        
        // Add required namespaces from generic constraints
        foreach (var ns in def.RequiredNamespaces.OrderBy(n => n, StringComparer.Ordinal))
        {
            // Skip the current namespace and the standard System namespaces we already added
            if (!ShouldSkipNamespace(ns, def.Namespace))
            {
                sourceCode.AppendLine($"using {ns};");
            }
        }

        AddReturnTypeNamespaces(sourceCode, def, effectiveReturnType);
        
        sourceCode.AppendLine();
    }

    private static void AddReturnTypeNamespaces(StringBuilder sourceCode, EnumTypeInfo def, string effectiveReturnType)
    {
        // Add default generic return type namespace if specified
        if (!string.IsNullOrEmpty(def.DefaultGenericReturnTypeNamespace))
        {
            var ns = def.DefaultGenericReturnTypeNamespace!;
            if (!ShouldSkipNamespace(ns, def.Namespace))
            {
                sourceCode.AppendLine($"using {ns};");
            }
        }
        
        // Add namespace for ReturnType if specified
        if (!string.IsNullOrEmpty(def.ReturnTypeNamespace))
        {
            // Use the explicitly provided namespace
            if (!ShouldSkipNamespace(def.ReturnTypeNamespace!, def.Namespace))
            {
                sourceCode.AppendLine($"using {def.ReturnTypeNamespace};");
            }
        }
        else if (!string.IsNullOrEmpty(effectiveReturnType))
        {
            // Extract namespace from ReturnType if not explicitly provided
            var cleanReturnType = effectiveReturnType!.TrimEnd('?');
            var lastDotIndex = cleanReturnType.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                var returnTypeNamespace = cleanReturnType.Substring(0, lastDotIndex);
                // Don't add System namespaces or the current namespace
                if (!ShouldSkipNamespace(returnTypeNamespace, def.Namespace))
                {
                    sourceCode.AppendLine($"using {returnTypeNamespace};");
                }
            }
        }
    }

    private static bool ShouldSkipNamespace(string ns, string targetNamespace)
    {
        return string.Equals(ns, targetNamespace, StringComparison.Ordinal) ||
               string.Equals(ns, "System", StringComparison.Ordinal) ||
               string.Equals(ns, "System.Linq", StringComparison.Ordinal) ||
               string.Equals(ns, "System.Collections.Generic", StringComparison.Ordinal) ||
               string.Equals(ns, "System.Collections.Immutable", StringComparison.Ordinal) ||
               ns.StartsWith("System", StringComparison.Ordinal);
    }
}