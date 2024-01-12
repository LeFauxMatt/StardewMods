namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Text.RegularExpressions;
using StardewMods.SpritePatcher.Framework.Enums;

/// <inheritdoc />
internal sealed class ComparableEnum<T>(T value) : IEquatable<string>
    where T : Enum
{
    private static readonly Regex Regex = new(@"^(<=|>=|!=|<|>|)?\s*(.+)$");
    private static readonly Dictionary<string, (CompareType CompareType, string? Value)> ExpressionCache = new();

    /// <inheritdoc />
    public bool Equals(string? expression)
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

        var (compareType, stringValue) = parsedExpression;
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return compareType == CompareType.WildCard;
        }

        return compareType switch
        {
            CompareType.WildCard => true,
            CompareType.EqualTo => this.ToString() == stringValue,
            CompareType.NotEqualTo => this.ToString() != stringValue,
            _ => false,
        };
    }

    /// <inheritdoc />
    public override string ToString() => value.ToString();

    private static (CompareType CompareType, string? Value) ParseExpression(string expression)
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