namespace StardewMods.Common.Services.Integrations.ExpandedStorage;

/// <summary>Mod API for Expanded Storage.</summary>
public interface IExpandedStorageApi
{
    /// <summary>Event triggered when an expanded storage chest is created.</summary>
    public event EventHandler<IChestCreatedEventArgs> ChestCreated;

    /// <summary>Tries to retrieve the storage data associated with the specified item.</summary>
    /// <param name="item">The item for which to retrieve the data.</param>
    /// <param name="storageData">
    /// When this method returns, contains the data associated with the specified item, if the
    /// retrieval succeeds; otherwise, null. This parameter is passed uninitialized.
    /// </param>
    /// <returns>true if the data was successfully retrieved; otherwise, false.</returns>
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData);
}