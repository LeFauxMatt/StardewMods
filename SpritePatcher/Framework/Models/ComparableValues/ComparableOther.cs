namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Text.RegularExpressions;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableOther(object value) : IEquatable<string>
{
    private static readonly Regex Regex = new(
        @"^(<=|>=|!=|<|>|)?\s*(.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Dictionary<string, (CompareType CompareType, string? Value)> ExpressionCache = new();

    /// <summary>Determines whether the specified value matches the given expression.</summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool Equals(object value, string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (!ComparableOther.ExpressionCache.TryGetValue(expression, out var parsedExpression))
        {
            parsedExpression = ComparableOther.ParseExpression(expression);
            ComparableOther.ExpressionCache[expression] = parsedExpression;
        }

        var (compareType, stringValue) = parsedExpression;
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => value.ToString() == stringValue,
            CompareType.NotEqualTo => value.ToString() != stringValue,
            _ => false,
        };
    }

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableOther.Equals(value, expression);

    private static (CompareType CompareType, string? Value) ParseExpression(string expression)
    {
        if (expression == "*")
        {
            return (CompareType.WildCard, null);
        }

        var match = ComparableOther.Regex.Match(expression);
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