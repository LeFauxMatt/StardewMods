namespace StardewMods.Common.Integrations.BetterChests;

/// <summary>
///     Event arguments for a StorageTypeRequested event.
/// </summary>
public interface IStorageTypeRequestedEventArgs
{
    /// <summary>
    ///     Gets the context object of the storage.
    /// </summary>
    public object Context { get; }

    /// <summary>
    ///     Loads storage data for this chest type.
    /// </summary>
    /// <param name="data">The storage data.</param>
    /// <param name="priority">The priority given to this storage type.</param>
    public void Load(IStorageData data, int priority);
}