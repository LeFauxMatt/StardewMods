namespace StardewMods.Common.Integrations.ExpandedStorage;

/// <summary>
///     API for ExpandedStorage.
/// </summary>
public interface IExpandedStorageApi
{
    /// <summary>
    ///     Loads an Expanded Storage content pack.
    /// </summary>
    /// <param name="manifest">The mod manifest of the content pack.</param>
    /// <param name="path">The path to the content pack data.</param>
    /// <returns>Returns true if the content pack could be loaded.</returns>
    public bool LoadContentPack(IManifest manifest, string path);

    /// <summary>
    ///     Loads an Expanded Storage content pack.
    /// </summary>
    /// <param name="contentPack">The content pack to load.</param>
    /// <returns>Returns true if the content pack could be loaded.</returns>
    public bool LoadContentPack(IContentPack contentPack);

    /// <summary>
    ///     Registers a custom Expanded Storage chest.
    /// </summary>
    /// <param name="name">The custom storage's name.</param>
    /// <param name="storage">The custom storage to load.</param>
    /// <returns>Returns true if the storage could be loaded.</returns>
    public bool RegisterStorage(string name, IManagedStorage storage);
}