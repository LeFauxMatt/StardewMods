// ReSharper disable UnassignedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global

using System.Collections.Generic;

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

        /// <summary>When specified, storage may only hold the listed item/category IDs.</summary>
        public IList<int> AllowList = new List<int>();

        /// <summary>When specified, storage may hold all/allowed items except for listed item/category IDs.</summary>
        public IList<int> BlockList = new List<int>();

        /// <summary>List of tabs to show on chest menu.</summary>
        public IList<string> Tabs = new List<string>();

        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;
    }
}