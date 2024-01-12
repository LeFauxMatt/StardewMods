namespace StardewMods.SpritePatcher.Framework.Models;

/// <summary>Represents a token.</summary>
internal sealed class Token(IEquatable<string> comparableValue, Dictionary<string, string> map, string stringValue)
{
    /// <summary>Gets the comparable value for this token.</summary>
    public IEquatable<string> Value { get; } = comparableValue;

    /// <summary>Gets mapped values for the attribute.</summary>
    public Dictionary<string, string> Map { get; } = map;

    /// <inheritdoc />
    public override string ToString() => stringValue;
}