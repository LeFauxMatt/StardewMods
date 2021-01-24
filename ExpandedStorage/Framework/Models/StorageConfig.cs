using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal class StorageConfig
    {
        /// <summary>Storage Name must match the name from Json Assets.</summary>
        public string StorageName;

        /// <summary>Modded capacity allows storing more/less than vanilla (36).</summary>
        public int Capacity;

        /// <summary>Allows storage to be picked up by the player.</summary>
        public bool CanCarry;
        
        /// <summary>Allows the storage to be </summary>
        public bool IsPlaceable;
        
        /// <summary>When specified, storage may only hold items with allowed context tags.</summary>
        public IList<string> AllowList;

        /// <summary>When specified, storage may hold allowed items except for those with blocked context tags.</summary>
        public IList<string> BlockList;

        /// <summary>List of tabs to show on chest menu.</summary>
        public IList<string> Tabs;
        
        internal StorageConfig()
            : this(null, Chest.capacity, true, true, new List<string>(), new List<string>(), new List<string>()) { }
        internal StorageConfig(StorageConfig config)
            : this(config.StorageName, config.Capacity, config.CanCarry, config.IsPlaceable, config.AllowList, config.BlockList, config.Tabs) { }
        internal StorageConfig(string storageName, int capacity, bool canCarry, bool isPlaceable, IList<string> allowList, IList<string> blockList, IList<string> tabs)
        {
            StorageName = storageName;
            Capacity = capacity;
            CanCarry = canCarry;
            IsPlaceable = isPlaceable;
            AllowList = allowList;
            BlockList = blockList;
            Tabs = tabs;
        }
    }
}