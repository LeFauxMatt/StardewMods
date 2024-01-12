namespace StardewMods.SpritePatcher.Framework.Interfaces;

/// <summary>Represents a token with an attribute name and mapped values.</summary>
public interface ITokenDefinition
{
    /// <summary>Gets the attribute that the token refers to.</summary>
    public string RefersTo { get; }

    /// <summary>Gets mapped values for the attribute.</summary>
    public Dictionary<string, string> Map { get; }
}