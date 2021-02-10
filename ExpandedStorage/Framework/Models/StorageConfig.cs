using ExpandedStorage.Framework.API;

namespace ExpandedStorage.Framework.Models
{
    public class StorageConfig : IStorageConfig
    {
        public int Capacity { get; set; } = 0;
        public bool AccessCarried { get; set; } = false;
        public bool CanCarry { get; set; } = true;
        public bool ShowSearchBar { get; set; } = false;
        public bool VacuumItems { get; set; } = false;

        internal StorageConfig() { }
        internal static StorageConfig Clone(IStorageConfig config) =>
            new()
            {
                Capacity = config.Capacity,
                CanCarry = config.CanCarry,
                AccessCarried = config.AccessCarried,
                ShowSearchBar = config.ShowSearchBar,
                VacuumItems = config.VacuumItems
            };
        
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