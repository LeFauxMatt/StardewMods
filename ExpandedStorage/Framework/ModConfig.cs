using System.Diagnostics.CodeAnalysis;

namespace ExpandedStorage.Framework
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class ModConfig
    {
        /// <summary>Allow carried chests to be accessed while in inventory.</summary>
        public bool AllowAccessCarriedChest { get; set; } = true;
        
        /// <summary>Allow chests to be picked up and placed with items.</summary>
        public bool AllowCarryingChests { get; set; } = true;

        /// <summary>Whether to allow modded storage to have capacity other than 36 slots.</summary>
        public bool AllowModdedCapacity { get; set; } = true;

        /// <summary>Allows storages to accept specific items.</summary>
        public bool AllowRestrictedStorage { get; set; } = true;

        /// <summary>Adds three extra rows to the Inventory Menu.</summary>
        public bool ExpandInventoryMenu { get; set; } = true;
        
        /// <summary>Adds clickable arrows to indicate when there are more items in the chest.</summary>
        public bool ShowOverlayArrows { get; set; } = true;

        /// <summary>Allows filtering Inventory Menu by searching for the the item name.</summary>
        public bool ShowSearchBar { get; set; } = true;

        /// <summary>Allows showing tabs in the Chest Menu.</summary>
        public bool ShowTabs { get; set; } = true;

        /// <summary>Control scheme for Expanded Storage features.</summary>
        public ModConfigKeys Controls { get; set; } = new();
    }
}