using ExpandedStorage.Framework.API;

namespace ExpandedStorage.Framework.Models
{
    public class StorageConfig : IStorageConfig
    {
        internal StorageConfig()
        {
        }

        public int Capacity { get; set; }
        public bool AccessCarried { get; set; }
        public bool CanCarry { get; set; } = true;
        public bool ShowSearchBar { get; set; }
        public bool VacuumItems { get; set; }

        internal static StorageConfig Clone(IStorageConfig config)
        {
            return new()
            {
                Capacity = config.Capacity,
                CanCarry = config.CanCarry,
                AccessCarried = config.AccessCarried,
                ShowSearchBar = config.ShowSearchBar,
                VacuumItems = config.VacuumItems
            };
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