using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ExpandedStorage.Framework
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class ModConfig
    {
        /// <summary>Allow carried chests to be accessed while in inventory.</summary>
        public bool AllowAccessCarriedChest { get; set; } = true;
        
        /// <summary>Allow chests to be picked up and placed with items.</summary>
        public bool AllowCarryingChests { get; set; } = true;

        /// <summary>Allow chest menu for held chest and accessed chest.</summary>
        public bool AllowChestToChest { get; set; } = true;

        /// <summary>Whether to allow modded storage to have capacity other than 36 slots.</summary>
        public bool AllowModdedCapacity { get; set; } = true;

        /// <summary>Allows storages to accept specific items.</summary>
        public bool AllowRestrictedStorage { get; set; } = true;

        /// <summary>Allows storages to pull items directly into their inventory.</summary>
        public bool AllowVacuumItems { get; set; } = true;
        
        /// <summary>Only vacuum to storages in the first row of player inventory.</summary>
        public bool VacuumToFirstRow { get; set; } = true;

        /// <summary>Adds three extra rows to the Inventory Menu.</summary>
        public bool ExpandInventoryMenu { get; set; } = true;

        /// <summary>Symbol used to search items by context tags.</summary>
        public string SearchTagSymbol { get; set; } = "#";
        
        /// <summary>Adds clickable arrows to indicate when there are more items in the chest.</summary>
        public bool ShowOverlayArrows { get; set; } = true;

        /// <summary>Allows filtering Inventory Menu by searching for the the item name.</summary>
        public bool ShowSearchBar { get; set; } = true;

        /// <summary>Allows showing tabs in the Chest Menu.</summary>
        public bool ShowTabs { get; set; } = true;

        /// <summary>Control scheme for Expanded Storage features.</summary>
        public ModConfigKeys Controls { get; set; } = new();

        public static void RegisterModConfig(IManifest modManifest, IGenericModConfigMenuAPI modConfigApi, ModConfig config)
        {
            modConfigApi.RegisterLabel(modManifest,
                "Controls",
                "Controller/Keyboard controls");
            
            modConfigApi.RegisterSimpleOption(modManifest,
                "Scroll Up",
                "Button for scrolling up",
                () => config.Controls.ScrollUp.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => config.Controls.ScrollUp = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(modManifest,
                "Scroll Down",
                "Button for scrolling down",
                () => config.Controls.ScrollDown.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => config.Controls.ScrollDown = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(modManifest,
                "Previous Tab",
                "Button for switching to the previous tab",
                () => config.Controls.PreviousTab.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => config.Controls.PreviousTab = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(modManifest,
                "Next Tab",
                "Button for switching to the next tab",
                () => config.Controls.NextTab.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => config.Controls.NextTab = KeybindList.ForSingle(value));
            
            modConfigApi.RegisterLabel(modManifest,
                "Tweaks",
                "Modify behavior for certain features");
            
            modConfigApi.RegisterSimpleOption(modManifest,
                "Search Symbol",
                "Symbol used to search items by context tag",
                () => config.SearchTagSymbol,
                value => config.SearchTagSymbol = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Vacuum To First Row",
                "Uncheck to allow vacuuming to any chest in player inventory",
                () => config.VacuumToFirstRow,
                value => config.VacuumToFirstRow = value);
            
            modConfigApi.RegisterLabel(modManifest,
                "Toggles",
                "Enable/Disable features (restart to revert patches)");
            
            modConfigApi.RegisterSimpleOption(modManifest,
                "Access Carried Chest",
                "Uncheck to globally disable accessing chest items for carried chests",
                () => config.AllowAccessCarriedChest,
                value => config.AllowAccessCarriedChest = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Carry Chest",
                "Uncheck to globally disable carrying chests",
                () => config.AllowCarryingChests,
                value => config.AllowCarryingChests = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Chest Tabs",
                "Uncheck to globally disable chest tabs",
                () => config.ShowTabs,
                value => config.ShowTabs = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Chest To Chest",
                "Uncheck to globally disable chest to chest menu",
                () => config.AllowChestToChest,
                value => config.AllowChestToChest = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Expand Inventory Menu",
                "Uncheck to globally disable resizing the inventory menu",
                () => config.ExpandInventoryMenu,
                value => config.ExpandInventoryMenu = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Modded Capacity",
                "Uncheck to globally disable non-vanilla capacity (36 item slots)",
                () => config.AllowModdedCapacity,
                value => config.AllowModdedCapacity = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Overlay Arrows",
                "Uncheck to globally disable adding arrow buttons",
                () => config.ShowOverlayArrows,
                value => config.ShowOverlayArrows = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Restricted Storage",
                "Uncheck to globally disable allow/block lists for chest items",
                () => config.AllowRestrictedStorage,
                value => config.AllowRestrictedStorage = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Search Bar",
                "Uncheck to globally disable the search bar",
                () => config.ShowSearchBar,
                value => config.ShowSearchBar = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Vacuum Items",
                "Uncheck to globally disable chests picking up items",
                () => config.AllowVacuumItems,
                value => config.AllowVacuumItems = value);
        }
        
        protected internal string SummaryReport =>
            "Expanded Storage Configuration\n" +
            $"\tAccess Carried     : {AllowAccessCarriedChest}\n" +
            $"\tCarry Chest        : {AllowCarryingChests}\n" +
            $"\tModded Capacity    : {AllowModdedCapacity}\n" +
            $"\tResize Menu        : {ExpandInventoryMenu}\n" +
            $"\tRestricted Storage : {AllowRestrictedStorage}\n" +
            $"\tShow Arrows        : {ShowOverlayArrows}\n" +
            $"\tShow Tabs          : {ShowTabs}\n" +
            $"\tShow Search        : {ShowSearchBar}\n" +
            $"\tVacuum Items       : {AllowVacuumItems}";
    }
}