using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using ExpandedStorage.Framework.Extensions;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ExpandedStorage.Framework.Models
{
    public enum SourceType
    {
        Unknown,
        Vanilla,
        JsonAssets,
        CustomChestTypes
    };
    
    public class Storage : StorageConfig
    {
        private static readonly HashSet<string> ExcludeModDataKeys = new()
        {
            "aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest"
        };
        
        /// <summary>Storage Name must match the name field for a BigCraftable Object.</summary>
        public string StorageName { get; set; }
        
        /// <summary>The game sound that will play when the storage is opened.</summary>
        public string OpenSound { get; set; } = "openChest";
        
        /// <summary>One of the special chest types (None, MiniShippingBin, JunimoChest).</summary>
        public string SpecialChestType { get; set; } = "None";
        
        /// <summary>Determines whether the storage type should be flagged as a Fridge.</summary>
        public bool IsFridge { get; set; } = false;
        
        /// <summary>Allows the storage to be placed in the world.</summary>
        public bool IsPlaceable { get; set; } = true;
        
        /// <summary>Add modData to placed chests (if key does not already exist).</summary>
        public IDictionary<string, string> ModData { get; set; }
        
        /// <summary>When specified, storage may only hold items with allowed context tags.</summary>
        public IList<string> AllowList { get; set; }
        
        /// <summary>When specified, storage may hold allowed items except for those with blocked context tags.</summary>
        public IList<string> BlockList { get; set; }
        
        /// <summary>List of tabs to show on chest menu.</summary>
        public IList<string> Tabs { get; set; }

        /// <summary>Which mod was used to load these assets into the game.</summary>
        internal SourceType SourceType { get; set; } = SourceType.Unknown;
        
        /// <summary>List of ParentSheetIndex related to this item.</summary>
        internal IList<int> ObjectIds = new List<int>();
        
        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;

        internal Storage() : this(null) { }
        internal Storage(string storageName)
        {
            StorageName = storageName;
            
            switch (storageName)
            {
                case "Mini-Shipping Bin":;
                    SpecialChestType = "MiniShippingBin";
                    break;
                case "Mini-Fridge":
                    IsFridge = true;
                    break;
                case "Junimo Chest":
                    SpecialChestType = "JunimoChest";
                    break;
            }
        }

        public bool MatchesContext(object context) =>
            context switch
            {
                AdventureGuild => false,
                LibraryMuseum => false,
                GameLocation => SpecialChestType == "MiniShippingBin",
                ShippingBin => SpecialChestType == "MiniShippingBin",
                JunimoHut => StorageName == "Junimo Hut",
                Chest chest when chest.fridge.Value => IsFridge,
                Object obj when obj.heldObject.Value is Chest => StorageName == "Auto-Grabber",
                Object obj when obj.bigCraftable.Value
                                && !obj.modData.Keys.Any(ExcludeModDataKeys.Contains)
                    => ObjectIds.Contains(obj.ParentSheetIndex),
                _ => false
            };
        
        private bool IsAllowed(Item item) => AllowList == null || !AllowList.Any() || AllowList.Any(item.MatchesTagExt);
        private bool IsBlocked(Item item) => BlockList != null && BlockList.Any() && BlockList.Any(item.MatchesTagExt);
        public bool Filter(Item item) => IsAllowed(item) && !IsBlocked(item);

        public bool HighlightMethod(Item item) =>
            Filter(item)
            && (!Enum.TryParse(SpecialChestType, out Chest.SpecialChestTypes specialChestType)
                || specialChestType != Chest.SpecialChestTypes.MiniShippingBin
                || Utility.highlightShippableObjects(item));
        
        internal int MenuCapacity =>
            Capacity switch
            {
                0 => -1, // Vanilla
                _ when Capacity < 0 => 72, // Unlimited
                _ => Math.Min(72, Capacity.RoundUp(12)) // Specific
            };

        internal int MenuRows =>
            Capacity switch
            {
                0 => 3, // Vanilla
                _ when Capacity < 0 => 6, // Unlimited
                _ => Math.Min(6, Capacity.RoundUp(12) / 12) // Specific
            };

        internal int MenuPadding => ShowSearchBar ? 24 : 0;
        internal int MenuOffset => 64 * (MenuRows - 3);
        internal string SummaryReport =>
            $"Loaded {StorageName} Config\n" +
            $"\tAccess Carried     : {AccessCarried}\n" +
            $"\tCarry Chest        : {CanCarry}\n" +
            $"\tModded Capacity    : {Capacity}\n" +
            $"\tOpen Sound         : {OpenSound}\n" +
            $"\tSpecial Chest Type : {SpecialChestType}\n" +
            $"\tPlaceable          : {IsPlaceable}\n" +
            $"\tShow Search        : {ShowSearchBar}\n" +
            $"\tVacuum Items       : {VacuumItems}";
    }
}