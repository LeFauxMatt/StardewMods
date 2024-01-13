namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using System.Text.RegularExpressions;
using StardewMods.SpritePatcher.Framework.Services;

/// <inheritdoc />
internal sealed class ComparableModel(IHaveModData source, DelegateManager.TryGetComparable tryGetComparable)
    : IEquatable<string>
{
    private static readonly Regex Regex = new(
        @"^([^\s]+)\s*(<=|>=|!=|<|>|~=|=~)\s*(.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, (string? Expression, string? Value)> ExpressionCache = new();

    /// <summary>Determines whether the specified value matches the given expression.</summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="tryGetComparable">Used to obtain the value from an object.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool Equals(IHaveModData value, DelegateManager.TryGetComparable tryGetComparable, string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (!ComparableModel.ExpressionCache.TryGetValue(expression, out var parsedExpression))
        {
            parsedExpression = ComparableModel.ParseExpression(expression);
            ComparableModel.ExpressionCache[expression] = parsedExpression;
        }

        var (path, newExpression) = parsedExpression;
        if (string.IsNullOrWhiteSpace(path) || !tryGetComparable(value, path, out var comparable))
        {
            return false;
        }

        return comparable.Equals(newExpression);
    }

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableModel.Equals(source, tryGetComparable, expression);

    private static (string? Expression, string? Value) ParseExpression(string expression)
    {
        var match = ComparableModel.Regex.Match(expression);
        if (!match.Success
            || string.IsNullOrWhiteSpace(match.Groups[1].Value)
            || string.IsNullOrWhiteSpace(match.Groups[3].Value))
        {
            return (null, null);
        }

        return (match.Groups[1].Value, match.Groups[2].Value + match.Groups[3].Value);
    }
}