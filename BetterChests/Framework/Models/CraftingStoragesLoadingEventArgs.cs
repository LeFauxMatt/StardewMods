namespace StardewMods.BetterChests.Framework.Models;

using System.Collections.Generic;
using StardewMods.BetterChests.Framework.Handlers;

/// <summary>
///     Event args for the CraftingStoragesLoading event.
/// </summary>
internal sealed class CraftingStoragesLoadingEventArgs
{
    private readonly IList<BaseStorage> _storages;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CraftingStoragesLoadingEventArgs" /> class.
    /// </summary>
    /// <param name="storages">The storages to add.</param>
    public CraftingStoragesLoadingEventArgs(IList<BaseStorage> storages)
    {
        this._storages = storages;
    }

    /// <summary>
    ///     Adds a storage to the crafting page.
    /// </summary>
    /// <param name="storage">The storage to add.</param>
    public void AddStorage(BaseStorage storage)
    {
        if (!this._storages.Contains(storage))
        {
            this._storages.Add(storage);
        }
    }

    /// <summary>
    ///     Adds a storages to the crafting page.
    /// </summary>
    /// <param name="storages">The storages to add.</param>
    public void AddStorages(IEnumerable<BaseStorage> storages)
    {
        foreach (var storage in storages)
        {
            this.AddStorage(storage);
        }
    }
}