namespace StardewMods.SpritePatcher.Framework.Models;

using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
public class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public bool DeveloperMode { get; set; }

    /// <inheritdoc />
    public Dictionary<AllPatches, bool> PatchedObjects { get; set; } = new();
}