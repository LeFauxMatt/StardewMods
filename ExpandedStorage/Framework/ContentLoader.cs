using System;
using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ExpandedStorage.Framework
{
    internal class ContentLoader
    {
        private static readonly HashSet<string> VanillaNames = new() { "Chest", "Stone Chest", "Mini-Fridge", "Junimo Chest", "Mini-Shipping Bin" };
        
        private readonly IMonitor _monitor;
        private readonly IContentHelper _contentHelper;
        private readonly IEnumerable<IContentPack> _contentPacks;

        internal bool IsOwnedLoaded { get; private set; }
        internal bool IsVanillaLoaded { get; private set; }
        internal ContentLoader(
            IMonitor monitor,
            IContentHelper contentHelper,
            IEnumerable<IContentPack> contentPacks)
        {
            _monitor = monitor;
            _contentHelper = contentHelper;
            _contentPacks = contentPacks;
        }

        /// <summary>Load Expanded Storage content packs</summary>
        internal void LoadOwnedStorages(
            IGenericModConfigMenuAPI api,
            IDictionary<string, StorageContentData> storageConfigs,
            IDictionary<string, TabContentData> tabConfigs)
        {
            _monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            storageConfigs.Clear();
            
            foreach (var contentPack in _contentPacks)
            {
                if (!contentPack.HasFile("expandedStorage.json"))
                {
                    _monitor.Log($"Cannot load {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                    continue;
                }
                
                _monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
                var contentData = contentPack.ReadJsonFile<ContentData>("expandedStorage.json");
                var defaultConfigData = contentData.ExpandedStorage.Select(StorageConfig.Clone).ToList();
                var configData = contentPack.HasFile("config.json")
                    ? contentPack.ReadJsonFile<IList<StorageConfig>>("config.json")
                    : defaultConfigData;
                
                if (!contentPack.HasFile("config.json"))
                    contentPack.WriteJsonFile("config.json", configData);

                api?.RegisterModConfig(contentPack.Manifest, RevertToDefault(contentPack, storageConfigs, defaultConfigData), SaveToFile(contentPack, storageConfigs));
                
                // Load expanded storage objects
                foreach (var content in contentData.ExpandedStorage)
                {
                    if (string.IsNullOrWhiteSpace(content.StorageName))
                        continue;

                    if (storageConfigs.Any(c => c.Value.StorageName.Equals(content.StorageName, StringComparison.OrdinalIgnoreCase)))
                    {
                        _monitor.Log($"Duplicate storage {content.StorageName} found in {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                        continue;
                    }
                    
                    var config = configData.FirstOrDefault(c => c.StorageName.Equals(content.StorageName, StringComparison.OrdinalIgnoreCase));
                    if (config == null)
                    {
                        config = StorageConfig.Clone(content);
                        configData.Add(config);
                        contentPack.WriteJsonFile("config.json", configData);
                    }
                    content.CopyFrom(config);
                    
                    content.ModUniqueId = contentPack.Manifest.UniqueID;
                    storageConfigs.Add(content.StorageName, content);
                    
                    if (api != null)
                        RegisterConfig(api, contentPack.Manifest, content);
                }
                
                // Load expanded storage tabs
                foreach (var storageTab in contentData.StorageTabs
                    .Where(t => !string.IsNullOrWhiteSpace(t.TabName) && !string.IsNullOrWhiteSpace(t.TabImage)))
                {
                    var tabName = $"{contentPack.Manifest.UniqueID}/{storageTab.TabName}";
                    var assetName = $"assets/{storageTab.TabImage}";
                    if (tabConfigs.ContainsKey(tabName))
                    {
                        _monitor.Log($"Duplicate tab {storageTab.TabName} found in {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                    }
                    else
                    {
                        storageTab.Texture = contentPack.HasFile(assetName)
                            ? contentPack.LoadAsset<Texture2D>(assetName)
                            : _contentHelper.Load<Texture2D>(assetName);
                        var tabNameLocale = contentPack.Translation.Get(storageTab.TabName).Default(storageTab.TabName);
                        storageTab.TabName = tabNameLocale;
                        storageTab.ModUniqueId = contentPack.Manifest.UniqueID;
                        tabConfigs.Add(tabName, storageTab);
                    }
                }
            }
            IsOwnedLoaded = true;
        }

        internal void LoadVanillaStorages(IDictionary<string, StorageContentData> storageConfigs, IDictionary<int, string> storageObjects)
        {
            foreach (var obj in Game1.bigCraftablesInformation)
            {
                var objData = obj.Value.Split('/').ToList();
                var storageName = objData.ElementAtOrDefault(0);
                var displayName = objData.ElementAtOrDefault(8);
                if (storageName == null
                    || displayName != "Chest"
                    && !VanillaNames.Contains(storageName))
                    continue;
                
                // Generate default config for non-recognized storages
                if (!storageConfigs.TryGetValue(storageName, out var storageConfig))
                {
                    _monitor.Log($"Generating default config for {storageName}.");
                    storageConfig = new StorageContentData(storageName);
                    storageConfigs.Add(objData.ElementAt(0), storageConfig);
                }
                
                storageConfig.IsVanilla = true;
                if (storageObjects.ContainsKey(obj.Key))
                    continue;
                
                _monitor.Log($"Loading {storageName} as a vanilla storage object.");
                storageObjects.Add(obj.Key, storageName);
            }
            IsVanillaLoaded = true;
        }
        
        private static Action RevertToDefault(IContentPack contentPack, IDictionary<string, StorageContentData> storageConfigs, List<StorageConfig> defaultConfigData) =>
            () =>
            {
                foreach (var content in storageConfigs)
                {
                    var config = defaultConfigData.First(c => c.StorageName.Equals(content.Key, StringComparison.OrdinalIgnoreCase));
                    if (config != null)
                        content.Value.CopyFrom(config);
                }
                SaveToFile(contentPack, storageConfigs).Invoke();
            };
        private static Action SaveToFile(IContentPack contentPack, IDictionary<string, StorageContentData> storageConfigs) =>
            () =>
            {
                var configData = storageConfigs.Where(s =>
                    s.Value.ModUniqueId != null
                    && s.Value.ModUniqueId.Equals(contentPack.Manifest.UniqueID, StringComparison.OrdinalIgnoreCase))
                    .Select(s => StorageConfig.Clone(s.Value))
                    .ToList();
                contentPack.WriteJsonFile("config.json", configData);
            };
        private static void RegisterConfig(
            IGenericModConfigMenuAPI api,
            IManifest manifest,
            StorageConfig content)
        {
            api.RegisterLabel(manifest, content.StorageName, "Added by Expanded Storage");
            api.RegisterSimpleOption(manifest, "Capacity", $"How many item slots should {content.StorageName} have?",
                () => content.Capacity,
                value => content.Capacity = value);
            api.RegisterSimpleOption(manifest, "Can Carry", $"Allow {content.StorageName} to be carried?",
                () => content.CanCarry,
                value => content.CanCarry = value);
            api.RegisterSimpleOption(manifest, "Access Carried", $"Allow {content.StorageName} to be access while carried?",
                () => content.AccessCarried,
                value => content.AccessCarried = value);
            api.RegisterSimpleOption(manifest, "Show Search Bar", $"Show search bar above chest inventory for {content.StorageName}?",
                () => content.ShowSearchBar,
                value => content.ShowSearchBar = value);
            api.RegisterSimpleOption(manifest, "Is Placeable", $"Allow {content.StorageName} to be placed?",
                () => content.IsPlaceable,
                value => content.IsPlaceable = value);
        }
    }
}