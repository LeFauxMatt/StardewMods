using System;
using StardewModdingAPI;
using StardewValley.Characters;

namespace ExpandedStorage.Framework.API
{
    public interface IExpandedStorageAPI
    {
        /// <summary>Load a directory as an Expanded Storage content pack.</summary>
        /// <param name="path">Path containing expandedStorage.json file.</param>
        /// <returns>True if content was loaded successfully.</returns>
        bool LoadContentPack(string path);

        /// <summary>Load an Expanded Storage content pack.</summary>
        /// <param name="contentPack">The content pack to load.</param>
        /// <returns>True if content was loaded successfully.</returns>
        bool LoadContentPack(IContentPack contentPack);

        /// <summary>Registers Expanded Storage features for a storage object.</summary>
        /// <param name="storage">Storage features to enable.</param>
        /// <param name="config">Storage config.</param>
        /// <returns>True if storage was found and updated.</returns>
        bool RegisterStorage(IStorage storage, IStorageConfig config);

        /// <summary>Registers Expanded Storage features for a storage object.</summary>
        /// <param name="sheetIndex">Sheet index on BigCraftables.</param>
        /// <param name="storage">Storage features to enable.</param>
        /// <param name="config">Storage config.</param>
        /// <returns>True if storage was found and updated.</returns>
        bool RegisterStorage(int sheetIndex, IStorage storage, IStorageConfig config);
        
        /// <summary>Update Config for Expanded Storage.</summary>
        /// <param name="storage">Storage features to enable.</param>
        /// <param name="config">Storage config.</param>
        /// <returns>True if storage was found and updated.</returns>
        bool UpdateStorageConfig(IStorage storage, IStorageConfig config);

        /// <summary>Update Config for Expanded Storage.</summary>
        /// <param name="sheetIndex">Sheet index on BigCraftables.</param>
        /// <param name="config">Storage config.</param>
        /// <returns>True if storage was found and updated.</returns>
        bool UpdateStorageConfig(int sheetIndex, IStorageConfig config);

        /// <summary>Event shortly after GameLaunch when content packs can be loaded.</summary>
        event EventHandler ReadyToLoad;

        /// <summary>Event raised after Assets are done loading.</summary>
        event EventHandler StoragesLoaded;
    }
}