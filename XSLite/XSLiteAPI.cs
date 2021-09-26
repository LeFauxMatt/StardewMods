namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common.Helpers.ItemMatcher;
    using Common.Integrations.DynamicGameAssets;
    using Common.Integrations.GenericModConfigMenu;
    using Common.Integrations.XSLite;
    using Common.Integrations.XSPlus;
    using Common.Services;
    using Microsoft.Xna.Framework.Graphics;
    using Migrations.JsonAsset;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
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

        private readonly IModHelper _helper;
        private readonly DynamicGameAssetsIntegration _dynamicAssets;
        private readonly GenericModConfigMenuIntegration _modConfigMenu;
        private readonly XSPlusIntegration _xsPlus;
        private readonly ItemMatcher _itemMatcher = new(string.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="XSLiteAPI"/> class.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        internal XSLiteAPI(IModHelper helper)
        {
            this._helper = helper;
            this._dynamicAssets = new DynamicGameAssetsIntegration(helper.ModRegistry);
            this._modConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);
            this._xsPlus = new XSPlusIntegration(helper.ModRegistry);
        }

        /// <inheritdoc/>
        public bool LoadContentPack(IManifest manifest, string path)
        {
            IContentPack contentPack = this._helper.ContentPacks.CreateTemporary(
                path,
                manifest.UniqueID,
                manifest.Name,
                manifest.Description,
                manifest.Author,
                manifest.Version);
            return this.LoadContentPack(contentPack);
        }

        /// <inheritdoc/>
        public bool LoadContentPack(IContentPack contentPack)
        {
            Log.Info($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}");

            IDictionary<string, Storage> storages = contentPack.ReadJsonFile<IDictionary<string, Storage>>("expanded-storage.json");

            if (storages is null)
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
                if (this._modConfigMenu.IsLoaded)
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

                    this._modConfigMenu.API.RegisterModConfig(
                        mod: contentPack.Manifest,
                        revertToDefault: RevertToDefault,
                        saveToFile: SaveToFile);
                    this._modConfigMenu.API.SetDefaultIngameOptinValue(
                        mod: contentPack.Manifest,
                        optedIn: true);

                    // Add a page for each storage
                    foreach (KeyValuePair<string, Storage> storage in storages)
                    {
                        this._modConfigMenu.API.RegisterPageLabel(
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
                if (this._xsPlus.IsLoaded)
                {
                    // Opt-in to Expanded Menu
                    this._xsPlus.API.EnableWithModData("ExpandedMenu", $"{XSLite.ModPrefix}/Storage", storage.Key, true);

                    // Enable filtering items
                    if (storage.Value.FilterItems.Any())
                    {
                        this._xsPlus.API.EnableWithModData("FilterItems", $"{XSLite.ModPrefix}/Storage", storage.Key, storage.Value.FilterItems);
                    }

                    // Enable additional capacity
                    else if (storageConfig.Capacity != 0)
                    {
                        this._xsPlus.API.EnableWithModData("Capacity", $"{XSLite.ModPrefix}/Storage", storage.Key, storageConfig.Capacity);
                    }

                    // Disable color picker if storage does not support player color
                    if (!storage.Value.PlayerColor)
                    {
                        this._xsPlus.API.EnableWithModData("ColorPicker", $"{XSLite.ModPrefix}/Storage", storage.Key, false);
                    }

                    // Enable other toggleable features
                    foreach (string featureName in storageConfig.EnabledFeatures)
                    {
                        this._xsPlus.API.EnableWithModData(featureName, $"{XSLite.ModPrefix}/Storage", storage.Key, true);
                    }
                }

                // Add GMCM page for storage
                if (this._modConfigMenu.IsLoaded && storage.Value.PlayerConfig)
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

                            this._xsPlus.API?.EnableWithModData(featureName, $"{XSLite.ModPrefix}/Storage", storage.Key, value);
                        };
                    }

                    this._modConfigMenu.API.StartNewPage(
                        mod: contentPack.Manifest,
                        pageName: storage.Key);
                    this._modConfigMenu.API.RegisterLabel(
                        mod: contentPack.Manifest,
                        labelName: storage.Key,
                        labelDesc: string.Empty);
                    this._modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Capacity",
                        optionDesc: "The carrying capacity for this chests.",
                        optionGet: () => storageConfig.Capacity,
                        optionSet: value =>
                        {
                            storageConfig.Capacity = value;
                            this._xsPlus.API?.EnableWithModData("Capacity", $"{XSLite.ModPrefix}/Storage", storage.Key, value);
                        });
                    this._modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Access Carried",
                        optionDesc: "Open this chest inventory while it's being carried.",
                        optionGet: OptionGet("AccessCarried"),
                        optionSet: OptionSet("AccessCarried"));
                    this._modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Carry Chest",
                        optionDesc: "Carry this chest even while it's holding items.",
                        optionGet: OptionGet("CanCarry"),
                        optionSet: OptionSet("CanCarry"));
                    this._modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Craft from Chest",
                        optionDesc: "Allows chest to be crafted from remotely.",
                        optionGet: OptionGet("CraftFromChest"),
                        optionSet: OptionSet("CraftFromChest"));
                    this._modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Stash to Chest",
                        optionDesc: "Allows chest to be stashed into remotely.",
                        optionGet: OptionGet("StashToChest"),
                        optionSet: OptionSet("StashToChest"));
                    this._modConfigMenu.API.RegisterSimpleOption(
                        mod: contentPack.Manifest,
                        optionName: "Vacuum Items",
                        optionDesc: "Allows chest to pick up dropped items while in player inventory.",
                        optionGet: OptionGet("VacuumItems"),
                        optionSet: OptionSet("VacuumItems"));
                }

                XSLite.Storages.Add(storage.Key, storage.Value);
            }

            if (!this._dynamicAssets.IsLoaded)
            {
                return true;
            }

            if (Directory.Exists(Path.Combine(contentPack.DirectoryPath, "BigCraftables")))
            {
                var migrator = JsonAssetsMigrator.FromContentPack(contentPack);
                if (migrator is null)
                {
                    return true;
                }

                if (contentPack.HasFile("content.json") || migrator.ToDynamicGameAssets(contentPack))
                {
                    foreach (var jsonAsset in migrator.JsonAssets)
                    {
                        if (!storages.TryGetValue(jsonAsset.Name, out Storage storage) || !string.IsNullOrWhiteSpace(storage.Image))
                        {
                            continue;
                        }

                        if (XSLite.Textures.ContainsKey(jsonAsset.Name) || !contentPack.HasFile($"assets/{jsonAsset.Name}.png"))
                        {
                            continue;
                        }

                        Texture2D texture = contentPack.LoadAsset<Texture2D>($"assets/{jsonAsset.Name}.png");
                        XSLite.Textures.Add(jsonAsset.Name, texture);
                        storage.Format = Storage.AssetFormat.JsonAssets;
                        storage.Frames = texture.Width / 16;
                        storage.PlayerColor = texture.Height > 32;
                    }
                }
            }

            foreach (KeyValuePair<string, Storage> storage in storages)
            {
                storage.Value.DisplayName = contentPack.Translation.Get($"big-craftable.{storage.Key}.name");
                storage.Value.Description = contentPack.Translation.Get($"big-craftable.{storage.Key}.description");
            }

            var manifest = new ContentPackManifest(contentPack.Manifest)
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
            this._dynamicAssets.API.AddEmbeddedPack(manifest, contentPack.DirectoryPath);
            return true;
        }

        /// <inheritdoc/>
        public bool AcceptsItem(Chest chest, Item item)
        {
            if (!chest.TryGetStorage(out Storage storage))
            {
                return true;
            }

            this._itemMatcher.SetSearch(storage.FilterItems);
            return this._itemMatcher.Matches(item);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetAllStorages()
        {
            return XSLite.Storages.Keys;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetOwnedStorages(IManifest manifest)
        {
            foreach (var storage in XSLite.Storages.Values)
            {
                if (storage.Manifest.UniqueID.Equals(manifest.UniqueID))
                {
                    yield return storage.Name;
                }
            }
        }
    }
}