using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ExpandedStorage.Framework
{
    internal class ContentLoader
    {
        private static readonly HashSet<string> VanillaNames = new() { "Chest", "Stone Chest", "Mini-Fridge", "Junimo Chest", "Mini-Shipping Bin" };
        
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;
        private readonly IEnumerable<IContentPack> _contentPacks;
        private readonly IDictionary<string, StorageContentData> _storageConfigs;
        private readonly IDictionary<string, TabContentData> _tabConfigs;
        
        private IGenericModConfigMenuAPI _modConfigApi;
        private IJsonAssetsApi _jsonAssetsApi;
        private bool IsContentLoaded { get; set; }
        internal ContentLoader(
                IMonitor monitor,
                IModHelper helper,
                IDictionary<string, StorageContentData> storageConfigs,
                IDictionary<string, TabContentData> tabConfigs
        )
        {
            _monitor = monitor;
            _helper = helper;
            _contentPacks = helper.ContentPacks.GetOwned();
            _storageConfigs = storageConfigs;
            _tabConfigs = tabConfigs;

            // Events
            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        /// <summary>Load Expanded Storage content packs</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _modConfigApi = _helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            _jsonAssetsApi = _helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            
            if (_jsonAssetsApi != null)
                _jsonAssetsApi.IdsAssigned += OnIdsAssigned;
            
            _monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            _storageConfigs.Clear();
            
            foreach (var contentPack in _contentPacks)
            {
                if (!contentPack.HasFile("expandedStorage.json"))
                {
                    _monitor.Log($"Cannot load {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                    continue;
                }
                
                _monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
                var contentData = contentPack.ReadJsonFile<ContentData>("expandedStorage.json");
                
                var defaultConfig = contentData.ExpandedStorage
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
                {
                    try
                    {
                        var legacyConfig = contentPack.ReadJsonFile<IList<StorageContentData>>("config.json");
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
                }

                if (playerConfig == null)
                {
                    playerConfig = defaultConfig;
                    contentPack.WriteJsonFile("config.json", playerConfig);
                }

                _modConfigApi?.RegisterModConfig(
                    contentPack.Manifest,
                    RevertToDefault(contentPack, defaultConfig),
                    SaveToFile(contentPack));
                
                // Load expanded storage objects
                foreach (var storageContent in contentData.ExpandedStorage.Where(storageContent => !string.IsNullOrWhiteSpace(storageContent.StorageName)))
                {
                    // Skip duplicate storage configs
                    if (_storageConfigs.ContainsKey(storageContent.StorageName))
                    {
                        _monitor.Log($"Duplicate storage {storageContent.StorageName} found in {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                        continue;
                    }

                    // Generate default config
                    if (!playerConfig.TryGetValue(storageContent.StorageName, out var storageConfig))
                    {
                        storageConfig = StorageConfig.Clone(storageContent);
                        playerConfig.Add(storageContent.StorageName, storageConfig);
                        contentPack.WriteJsonFile("config.json", playerConfig);
                    }

                    // Generate player instance of storage content
                    var playerContent = new StorageContentData
                    {
                        StorageName = storageContent.StorageName,
                        ModUniqueId = contentPack.Manifest.UniqueID,
                        SourceType = storageContent.SourceType,
                        OpenSound = storageContent.OpenSound,
                        SpecialChestType = storageContent.SpecialChestType,
                        IsFridge = storageContent.IsFridge,
                        IsPlaceable = storageContent.IsPlaceable,
                        ModData = storageContent.ModData,
                        AllowList = storageContent.AllowList,
                        BlockList = storageContent.BlockList,
                        Tabs = storageContent.Tabs
                    };
                    playerContent.CopyFrom(storageConfig);
                    _storageConfigs.Add(storageContent.StorageName, playerContent);
                    RegisterConfig(contentPack.Manifest, playerContent, storageContent.StorageName);
                    
                    _monitor.Log(playerContent.SummaryReport, LogLevel.Debug);
                }
                
                // Load expanded storage tabs
                foreach (var storageTab in contentData.StorageTabs.Where(t => !string.IsNullOrWhiteSpace(t.TabName) && !string.IsNullOrWhiteSpace(t.TabImage)))
                {
                    var tabName = $"{contentPack.Manifest.UniqueID}/{storageTab.TabName}";
                    var assetName = $"assets/{storageTab.TabImage}";
                    
                    // Skip duplicate tab names
                    if (_tabConfigs.ContainsKey(tabName))
                    {
                        _monitor.Log($"Duplicate tab {storageTab.TabName} found in {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                        continue;
                    }
                    
                    storageTab.ModUniqueId = contentPack.Manifest.UniqueID;
                    storageTab.TabName = contentPack.Translation.Get(storageTab.TabName).Default(storageTab.TabName);
                    storageTab.Texture = contentPack.HasFile(assetName)
                        ? contentPack.LoadAsset<Texture2D>(assetName)
                        : _helper.Content.Load<Texture2D>(assetName);
                    
                    _tabConfigs.Add(tabName, storageTab);
                }
            }
            IsContentLoaded = true;
        }
        
        /// <summary>Load Json Asset Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnIdsAssigned(object sender, EventArgs e)
        {
            // Clear out old object ids
            foreach (var storageConfig in _storageConfigs
                .Where(config => config.Value.SourceType == SourceType.JsonAssets))
            {
                storageConfig.Value.ObjectIds.Clear();
            }
            
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
        }

        /// <summary>Load Vanilla Asset Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnAssetsLoaded(object sender, UpdateTickedEventArgs e)
        {
            if (!IsContentLoaded)
                return;
            
            _helper.Events.GameLoop.UpdateTicked -= OnAssetsLoaded;
            
            _monitor.Log("Loading default storage config");
            var defaultConfig = _helper.Data.ReadJsonFile<StorageContentData>("expandedStorage.json") ?? new StorageContentData();

            if (!File.Exists(Path.Combine(_helper.DirectoryPath, "expandedStorage.json")))
                _helper.Data.WriteJsonFile("expandedStorage.json", defaultConfig);
            
            // Clear out old object ids
            foreach (var storageConfig in _storageConfigs
                .Where(config => config.Value.SourceType != SourceType.JsonAssets))
            {
                storageConfig.Value.ObjectIds.Clear();
            }
            
            foreach (var obj in Game1.bigCraftablesInformation
                .ToDictionary(obj => obj.Key, obj => obj.Value.Split('/').ToArray())
                .Where(obj => obj.Value.Length == 9 && obj.Value[8] == "Chest" || VanillaNames.Contains(obj.Value[0])))
            {
                // Generate default config for non-recognized storages
                if (!_storageConfigs.TryGetValue(obj.Value[0], out var storageConfig))
                {
                    _monitor.Log($"Generating default config for {obj.Value[0]}.");
                    storageConfig = new StorageContentData(obj.Value[0]);
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

        private void RegisterConfig(
            IManifest manifest,
            StorageConfig playerConfig,
            string storageName)
        {
            _modConfigApi?.RegisterLabel(manifest, storageName, "Added by Expanded Storage");
            _modConfigApi?.RegisterSimpleOption(manifest, "Capacity", $"How many item slots should {storageName} have?",
                () => playerConfig.Capacity,
                value => playerConfig.Capacity = value);
            _modConfigApi?.RegisterSimpleOption(manifest, "Can Carry", $"Allow {storageName} to be carried?",
                () => playerConfig.CanCarry,
                value => playerConfig.CanCarry = value);
            _modConfigApi?.RegisterSimpleOption(manifest, "Access Carried", $"Allow {storageName} to be access while carried?",
                () => playerConfig.AccessCarried,
                value => playerConfig.AccessCarried = value);
            _modConfigApi?.RegisterSimpleOption(manifest, "Search Bar", $"Show search bar above chest inventory for {storageName}?",
                () => playerConfig.ShowSearchBar,
                value => playerConfig.ShowSearchBar = value);
            _modConfigApi?.RegisterSimpleOption(manifest, "Vacuum Items", $"Allow {storageName} to be collect debris?",
                () => playerConfig.VacuumItems,
                value => playerConfig.VacuumItems = value);
        }
        
        private Action RevertToDefault(IContentPack contentPack, IDictionary<string, StorageConfig> defaultConfig) =>
            () =>
            {
                foreach (var defaultValue in defaultConfig)
                {
                    if (_storageConfigs.TryGetValue(defaultValue.Key, out var storageConfig))
                        storageConfig.CopyFrom(defaultValue.Value);
                }
                SaveToFile(contentPack).Invoke();
            };
        
        private Action SaveToFile(IContentPack contentPack) =>
            () => contentPack.WriteJsonFile("config.json",
                _storageConfigs
                    .Where(c => c.Value.ModUniqueId == contentPack.Manifest.UniqueID)
                    .ToDictionary(c => c.Key, c => StorageConfig.Clone(c.Value)));
    }
}