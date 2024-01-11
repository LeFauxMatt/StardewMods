namespace StardewMods.ItemIconOverlays.Framework.Models;

using StardewMods.ItemIconOverlays.Framework.Interfaces;

/// <inheritdoc />
public class ComparableValue<T>(T value, Func<string, bool> compare) : IComparableValue<T>
{
    /// <inheritdoc />
    public T Value { get; } = value;

    /// <inheritdoc />
    public bool CompareTo(string value) => compare(value);
}