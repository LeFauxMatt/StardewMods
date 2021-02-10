using System;
using Common.API.GenericModConfigMenu;
using ExpandedStorage.Framework.API;
using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    internal class ContentLoader
    {
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;

        private readonly IExpandedStorageAPI _expandedStorageAPI;
        internal ContentLoader(IMonitor monitor, IModHelper helper, IExpandedStorageAPI expandedStorageAPI)
        {
            _monitor = monitor;
            _helper = helper;
            _expandedStorageAPI = expandedStorageAPI;

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
            {
                if (!_expandedStorageAPI.LoadContentPack(contentPack))
                    continue;
            }
        }
    }
}