namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Text.RegularExpressions;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableEnum<T>(Func<T> getter) : IEquatable<string>
    where T : Enum
{
    private static readonly Regex Regex = new(
        @"^(<=|>=|!=|<|>|~=|=~)?\s*(.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, (CompareType CompareType, string[]? Values)> ExpressionCache = new();

    /// <summary>Determines whether the specified value matches the given expression.</summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool Equals(T value, string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (!ComparableEnum<T>.ExpressionCache.TryGetValue(expression, out var parsedExpression))
        {
            parsedExpression = ComparableEnum<T>.ParseExpression(expression);
            ComparableEnum<T>.ExpressionCache[expression] = parsedExpression;
        }

        var (compareType, values) = parsedExpression;
        if (values?.Any() != true)
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => values.Any(stringValue => stringValue == value.ToString()),
            CompareType.NotEqualTo => values.Any(stringValue => stringValue != value.ToString()),
            CompareType.LeftContains => values.Any(value.ToString().Contains),
            CompareType.RightContains => values.Any(stringValue => stringValue.Contains(value.ToString())),
            _ => false,
        };
    }

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableEnum<T>.Equals(getter(), expression);

    /// <inheritdoc />
    public override string ToString() => getter().ToString();

    private static (CompareType CompareType, string[]? Values) ParseExpression(string expression)
    {
        if (expression == "*")
        {
            return (CompareType.WildCard, null);
        }

        var match = ComparableEnum<T>.Regex.Match(expression);
        if (!match.Success || string.IsNullOrWhiteSpace(match.Groups[2].Value))
        {
            return (CompareType.EqualTo, null);
        }

        var values = match
            .Groups[2]
            .Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

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