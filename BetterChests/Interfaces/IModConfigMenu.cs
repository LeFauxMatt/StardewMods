namespace Mod.BetterChests.Interfaces;

using Mod.BetterChests.Services;
using StardewModdingAPI;

/// <summary>
/// The public surface of <see cref="ModConfigMenu" /> service.
/// </summary>
public interface IModConfigMenu
{
    /// <summary>
    /// Adds GMCM options for chest data.
    /// </summary>
    /// <param name="manifest">The mod's manifest.</param>
    /// <param name="config">The chest data to configure.</param>
    public void ChestConfig(IManifest manifest, IChestData config);
}