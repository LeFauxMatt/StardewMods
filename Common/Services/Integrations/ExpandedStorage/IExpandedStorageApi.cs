namespace StardewMods.Common.Services.Integrations.ExpandedStorage;

/// <summary>Mod API for Expanded Storage.</summary>
public interface IExpandedStorageApi
{
    /// <summary>Tries to retrieve the storage data associated with the specified item.</summary>
    /// <param name="item">The item for which to retrieve the data.</param>
    /// <param name="storageData">
    /// When this method returns, contains the data associated with the specified item, if the
    /// retrieval succeeds; otherwise, null. This parameter is passed uninitialized.
    /// </param>
    /// <returns>true if the data was successfully retrieved; otherwise, false.</returns>
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData);

    /// <summary>Subscribes to an event handler.</summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="handler">The event handler to subscribe.</param>
    void Subscribe<TEventArgs>(Action<TEventArgs> handler);

    /// <summary>Unsubscribes an event handler from an event.</summary>
    /// <param name="handler">The event handler to unsubscribe.</param>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    void Unsubscribe<TEventArgs>(Action<TEventArgs> handler);
}