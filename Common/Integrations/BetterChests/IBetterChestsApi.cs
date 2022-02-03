namespace Common.Integrations.BetterChests;

using System.Collections.Generic;
using StardewModdingAPI;

/// <summary>
///     API for Better Chests.
/// </summary>
public interface IBetterChestsApi
{
    /// <summary>
    ///     Adds GMCM options for chest data.
    /// </summary>
    /// <param name="manifest">The mod's manifest.</param>
    /// <param name="data">A dictionary of key/value strings representing chest data.</param>
    public void AddChestOptions(IManifest manifest, IDictionary<string, string> data);

    /// <summary>
    ///     Registers an Item as a chest based on its name.
    /// </summary>
    /// <param name="name">The name of the chest to register.</param>
    /// <returns>True if the data was successfully saved.</returns>
    public bool RegisterChest(string name);
}