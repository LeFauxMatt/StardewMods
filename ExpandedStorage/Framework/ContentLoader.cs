using System;
using System.Collections.Generic;
using System.Linq;
using ImJustMatt.ExpandedStorage.API;
using ImJustMatt.ExpandedStorage.Framework.Integrations;
using ImJustMatt.ExpandedStorage.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ImJustMatt.ExpandedStorage.Framework
{
    internal class ContentLoader
    {
        private readonly ModConfig _config;
        private readonly IExpandedStorageAPI _expandedStorageAPI;
        private readonly IModHelper _helper;
        private readonly IManifest _manifest;
        private readonly IMonitor _monitor;

        private IGenericModConfigMenuAPI _modConfigAPI;

        internal ContentLoader(IModHelper helper,
            IManifest manifest,
            IMonitor monitor,
            ModConfig config,
            IExpandedStorageAPI expandedStorageAPI)
        {
            _helper = helper;
            _manifest = manifest;
            _monitor = monitor;
            _config = config;

            _expandedStorageAPI = expandedStorageAPI;

            // Default Exclusions
            _expandedStorageAPI.DisableWithModData("aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest");
            _expandedStorageAPI.DisableDrawWithModData("aedenthorn.CustomChestTypes/IsCustomChest");

            // Events
            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            _expandedStorageAPI.ReadyToLoad += OnReadyToLoad;
            _expandedStorageAPI.StoragesLoaded += OnStoragesLoaded;
        }

        /// <summary>Load Expanded Storage content packs</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _modConfigAPI = _helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
        }

        /// <summary>Load Expanded Storage Content Packs.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReadyToLoad(object sender, EventArgs e)
        {
            var contentPacks = _helper.ContentPacks.GetOwned();
            _monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (var contentPack in contentPacks)
            {
                _expandedStorageAPI.LoadContentPack(contentPack);

                var storageNames = _expandedStorageAPI.GetOwnedStorages(contentPack.Manifest);
                var playerConfigs = contentPack.ReadJsonFile<Dictionary<string, StorageConfig>>("config.json")
                                    ?? new Dictionary<string, StorageConfig>();
                var defaultConfigs = new Dictionary<string, StorageConfig>();

                var revertToDefault = GetRevertToDefault(playerConfigs, defaultConfigs);
                var saveToFile = GetSaveToFile(contentPack, playerConfigs);
                _modConfigAPI?.RegisterModConfig(contentPack.Manifest, revertToDefault, saveToFile);

                // Load player config for storages
                foreach (var storageName in storageNames)
                {
                    if (!_expandedStorageAPI.TryGetStorage(storageName, out var config))
                        continue;

                    var defaultConfig = StorageConfig.Clone(config);
                    defaultConfigs.Add(storageName, defaultConfig);

                    if (!playerConfigs.TryGetValue(storageName, out var playerConfig))
                    {
                        // Generate default player config
                        playerConfig = StorageConfig.Clone(config);
                        playerConfigs.Add(storageName, playerConfig);
                    }

                    _expandedStorageAPI.SetStorageConfig(contentPack.Manifest, storageName, playerConfig);
                    RegisterConfig(contentPack.Manifest, storageName, playerConfig);
                }

                saveToFile.Invoke();
            }

            // Load Default Tabs
            foreach (var storageTab in _config.DefaultTabs)
            {
                // Localized Tab Name
                storageTab.Value.TabName = _helper.Translation.Get(storageTab.Key).Default(storageTab.Key);
                _expandedStorageAPI.RegisterStorageTab(_manifest, storageTab.Key, storageTab.Value);
            }
        }

        /// <summary>Load Vanilla Storages with default config.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnStoragesLoaded(object sender, EventArgs e)
        {
            var expandedStorages = _expandedStorageAPI.GetAllStorages();
            var bigCraftables = Game1.bigCraftablesInformation.Where(Storage.IsVanillaStorage);
            foreach (var bigCraftable in bigCraftables)
            {
                var data = bigCraftable.Value.Split('/').ToArray();
                if (expandedStorages.Any(data[0].Equals))
                    continue;
                _expandedStorageAPI.RegisterStorage(_manifest, data[0], _config.DefaultStorage);
                _expandedStorageAPI.SetStorageConfig(_manifest, data[0], _config.DefaultStorage);
            }
        }

        internal void ReloadDefaultStorageConfigs()
        {
            var storageNames = _expandedStorageAPI.GetOwnedStorages(_manifest);
            foreach (var storageName in storageNames)
            {
                _expandedStorageAPI.SetStorageConfig(_manifest, storageName, _config.DefaultStorage);
            }
        }

        private static Action GetRevertToDefault(IDictionary<string, StorageConfig> playerConfigs, IDictionary<string, StorageConfig> defaultConfigs)
        {
            void RevertToDefault()
            {
                foreach (var defaultConfig in defaultConfigs)
                    if (playerConfigs.TryGetValue(defaultConfig.Key, out var playerConfig))
                        playerConfig.CopyFrom(defaultConfig.Value);
            }

            return RevertToDefault;
        }

        private Action GetSaveToFile(IContentPack contentPack, IDictionary<string, StorageConfig> playerConfigs)
        {
            void SaveToFile()
            {
                foreach (var playerConfig in playerConfigs)
                    _expandedStorageAPI.SetStorageConfig(contentPack.Manifest, playerConfig.Key, playerConfig.Value);
                contentPack.WriteJsonFile("config.json", playerConfigs);
            }

            return SaveToFile;
        }

        private void RegisterConfig(IManifest manifest, string storageName, IStorageConfig config)
        {
            _modConfigAPI?.RegisterLabel(manifest, storageName, manifest.Description);
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
            _modConfigAPI?.RegisterSimpleOption(manifest, "Vacuum Items", $"Allow {storageName} to collect debris?",
                () => config.VacuumItems,
                value => config.VacuumItems = value);
        }
    }
}