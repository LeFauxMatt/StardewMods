namespace StardewMods.BetterChests.Models;

using StardewMods.Common.Integrations.BetterChests;

/// <summary>
///     A wrapper for a Storage Object.
/// </summary>
internal sealed class StorageWrapper
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageWrapper" /> class.
    /// </summary>
    /// <param name="storage">The storage object.</param>
    public StorageWrapper(IStorageObject storage)
    {
        this.Storage = storage;
    }

    /// <summary>
    ///     Gets the storage object.
    /// </summary>
    public IStorageObject Storage { get; }
}