using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Extensions;
using Common.Integrations.DynamicGameAssets;
using Common.Integrations.GenericModConfigMenu;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using Common.Integrations.XSLite;
using Common.Integrations.XSPlus;

namespace XSLite
{
    public class XSLiteAPI : IXSLiteAPI
    {
        private static readonly HashSet<string> VanillaNames = new()
        {
            "Chest",
            "Stone Chest",
            "Junimo Chest",
            "Mini-Shipping Bin",
            "Mini-Fridge",
            "Auto-Grabber"
        };
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly DynamicGameAssetsIntegration _dynamicAssets;
        private readonly GenericModConfigMenuIntegration _modConfigMenu;
        private readonly XSPlusIntegration _xsPlus;
        internal XSLiteAPI(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _dynamicAssets = new DynamicGameAssetsIntegration(helper.ModRegistry);
            _modConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);
            _xsPlus = new XSPlusIntegration(helper.ModRegistry);
        }
        public bool LoadContentPack(IManifest manifest, string path)
        {
            var contentPack = _helper.ContentPacks.CreateTemporary(
                path,
                manifest.UniqueID,
                manifest.Name,
                manifest.Description,
                manifest.Author,
                manifest.Version);
            return LoadContentPack(contentPack);
        }
        
        public bool LoadContentPack(IContentPack contentPack)
        {
            _monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
            
            var storages = contentPack.ReadJsonFile<IDictionary<string, Storage>>("expanded-storage.json");
            
            if (storages == null)
            {
                _monitor.Log($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                return false;
            }
            
            // Remove any duplicate storages
            foreach (var storage in storages.Where(storage => XSLite.Storages.ContainsKey(storage.Key)))
            {
                _monitor.Log($"Duplicate storage {storage.Key} in {contentPack.Manifest.UniqueID}.", LogLevel.Warn);
                storages.Remove(storage.Key);
            }
            
            if (storages.Count == 0)
            {
                _monitor.Log($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                return false;
            }
            
            // Setup GMCM for Content Pack
            Dictionary<string, ModConfig> config = null;
            if (storages.Any(storage => storage.Value.PlayerConfig))
            {
                if (_modConfigMenu.IsLoaded)
                {
                    void RevertToDefault()
                    {
                        foreach (var storage in storages)
                        {
                            storage.Value.Config.Capacity = storage.Value.Capacity;
                            storage.Value.Config.EnabledFeatures = storage.Value.EnabledFeatures;
                        }
                    }
                    void SaveToFile()
                    {
                        contentPack.WriteJsonFile("config.json", storages.ToDictionary(
                            storage => storage.Key,
                            storage => storage.Value.Config
                        ));
                    }
                    _modConfigMenu.API.RegisterModConfig(
                        mod: contentPack.Manifest,
                        revertToDefault: RevertToDefault,
                        saveToFile: SaveToFile
                    );
                    _modConfigMenu.API.SetDefaultIngameOptinValue(
                        mod: contentPack.Manifest,
                        optedIn: true
                    );
                    // Add a page for each storage
                    foreach (var storage in storages)
                    {
                        _modConfigMenu.API.RegisterPageLabel(
                            mod: contentPack.Manifest,
                            labelName: storage.Key,
                            labelDesc: "",
                            newPage: storage.Key
                        );
                    }
                }
                config = contentPack.ReadJsonFile<Dictionary<string, ModConfig>>("config.json");
            }
            config ??= new Dictionary<string, ModConfig>();
            
            // Load expanded storages
            foreach (var storage in storages)
            {
                storage.Value.Name = storage.Key;
                storage.Value.Manifest = contentPack.Manifest;
                
                // Load base texture
                if (!string.IsNullOrWhiteSpace(storage.Value.Image) && !contentPack.HasFile($"{storage.Value.Image}") && contentPack.HasFile($"assets/{storage.Value.Image}"))
                    storage.Value.Image = Path.Combine("assets", storage.Value.Image);
                if (!string.IsNullOrWhiteSpace(storage.Value.Image) && contentPack.HasFile(storage.Value.Image))
                {
                    var texture = contentPack.LoadAsset<Texture2D>(storage.Value.Image);
                    XSLite.Textures.Add(storage.Key, texture);
                }
                
                // Add to config
                if (!config.TryGetValue(storage.Key, out var storageConfig))
                {
                    storageConfig = new ModConfig
                    {
                        Capacity = storage.Value.Capacity,
                        EnabledFeatures = storage.Value.EnabledFeatures
                    };
                    config.Add(storage.Key, storageConfig);
                }
                storage.Value.Config = storageConfig;
                
                // Enable XSPlus features
                if (_xsPlus.IsLoaded)
                {
                    // Opt-in to Expanded Menu
                    _xsPlus.API.EnableWithModData("ExpandedMenu", $"{XSLite.ModPrefix}/Storage", storage.Key, true);
                    
                    // Enable filtering items
                    if (storage.Value.FilterItems.Any())
                    {
                        _xsPlus.API.EnableWithModData("FilterItems", $"{XSLite.ModPrefix}/Storage", storage.Key, storage.Value.FilterItems);
                    }
                    
                    // Enable other toggleable features
                    foreach (var featureName in storageConfig.EnabledFeatures)
                    {
                        _xsPlus.API.EnableWithModData(featureName, $"{XSLite.ModPrefix}/Storage", storage.Key, true);
                    }
                }
                
                // Add GMCM page for storage
                if (_modConfigMenu.IsLoaded && storage.Value.PlayerConfig)
                {
                    Func<bool> OptionGet(string featureName)
                    {
                        return () => storageConfig.EnabledFeatures.Contains(featureName);
                    }
                    
                    Action<bool> OptionSet(string featureName)
                    {
                        return value =>
                        {
                            if (value)
                                storageConfig.EnabledFeatures.Add(featureName);
                            else
                                storageConfig.EnabledFeatures.Remove(featureName);
                            _xsPlus.API?.EnableWithModData(featureName, $"{XSLite.ModPrefix}/Storage", storage.Key, value);
                        };
                    }
                    
                    _modConfigMenu.API.StartNewPage(
                        mod: contentPack.Manifest,
                        pageName: storage.Key
                    );
                    _modConfigMenu.API.RegisterLabel(
                        mod: contentPack.Manifest,
                        labelName: storage.Key,
                        labelDesc: ""
                    );
                    _modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Capacity",
                        optionDesc: "The carrying capacity for this chests.",
                        optionGet: () => storageConfig.Capacity,
                        optionSet: value => storage.Value.Config.Capacity = value
                    );
                    _modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Access Carried",
                        optionDesc: "Open this chest inventory while it's being carried.",
                        optionGet: OptionGet("AccessCarried"),
                        optionSet: OptionSet("AccessCarried")
                    );
                    _modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Carry Chest",
                        optionDesc: "Carry this chest even while it's holding items.",
                        optionGet: OptionGet("CanCarry"),
                        optionSet: OptionSet("CanCarry")
                    );
                    _modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Craft from Chest",
                        optionDesc: "Allows chest to be crafted from remotely.",
                        optionGet: OptionGet("CraftFromChest"),
                        optionSet: OptionSet("CraftFromChest")
                    );
                    _modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Stash to Chest",
                        optionDesc: "Allows chest to be stashed into remotely.",
                        optionGet: OptionGet("StashToChest"),
                        optionSet: OptionSet("StashToChest")
                    );
                    _modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Vacuum Items",
                        optionDesc: "Allows chest to pick up dropped items while in player inventory.",
                        optionGet: OptionGet("VacuumItems"),
                        optionSet: OptionSet("VacuumItems")
                    );
                }
                XSLite.Storages.Add(storage.Key, storage.Value);
            }
            if (!contentPack.HasFile("content.json") && !MigrationHelper.CreateDynamicAsset(contentPack))
                return true;
            foreach (var storage in storages)
            {
                storage.Value.DisplayName = contentPack.Translation.Get($"big-craftable.{storage.Key}.name");
                storage.Value.Description = contentPack.Translation.Get($"big-craftable.{storage.Key}.description");
                if (storage.Value.Format == Storage.AssetFormat.Vanilla)
                    storage.Value.Format = Storage.AssetFormat.DynamicGameAssets;
            }
            var manifest = new Manifest(contentPack.Manifest)
            {
                ContentPackFor = new ManifestContentPackFor
                {
                    UniqueID = "spacechase0.DynamicGameAsset",
                    MinimumVersion = null
                },
                ExtraFields = new Dictionary<string, object>
                {
                    { "DGA.FormatVersion", "2" },
                    { "DGA.ConditionsFormatVersion", "1.23.0" }
                }
            };
            _dynamicAssets.API.AddEmbeddedPack(manifest, contentPack.DirectoryPath);
            return true;
        }
        public bool AcceptsItem(Chest chest, Item item)
        {
            return !chest.TryGetStorage(out var storage) || item.MatchesTagExt(storage.FilterItems);
        }
        public IList<string> GetAllStorages()
        {
            return XSLite.Storages.Keys.ToList();
        }
        public IList<string> GetOwnedStorages(IManifest manifest)
        {
            return XSLite.Storages.Values.Where(storage => storage.Manifest.UniqueID.Equals(manifest.UniqueID, StringComparison.OrdinalIgnoreCase)).Select(storage => storage.Name).ToList();
        }

        private record ManifestContentPackFor : IManifestContentPackFor
        {
            public string UniqueID { get; set; }
            public ISemanticVersion MinimumVersion { get; set; }
        }
        private record Manifest : IManifest
        {
            public string Name { get; }
            public string Description { get; }
            public string Author { get; }
            public ISemanticVersion Version { get; }
            public ISemanticVersion MinimumApiVersion { get; }
            public string UniqueID { get; }
            public string EntryDll { get; }
            public IManifestContentPackFor ContentPackFor { get; set; }
            public IManifestDependency[] Dependencies { get; }
            public string[] UpdateKeys { get; }
            public IDictionary<string, object> ExtraFields { get; set; }
            public Manifest(IManifest manifest)
            {
                Name = manifest.Name;
                Description = manifest.Description;
                Author = manifest.Author;
                Version = manifest.Version;
                MinimumApiVersion = manifest.MinimumApiVersion;
                UniqueID = manifest.UniqueID;
                EntryDll = manifest.EntryDll;
                ContentPackFor = manifest.ContentPackFor;
                Dependencies = manifest.Dependencies;
                UpdateKeys = manifest.UpdateKeys;
                ExtraFields = manifest.ExtraFields;
            }
        }
    }
}