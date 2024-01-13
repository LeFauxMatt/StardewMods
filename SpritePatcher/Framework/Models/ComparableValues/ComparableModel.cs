namespace StardewMods.SpritePatcher.Framework.Models.ComparableValues;

using StardewMods.SpritePatcher.Framework.Services;

/// <inheritdoc />
internal sealed class ComparableModel(IHaveModData source, DelegateManager.TryGetComparable tryGetComparable)
    : IEquatable<string>
{
    /// <summary>Determines whether the specified value matches the given expression.</summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="tryGetComparable">Used to obtain the value from an object.</param>
    /// <param name="expression">The expression to match against the value.</param>
    /// <returns>True if the value matches the expression; otherwise, false.</returns>
    public static bool
        Equals(IHaveModData value, DelegateManager.TryGetComparable tryGetComparable, string? expression) =>
        !string.IsNullOrWhiteSpace(expression) && tryGetComparable(value, expression, out _);

    /// <inheritdoc />
    public bool Equals(string? expression) => ComparableModel.Equals(source, tryGetComparable, expression);
}