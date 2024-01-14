namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Globalization;
using System.Text.RegularExpressions;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableInt(Func<int> getter) : IEquatable<string>
{
    private static readonly Regex Regex = new(
        @"^(<=|>=|!=|<|>|~=|=~)?\s*(\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, (CompareType CompareType, int[]? Values)> ExpressionCache = new();

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

        var (compareType, values) = parsedExpression;
        if (values?.Any() != true)
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => values.Any(intValue => value == intValue),
            CompareType.NotEqualTo => values.Any(intValue => value != intValue),
            CompareType.LessThan => values.Any(intValue => value < intValue),
            CompareType.LessThanOrEqualTo => values.Any(intValue => value <= intValue),
            CompareType.GreaterThan => values.Any(intValue => value > intValue),
            CompareType.GreaterThanOrEqualTo => values.Any(intValue => value >= intValue),
            _ => false,
        };
    }

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableInt.Equals(getter(), expression);

    /// <inheritdoc />
    public override string ToString() => getter().ToString(CultureInfo.InvariantCulture);

    private static (CompareType CompareType, int[]? Values) ParseExpression(string expression)
    {
        if (expression == "*")
        {
            return (CompareType.WildCard, null);
        }

        var match = ComparableInt.Regex.Match(expression);
        if (!match.Success || string.IsNullOrWhiteSpace(match.Groups[2].Value))
        {
            return (CompareType.EqualTo, null);
        }

        var values = match
            .Groups[2]
            .Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(value => int.TryParse(value, out _))
            .Select(int.Parse)
            .ToArray();

        return match.Groups[1].Value switch
        {
            "<=" => (CompareType.LessThanOrEqualTo, values),
            ">=" => (CompareType.GreaterThanOrEqualTo, values),
            "!=" => (CompareType.NotEqualTo, values),
            "<" => (CompareType.LessThan, values),
            ">" => (CompareType.GreaterThan, values),
            "=~" => (CompareType.LeftContains, values),
            "~=" => (CompareType.RightContains, values),
            _ => (CompareType.EqualTo, values),
        };
    }
}