using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace ImJustMatt.ExpandedStorage.API
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

        /// <summary></summary>
        /// <param name="manifest">Mod manifest</param>
        /// <param name="storageName">The name of the storage.</param>
        /// <param name="storage">Storage data</param>
        void RegisterStorage(IManifest manifest, string storageName, IStorage storage);

        /// <summary></summary>
        /// <param name="manifest">Mod manifest</param>
        /// <param name="storageName">The name of the storage.</param>
        /// <param name="config">Storage config</param>
        void SetStorageConfig(IManifest manifest, string storageName, IStorageConfig config);

        /// <summary>Returns all Expanded Storage by name.</summary>
        /// <returns>List of storages</returns>
        IList<string> GetAllStorages();

        /// <summary>Returns owned Expanded Storage by name.</summary>
        /// <param name="manifest">Mod manifest</param>
        /// <returns>List of storages</returns>
        IList<string> GetOwnedStorages(IManifest manifest);

        /// <summary>Returns storage info based on name.</summary>
        /// <param name="storageName">The name of the storage.</param>
        /// <param name="storage">Storage Info</param>
        /// <returns>True if storage was found</returns>
        bool TryGetStorage(string storageName, out IStorage storage);

        /// <summary></summary>
        /// <param name="manifest">Mod manifest</param>
        /// <param name="tabName">Storage tab name</param>
        /// <param name="storageTab">Storage tab data</param>
        void RegisterStorageTab(IManifest manifest, string tabName, IStorageTab storageTab);

        /// <summary>Event shortly after GameLaunch when content packs can be loaded.</summary>
        event EventHandler ReadyToLoad;

        /// <summary>Event raised after Assets are done loading.</summary>
        event EventHandler StoragesLoaded;
    }
}