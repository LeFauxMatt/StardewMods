using System;
using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework
{
    internal class ContentLoader
    {
        private readonly IMonitor _monitor;
        private readonly IContentHelper _contentHelper;
        private readonly IEnumerable<IContentPack> _contentPacks;
        public bool IsOwnedLoaded;
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
            IGenericModConfigMenuAPI modConfigApi,
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
                var defaultConfigData = contentData.ExpandedStorage.Select(s => new StorageConfig(s)).ToList();
                var configData = contentPack.HasFile("config.json")
                    ? contentPack.ReadJsonFile<IList<StorageConfig>>("config.json")
                    : defaultConfigData;
                
                if (!contentPack.HasFile("config.json"))
                    contentPack.WriteJsonFile("config.json", configData);

                modConfigApi?.RegisterModConfig(contentPack.Manifest, RevertToDefault(contentPack, storageConfigs, defaultConfigData), SaveToFile(contentPack, storageConfigs));
                
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
                    
                    var config = configData.First(c => c.StorageName.Equals(content.StorageName, StringComparison.OrdinalIgnoreCase));
                    content.CopyFrom(config);
                    content.ModUniqueId = contentPack.Manifest.UniqueID;
                    storageConfigs.Add(content.StorageName, content);
                    
                    if (modConfigApi == null)
                        continue;

                    RegisterConfig(modConfigApi, contentPack.Manifest, content);
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
                        storageTab.ModUniqueId = contentPack.Manifest.UniqueID;
                        tabConfigs.Add(tabName, storageTab);
                    }
                }
            }

            IsOwnedLoaded = true;
        }

        internal IDictionary<int, string> LoadVanillaStorages(IDictionary<string, StorageContentData> storageConfigs, IDictionary<int, string> storageObjects)
        {
            var vanillaStorages = new Dictionary<int, string>();
            var vanillaNames = new[] {"Chest", "Stone Chest", "Mini-Fridge"};
            var chestObjects = Game1.bigCraftablesInformation
                .Select(obj => new KeyValuePair<int, string[]>(obj.Key, obj.Value.Split('/')))
                .Where(obj => vanillaNames.Contains(obj.Value[0]) || vanillaNames.Contains(obj.Value[8]));
            
            foreach (var obj in chestObjects)
            {
                // Default Config for Non-Recognized Storages
                if (!storageConfigs.ContainsKey(obj.Value[0]))
                    storageConfigs.Add(obj.Value[0], new StorageContentData()
                    {
                        StorageName = obj.Value[0],
                        Capacity = Chest.capacity,
                        CanCarry = true
                    });
                
                if (storageObjects.ContainsKey(obj.Key))
                    continue;
                
                storageObjects.Add(obj.Key, obj.Value[0]);
                vanillaStorages.Add(obj.Key, obj.Value[0]);
            }
            
            return vanillaStorages;
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
                var configData = storageConfigs
                    .Where(s => s.Value.ModUniqueId.Equals(contentPack.Manifest.UniqueID))
                    .Select(s => new StorageConfig(s.Value)).ToList();
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
        }
    }
}