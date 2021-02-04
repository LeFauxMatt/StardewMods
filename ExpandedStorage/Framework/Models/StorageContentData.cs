using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class StorageContentData : StorageConfig
    {
        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;

        /// <summary>True for assets loaded into Game1.bigCraftables outside of JsonAssets.</summary>
        internal bool IsVanilla;
        
        /// <summary>The game sound that will play when the storage is opened.</summary>
        public string OpenSound = "openChest";

        /// <summary>One of the special chest types (None, MiniShippingBin, JunimoChest).</summary>
        public string SpecialChestType = "None";
        
        /// <summary>Determines whether the storage type should be flagged as a Fridge.</summary>
        public bool IsFridge = false;
        
        /// <summary>Allows the storage to be placed in the world.</summary>
        public bool IsPlaceable = true;

        /// <summary>When specified, storage may only hold items with allowed context tags.</summary>
        public IList<string> AllowList = new List<string>();

        /// <summary>When specified, storage may hold allowed items except for those with blocked context tags.</summary>
        public IList<string> BlockList = new List<string>();

        /// <summary>List of tabs to show on chest menu.</summary>
        public IList<string> Tabs;

        internal StorageContentData() : this(null) { }
        internal StorageContentData(string storageName) : base(storageName) { }
        
        protected internal bool IsAllowed(Item item) => !AllowList.Any() || AllowList.Any(item.HasContextTag);
        protected internal bool IsBlocked(Item item) => BlockList.Any() && BlockList.Any(item.HasContextTag);
        protected internal bool Filter(Item item) => IsAllowed(item) && !IsBlocked(item);

        protected internal bool HighlightMethod(Item item) =>
            Filter(item)
            && (!Enum.TryParse(SpecialChestType, out Chest.SpecialChestTypes specialChestType)
                || specialChestType != Chest.SpecialChestTypes.MiniShippingBin
                || Utility.highlightShippableObjects(item));
        
        protected internal int MenuCapacity =>
            Capacity switch
            {
                0 => -1, // Vanilla
                _ when Capacity < 0 => 72, // Unlimited
                _ => Math.Min(72, Capacity.RoundUp(12)) // Specific
            };

        protected internal int MenuRows =>
            Capacity switch
            {
                0 => 3, // Vanilla
                _ when Capacity < 0 => 6, // Unlimited
                _ => Math.Min(6, Capacity.RoundUp(12) / 12) // Specific
            };

        protected internal int MenuPadding => ShowSearchBar ? 24 : 0;
        protected internal int MenuOffset => 64 * (MenuRows - 3);
        protected internal string SummaryReport =>
            $"Loaded {StorageName} Config\n" +
            $"\tAccess Carried     : {AccessCarried}\n" +
            $"\tCarry Chest        : {CanCarry}\n" +
            $"\tDisable Automate   : {DisableAutomate}\n" +
            $"\tModded Capacity    : {Capacity}\n" +
            $"\tOpen Sound         : {OpenSound}\n" +
            $"\tSpecial Chest Type : {SpecialChestType}\n" +
            $"\tPlaceable          : {IsPlaceable}\n" +
            $"\tShow Search        : {ShowSearchBar}\n" +
            $"\tVacuum Items       : {VacuumItems}";
    }
}