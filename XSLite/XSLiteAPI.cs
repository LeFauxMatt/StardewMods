namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Integrations.DynamicGameAssets;
    using Common.Integrations.GenericModConfigMenu;
    using Common.Integrations.XSLite;
    using Common.Integrations.XSPlus;
    using Common.Services;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Objects;

    public class XSLiteAPI : IXSLiteAPI
    {
        private static readonly HashSet<string> VanillaNames = new()
        {
            "Chest",
            "Stone Chest",
            "Junimo Chest",
            "Mini-Shipping Bin",
            "Mini-Fridge",
            "Auto-Grabber",
        };

        private readonly IModHelper Helper;
        private readonly DynamicGameAssetsIntegration DynamicAssets;
        private readonly GenericModConfigMenuIntegration ModConfigMenu;
        private readonly XSPlusIntegration XSPlus;

        internal XSLiteAPI(IModHelper helper)
        {
            this.Helper = helper;
            this.DynamicAssets = new DynamicGameAssetsIntegration(helper.ModRegistry);
            this.ModConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);
            this.XSPlus = new XSPlusIntegration(helper.ModRegistry);
        }

        public bool LoadContentPack(IManifest manifest, string path)
        {
            IContentPack contentPack = this.Helper.ContentPacks.CreateTemporary(
                path,
                manifest.UniqueID,
                manifest.Name,
                manifest.Description,
                manifest.Author,
                manifest.Version);
            return this.LoadContentPack(contentPack);
        }

        public bool LoadContentPack(IContentPack contentPack)
        {
            Log.Info($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}");

            IDictionary<string, Storage> storages = contentPack.ReadJsonFile<IDictionary<string, Storage>>("expanded-storage.json");

            if (storages == null)
            {
                Log.Warn($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}");
                return false;
            }

            // Remove any duplicate storages
            foreach (KeyValuePair<string, Storage> storage in storages.Where(storage => XSLite.Storages.ContainsKey(storage.Key)))
            {
                Log.Warn($"Duplicate storage {storage.Key} in {contentPack.Manifest.UniqueID}.");
                storages.Remove(storage.Key);
            }

            if (storages.Count == 0)
            {
                Log.Warn($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}");
                return false;
            }

            // Setup GMCM for Content Pack
            Dictionary<string, ModConfig> config = null;
            if (storages.Any(storage => storage.Value.PlayerConfig))
            {
                if (this.ModConfigMenu.IsLoaded)
                {
                    void RevertToDefault()
                    {
                        foreach (KeyValuePair<string, Storage> storage in storages)
                        {
                            storage.Value.Config.Capacity = storage.Value.Capacity;
                            storage.Value.Config.EnabledFeatures = storage.Value.EnabledFeatures;
                        }
                    }

                    void SaveToFile()
                    {
                        contentPack.WriteJsonFile("config.json", storages.ToDictionary(
                            storage => storage.Key,
                            storage => storage.Value.Config));
                    }

                    this.ModConfigMenu.API.RegisterModConfig(
                        mod: contentPack.Manifest,
                        revertToDefault: RevertToDefault,
                        saveToFile: SaveToFile);
                    this.ModConfigMenu.API.SetDefaultIngameOptinValue(
                        mod: contentPack.Manifest,
                        optedIn: true);

                    // Add a page for each storage
                    foreach (KeyValuePair<string, Storage> storage in storages)
                    {
                        this.ModConfigMenu.API.RegisterPageLabel(
                            mod: contentPack.Manifest,
                            labelName: storage.Key,
                            labelDesc: string.Empty,
                            newPage: storage.Key);
                    }
                }

                config = contentPack.ReadJsonFile<Dictionary<string, ModConfig>>("config.json");
            }

            config ??= new Dictionary<string, ModConfig>();

            // Load expanded storages
            foreach (KeyValuePair<string, Storage> storage in storages)
            {
                storage.Value.Name = storage.Key;
                storage.Value.Manifest = contentPack.Manifest;
                storage.Value.Format = XSLiteAPI.VanillaNames.Contains(storage.Value.Name) ? Storage.AssetFormat.Vanilla : Storage.AssetFormat.DynamicGameAssets;

                // Load base texture
                if (!string.IsNullOrWhiteSpace(storage.Value.Image) && !contentPack.HasFile($"{storage.Value.Image}") && contentPack.HasFile($"assets/{storage.Value.Image}"))
                {
                    storage.Value.Image = Path.Combine("assets", storage.Value.Image);
                }

                if (!string.IsNullOrWhiteSpace(storage.Value.Image) && contentPack.HasFile(storage.Value.Image))
                {
                    Texture2D texture = contentPack.LoadAsset<Texture2D>(storage.Value.Image);
                    XSLite.Textures.Add(storage.Key, texture);
                }

                // Add to config
                if (!config.TryGetValue(storage.Key, out ModConfig storageConfig))
                {
                    storageConfig = new ModConfig
                    {
                        Capacity = storage.Value.Capacity,
                        EnabledFeatures = storage.Value.EnabledFeatures,
                    };
                    config.Add(storage.Key, storageConfig);
                }

                storage.Value.Config = storageConfig;

                // Enable XSPlus features
                if (this.XSPlus.IsLoaded)
                {
                    // Opt-in to Expanded Menu
                    this.XSPlus.API.EnableWithModData("ExpandedMenu", $"{XSLite.ModPrefix}/Storage", storage.Key, true);

                    // Enable filtering items
                    if (storage.Value.FilterItems.Any())
                    {
                        this.XSPlus.API.EnableWithModData("FilterItems", $"{XSLite.ModPrefix}/Storage", storage.Key, storage.Value.FilterItems);
                    }

                    // Enable additional capacity
                    else if (storageConfig.Capacity != 0)
                    {
                        this.XSPlus.API.EnableWithModData("Capacity", $"{XSLite.ModPrefix}/Storage", storage.Key, storageConfig.Capacity);
                    }

                    // Disable color picker if storage does not support player color
                    if (!storage.Value.PlayerColor)
                    {
                        this.XSPlus.API.EnableWithModData("ColorPicker", $"{XSLite.ModPrefix}/Storage", storage.Key, false);
                    }

                    // Enable other toggleable features
                    foreach (string featureName in storageConfig.EnabledFeatures)
                    {
                        this.XSPlus.API.EnableWithModData(featureName, $"{XSLite.ModPrefix}/Storage", storage.Key, true);
                    }
                }

                // Add GMCM page for storage
                if (this.ModConfigMenu.IsLoaded && storage.Value.PlayerConfig)
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
                            {
                                storageConfig.EnabledFeatures.Add(featureName);
                            }
                            else
                            {
                                storageConfig.EnabledFeatures.Remove(featureName);
                            }

                            this.XSPlus.API?.EnableWithModData(featureName, $"{XSLite.ModPrefix}/Storage", storage.Key, value);
                        };
                    }

                    this.ModConfigMenu.API.StartNewPage(
                        mod: contentPack.Manifest,
                        pageName: storage.Key);
                    this.ModConfigMenu.API.RegisterLabel(
                        mod: contentPack.Manifest,
                        labelName: storage.Key,
                        labelDesc: string.Empty);
                    this.ModConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Capacity",
                        optionDesc: "The carrying capacity for this chests.",
                        optionGet: () => storageConfig.Capacity,
                        optionSet: value =>
                        {
                            storageConfig.Capacity = value;
                            this.XSPlus.API?.EnableWithModData("Capacity", $"{XSLite.ModPrefix}/Storage", storage.Key, value);
                        });
                    this.ModConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Access Carried",
                        optionDesc: "Open this chest inventory while it's being carried.",
                        optionGet: OptionGet("AccessCarried"),
                        optionSet: OptionSet("AccessCarried"));
                    this.ModConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Carry Chest",
                        optionDesc: "Carry this chest even while it's holding items.",
                        optionGet: OptionGet("CanCarry"),
                        optionSet: OptionSet("CanCarry"));
                    this.ModConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Craft from Chest",
                        optionDesc: "Allows chest to be crafted from remotely.",
                        optionGet: OptionGet("CraftFromChest"),
                        optionSet: OptionSet("CraftFromChest"));
                    this.ModConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Stash to Chest",
                        optionDesc: "Allows chest to be stashed into remotely.",
                        optionGet: OptionGet("StashToChest"),
                        optionSet: OptionSet("StashToChest"));
                    this.ModConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Vacuum Items",
                        optionDesc: "Allows chest to pick up dropped items while in player inventory.",
                        optionGet: OptionGet("VacuumItems"),
                        optionSet: OptionSet("VacuumItems"));
                }

                XSLite.Storages.Add(storage.Key, storage.Value);
            }

            if (!this.DynamicAssets.IsLoaded || (!contentPack.HasFile("content.json") && !MigrationHelper.CreateDynamicAsset(contentPack)))
            {
                return true;
            }

            foreach (KeyValuePair<string, Storage> storage in storages)
            {
                storage.Value.DisplayName = contentPack.Translation.Get($"big-craftable.{storage.Key}.name");
                storage.Value.Description = contentPack.Translation.Get($"big-craftable.{storage.Key}.description");
            }

            var manifest = new Manifest(contentPack.Manifest)
            {
                ContentPackFor = new ManifestContentPackFor
                {
                    UniqueID = "spacechase0.DynamicGameAsset",
                    MinimumVersion = null,
                },
                ExtraFields = new Dictionary<string, object>
                {
                    { "DGA.FormatVersion", "2" },
                    { "DGA.ConditionsFormatVersion", "1.23.0" },
                },
            };
            this.DynamicAssets.API.AddEmbeddedPack(manifest, contentPack.DirectoryPath);
            return true;
        }

        public bool AcceptsItem(Chest chest, Item item)
        {
            return !chest.TryGetStorage(out Storage storage) || item.MatchesTagExt(storage.FilterItems);
        }

        public IEnumerable<string> GetAllStorages()
        {
            return XSLite.Storages.Keys;
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
            public Manifest(IManifest manifest)
            {
                this.Name = manifest.Name;
                this.Description = manifest.Description;
                this.Author = manifest.Author;
                this.Version = manifest.Version;
                this.MinimumApiVersion = manifest.MinimumApiVersion;
                this.UniqueID = manifest.UniqueID;
                this.EntryDll = manifest.EntryDll;
                this.ContentPackFor = manifest.ContentPackFor;
                this.Dependencies = manifest.Dependencies;
                this.UpdateKeys = manifest.UpdateKeys;
                this.ExtraFields = manifest.ExtraFields;
            }

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
        }
    }
}