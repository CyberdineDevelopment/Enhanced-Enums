// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Suppress SA1402 warning for multiple types in a single file
[assembly: SuppressMessage(
$"StyleCop.CSharp.MaintainabilityRules",
$"SA1402:FileMayOnlyContainASingleType",
    Justification = $"Generic variants and related types are maintained in the same file")]

// Suppress SA1408 warnings about conditional expression precedence
[assembly: SuppressMessage(
$"StyleCop.CSharp.ReadabilityRules",
$"SA1408:ConditionalExpressionsShouldDeclarePrecedence",
    Justification = $"Precedence is clear from C# operator precedence rules")]
