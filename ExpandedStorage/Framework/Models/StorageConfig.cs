using ImJustMatt.ExpandedStorage.API;

namespace ImJustMatt.ExpandedStorage.Framework.Models
{
    public class StorageConfig : IStorageConfig
    {
        public int Capacity { get; set; }
        public bool AccessCarried { get; set; }
        public bool CanCarry { get; set; }
        public bool ShowSearchBar { get; set; }
        public bool VacuumItems { get; set; }
        
        internal static StorageConfig Clone(IStorageConfig config)
        {
            var newConfig = new StorageConfig();
            newConfig.CopyFrom(config);
            return newConfig;
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