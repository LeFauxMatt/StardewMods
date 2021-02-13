using System;
using ExpandedStorage.API;
using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    internal class ContentLoader
    {
        private readonly IExpandedStorageAPI _expandedStorageAPI;
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        internal ContentLoader(IMonitor monitor, IModHelper helper, ExpandedStorageAPI expandedStorageAPI)
        {
            _monitor = monitor;
            _helper = helper;
            _expandedStorageAPI = expandedStorageAPI;

            // Default Exclusions
            _expandedStorageAPI.DisableWithModData("aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest");
            _expandedStorageAPI.DisableDrawWithModData("aedenthorn.CustomChestTypes/IsCustomChest");

            // Events
            _expandedStorageAPI.ReadyToLoad += OnReadyToLoad;
        }

        /// <summary>Load Expanded Storage content packs</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReadyToLoad(object sender, EventArgs e)
        {
            var contentPacks = _helper.ContentPacks.GetOwned();

            _monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (var contentPack in contentPacks)
                _expandedStorageAPI.LoadContentPack(contentPack);
        }
    }
}