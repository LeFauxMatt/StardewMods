namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Text.RegularExpressions;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableString(string value) : IEquatable<string>
{
    private static readonly Regex Regex = new(
        @"^(<=|>=|!=|<|>|)?\s*(.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Dictionary<string, (CompareType CompareType, string? Value)> ExpressionCache = new();

    /// <summary>Determines whether the specified value matches the given expression.</summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool Equals(string value, string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (!ComparableString.ExpressionCache.TryGetValue(expression, out var parsedExpression))
        {
            parsedExpression = ComparableString.ParseExpression(expression);
            ComparableString.ExpressionCache[expression] = parsedExpression;
        }

        var (compareType, stringValue) = parsedExpression;
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => value == stringValue,
            CompareType.NotEqualTo => value != stringValue,
            _ => false,
        };
    }

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableString.Equals(value, expression);

    /// <inheritdoc />
    public override string ToString() => value;

    private static (CompareType CompareType, string? Value) ParseExpression(string expression)
    {
        if (expression == "*")
        {
            return (CompareType.WildCard, null);
        }

        var match = ComparableString.Regex.Match(expression);
        if (!match.Success || string.IsNullOrWhiteSpace(match.Groups[2].Value))
        {
            return (CompareType.EqualTo, null);
        }

        return match.Groups[1].Value switch
        {
            "<=" => (CompareType.LessThanOrEqualTo, match.Groups[2].Value),
            ">=" => (CompareType.GreaterThanOrEqualTo, match.Groups[2].Value),
            "!=" => (CompareType.NotEqualTo, match.Groups[2].Value),
            "<" => (CompareType.LessThan, match.Groups[2].Value),
            ">" => (CompareType.GreaterThan, match.Groups[2].Value),
            _ => (CompareType.EqualTo, match.Groups[2].Value),
        };
    }
}