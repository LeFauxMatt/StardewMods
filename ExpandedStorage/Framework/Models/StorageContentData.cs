using System.Linq;
using StardewValley;

namespace ExpandedStorage.Framework.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class StorageContentData : StorageConfig
    {
        public string OpenSound { get; set; } = "openChest";

        public string SpecialChestType { get; set; } = "None";

        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;

        internal StorageContentData() : this(null) {}
        internal StorageContentData(string storageName) : base(storageName) { }
        internal bool IsAllowed(Item item) => !AllowList.Any() || AllowList.Any(item.HasContextTag);
        internal bool IsBlocked(Item item) => BlockList.Any() && BlockList.Any(item.HasContextTag);
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