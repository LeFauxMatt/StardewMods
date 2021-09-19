namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Helpers;
    using Common.Integrations.GenericModConfigMenu;
    using Common.Integrations.XSPlus;
    using HarmonyLib;
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
        private ModConfig Config { get; set; }

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Init(this.Monitor);
            XSPlus.GetActiveLocations = this.Helper.Multiplayer.GetActiveLocations;
            this.Config = this.Helper.ReadConfig<ModConfig>();
            this._modConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);

            // Events
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            // Features
            this._featureManager = FeatureManager.Init(this.Helper, new Harmony(this.ModManifest.UniqueID), this.Config.Global);
            this._featureManager.AddFeature(new Features.CommonFeature());
            this._featureManager.AddFeature(new Features.AccessCarried(this.Helper.Input));
            this._featureManager.AddFeature(new Features.Capacity(() => this.Config.Capacity));
            this._featureManager.AddFeature(new Features.ColorPicker(this.Helper.Content));
            this._featureManager.AddFeature(new Features.CraftFromChest(this.Helper.Input, () => this.Config.OpenCrafting, () => this.Config.CraftingRange));
            this._featureManager.AddFeature(new Features.ExpandedMenu(this.Helper.Input, () => this.Config.ScrollUp, () => this.Config.ScrollDown, () => this.Config.MenuRows));
            this._featureManager.AddFeature(new Features.FilterItems());
            this._featureManager.AddFeature(new Features.InventoryTabs(this.Helper.Content, this.Helper.Input, () => this.Config.PreviousTab, () => this.Config.NextTab));
            this._featureManager.AddFeature(new Features.SearchItems(this.Helper.Content, this.Helper.Input, () => this.Config.SearchTagSymbol));
            this._featureManager.AddFeature(new Features.StashToChest(this.Helper.Input, () => this.Config.StashItems, () => this.Config.StashingRange, () => this.Config.SearchTagSymbol));
            this._featureManager.AddFeature(new Features.Unbreakable());
            this._featureManager.AddFeature(new Features.Unplaceable());
            this._featureManager.AddFeature(new Features.VacuumItems());

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
                revertToDefault: () => this.Config = new ModConfig(),
                saveToFile: () => this.Helper.WriteConfig(this.Config));

            // Allow config in game
            this._modConfigMenu.API.SetDefaultIngameOptinValue(this.ModManifest, true);

            // Config options
            this._modConfigMenu.API.RegisterLabel(this.ModManifest, "General", string.Empty);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Open Crafting Button",
                optionDesc: "Key to open the crafting menu for accessible chests.",
                optionGet: () => this.Config.OpenCrafting,
                optionSet: value => this.Config.OpenCrafting = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Stash Items Button",
                optionDesc: "Key to stash items into accessible chests.",
                optionGet: () => this.Config.StashItems,
                optionSet: value => this.Config.StashItems = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Scroll Up",
                optionDesc: "Key to scroll up in expanded inventory menus.",
                optionGet: () => this.Config.ScrollUp,
                optionSet: value => this.Config.ScrollUp = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Scroll Down",
                optionDesc: "Key to scroll down in expanded inventory menus.",
                optionGet: () => this.Config.ScrollDown,
                optionSet: value => this.Config.ScrollDown = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Previous Tab",
                optionDesc: "Key to switch to previous tab.",
                optionGet: () => this.Config.PreviousTab,
                optionSet: value => this.Config.PreviousTab = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Next Tab",
                optionDesc: "Key to switch to next tab.",
                optionGet: () => this.Config.NextTab,
                optionSet: value => this.Config.NextTab = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Capacity",
                optionDesc: "How many items each chest will hold (use -1 for maximum capacity).",
                optionGet: () => this.Config.Capacity,
                optionSet: value => this.Config.Capacity = value);
            this._modConfigMenu.API.RegisterClampedOption(
                mod: this.ModManifest,
                optionName: "Menu Rows",
                optionDesc: "The most number of rows that the menu can expand into.",
                optionGet: () => this.Config.MenuRows,
                optionSet: value => this.Config.MenuRows = value,
                min: 3,
                max: 6,
                interval: 1);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Crafting Range",
                optionDesc: "The default range that chests can be remotely crafted from.",
                optionGet: () => this.Config.CraftingRange,
                optionSet: value => this.Config.CraftingRange = value,
                choices: new[] { "Inventory", "Location", "World", "Default", "Disabled" });
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Stashing Range",
                optionDesc: "The default range that chests can be remotely stashed into.",
                optionGet: () => this.Config.StashingRange,
                optionSet: value => this.Config.StashingRange = value,
                choices: new[] { "Inventory", "Location", "World", "Default", "Disabled" });

            this._modConfigMenu.API.RegisterLabel(this.ModManifest, "Global Overrides", "Enable/disable features for all chests");
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Access Carried",
                optionDesc: "Open the currently held chest in your inventory.",
                optionGet: this.Config.GetConfig("AccessCarried"),
                optionSet: this.Config.SetConfig("AccessCarried"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Color Picker",
                optionDesc: "Adds an HSL Color Picker to the chest menu.",
                optionGet: this.Config.GetConfig("ColorPicker"),
                optionSet: this.Config.SetConfig("ColorPicker"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Expanded Menu",
                optionDesc: "Expands or shrinks the chest menu.",
                optionGet: this.Config.GetConfig("ExpandedMenu"),
                optionSet: this.Config.SetConfig("ExpandedMenu"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Inventory Tabs",
                optionDesc: "Adds tabs to the chest menu.",
                optionGet: this.Config.GetConfig("InventoryTabs"),
                optionSet: this.Config.SetConfig("InventoryTabs"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Search Items",
                optionDesc: "Adds a search bar to the chest menu.",
                optionGet: this.Config.GetConfig("SearchItems"),
                optionSet: this.Config.SetConfig("SearchItems"),
                choices: configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Vacuum Items",
                optionDesc: "Allows chests in player inventory to pick up dropped items.",
                optionGet: this.Config.GetConfig("VacuumItems"),
                optionSet: this.Config.SetConfig("VacuumItems"),
                choices: configChoices);
        }
    }
}