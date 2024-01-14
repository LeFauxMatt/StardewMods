namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

/// <inheritdoc />
internal sealed class ComparableList<T>(Func<IEnumerable<T>> getter, Func<T, string, bool> comparer)
    : IEquatable<string>
{
    /// <summary>Determines whether the specified values matches the given expression.</summary>
    /// <param name="values">The values to compare.</param>
    /// <param name="comparer">A comparer for the type of values stored in this list.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool Equals(IEnumerable<T> values, Func<T, string, bool> comparer, string? expression) =>
        !string.IsNullOrWhiteSpace(expression) && values.Any(value => comparer(value, expression));

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableList<T>.Equals(getter(), comparer, expression);
}