namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableColor(Color value) : IEquatable<string>
{
    private static readonly Regex Regex = new(
        @"^(<=|>=|!=|<|>|~=|=~)?\s*(.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, (CompareType CompareType, Color[]? Values)> ExpressionCache = new();

    /// <summary>Determines whether the specified value matches the given expression.</summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool Equals(Color value, string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (!ComparableColor.ExpressionCache.TryGetValue(expression, out var parsedExpression))
        {
            parsedExpression = ComparableColor.ParseExpression(expression);
            ComparableColor.ExpressionCache[expression] = parsedExpression;
        }

        var (compareType, values) = parsedExpression;
        if (values?.Any() != true)
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => values.Any(colorValue => value == colorValue),
            CompareType.NotEqualTo => values.Any(colorValue => value != colorValue),
            _ => false,
        };
    }

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableColor.Equals(value, expression);

    /// <inheritdoc />
    public override string ToString() => value.ToString();

    private static (CompareType CompareType, Color[]? Values) ParseExpression(string expression)
    {
        if (expression == "*")
        {
            return (CompareType.WildCard, null);
        }

        var match = ComparableColor.Regex.Match(expression);
        if (!match.Success || string.IsNullOrWhiteSpace(match.Groups[2].Value))
        {
            return (CompareType.EqualTo, null);
        }

        var values = match
            .Groups[2]
            .Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(Utility.StringToColor)
            .OfType<Color>()
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