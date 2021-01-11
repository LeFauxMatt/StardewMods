namespace ExpandedStorage.Framework.Models
{
    public class ExpandedStorageConfig
    {
        /// <summary>Storage Name must match the name from Json Assets.</summary>
        public string StorageName { get; set; }
        
        /// <summary>Modded capacity allows storing more/less than vanilla (36).</summary>
        public int Capacity { get; set; } = 36;

        /// <summary>Allows storage to be picked up by the player.</summary>
        public bool CanCarry { get; set; } = true;
        
        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId { get; set; }
    }
}