namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

/// <inheritdoc />
internal sealed class ComparableDict<TKey, TValue>(
    IDictionary<TKey, TValue> dictionary,
    Func<TValue, string, bool> comparer) : IEquatable<string>
{
    /// <inheritdoc />
    public bool Equals(string? expression) =>
        !string.IsNullOrWhiteSpace(expression) && dictionary.Values.Any(value => comparer(value, expression));
}