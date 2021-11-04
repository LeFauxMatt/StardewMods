namespace Common.Integrations.MoreChests
{
    using System.Collections.Generic;
    using StardewModdingAPI;

    public interface IMoreChestsApi
    {
        /// <summary>Load a directory as a More Chests content pack.</summary>
        /// <param name="manifest">Manifest for content pack.</param>
        /// <param name="path">Path containing expandedStorage.json file.</param>
        /// <returns>True if content was loaded successfully.</returns>
        bool LoadContentPack(IManifest manifest, string path);

        /// <summary>Load a More Chests content pack.</summary>
        /// <param name="contentPack">The content pack to load.</param>
        /// <returns>True if content was loaded successfully.</returns>
        bool LoadContentPack(IContentPack contentPack);

        /// <summary>Returns all Custom Chests by name.</summary>
        /// <returns>List of storages.</returns>
        IEnumerable<string> GetAllChests();
    }
}