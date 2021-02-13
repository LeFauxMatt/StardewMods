namespace ExpandedStorage.API
{
    public interface IStorageConfig
    {
        /// <summary>Modded capacity allows storing more/less than vanilla (36).</summary>
        int Capacity { get; set; }

        /// <summary>Allows storage to be access while carried.</summary>
        bool AccessCarried { get; set; }

        /// <summary>Allows storage to be picked up by the player.</summary>
        bool CanCarry { get; set; }

        /// <summary>Show search bar above chest inventory.</summary>
        bool ShowSearchBar { get; set; }

        /// <summary>Debris will be loaded straight into this chest's inventory for allowed items.</summary>
        bool VacuumItems { get; set; }
    }
}