namespace MoreChests.Models
{
    using System.Collections.Generic;

    internal class ChestData : ChestConfig
    {
        public int Depth { get; set; }
        public string Description { get; set; }
        public HashSet<string> DisabledFeatures { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<string, bool> FilterItems { get; set; }
        public int Frames { get; set; }
        public string Image { get; set; }
        public float OpenNearby { get; set; }
        public bool PlayerColor { get; set; }
        public bool PlayerConfig { get; set; }
    }
}