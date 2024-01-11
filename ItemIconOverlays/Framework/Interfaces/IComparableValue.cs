namespace StardewMods.ItemIconOverlays.Framework.Interfaces;

/// <summary>Represents an interface for comparing values of type T.</summary>
/// <typeparam name="T">The type of value to compare.</typeparam>
public interface IComparableValue<out T> : IComparableValue
{
    /// <summary>Gets the value.</summary>
    T Value { get; }
}

public interface IComparableValue
{
    /// <summary>Compares the value with the specified string value.</summary>
    /// <param name="value">The string value to compare to.</param>
    /// <returns>A value indicating whether the string is comparable to the value.</returns>
    bool CompareTo(string value);
}