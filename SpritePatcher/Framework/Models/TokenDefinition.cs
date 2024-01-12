namespace StardewMods.SpritePatcher.Framework.Models;

using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class TokenDefinition(string refersTo, Dictionary<string, string>? values = null) : ITokenDefinition
{
    /// <inheritdoc />
    public string RefersTo { get; set; } = refersTo;

    /// <inheritdoc />
    public Dictionary<string, string> Map { get; set; } = values ?? [];
}