namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Globalization;
using System.Text.RegularExpressions;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableInt(int value) : IEquatable<string>
{
    private static readonly Regex Regex = new(
        @"^(<=|>=|!=|<|>|)?\s*(\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Dictionary<string, (CompareType CompareType, int? Value)> ExpressionCache = new();

    /// <summary>Determines whether the specified value matches the given expression.</summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool Equals(int value, string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (!ComparableInt.ExpressionCache.TryGetValue(expression, out var parsedExpression))
        {
            parsedExpression = ComparableInt.ParseExpression(expression);
            ComparableInt.ExpressionCache[expression] = parsedExpression;
        }

        var (compareType, intValue) = parsedExpression;
        if (!intValue.HasValue)
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => value == intValue,
            CompareType.NotEqualTo => value != intValue,
            CompareType.LessThan => value < intValue,
            CompareType.LessThanOrEqualTo => value <= intValue,
            CompareType.GreaterThan => value > intValue,
            CompareType.GreaterThanOrEqualTo => value >= intValue,
            _ => false,
        };
    }

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableInt.Equals(value, expression);

    /// <inheritdoc />
    public override string ToString() => value.ToString(CultureInfo.InvariantCulture);

    private static (CompareType CompareType, int? Value) ParseExpression(string expression)
    {
        if (expression == "*")
        {
            return (CompareType.WildCard, null);
        }

        var match = ComparableInt.Regex.Match(expression);
        if (!match.Success || !int.TryParse(match.Groups[2].Value, out var intValue))
        {
            return (CompareType.EqualTo, null);
        }

        return match.Groups[1].Value switch
        {
            "<=" => (CompareType.LessThanOrEqualTo, intValue),
            ">=" => (CompareType.GreaterThanOrEqualTo, intValue),
            "!=" => (CompareType.NotEqualTo, intValue),
            "<" => (CompareType.LessThan, intValue),
            ">" => (CompareType.GreaterThan, intValue),
            _ => (CompareType.EqualTo, intValue),
        };
    }
}