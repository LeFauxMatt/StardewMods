using System;
using System.Collections.Generic;
using System.Linq;
using ImJustMatt.Common.Extensions;
using ImJustMatt.ExpandedStorage.API;
using ImJustMatt.ExpandedStorage.Framework.Extensions;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ImJustMatt.ExpandedStorage.Framework.Models
{
    public enum SourceType
    {
        Unknown,
        Vanilla,
        JsonAssets,
        CustomChestTypes
    }

    public class Storage : IStorage
    {
        private static readonly HashSet<string> ExcludeModDataKeys = new();

        public static readonly HashSet<string> VanillaNames = new()
        {
            "Chest",
            "Stone Chest",
            "Junimo Chest",
            "Mini-Shipping Bin",
            "Mini-Fridge"
        };

        /// <summary>List of ParentSheetIndex related to this item.</summary>
        internal readonly HashSet<int> ObjectIds = new();

        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;

        internal Storage() : this(null)
        {
        }

        internal Storage(string storageName)
        {
            //StorageName = storageName;

            switch (storageName)
            {
                case "Mini-Shipping Bin":
                    SpecialChestType = "MiniShippingBin";
                    OpenSound = "shwip";
                    break;
                case "Mini-Fridge":
                    IsFridge = true;
                    OpenSound = "doorCreak";
                    PlaceSound = "hammer";
                    break;
                case "Junimo Chest":
                    SpecialChestType = "JunimoChest";
                    break;
                case "Stone Chest":
                    PlaceSound = "hammer";
                    break;
            }
        }

        /// <summary>Which mod was used to load these assets into the game.</summary>
        internal SourceType SourceType { get; set; } = SourceType.Unknown;

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
            $"\tModded Capacity    : {Capacity}\n" +
            $"\tCarry Chest        : {CanCarry}\n" +
            $"\tAccess Carried     : {AccessCarried}\n" +
            $"\tShow Search        : {ShowSearchBar}\n" +
            $"\tVacuum Items       : {VacuumItems}\n" +
            $"\tOpen Sound         : {OpenSound}\n" +
            $"\tPlace Sound        : {PlaceSound}\n" +
            $"\tSpecial Chest Type : {SpecialChestType}\n" +
            $"\tIs Fridge          : {IsFridge}\n" +
            $"\tPlaceable          : {IsPlaceable}\n" +
            $"\tAllow list         : {string.Join(", ", AllowList)}\n" +
            $"\tBlock List         : {string.Join(", ", BlockList)}\n" +
            $"\tTabs               : {string.Join(", ", Tabs)}";

        public string OpenSound { get; set; } = "openChest";
        public string PlaceSound { get; set; } = "axe";
        public string SpecialChestType { get; set; } = "None";
        public bool IsFridge { get; set; }
        public bool IsPlaceable { get; set; } = true;
        public IDictionary<string, string> ModData { get; set; } = new Dictionary<string, string>();
        public IList<string> AllowList { get; set; } = new List<string>();
        public IList<string> BlockList { get; set; } = new List<string>();
        public IList<string> Tabs { get; set; } = new List<string>();
        public int Capacity { get; set; }
        public bool AccessCarried { get; set; }
        public bool CanCarry { get; set; } = true;
        public bool ShowSearchBar { get; set; } = true;
        public bool VacuumItems { get; set; }

        internal static void AddExclusion(string modDataKey)
        {
            if (!ExcludeModDataKeys.Contains(modDataKey))
                ExcludeModDataKeys.Add(modDataKey);
        }

        public bool MatchesContext(object context)
        {
            return context switch
            {
                Item item when item.modData.Keys.Any(ExcludeModDataKeys.Contains) => false,
                AdventureGuild => false,
                LibraryMuseum => false,
                GameLocation => SpecialChestType == "MiniShippingBin",
                ShippingBin => SpecialChestType == "MiniShippingBin",
                Chest chest when chest.fridge.Value => IsFridge,
                Object obj when obj.bigCraftable.Value => ObjectIds.Contains(obj.ParentSheetIndex),
                _ => false
            };
        }

        internal static bool IsVanillaStorage(KeyValuePair<int, string> obj)
        {
            return obj.Value.EndsWith("Chest") || VanillaNames.Any(obj.Value.StartsWith);
        }

        private bool IsAllowed(Item item)
        {
            return AllowList == null || !AllowList.Any() || AllowList.Any(item.MatchesTagExt);
        }

        private bool IsBlocked(Item item)
        {
            return BlockList != null && BlockList.Any() && BlockList.Any(item.MatchesTagExt);
        }

        public bool Filter(Item item)
        {
            return IsAllowed(item) && !IsBlocked(item);
        }

        public bool HighlightMethod(Item item)
        {
            return Filter(item)
                   && (!Enum.TryParse(SpecialChestType, out Chest.SpecialChestTypes specialChestType)
                       || specialChestType != Chest.SpecialChestTypes.MiniShippingBin
                       || Utility.highlightShippableObjects(item));
        }

        internal static Storage Clone(IStorage storage)
        {
            var newStorage = new Storage();
            newStorage.CopyFrom(storage);
            return newStorage;
        }

        internal void CopyFrom(IStorage storage)
        {
            OpenSound = OpenSound == "openChest" ? storage.OpenSound : OpenSound;
            PlaceSound = PlaceSound == "axe" ? storage.PlaceSound : PlaceSound;
            SpecialChestType = SpecialChestType == "None" ? storage.SpecialChestType : SpecialChestType;
            IsFridge = IsFridge || storage.IsFridge;
            IsPlaceable = storage.IsPlaceable;
            ModData = storage.ModData;
            AllowList = storage.AllowList;
            BlockList = storage.BlockList;
            Tabs = storage.Tabs;
            Capacity = storage.Capacity;
            CanCarry = storage.CanCarry;
            AccessCarried = storage.AccessCarried;
            ShowSearchBar = storage.ShowSearchBar;
            VacuumItems = storage.VacuumItems;
        }

        internal void CopyFrom(IStorageConfig config)
        {
            Capacity = config.Capacity;
            CanCarry = config.CanCarry;
            AccessCarried = config.AccessCarried;
            ShowSearchBar = config.ShowSearchBar;
            VacuumItems = config.VacuumItems;
        }
    }
}