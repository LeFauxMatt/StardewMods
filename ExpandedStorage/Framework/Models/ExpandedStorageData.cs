namespace ExpandedStorage.Framework.Models
{
    public class ExpandedStorageData
    {
        /// <summary>Storage Name must match the name from Json Assets.</summary>
        public string StorageName { get; set; }
        
        /// <summary>Modded capacity allows storing more/less than vanilla (36).</summary>
        public int Capacity { get; set; } = 36;

        /// <summary>Allows storage to be picked up by the player.</summary>
        public bool CanCarry { get; set; } = true;
        
        /// <summary>The ParentSheetIndex as provided by Json Assets.</summary>
        internal int ParentSheetIndex { get; set; }
    }
}