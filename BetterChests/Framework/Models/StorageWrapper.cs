namespace StardewMods.BetterChests.Framework.Models;

using StardewMods.BetterChests.Framework.Handlers;

/// <summary>
///     A wrapper for a Storage Object.
/// </summary>
internal sealed class StorageWrapper
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageWrapper" /> class.
    /// </summary>
    /// <param name="storage">The storage object.</param>
    public StorageWrapper(BaseStorage storage)
    {
        this.Storage = storage;
    }

    /// <summary>
    ///     Gets the storage object.
    /// </summary>
    public BaseStorage Storage { get; }
}