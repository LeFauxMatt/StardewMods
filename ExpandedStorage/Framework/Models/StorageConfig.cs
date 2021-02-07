using System.Diagnostics.CodeAnalysis;

namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class StorageConfig
    {
        /// <summary>Modded capacity allows storing more/less than vanilla (36).</summary>
        public int Capacity;

        /// <summary>Allows storage to be access while carried.</summary>
        public bool AccessCarried;
        
        /// <summary>Allows storage to be picked up by the player.</summary>
        public bool CanCarry;

        /// <summary>Show search bar above chest inventory.</summary>
        public bool ShowSearchBar;
        
        /// <summary>Debris will be loaded straight into this chest's inventory for allowed items.</summary>
        public bool VacuumItems;
        
        internal StorageConfig(
            int capacity = 0,
            bool canCarry = true,
            bool accessCarried = false,
            bool showSearchBar = false,
            bool vacuumItems = false)
        {
            Capacity = capacity;
            CanCarry = canCarry;
            AccessCarried = accessCarried;
            ShowSearchBar = showSearchBar;
            VacuumItems = vacuumItems;
        }
        internal static StorageConfig Clone(StorageConfig config) =>
            new(
                config.Capacity,
                config.CanCarry,
                config.AccessCarried,
                config.ShowSearchBar,
                config.VacuumItems
            );
        
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