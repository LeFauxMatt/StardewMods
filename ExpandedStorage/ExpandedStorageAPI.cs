using System;
using System.Collections.Generic;
using System.Linq;
using ImJustMatt.ExpandedStorage.API;
using ImJustMatt.ExpandedStorage.Framework.Integrations;
using ImJustMatt.ExpandedStorage.Framework.Models;
using ImJustMatt.ExpandedStorage.Framework.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ImJustMatt.ExpandedStorage
{
    public class ExpandedStorageAPI : IExpandedStorageAPI, IAssetEditor
    {
        private readonly IList<string> _contentDirs = new List<string>();
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly IDictionary<string, Storage> _storageConfigs;
        private readonly IDictionary<string, StorageTab> _tabConfigs;

        private bool _isContentLoaded;
        private IJsonAssetsAPI _jsonAssetsAPI;

        internal ExpandedStorageAPI(
            IModHelper helper,
            IMonitor monitor,
            IDictionary<string, Storage> storageConfigs,
            IDictionary<string, StorageTab> tabConfigs)
        {
            _helper = helper;
            _monitor = monitor;
            _storageConfigs = storageConfigs;
            _tabConfigs = tabConfigs;

            // Events
            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            // Load bigCraftable on next tick for vanilla storages
            if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
                _helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public void Edit<T>(IAssetData asset)
        {
        }

        public event EventHandler ReadyToLoad;
        public event EventHandler StoragesLoaded;

        public void DisableWithModData(string modDataKey)
        {
            Storage.AddExclusion(modDataKey);
        }

        public void DisableDrawWithModData(string modDataKey)
        {
            ChestPatch.AddExclusion(modDataKey);
            ObjectPatch.AddExclusion(modDataKey);
        }

        public IList<string> GetAllStorages()
        {
            return _storageConfigs.Keys.ToList();
        }

        public IList<string> GetOwnedStorages(IManifest manifest)
        {
            return _storageConfigs
                .Where(storageConfig => storageConfig.Value.ModUniqueId == manifest.UniqueID)
                .Select(storageConfig => storageConfig.Key)
                .ToList();
        }

        public bool TryGetStorage(string storageName, out IStorage storage)
        {
            if (_storageConfigs.TryGetValue(storageName, out var storageConfig))
            {
                storage = Storage.Clone(storageConfig);
                return true;
            }

            storage = null;
            return false;
        }

        public bool LoadContentPack(string path)
        {
            var temp = _helper.ContentPacks.CreateFake(path);
            var info = temp.ReadJsonFile<ContentPack>("content-pack.json");

            if (info == null)
            {
                _monitor.Log($"Cannot read content-data.json from {path}", LogLevel.Warn);
                return false;
            }

            var contentPack = _helper.ContentPacks.CreateTemporary(
                path,
                info.UniqueID,
                info.Name,
                info.Description,
                info.Author,
                new SemanticVersion(info.Version));

            return LoadContentPack(contentPack);
        }

        public bool LoadContentPack(IContentPack contentPack)
        {
            _monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);

            var expandedStorages = contentPack.ReadJsonFile<IDictionary<string, Storage>>("expanded-storage.json");
            var storageTabs = contentPack.ReadJsonFile<IDictionary<string, StorageTab>>("storage-tabs.json");

            if (expandedStorages == null)
            {
                _monitor.Log($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}");
                return false;
            }

            // Load expanded storages
            foreach (var expandedStorage in expandedStorages)
            {
                RegisterStorage(contentPack.Manifest, expandedStorage.Key, expandedStorage.Value);
            }

            // Generate file for Json Assets
            if (_jsonAssetsAPI != null && !expandedStorages.Keys.All(Storage.VanillaNames.Contains))
            {
                // Generate content-pack.json
                contentPack.WriteJsonFile("content-pack.json", new ContentPack
                {
                    Author = contentPack.Manifest.Author,
                    Description = contentPack.Manifest.Description,
                    Name = contentPack.Manifest.Name,
                    UniqueID = contentPack.Manifest.UniqueID,
                    UpdateKeys = contentPack.Manifest.UpdateKeys,
                    Version = contentPack.Manifest.Version.ToString()
                });

                _contentDirs.Add(contentPack.DirectoryPath);
            }

            if (storageTabs == null)
                return true;

            // Add asset to tab image dictionary
            StorageTab.AddTabImageLoader(contentPack.Manifest, contentPack.LoadAsset<Texture2D>);

            // Load expanded storage tabs
            foreach (var storageTab in storageTabs)
            {
                // Localized Tab Name
                storageTab.Value.TabName = contentPack.Translation.Get(storageTab.Key).Default(storageTab.Key);
                RegisterStorageTab(contentPack.Manifest, storageTab.Key, storageTab.Value);
            }

            return true;
        }

        public void SetStorageConfig(IManifest manifest, string storageName, IStorageConfig config)
        {
            if (!_storageConfigs.TryGetValue(storageName, out var storage) || storage.ModUniqueId != manifest.UniqueID)
            {
                _monitor.Log($"Unknown storage {storageName} in {manifest.UniqueID}.", LogLevel.Warn);
                return;
            }

            storage.CopyFrom(config);
            _monitor.Log($"{storageName} Config:\n{storage.SummaryReport}", LogLevel.Debug);
        }

        public void RegisterStorage(IManifest manifest, string storageName, IStorage storage)
        {
            // Skip duplicate storage configs
            if (_storageConfigs.TryGetValue(storageName, out var storageConfig) && storageConfig.ModUniqueId != manifest.UniqueID)
            {
                _monitor.Log($"Duplicate storage {storageName} in {manifest.UniqueID}.", LogLevel.Warn);
                return;
            }

            // Update existing storage
            if (storageConfig != null)
            {
                storageConfig.CopyFrom(storage);
                return;
            }

            // Add new storage
            storageConfig = new Storage(storageName);
            storageConfig.CopyFrom(storage);
            storageConfig.ModUniqueId = manifest.UniqueID;
            _storageConfigs.Add(storageName, storageConfig);
        }

        public void RegisterStorageTab(IManifest manifest, string tabName, IStorageTab storageTab)
        {
            var tabId = $"{manifest.UniqueID}/{tabName}";
            if (_tabConfigs.TryGetValue(tabId, out var tabConfig))
            {
                tabConfig.CopyFrom(storageTab);
                tabConfig.ModUniqueId = manifest.UniqueID;
            }
            else
            {
                tabConfig = StorageTab.Clone(storageTab);
                tabConfig.ModUniqueId = manifest.UniqueID;
                _tabConfigs.Add(tabId, tabConfig);
            }
        }

        /// <summary>Load Json Asset API</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _jsonAssetsAPI = _helper.ModRegistry.GetApi<IJsonAssetsAPI>("spacechase0.JsonAssets");
            if (_jsonAssetsAPI != null)
                _jsonAssetsAPI.IdsAssigned += OnIdsLoaded;
            else
                _monitor.Log("Json Assets not detected, Expanded Storages will not be loaded", LogLevel.Warn);
            _helper.Events.GameLoop.UpdateTicked += OnReadyToLoad;
        }

        /// <summary>Load Expanded Storage content packs.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReadyToLoad(object sender, UpdateTickedEventArgs e)
        {
            _helper.Events.GameLoop.UpdateTicked -= OnReadyToLoad;
            InvokeAll(ReadyToLoad);
            foreach (var contentDir in _contentDirs)
                _jsonAssetsAPI?.LoadAssets(contentDir);
            _isContentLoaded = true;
        }

        /// <summary>Load Json Assets Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnIdsLoaded(object sender, EventArgs e)
        {
            // Clear out old object ids
            foreach (var storageConfig in _storageConfigs
                .Where(config => config.Value.SourceType == SourceType.JsonAssets))
                storageConfig.Value.ObjectIds.Clear();

            // Add new object ids
            var bigCraftables = _jsonAssetsAPI.GetAllBigCraftableIds();
            foreach (var bigCraftable in bigCraftables)
            {
                if (!_storageConfigs.TryGetValue(bigCraftable.Key, out var storageConfig))
                    continue;
                storageConfig.SourceType = SourceType.JsonAssets;
                if (!storageConfig.ObjectIds.Contains(bigCraftable.Value))
                    storageConfig.ObjectIds.Add(bigCraftable.Value);
            }
        }

        /// <summary>Load Vanilla Asset Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_isContentLoaded)
                return;

            _helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            InvokeAll(StoragesLoaded);

            // Clear out old object ids
            foreach (var storageConfig in _storageConfigs
                .Where(config => config.Value.SourceType != SourceType.JsonAssets))
                storageConfig.Value.ObjectIds.Clear();

            var bigCraftables = Game1.bigCraftablesInformation.Where(Storage.IsVanillaStorage);
            foreach (var bigCraftable in bigCraftables)
            {
                var data = bigCraftable.Value.Split('/').ToArray();

                if (!_storageConfigs.TryGetValue(data[0], out var storageConfig))
                    continue;

                if (Storage.VanillaNames.Contains(data[0]))
                    storageConfig.SourceType = SourceType.Vanilla;
                else if (bigCraftable.Key >= 424000 && bigCraftable.Key < 425000)
                    storageConfig.SourceType = SourceType.CustomChestTypes;

                if (!storageConfig.ObjectIds.Contains(bigCraftable.Key))
                    storageConfig.ObjectIds.Add(bigCraftable.Key);
            }
        }

        private void InvokeAll(EventHandler eventHandler)
        {
            if (eventHandler == null)
                return;

            foreach (var @delegate in eventHandler.GetInvocationList()) @delegate.DynamicInvoke(this, null);
        }
    }
}