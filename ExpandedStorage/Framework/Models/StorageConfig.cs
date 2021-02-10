namespace ExpandedStorage.Framework.Models
{
    public class StorageConfig
    {
        /// <summary>Modded capacity allows storing more/less than vanilla (36).</summary>
        public int Capacity { get; set; } = 0;
        
        /// <summary>Allows storage to be access while carried.</summary>
        public bool AccessCarried { get; set; } = false;
        
        /// <summary>Allows storage to be picked up by the player.</summary>
        public bool CanCarry { get; set; } = true;
        
        /// <summary>Show search bar above chest inventory.</summary>
        public bool ShowSearchBar { get; set; } = false;
        
        /// <summary>Debris will be loaded straight into this chest's inventory for allowed items.</summary>
        public bool VacuumItems { get; set; } = false;

        internal StorageConfig() { }
        internal static StorageConfig Clone(StorageConfig config) =>
            new()
            {
                Capacity = config.Capacity,
                CanCarry = config.CanCarry,
                AccessCarried = config.AccessCarried,
                ShowSearchBar = config.ShowSearchBar,
                VacuumItems = config.VacuumItems
            };
        
        internal void CopyFrom(StorageConfig config)
        {
            Capacity = config.Capacity;
            CanCarry = config.CanCarry;
            AccessCarried = config.AccessCarried;
            ShowSearchBar = config.ShowSearchBar;
            VacuumItems = config.VacuumItems;
        }
    }
}