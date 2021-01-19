using System.Collections.Generic;
using System.Linq;
using StardewValley;
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global
namespace ExpandedStorage.Framework.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ExpandedStorageConfig
    {
        /// <summary>Storage Name must match the name from Json Assets.</summary>
        public string StorageName;
        
        /// <summary>Modded capacity allows storing more/less than vanilla (36).</summary>
        public int Capacity = 36;

        /// <summary>Allows storage to be picked up by the player.</summary>
        public bool CanCarry = true;

        /// <summary>When specified, storage may only hold items with allowed context tags.</summary>
        public IList<string> AllowList = new List<string>();

        /// <summary>When specified, storage may hold allowed items except for those with blocked context tags.</summary>
        public IList<string> BlockList = new List<string>();

        /// <summary>List of tabs to show on chest menu.</summary>
        public IList<string> Tabs = new List<string>();

        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;

        public bool IsAllowed(Item item) => !AllowList.Any() || AllowList.Any(item.HasContextTag);
        public bool IsBlocked(Item item) => BlockList.Any() && BlockList.Any(item.HasContextTag);
    }
}