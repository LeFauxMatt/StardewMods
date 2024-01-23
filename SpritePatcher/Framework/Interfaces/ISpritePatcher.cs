namespace StardewMods.SpritePatcher.Framework.Interfaces;

using StardewMods.SpritePatcher.Framework.Enums.Patches;

/// <summary>Implementation of draw patches.</summary>
public interface ISpritePatcher
{
    /// <summary>Gets a unique identifier associated the patches.</summary>
    public string Id { get; }

    /// <summary>Gets the <see cref="AllPatches" /> value that corresponds to the patched object.</summary>
    public AllPatches Type { get; }
}