namespace StardewMods.Common.Integrations.BetterChests;

using System.Collections.Generic;

/// <summary>
///     Event args for the CraftingStoragesLoading event.
/// </summary>
public interface ICraftingStoragesLoadingEventArgs
{
    /// <summary>
    ///     Adds a storage to the crafting page.
    /// </summary>
    /// <param name="storage">The storage to add.</param>
    public void AddStorage(IStorageObject storage);

    /// <summary>
    ///     Adds a storages to the crafting page.
    /// </summary>
    /// <param name="storages">The storages to add.</param>
    public void AddStorages(IEnumerable<IStorageObject> storages);
}