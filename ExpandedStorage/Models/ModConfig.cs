namespace StardewMods.ExpandedStorage.Models;

using System.Collections.Generic;

/// <summary>
///     Mod config data for Expanded Storage.
/// </summary>
internal sealed class ModConfig
{
    /// <summary>
    ///     Config options for each Expanded Storage chest type.
    /// </summary>
    public Dictionary<string, StorageConfig> Config = new();

    /// <summary>
    ///     Copies all ModConfig data to another ModConfig instance.
    /// </summary>
    /// <param name="other">The ModConfig instance to copy to.</param>
    public void CopyTo(ModConfig other)
    {
        foreach (var (id, config) in other.Config)
        {
            other.Config[id] = new();
        }
    }
}