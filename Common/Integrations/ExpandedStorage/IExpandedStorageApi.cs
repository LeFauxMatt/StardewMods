namespace StardewMods.Common.Integrations.ExpandedStorage;

/// <summary>
///     API for ExpandedStorage.
/// </summary>
public interface IExpandedStorageApi
{
    /// <summary>
    ///     Registers a custom Expanded Storage chest.
    /// </summary>
    /// <param name="id">A unique id for the custom storage.</param>
    /// <param name="storage">The custom storage to load.</param>
    /// <returns>Returns true if the storage could be loaded.</returns>
    public bool RegisterStorage(string id, ICustomStorage storage);
}