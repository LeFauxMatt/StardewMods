namespace Common.Integrations.BetterChests
{
    using System.Collections.Generic;

    public interface IBetterChestsApi
    {
        /// <summary>
        /// Register a Big Craftable <see cref="StardewValley.Object" /> as a custom chest.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="key">ModData key to identify the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="value">ModData value to identify the custom <see cref="StardewValley.Objects.Chest" />.</param>
        public void RegisterCustomChest(string name, string key, string value);

        /// <summary>
        /// Sets whether the <see cref="StardewValley.Objects.Chest" /> can be accessed while carried.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="enabled">Set to true to enable or false to disable this feature.</param>
        public void SetAccessCarried(string name, bool enabled);

        /// <summary>
        /// Sets the maximum number of items the <see cref="StardewValley.Objects.Chest" /> is able to hold.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="capacity"></param>
        public void SetCapacity(string name, int capacity);

        /// <summary>
        /// Sets whether the <see cref="StardewValley.Objects.Chest" /> can be carried by the player.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="enabled">Set to true to enable or false to disable this feature.</param>
        public void SetCarryChest(string name, bool enabled);

        /// <summary>
        /// Sets whether the <see cref="StardewValley.Objects.Chest" /> can collect <see cref="StardewValley.Debris" />.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="enabled">Set to true to enable or false to disable this feature.</param>
        public void SetCollectItems(string name, bool enabled);

        /// <summary>
        /// Sets the range that the <see cref="StardewValley.Objects.Chest" /> can be remotely crafted from.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="range"></param>
        public void SetCraftingRange(string name, string range);

        /// <summary>
        /// Sets the range that the <see cref="StardewValley.Objects.Chest" /> can be remotely stashed into.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="range"></param>
        public void SetStashingRange(string name, string range);

        /// <summary>
        /// Sets items that the <see cref="StardewValley.Objects.Chest" /> can accept or will block.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
        /// <param name="filters">Search terms of context tags to select allowed items.</param>
        public void SetItemFilters(string name, HashSet<string> filters);
    }
}