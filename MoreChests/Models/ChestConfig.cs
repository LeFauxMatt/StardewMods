namespace MoreChests.Models
{
    using System.Collections.Generic;

    internal class ChestConfig
    {
        public ChestConfig(int capacity, HashSet<string> enabledFeatures)
        {
            this.Capacity = capacity;
            this.EnabledFeatures = enabledFeatures;
        }

        protected ChestConfig()
        {
        }

        public int Capacity { get; set; }

        public HashSet<string> EnabledFeatures { get; set; }
    }
}