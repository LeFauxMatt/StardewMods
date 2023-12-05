namespace StardewMods.BetterChests.Framework.Models;

/// <summary>Event args for the CraftingStoragesLoading event.</summary>
internal sealed class CraftingStoragesLoadingEventArgs
{
    private readonly IList<StorageNode> storages;

    /// <summary>Initializes a new instance of the <see cref="CraftingStoragesLoadingEventArgs" /> class.</summary>
    /// <param name="storages">The storages to add.</param>
    public CraftingStoragesLoadingEventArgs(IList<StorageNode> storages) => this.storages = storages;

    /// <summary>Adds a storage to the crafting page.</summary>
    /// <param name="storage">The storage to add.</param>
    public void AddStorage(StorageNode storage)
    {
        if (!this.storages.Contains(storage))
        {
            this.storages.Add(storage);
        }
    }

    /// <summary>Adds a storages to the crafting page.</summary>
    /// <param name="storagesToAdd">The storages to add.</param>
    public void AddStorages(IEnumerable<StorageNode> storagesToAdd)
    {
        foreach (var storage in storagesToAdd)
        {
            this.AddStorage(storage);
        }
    }
}
