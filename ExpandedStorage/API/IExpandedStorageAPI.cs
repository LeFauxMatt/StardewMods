using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace ExpandedStorage.API
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

        /// <summary>Prevent a chest from being handled by Expanded Storage.</summary>
        /// <param name="modDataKey">The modData key.</param>
        void DisableWithModData(string modDataKey);

        /// <summary>Prevents chest draw from being handled by Expanded Storage.</summary>
        /// <param name="modDataKey">The modData key.</param>
        void DisableDrawWithModData(string modDataKey);

        /// <summary>Returns all Expanded Storage by name.</summary>
        /// <returns>List of storages</returns>
        IList<string> GetAllStorages();

        /// <summary>Returns all Expanded Storage by sheet index.</summary>
        /// <returns>List of storage ids</returns>
        IList<int> GetAllStorageIds();

        /// <summary>Returns storage info based on name.</summary>
        /// <param name="storageName">The name of the storage.</param>
        /// <returns>Storage Info</returns>
        IStorage GetStorage(string storageName);

        /// <summary>Returns storage config based on name.</summary>
        /// <param name="storageName">The name of the storage.</param>
        /// <returns>Storage Config</returns>
        IStorageConfig GetStorageConfig(string storageName);

        /// <summary>Event shortly after GameLaunch when content packs can be loaded.</summary>
        event EventHandler ReadyToLoad;

        /// <summary>Event raised after Assets are done loading.</summary>
        event EventHandler StoragesLoaded;
    }
}