using System;
using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ExpandedStorage.Framework
{
    internal class DataLoader
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private IJsonAssetsApi _jsonAssetsApi;
        private readonly List<ExpandedStorageData> _expandedStorage = new List<ExpandedStorageData>();
        internal DataLoader(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            
            // Events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }
        
        /// <summary>
        /// Loads Expanded Storage content pack data.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _jsonAssetsApi = _helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            _jsonAssetsApi.IdsAssigned += OnIdsAssigned;
            
            _monitor.Log($"Loading Content Packs", LogLevel.Info);
            foreach (var contentPack in _helper.ContentPacks.GetOwned())
            {
                if (!contentPack.HasFile("expandedStorage.json"))
                {
                    _monitor.Log($"Cannot load {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.Manifest.Description}", LogLevel.Warn);
                    continue;
                }
                _monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.Manifest.Description}", LogLevel.Info);
                var contentData = contentPack.ReadJsonFile<ContentPackData>("expandedStorage.json");
                _expandedStorage.AddRange(contentData.ExpandedStorage.Where(s => !string.IsNullOrWhiteSpace(s.StorageName)));
            }
        }
        /// <summary>
        /// Gets ParentSheetIndex for Expanded Storages from Json Assets API.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnIdsAssigned(object sender, EventArgs e)
        {
            _monitor.Log("Loading Expanded Storage IDs", LogLevel.Info);
            var ids = _jsonAssetsApi.GetAllBigCraftableIds();
            foreach (var expandedStorage in _expandedStorage)
            {
                if (ids.TryGetValue(expandedStorage.StorageName, out var id))
                {
                    expandedStorage.ParentSheetIndex = id;
                }
                else
                {
                    _monitor.Log($"Cannot convert {expandedStorage.StorageName} into Expanded Storage. Object is not loaded!", LogLevel.Warn);
                    _expandedStorage.Remove(expandedStorage);
                }
            }
            ItemExtensions.Init(_expandedStorage);
        }
    }
}