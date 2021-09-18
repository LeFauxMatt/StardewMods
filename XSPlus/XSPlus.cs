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
        private readonly IXSPlusAPI API = new XSPlusAPI();
        private FeatureManager FeatureManager;
        private GenericModConfigMenuIntegration ModConfigMenu;

        /// <summary>
        /// Gets placed Chests that are accessible to the player.
        /// </summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        public static IEnumerable<Chest> AccessibleChests
        {
            get => XSPlus.AccessibleLocations.SelectMany(location => location.Objects.Values.OfType<Chest>());
        }

        /// <summary>
        /// Gets config options for mod.
        /// </summary>
        internal ModConfig Config { get; private set; }

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

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Init(this.Monitor);
            XSPlus.GetActiveLocations = this.Helper.Multiplayer.GetActiveLocations;
            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.ModConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);

            // Events
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            // Features
            this.FeatureManager = FeatureManager.Init(this.Helper, new Harmony(this.ModManifest.UniqueID), this.Config.Global);
            this.FeatureManager.AddFeature(new Features.CommonFeature());
            this.FeatureManager.AddFeature(new Features.AccessCarried(this.Helper.Input));
            this.FeatureManager.AddFeature(new Features.Capacity(() => this.Config.Capacity));
            this.FeatureManager.AddFeature(new Features.CraftFromChest(this.Helper.Input, () => this.Config.OpenCrafting, () => this.Config.CraftingRange));
            this.FeatureManager.AddFeature(new Features.ExpandedMenu(this.Helper.Input, () => this.Config.ScrollUp, () => this.Config.ScrollDown, () => this.Config.MenuRows));
            this.FeatureManager.AddFeature(new Features.FilterItems());
            this.FeatureManager.AddFeature(new Features.InventoryTabs(this.Helper.Content, this.Helper.Input, () => this.Config.PreviousTab, () => this.Config.NextTab));
            this.FeatureManager.AddFeature(new Features.SearchItems(this.Helper.Content, this.Helper.Input, () => this.Config.SearchTagSymbol));
            this.FeatureManager.AddFeature(new Features.StashToChest(this.Helper.Input, () => this.Config.StashItems, () => this.Config.StashingRange, () => this.Config.SearchTagSymbol));
            this.FeatureManager.AddFeature(new Features.Unbreakable());
            this.FeatureManager.AddFeature(new Features.Unplaceable());
            this.FeatureManager.AddFeature(new Features.VacuumItems());

            // Activate
            this.FeatureManager.ActivateFeatures();
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this.API;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            string[] configChoices = { "Default", "Enable", "Disable" };
            if (!this.ModConfigMenu.IsLoaded)
            {
                return;
            }

            // Register mod configuration
            this.ModConfigMenu.API.RegisterModConfig(
                mod: this.ModManifest,
                revertToDefault: () => this.Config = new ModConfig(),
                saveToFile: () => this.Helper.WriteConfig(this.Config));

            // Allow config in game
            this.ModConfigMenu.API.SetDefaultIngameOptinValue(this.ModManifest, true);

            // Config options
            this.ModConfigMenu.API.RegisterLabel(this.ModManifest, "General", string.Empty);
            this.ModConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Open Crafting Button",
                optionDesc: "Key to open the crafting menu for accessible chests.",
                optionGet: () => this.Config.OpenCrafting,
                optionSet: value => this.Config.OpenCrafting = value);
            this.ModConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Stash Items Button",
                optionDesc: "Key to stash items into accessible chests.",
                optionGet: () => this.Config.StashItems,
                optionSet: value => this.Config.StashItems = value);
            this.ModConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Scroll Up",
                optionDesc: "Key to scroll up in expanded inventory menus.",
                optionGet: () => this.Config.ScrollUp,
                optionSet: value => this.Config.ScrollUp = value);
            this.ModConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Scroll Down",
                optionDesc: "Key to scroll down in expanded inventory menus.",
                optionGet: () => this.Config.ScrollDown,
                optionSet: value => this.Config.ScrollDown = value);
            this.ModConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Previous Tab",
                optionDesc: "Key to switch to previous tab.",
                optionGet: () => this.Config.PreviousTab,
                optionSet: value => this.Config.PreviousTab = value);
            this.ModConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Next Tab",
                optionDesc: "Key to switch to next tab.",
                optionGet: () => this.Config.NextTab,
                optionSet: value => this.Config.NextTab = value);
            this.ModConfigMenu.API.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: "Capacity",
                optionDesc: "How many items each chest will hold (use -1 for maximum capacity).",
                optionGet: () => this.Config.Capacity,
                optionSet: value => this.Config.Capacity = value);
            this.ModConfigMenu.API.RegisterClampedOption(
                mod: this.ModManifest,
                optionName: "Menu Rows",
                optionDesc: "The most number of rows that the menu can expand into.",
                optionGet: () => this.Config.MenuRows,
                optionSet: value => this.Config.MenuRows = value,
                min: 3,
                max: 6,
                interval: 1);
            this.ModConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Crafting Range",
                optionDesc: "The default range that chests can be remotely crafted from.",
                optionGet: () => this.Config.CraftingRange,
                optionSet: value => this.Config.CraftingRange = value,
                choices: new[] { "Inventory", "Location", "World", "Default", "Disabled" });
            this.ModConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Stashing Range",
                optionDesc: "The default range that chests can be remotely stashed into.",
                optionGet: () => this.Config.StashingRange,
                optionSet: value => this.Config.StashingRange = value,
                choices: new[] { "Inventory", "Location", "World", "Default", "Disabled" });

            this.ModConfigMenu.API.RegisterLabel(this.ModManifest, "Global Overrides", "Enable/disable features for all chests");
            this.ModConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Access Carried",
                optionDesc: "Open the currently held chest in your inventory.",
                optionGet: this.Config.GetConfig("AccessCarried"),
                optionSet: this.Config.SetConfig("AccessCarried"),
                choices: configChoices);
            this.ModConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Expanded Menu",
                optionDesc: "Expands or shrinks the chest menu.",
                optionGet: this.Config.GetConfig("ExpandedMenu"),
                optionSet: this.Config.SetConfig("ExpandedMenu"),
                choices: configChoices);
            this.ModConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Inventory Tabs",
                optionDesc: "Adds tabs to the chest menu.",
                optionGet: this.Config.GetConfig("InventoryTabs"),
                optionSet: this.Config.SetConfig("InventoryTabs"),
                choices: configChoices);
            this.ModConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Search Items",
                optionDesc: "Adds a search bar to the chest menu.",
                optionGet: this.Config.GetConfig("SearchItems"),
                optionSet: this.Config.SetConfig("SearchItems"),
                choices: configChoices);
            this.ModConfigMenu.API.RegisterChoiceOption(
                mod: this.ModManifest,
                optionName: "Vacuum Items",
                optionDesc: "Allows chests in player inventory to pick up dropped items.",
                optionGet: this.Config.GetConfig("VacuumItems"),
                optionSet: this.Config.SetConfig("VacuumItems"),
                choices: configChoices);
        }
    }
}