using System;
using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.API;
using ExpandedStorage.Framework;
using ExpandedStorage.Framework.Integrations;
using ExpandedStorage.Framework.Models;
using ExpandedStorage.Framework.Patches;
using Microsoft.Xna.Framework.Graphics;
using MoreCraftables.API;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ExpandedStorage
{
    public class ExpandedStorageAPI : IExpandedStorageAPI
    {
        private static readonly HashSet<string> VanillaNames = new() {"Chest", "Stone Chest", "Mini-Fridge", "Junimo Chest", "Mini-Shipping Bin"};
        private readonly IList<string> _contentDirs = new List<string>();
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly IDictionary<string, Storage> _storageConfigs;
        private readonly IDictionary<string, StorageTab> _tabConfigs;

        private bool _isContentLoaded;
        private IGenericModConfigMenuAPI _modConfigAPI;
        private IMoreCraftablesAPI _moreCraftablesAPI;

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

        public event EventHandler ReadyToLoad;
        public event EventHandler StoragesLoaded;

        public void DisableWithModData(string modDataKey)
        {
            Storage.AddExclusion(modDataKey);
        }

        public void DisableDrawWithModData(string modDataKey)
        {
            ChestPatch.AddExclusion(modDataKey);
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

        public IStorageConfig GetStorageConfig(string storageName)
        {
            return _storageConfigs.TryGetValue(storageName, out var storage)
                ? storage
                : null;
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

            if (contentPack.HasFile("content-pack.json"))
                _contentDirs.Add(contentPack.DirectoryPath);

            var defaultConfig = expandedStorages.ToDictionary(
                s => s.Key,
                s => StorageConfig.Clone(s.Value));

            var playerConfig = contentPack.ReadJsonFile<Dictionary<string, StorageConfig>>("config.json");
            if (playerConfig == null)
            {
                playerConfig = defaultConfig.ToDictionary(
                    c => c.Key,
                    c => StorageConfig.Clone(c.Value));
                contentPack.WriteJsonFile("config.json", playerConfig);
            }

            _modConfigAPI?.RegisterModConfig(
                contentPack.Manifest,
                RevertToDefault(contentPack, defaultConfig),
                SaveToFile(contentPack));

            // Load expanded storage objects
            foreach (var expandedStorage in expandedStorages)
            {
                expandedStorage.Value.ModUniqueId = contentPack.Manifest.UniqueID;
                if (!RegisterStorage(expandedStorage.Key, expandedStorage.Value))
                    continue;

                // Generate default config
                if (!playerConfig.TryGetValue(expandedStorage.Key, out var storageConfig))
                {
                    storageConfig = StorageConfig.Clone(expandedStorage.Value);
                    playerConfig.Add(expandedStorage.Key, storageConfig);
                    contentPack.WriteJsonFile("config.json", playerConfig);
                }

                // Copy player config into storage content
                expandedStorage.Value.CopyFrom(storageConfig);
                _monitor.Log(expandedStorage.Value.SummaryReport, LogLevel.Debug);

                RegisterConfig(contentPack.Manifest, expandedStorage.Value, expandedStorage.Key);
            }

            if (storageTabs == null)
                return true;

            // Load expanded storage tabs
            foreach (var storageTab in storageTabs)
            {
                storageTab.Value.ModUniqueId = contentPack.Manifest.UniqueID;
                if (!RegisterStorageTab(storageTab.Key, storageTab.Value))
                    continue;

                // Localize Tab Name
                storageTab.Value.TabName = contentPack.Translation.Get(storageTab.Key).Default(storageTab.Key);

                // Assign Load Texture function
                storageTab.Value.LoadTexture = LoadTexture(contentPack, $"assets/{storageTab.Value.TabImage}");
            }

            return true;
        }

        /// <summary>Load Expanded Storage content packs</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _modConfigAPI = _helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            _moreCraftablesAPI = _helper.ModRegistry.GetApi<IMoreCraftablesAPI>("furyx639.MoreCraftables");
            _moreCraftablesAPI.ReadyToLoad += OnReadyToLoad;
            _moreCraftablesAPI.IdsLoaded += OnIdsLoaded;
        }

        /// <summary>Load More Craftables Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReadyToLoad(object sender, EventArgs e)
        {
            _monitor.Log("ReadyToLoad");
            InvokeAll(ReadyToLoad);
            foreach (var contentDir in _contentDirs)
                _moreCraftablesAPI.LoadContentPack(contentDir);
            _moreCraftablesAPI.AddHandledObject("furyx639.ExpandedStorage", new HandledObject());
            _isContentLoaded = true;
        }

        /// <summary>Load More Craftables Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnIdsLoaded(object sender, EventArgs e)
        {
            // Clear out old object ids
            foreach (var storageConfig in _storageConfigs
                .Where(config => config.Value.SourceType == SourceType.MoreCraftables))
                storageConfig.Value.ObjectIds.Clear();

            // Add new object ids
            var bigCraftables = _moreCraftablesAPI.GetAllBigCraftableIds();
            foreach (var bigCraftable in bigCraftables)
            {
                if (!_storageConfigs.TryGetValue(bigCraftable.Key, out var storageConfig))
                    continue;
                storageConfig.SourceType = SourceType.MoreCraftables;
                if (!storageConfig.ObjectIds.Contains(bigCraftable.Value))
                    storageConfig.ObjectIds.Add(bigCraftable.Value);
            }

            _monitor.Log("StoragesLoaded");
            InvokeAll(StoragesLoaded);
        }

        /// <summary>Load Vanilla Asset Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_isContentLoaded)
                return;

            _helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;

            if (!_storageConfigs.TryGetValue("Default", out var defaultConfig))
                defaultConfig = new Storage();

            // Clear out old object ids
            foreach (var storageConfig in _storageConfigs
                .Where(config => config.Value.SourceType != SourceType.MoreCraftables))
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
        }

        private Func<Texture2D> LoadTexture(IContentPack contentPack, string assetName)
        {
            return () => contentPack.HasFile(assetName)
                ? contentPack.LoadAsset<Texture2D>(assetName)
                : _helper.Content.Load<Texture2D>(assetName);
        }

        private bool RegisterStorage(string storageName, Storage storageContent)
        {
            // Skip duplicate storage configs
            if (_storageConfigs.ContainsKey(storageName))
            {
                _monitor.Log($"Duplicate storage {storageName} in {storageContent.ModUniqueId}.", LogLevel.Warn);
                return false;
            }

            _storageConfigs.Add(storageName, storageContent);
            return true;
        }

        private bool RegisterStorageTab(string tabName, StorageTab storageTab)
        {
            var tabId = $"{storageTab.ModUniqueId}/{tabName}";

            // Skip duplicate tab names
            if (_tabConfigs.ContainsKey(tabId))
            {
                _monitor.Log($"Duplicate tab {tabName} in {storageTab.ModUniqueId}", LogLevel.Warn);
                return false;
            }

            _tabConfigs.Add(tabId, storageTab);
            return true;
        }

        private void RegisterConfig(
            IManifest manifest,
            IStorageConfig config,
            string storageName)
        {
            _modConfigAPI?.RegisterLabel(manifest, storageName, "Added by Expanded Storage");
            _modConfigAPI?.RegisterSimpleOption(manifest, "Capacity", $"How many item slots should {storageName} have?",
                () => config.Capacity,
                value => config.Capacity = value);
            _modConfigAPI?.RegisterSimpleOption(manifest, "Can Carry", $"Allow {storageName} to be carried?",
                () => config.CanCarry,
                value => config.CanCarry = value);
            _modConfigAPI?.RegisterSimpleOption(manifest, "Access Carried", $"Allow {storageName} to be access while carried?",
                () => config.AccessCarried,
                value => config.AccessCarried = value);
            _modConfigAPI?.RegisterSimpleOption(manifest, "Search Bar", $"Show search bar above chest inventory for {storageName}?",
                () => config.ShowSearchBar,
                value => config.ShowSearchBar = value);
            _modConfigAPI?.RegisterSimpleOption(manifest, "Vacuum Items", $"Allow {storageName} to be collect debris?",
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