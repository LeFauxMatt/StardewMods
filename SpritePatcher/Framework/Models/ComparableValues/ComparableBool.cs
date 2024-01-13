namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Text.RegularExpressions;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableBool(bool value) : IEquatable<string>
{
    private static readonly Regex Regex = new(
        @"^(<=|>=|!=|<|>|~=|=~)?\s*(true|false)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, (CompareType CompareType, bool? Value)> ExpressionCache = new();

    /// <summary>Determines whether the specified value matches the given expression.</summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool Equals(bool value, string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (!ComparableBool.ExpressionCache.TryGetValue(expression, out var parsedExpression))
        {
            parsedExpression = ComparableBool.ParseExpression(expression);
            ComparableBool.ExpressionCache[expression] = parsedExpression;
        }

        var (compareType, boolValue) = parsedExpression;
        if (!boolValue.HasValue)
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => value == boolValue,
            CompareType.NotEqualTo => value != boolValue,
            _ => false,
        };
    }

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableBool.Equals(value, expression);

    /// <inheritdoc />
    public override string ToString() => value.ToString();

    private static (CompareType CompareType, bool? Value) ParseExpression(string expression)
    {
        if (expression == "*")
        {
            return (CompareType.WildCard, null);
        }

        var match = ComparableBool.Regex.Match(expression);
        if (!match.Success || !bool.TryParse(match.Groups[2].Value, out var boolValue))
        {
            return (CompareType.EqualTo, null);
        }

        return match.Groups[1].Value switch
        {
            "<=" => (CompareType.LessThanOrEqualTo, boolValue),
            ">=" => (CompareType.GreaterThanOrEqualTo, boolValue),
            "!=" => (CompareType.NotEqualTo, boolValue),
            "<" => (CompareType.LessThan, boolValue),
            ">" => (CompareType.GreaterThan, boolValue),
            "=~" => (CompareType.LeftContains, boolValue),
            "~=" => (CompareType.RightContains, boolValue),
            _ => (CompareType.EqualTo, boolValue),
        };
    }
}