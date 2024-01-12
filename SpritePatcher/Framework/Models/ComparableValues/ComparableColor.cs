namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableColor(Color value) : IEquatable<string>
{
    private static readonly Regex Regex = new(@"^(<=|>=|!=|<|>|)?\s*(.+)$");
    private static readonly Dictionary<string, (CompareType CompareType, Color? Value)> ExpressionCache = new();

    /// <inheritdoc />
    public bool Equals(string? expression)
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

        var (compareType, colorValue) = parsedExpression;
        if (!colorValue.HasValue)
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => value == colorValue,
            CompareType.NotEqualTo => value != colorValue,
            _ => false,
        };
    }

    /// <inheritdoc />
    public override string ToString() => value.ToString();

    private static (CompareType CompareType, Color? Value) ParseExpression(string expression)
    {
        if (expression == "*")
        {
            return (CompareType.WildCard, null);
        }

        var match = ComparableColor.Regex.Match(expression);
        if (!match.Success)
        {
            return (CompareType.EqualTo, null);
        }

        var colorValue = Utility.StringToColor(match.Groups[2].Value);
        if (!colorValue.HasValue)
        {
            return (CompareType.EqualTo, null);
        }

        return match.Groups[1].Value switch
        {
            "<=" => (CompareType.LessThanOrEqualTo, colorValue),
            ">=" => (CompareType.GreaterThanOrEqualTo, colorValue),
            "!=" => (CompareType.NotEqualTo, colorValue),
            "<" => (CompareType.LessThan, colorValue),
            ">" => (CompareType.GreaterThan, colorValue),
            _ => (CompareType.EqualTo, colorValue),
        };
    }
}