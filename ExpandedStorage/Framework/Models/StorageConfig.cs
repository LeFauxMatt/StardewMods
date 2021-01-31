using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class StorageConfig
    {
        /// <summary>Storage Name must match the name from Json Assets.</summary>
        public string StorageName;

        /// <summary>Modded capacity allows storing more/less than vanilla (36).</summary>
        public int Capacity;

        /// <summary>Allows storage to be picked up by the player.</summary>
        public bool CanCarry;
        
        /// <summary>Allows storage to be access while carried.</summary>
        public bool AccessCarried;

        /// <summary>Debris will be loaded straight into this chest's inventory for allowed items.</summary>
        public bool VacuumItems;

        /// <summary>Show search bar above chest inventory.</summary>
        public bool ShowSearchBar;
        
        /// <summary>Allows the storage to be </summary>
        public bool IsPlaceable;
        
        /// <summary>When specified, storage may only hold items with allowed context tags.</summary>
        public IList<string> AllowList;

        /// <summary>When specified, storage may hold allowed items except for those with blocked context tags.</summary>
        public IList<string> BlockList;

        /// <summary>List of tabs to show on chest menu.</summary>
        public IList<string> Tabs;
        
        internal StorageConfig() : this(null) {}
        internal StorageConfig(
            string storageName,
            int capacity = 0,
            bool canCarry = true,
            bool accessCarried = false,
            bool showSearchBar = false,
            bool isPlaceable = true,
            IList<string> allowList = null,
            IList<string> blockList = null,
            IList<string> tabs = null)
        {
            StorageName = storageName;
            Capacity = capacity;
            CanCarry = canCarry;
            AccessCarried = accessCarried;
            ShowSearchBar = showSearchBar;
            IsPlaceable = isPlaceable;
            AllowList = allowList ?? new List<string>();
            BlockList = blockList ?? new List<string>();
            Tabs = tabs ?? new List<string>();
        }
        internal static StorageConfig Clone(StorageConfig config) =>
            new StorageConfig(config.StorageName, config.Capacity, config.CanCarry, config.AccessCarried, config.ShowSearchBar, config.IsPlaceable, config.AllowList, config.BlockList, config.Tabs);
    }
}