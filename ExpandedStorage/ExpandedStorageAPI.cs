using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandedStorage.API;
using ExpandedStorage.Framework.Integrations;
using ExpandedStorage.Framework.Models;
using ExpandedStorage.Framework.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ExpandedStorage
{
    public class ExpandedStorageAPI : IExpandedStorageAPI
    {
        private static readonly HashSet<string> VanillaNames = new() {"Chest", "Stone Chest", "Mini-Fridge", "Junimo Chest", "Mini-Shipping Bin"};
        private readonly IModHelper _helper;

        private readonly IMonitor _monitor;
        private readonly IDictionary<string, Storage> _storageConfigs;
        private readonly IDictionary<string, StorageTab> _tabConfigs;

        private bool _isContentLoaded;
        private IJsonAssetsAPI _jsonAssetsApi;
        private IGenericModConfigMenuAPI _modConfigApi;

        internal ExpandedStorageAPI(
            IMonitor monitor,
            IModHelper helper,
            IDictionary<string, Storage> storageConfigs,
            IDictionary<string, StorageTab> tabConfigs)
        {
            _monitor = monitor;
            _helper = helper;
            _storageConfigs = storageConfigs;
            _tabConfigs = tabConfigs;

            // Events
            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
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

        public IList<int> GetAllStorageIds()
        {
            return _storageConfigs.SelectMany(storageConfig => storageConfig.Value.ObjectIds).ToList();
        }

        public IStorage GetStorage(string storageName)
        {
            return _storageConfigs.TryGetValue(storageName, out var storage)
                ? storage
                : null;
        }

        public IStorage GetStorage(int sheetIndex)
        {
            return _storageConfigs
                .Select(storageData => storageData.Value)
                .FirstOrDefault(storageData => storageData.ObjectIds.Contains(sheetIndex));
        }

        public IStorageConfig GetStorageConfig(string storageName)
        {
            return _storageConfigs.TryGetValue(storageName, out var storage)
                ? storage
                : null;
        }

        public IStorageConfig GetStorageConfig(int sheetIndex)
        {
            return _storageConfigs
                .Select(storageData => storageData.Value)
                .FirstOrDefault(storageData => storageData.ObjectIds.Contains(sheetIndex));
        }

        public bool RegisterStorage(IStorage storage, IStorageConfig config = null)
        {
            if (!_storageConfigs.TryGetValue(storage.StorageName, out var storageConfig))
                return false;

            storageConfig.CopyFrom(storage);

            if (config != null)
                storageConfig.CopyFrom(config);

            return true;
        }

        public bool RegisterStorage(int sheetIndex, IStorage storage, IStorageConfig config = null)
        {
            var storageConfig = _storageConfigs
                .Select(storageData => storageData.Value)
                .FirstOrDefault(storageData => storageData.ObjectIds.Contains(sheetIndex));

            if (storageConfig == null)
                return false;

            storageConfig.CopyFrom(storage);

            if (config != null)
                storageConfig.CopyFrom(config);

            return true;
        }

        public bool UpdateStorageConfig(IStorage storage, IStorageConfig config)
        {
            if (!_storageConfigs.TryGetValue(storage.StorageName, out var storageConfig))
                return false;

            storageConfig.CopyFrom(config);
            return true;
        }

        public bool UpdateStorageConfig(int sheetIndex, IStorageConfig config)
        {
            var storageConfig = _storageConfigs
                .Select(storageData => storageData.Value)
                .FirstOrDefault(storageData => storageData.ObjectIds.Contains(sheetIndex));

            if (storageConfig == null)
                return false;

            storageConfig.CopyFrom(config);

            return true;
        }

        public bool LoadContentPack(string path)
        {
            var temp = _helper.ContentPacks.CreateFake(path);
            var info = temp.ReadJsonFile<IManifest>("content-pack.json");

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
                info.Version);

            return LoadContentPack(contentPack);
        }

        public bool LoadContentPack(IContentPack contentPack)
        {
            _monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
            var contentData = contentPack.ReadJsonFile<ContentData>("expandedStorage.json");

            if (contentData?.ExpandedStorage == null)
            {
                _monitor.Log($"Cannot load {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                return false;
            }

            var defaultConfig =
                contentData.ExpandedStorage
                    .ToDictionary(config => config.StorageName, StorageConfig.Clone);

            Dictionary<string, StorageConfig> playerConfig;
            try
            {
                playerConfig = contentPack.ReadJsonFile<Dictionary<string, StorageConfig>>("config.json");
            }
            catch (Exception)
            {
                playerConfig = null;
            }

            if (playerConfig == null)
                try
                {
                    var legacyConfig = contentPack.ReadJsonFile<IList<Storage>>("config.json");
                    if (legacyConfig != null)
                    {
                        playerConfig = legacyConfig.ToDictionary(c => c.StorageName, StorageConfig.Clone);
                        contentPack.WriteJsonFile("config.json", playerConfig);
                    }
                }
                catch (Exception)
                {
                    playerConfig = null;
                }

            if (playerConfig == null)
            {
                playerConfig = defaultConfig.ToDictionary(
                    config => config.Key,
                    config => StorageConfig.Clone(config.Value));
                contentPack.WriteJsonFile("config.json", playerConfig);
            }

            _modConfigApi?.RegisterModConfig(
                contentPack.Manifest,
                RevertToDefault(contentPack, defaultConfig),
                SaveToFile(contentPack));

            // Load expanded storage objects
            foreach (var storageContent in contentData.ExpandedStorage.Where(storageContent => !string.IsNullOrWhiteSpace(storageContent.StorageName)))
            {
                storageContent.ModUniqueId = contentPack.Manifest.UniqueID;
                if (!RegisterStorage(storageContent))
                    continue;

                // Generate default config
                if (!playerConfig.TryGetValue(storageContent.StorageName, out var storageConfig))
                {
                    storageConfig = StorageConfig.Clone(storageContent);
                    playerConfig.Add(storageContent.StorageName, storageConfig);
                    contentPack.WriteJsonFile("config.json", playerConfig);
                }

                // Copy player config into storage content
                storageContent.CopyFrom(storageConfig);
                _monitor.Log(storageContent.SummaryReport, LogLevel.Debug);

                RegisterConfig(contentPack.Manifest, storageContent, storageContent.StorageName);
            }

            if (contentData.StorageTabs == null)
                return true;

            // Load expanded storage tabs
            foreach (var storageTab in contentData.StorageTabs.Where(t => !string.IsNullOrWhiteSpace(t.TabName) && !string.IsNullOrWhiteSpace(t.TabImage)))
            {
                storageTab.ModUniqueId = contentPack.Manifest.UniqueID;
                if (!RegisterStorageTab(storageTab))
                    continue;

                // Localize Tab Name
                storageTab.TabName = contentPack.Translation.Get(storageTab.TabName).Default(storageTab.TabName);

                // Assign Load Texture function
                storageTab.LoadTexture = LoadTexture(contentPack, $"assets/{storageTab.TabImage}");
            }

            return true;
        }

        /// <summary>Load Expanded Storage content packs</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _modConfigApi = _helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            _jsonAssetsApi = _helper.ModRegistry.GetApi<IJsonAssetsAPI>("spacechase0.JsonAssets");

            if (_jsonAssetsApi != null)
                _jsonAssetsApi.IdsAssigned += OnIdsAssigned;

            InvokeAll(ReadyToLoad);
            _isContentLoaded = true;
        }

        /// <summary>Load Json Asset Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnIdsAssigned(object sender, EventArgs e)
        {
            // Clear out old object ids
            foreach (var storageConfig in _storageConfigs
                .Where(config => config.Value.SourceType == SourceType.JsonAssets))
                storageConfig.Value.ObjectIds.Clear();

            // Add new object ids
            foreach (var bigCraftable in _jsonAssetsApi
                .GetAllBigCraftableIds()
                .Where(obj => _storageConfigs.ContainsKey(obj.Key)))
            {
                if (!_storageConfigs.TryGetValue(bigCraftable.Key, out var storageConfig))
                    continue;
                storageConfig.SourceType = SourceType.JsonAssets;

                if (!storageConfig.ObjectIds.Contains(bigCraftable.Value))
                    storageConfig.ObjectIds.Add(bigCraftable.Value);
            }

            InvokeAll(StoragesLoaded);
        }

        /// <summary>Load Vanilla Asset Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnAssetsLoaded(object sender, UpdateTickedEventArgs e)
        {
            if (!_isContentLoaded)
                return;

            _helper.Events.GameLoop.UpdateTicked -= OnAssetsLoaded;

            _monitor.Log("Loading default storage config");
            var defaultConfig = _helper.Data.ReadJsonFile<Storage>("expandedStorage.json") ?? new Storage();

            if (!File.Exists(Path.Combine(_helper.DirectoryPath, "expandedStorage.json")))
                _helper.Data.WriteJsonFile("expandedStorage.json", defaultConfig);

            // Clear out old object ids
            foreach (var storageConfig in _storageConfigs
                .Where(config => config.Value.SourceType != SourceType.JsonAssets))
                storageConfig.Value.ObjectIds.Clear();

            foreach (var obj in Game1.bigCraftablesInformation
                .ToDictionary(obj => obj.Key, obj => obj.Value.Split('/').ToArray())
                .Where(obj => obj.Value.Length == 9 && obj.Value[8] == "Chest" || VanillaNames.Contains(obj.Value[0])))
            {
                // Generate default config for non-recognized storages
                if (!_storageConfigs.TryGetValue(obj.Value[0], out var storageConfig))
                {
                    _monitor.Log($"Generating default config for {obj.Value[0]}.");
                    storageConfig = new Storage(obj.Value[0]);
                    storageConfig.CopyFrom(defaultConfig);
                    _storageConfigs.Add(obj.Value[0], storageConfig);
                }

                if (VanillaNames.Contains(obj.Value[0]))
                    storageConfig.SourceType = SourceType.Vanilla;
                else if (obj.Key >= 424000 && obj.Key < 425000)
                    storageConfig.SourceType = SourceType.CustomChestTypes;

                if (!storageConfig.ObjectIds.Contains(obj.Key))
                    storageConfig.ObjectIds.Add(obj.Key);
            }

            InvokeAll(StoragesLoaded);
        }

        private Func<Texture2D> LoadTexture(IContentPack contentPack, string assetName)
        {
            return () => contentPack.HasFile(assetName)
                ? contentPack.LoadAsset<Texture2D>(assetName)
                : _helper.Content.Load<Texture2D>(assetName);
        }

        private bool RegisterStorage(Storage storageContent)
        {
            // Skip duplicate storage configs
            if (_storageConfigs.ContainsKey(storageContent.StorageName))
            {
                _monitor.Log($"Duplicate storage {storageContent.StorageName} in {storageContent.ModUniqueId}.", LogLevel.Warn);
                return false;
            }

            _storageConfigs.Add(storageContent.StorageName, storageContent);
            return true;
        }

        private bool RegisterStorageTab(StorageTab storageTab)
        {
            var tabName = $"{storageTab.ModUniqueId}/{storageTab.TabName}";

            // Skip duplicate tab names
            if (_tabConfigs.ContainsKey(tabName))
            {
                _monitor.Log($"Duplicate tab {storageTab.TabName} in {storageTab.ModUniqueId}", LogLevel.Warn);
                return false;
            }

            _tabConfigs.Add(tabName, storageTab);
            return true;
        }

        private void RegisterConfig(
            IManifest manifest,
            IStorageConfig config,
            string storageName)
        {
            _modConfigApi?.RegisterLabel(manifest, storageName, "Added by Expanded Storage");
            _modConfigApi?.RegisterSimpleOption(manifest, "Capacity", $"How many item slots should {storageName} have?",
                () => config.Capacity,
                value => config.Capacity = value);
            _modConfigApi?.RegisterSimpleOption(manifest, "Can Carry", $"Allow {storageName} to be carried?",
                () => config.CanCarry,
                value => config.CanCarry = value);
            _modConfigApi?.RegisterSimpleOption(manifest, "Access Carried", $"Allow {storageName} to be access while carried?",
                () => config.AccessCarried,
                value => config.AccessCarried = value);
            _modConfigApi?.RegisterSimpleOption(manifest, "Search Bar", $"Show search bar above chest inventory for {storageName}?",
                () => config.ShowSearchBar,
                value => config.ShowSearchBar = value);
            _modConfigApi?.RegisterSimpleOption(manifest, "Vacuum Items", $"Allow {storageName} to be collect debris?",
                () => config.VacuumItems,
                value => config.VacuumItems = value);
        }

        private Action RevertToDefault(IContentPack contentPack, IDictionary<string, StorageConfig> defaultConfig)
        {
            return () =>
            {
                foreach (var defaultValue in defaultConfig)
                    if (_storageConfigs.TryGetValue(defaultValue.Key, out var storageConfig))
                        storageConfig.CopyFrom(defaultValue.Value);

                SaveToFile(contentPack).Invoke();
            };
        }

        private Action SaveToFile(IContentPack contentPack)
        {
            return () => contentPack.WriteJsonFile("config.json",
                _storageConfigs
                    .Where(c => c.Value.ModUniqueId == contentPack.Manifest.UniqueID)
                    .ToDictionary(c => c.Key, c => StorageConfig.Clone(c.Value)));
        }

        private void InvokeAll(EventHandler eventHandler)
        {
            if (eventHandler == null)
                return;

            foreach (var @delegate in eventHandler.GetInvocationList()) @delegate.DynamicInvoke(this, null);
        }
    }
}