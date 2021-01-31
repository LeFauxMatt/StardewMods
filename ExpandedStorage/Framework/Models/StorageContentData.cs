using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class StorageContentData : StorageConfig
    {
        /// <summary>The game sound that will play when the storage is opened.</summary>
        public string OpenSound;
        
        /// <summary>One of the special chest types (None, MiniShippingBin, JunimoChest).</summary>
        public string SpecialChestType;

        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;

        /// <summary>True for assets loaded into Game1.bigCraftables outside of JsonAssets.</summary>
        internal bool IsVanilla;
        internal StorageContentData() : this(null)
        {
            OpenSound = "openChest";
            SpecialChestType = "None";
        }
        internal StorageContentData(string storageName) : base(storageName) { }
        internal bool IsAllowed(Item item) => !AllowList.Any() || AllowList.Any(item.HasContextTag);
        internal bool IsBlocked(Item item) => BlockList.Any() && BlockList.Any(item.HasContextTag);
        internal bool HighlightMethod(Item item) =>
            IsAllowed(item) && !IsBlocked(item)
            && (!Enum.TryParse(SpecialChestType, out Chest.SpecialChestTypes specialChestType)
                || specialChestType != Chest.SpecialChestTypes.MiniShippingBin
                || Utility.highlightShippableObjects(item));
        internal void CopyFrom(StorageConfig config)
        {
            Capacity = config.Capacity;
            CanCarry = config.CanCarry;
            AccessCarried = config.AccessCarried;
            ShowSearchBar = config.ShowSearchBar;
            IsPlaceable = config.IsPlaceable;
            AllowList = config.AllowList;
            BlockList = config.BlockList;
            Tabs = config.Tabs;
        }
    }
}