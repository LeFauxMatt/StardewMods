namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Helpers;
    using Common.Integrations.GenericModConfigMenu;
    using Common.Integrations.XSPlus;
    using Common.Services;
    using Features;
    using HarmonyLib;
    using Models;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Locations;
    using StardewValley.Objects;

    /// <inheritdoc cref="StardewModdingAPI.Mod" />
    public class XSPlus : Mod
    {
        /// <summary>Mod-specific prefix for modData.</summary>
        internal const string ModPrefix = "furyx639.ExpandedStorage";

        private static Func<IEnumerable<GameLocation>> GetActiveLocations;
        private readonly IXSPlusAPI _api = new XSPlusAPI();
        private FeatureManager _featureManager;
        private GenericModConfigMenuIntegration _modConfigMenu;

        /// <summary>Gets placed Chests that are accessible to the player.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        public static IEnumerable<Chest> AccessibleChests
        {
            get => XSPlus.AccessibleLocations.SelectMany(location => location.Objects.Values.OfType<Chest>());
        }

        private static IEnumerable<GameLocation> AccessibleLocations
        {
            get
            {
                if (Context.IsMainPlayer)
                {
                    return Game1.locations.Concat(
                        Game1.locations.OfType<BuildableGameLocation>().SelectMany(
                            location => location.buildings.Where(building => building.indoors.Value is not null).Select(building => building.indoors.Value)));
                }

                return XSPlus.GetActiveLocations();
            }
        }

        /// <summary>Gets or sets config options for mod.</summary>
        private ModConfig ModConfig { get; set; } = null!;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Init(this.Monitor);
            XSPlus.GetActiveLocations = this.Helper.Multiplayer.GetActiveLocations;
            this.ModConfig = this.Helper.ReadConfig<ModConfig>();
            this._modConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);

            // Events
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            // Services
            var itemGrabMenuConstructedService = new ItemGrabMenuConstructedService(harmony);
            var itemGrabMenuChangedService = new ItemGrabMenuChangedService(this.Helper.Events.Display);
            var renderingActiveMenuService = new RenderingActiveMenuService(this.Helper.Events.Display, itemGrabMenuChangedService);
            var renderedActiveMenuService = new RenderedActiveMenuService(this.Helper.Events.Display, itemGrabMenuChangedService);
            var highlightChestItemsService = new HighlightItemsService(itemGrabMenuConstructedService, HighlightItemsService.InventoryType.Chest);
            var highlightPlayerItemsService = new HighlightItemsService(itemGrabMenuConstructedService, HighlightItemsService.InventoryType.Player);

            // Features
            this._featureManager = FeatureManager.Init(this.Helper, harmony, this.ModConfig.Global);
            this._featureManager.AddFeature(new AccessCarriedFeature(this.Helper.Input));
            this._featureManager.AddFeature(new CapacityFeature(() => this.ModConfig.Capacity));
            this._featureManager.AddFeature(new ColorPickerFeature(this.Helper.Content, itemGrabMenuConstructedService, itemGrabMenuChangedService, renderedActiveMenuService));
            this._featureManager.AddFeature(new CraftFromChestFeature(this.Helper.Input, () => this.ModConfig.OpenCrafting, () => this.ModConfig.CraftingRange));
            this._featureManager.AddFeature(new ExpandedMenuFeature(this.Helper.Input, itemGrabMenuConstructedService, itemGrabMenuChangedService, () => this.ModConfig.ScrollUp, () => this.ModConfig.ScrollDown, () => this.ModConfig.MenuRows));
            this._featureManager.AddFeature(new FilterItemsFeature(itemGrabMenuChangedService, highlightPlayerItemsService));
            this._featureManager.AddFeature(new InventoryTabsFeature(this.Helper.Content, this.Helper.Input, itemGrabMenuChangedService, highlightChestItemsService, renderingActiveMenuService, () => this.ModConfig.PreviousTab, () => this.ModConfig.NextTab));
            this._featureManager.AddFeature(new SearchItemsFeature(this.Helper.Content, this.Helper.Input, itemGrabMenuConstructedService, itemGrabMenuChangedService, highlightChestItemsService, renderedActiveMenuService, () => this.ModConfig.SearchTagSymbol));
            this._featureManager.AddFeature(new StashToChestFeature(this.Helper.Input, () => this.ModConfig.StashItems, () => this.ModConfig.StashingRange, () => this.ModConfig.SearchTagSymbol));
            this._featureManager.AddFeature(new UnbreakableFeature());
            this._featureManager.AddFeature(new UnplaceableFeature());
            this._featureManager.AddFeature(new VacuumItemsFeature());

            // Activate
            this._featureManager.ActivateFeatures();
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this._api;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            string[] configChoices = { "Default", "Enable", "Disable" };
            if (!this._modConfigMenu.IsLoaded)
            {
                return;
            }

            // Register mod configuration
            this._modConfigMenu.API.RegisterModConfig(
                mod: this.ModManifest,
                revertToDefault: () => this.ModConfig = new ModConfig(),
                saveToFile: () => this.Helper.WriteConfig(this.ModConfig));

            // Allow config in game
            this._modConfigMenu.API.SetDefaultIngameOptinValue(this.ModManifest, true);

            // Config options
            this._modConfigMenu.API.RegisterLabel(this.ModManifest, "General", string.Empty);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Open Crafting Button",
                optionDesc: "Key to open the crafting menu for accessible chests.",
                optionGet: () => this.ModConfig.OpenCrafting,
                optionSet: value => this.ModConfig.OpenCrafting = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Stash Items Button",
                optionDesc: "Key to stash items into accessible chests.",
                optionGet: () => this.ModConfig.StashItems,
                optionSet: value => this.ModConfig.StashItems = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Scroll Up",
                optionDesc: "Key to scroll up in expanded inventory menus.",
                optionGet: () => this.ModConfig.ScrollUp,
                optionSet: value => this.ModConfig.ScrollUp = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Scroll Down",
                optionDesc: "Key to scroll down in expanded inventory menus.",
                optionGet: () => this.ModConfig.ScrollDown,
                optionSet: value => this.ModConfig.ScrollDown = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Previous Tab",
                optionDesc: "Key to switch to previous tab.",
                optionGet: () => this.ModConfig.PreviousTab,
                optionSet: value => this.ModConfig.PreviousTab = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Next Tab",
                optionDesc: "Key to switch to next tab.",
                optionGet: () => this.ModConfig.NextTab,
                optionSet: value => this.ModConfig.NextTab = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Capacity",
                optionDesc: "How many items each chest will hold (use -1 for maximum capacity).",
                optionGet: () => this.ModConfig.Capacity,
                optionSet: value => this.ModConfig.Capacity = value);
            this._modConfigMenu.API.RegisterClampedOption(
                mod: this.ModManifest,
                optionName: "Menu Rows",
                optionDesc: "The most number of rows that the menu can expand into.",
                optionGet: () => this.ModConfig.MenuRows,
                optionSet: value => this.ModConfig.MenuRows = value,
                min: 3,
                max: 6,
                interval: 1);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Crafting Range",
                optionDesc: "The default range that chests can be remotely crafted from.",
                optionGet: () => this.ModConfig.CraftingRange,
                optionSet: value => this.ModConfig.CraftingRange = value,
                choices: new[] { "Inventory", "Location", "World", "Default", "Disabled" });
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Stashing Range",
                optionDesc: "The default range that chests can be remotely stashed into.",
                optionGet: () => this.ModConfig.StashingRange,
                optionSet: value => this.ModConfig.StashingRange = value,
                choices: new[] { "Inventory", "Location", "World", "Default", "Disabled" });

            this._modConfigMenu.API.RegisterLabel(this.ModManifest, "Global Overrides", "Enable/disable features for all chests");
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Access Carried",
                optionDesc: "Open the currently held chest in your inventory.",
                optionGet: this.ModConfig.GetConfig("AccessCarried"),
                optionSet: this.ModConfig.SetConfig("AccessCarried"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Color Picker",
                optionDesc: "Adds an HSL Color Picker to the chest menu.",
                optionGet: this.ModConfig.GetConfig("ColorPicker"),
                optionSet: this.ModConfig.SetConfig("ColorPicker"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Expanded Menu",
                optionDesc: "Expands or shrinks the chest menu.",
                optionGet: this.ModConfig.GetConfig("ExpandedMenu"),
                optionSet: this.ModConfig.SetConfig("ExpandedMenu"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Inventory Tabs",
                optionDesc: "Adds tabs to the chest menu.",
                optionGet: this.ModConfig.GetConfig("InventoryTabs"),
                optionSet: this.ModConfig.SetConfig("InventoryTabs"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Search Items",
                optionDesc: "Adds a search bar to the chest menu.",
                optionGet: this.ModConfig.GetConfig("SearchItems"),
                optionSet: this.ModConfig.SetConfig("SearchItems"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Vacuum Items",
                optionDesc: "Allows chests in player inventory to pick up dropped items.",
                optionGet: this.ModConfig.GetConfig("VacuumItems"),
                optionSet: this.ModConfig.SetConfig("VacuumItems"),
                choices: configChoices);
        }
    }
}