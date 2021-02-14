using System;
using System.IO;
using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    internal class ContentLoader : IAssetEditor
    {
        private readonly ExpandedStorageAPI _expandedStorageAPI;
        private readonly IModHelper _helper;
        private readonly IManifest _manifest;
        private readonly IMonitor _monitor;

        internal ContentLoader(IModHelper helper, IManifest manifest, IMonitor monitor, ExpandedStorageAPI expandedStorageAPI)
        {
            _helper = helper;
            _manifest = manifest;
            _monitor = monitor;

            _expandedStorageAPI = expandedStorageAPI;
            _expandedStorageAPI.ReadyToLoad += OnReadyToLoad;

            // Default Exclusions
            _expandedStorageAPI.DisableWithModData("aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest");
            _expandedStorageAPI.DisableDrawWithModData("aedenthorn.CustomChestTypes/IsCustomChest");
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            // Load bigCraftable on next tick for vanilla storages
            if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
                _helper.Events.GameLoop.UpdateTicked += _expandedStorageAPI.OnUpdateTicked;
            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public void Edit<T>(IAssetData asset)
        {
        }

        /// <summary>Load More Craftables Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReadyToLoad(object sender, EventArgs e)
        {
            var contentPacks = _helper.ContentPacks.GetOwned();
            _monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (var contentPack in contentPacks) _expandedStorageAPI.LoadContentPack(contentPack);

            // Load Default Config
            _expandedStorageAPI.LoadContentPack(Path.Combine(_helper.DirectoryPath, "default"));
        }
    }
}