namespace BetterChests.Models
{
    using System.Collections.Generic;
    using Common.Enums;

    internal class ChestConfig
    {
        /// <summary>
        /// Gets or sets whether the <see cref="StardewValley.Objects.Chest" /> can be accessed while carried.
        /// </summary>
        public FeatureOption AccessCarried { get; set; } = FeatureOption.Default;

        /// <summary>
        /// Gets or sets the maximum number of items the <see cref="StardewValley.Objects.Chest" /> is able to hold.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets whether the <see cref="StardewValley.Objects.Chest" /> can be carried by the player.
        /// </summary>
        public FeatureOption CarryChest { get; set; } = FeatureOption.Default;

        /// <summary>
        /// Gets or sets whether the <see cref="StardewValley.Objects.Chest" /> can collect <see cref="StardewValley.Debris" />.
        /// </summary>
        public FeatureOption CollectItems { get; set; } = FeatureOption.Default;

        /// <summary>
        /// Gets or sets the range that the <see cref="StardewValley.Objects.Chest" /> can be remotely stashed into.
        /// </summary>
        public FeatureOptionRange CraftingRange { get; set; } = FeatureOptionRange.Default;

        /// <summary>
        /// Gets or sets the range that the <see cref="StardewValley.Objects.Chest" /> can be remotely crafted from.
        /// </summary>
        public FeatureOptionRange StashingRange { get; set; } = FeatureOptionRange.Default;

        /// <summary>
        /// Gets or sets items that the <see cref="StardewValley.Objects.Chest" /> can accept or will block.
        /// </summary>
        public HashSet<string> FilterItems { get; set; } = new();
    }
}