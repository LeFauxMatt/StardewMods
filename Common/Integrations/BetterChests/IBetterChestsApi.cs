namespace Common.Integrations.BetterChests;

using System.Collections.Generic;
using StardewModdingAPI;

public interface IBetterChestsApi
{
    /// <summary>
    /// Register a Big Craftable <see cref="StardewValley.Object" /> as a custom chest.
    /// </summary>
    /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
    /// <returns>True if custom chest was successfully registered.</returns>
    public bool RegisterCustomChest(string name);

    /// <summary>
    /// Adds user config options to GMCM for custom chest. 
    /// </summary>
    /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
    /// <returns>True if custom chest exists.</returns>
    public bool AddGMCMOptions(string name);

    /// <summary>
    /// Adds user config options to GMCM for custom chest. 
    /// </summary>
    /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
    /// <param name="manifest">Mod Manifest (must already be registered with GMCM).</param>
    /// <param name="options">List of config options to add.</param>
    /// <returns>True if custom chest exists.</returns>
    public bool AddGMCMOptions(string name, IManifest manifest, string[] options);

    /// <summary>
    /// Sets the maximum number of items the <see cref="StardewValley.Objects.Chest" /> is able to hold.
    /// </summary>
    /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
    /// <param name="capacity"></param>
    /// <returns>True if custom chest exists.</returns>
    public bool SetCapacity(string name, int capacity);

    /// <summary>
    /// Sets whether the <see cref="StardewValley.Objects.Chest" /> can collect <see cref="StardewValley.Debris" />.
    /// </summary>
    /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
    /// <param name="enabled">Set to true to enable or false to disable this feature.</param>
    /// <returns>True if custom chest exists.</returns>
    public bool SetCollectItems(string name, bool enabled);

    /// <summary>
    /// Sets the range that the <see cref="StardewValley.Objects.Chest" /> can be remotely crafted from.
    /// </summary>
    /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
    /// <param name="range"></param>
    /// <returns>True if custom chest exists.</returns>
    public bool SetCraftingRange(string name, string range);

    /// <summary>
    /// Sets the range that the <see cref="StardewValley.Objects.Chest" /> can be remotely stashed into.
    /// </summary>
    /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
    /// <param name="range"></param>
    /// <returns>True if custom chest exists.</returns>
    public bool SetStashingRange(string name, string range);

    /// <summary>
    /// Sets items that the <see cref="StardewValley.Objects.Chest" /> can accept or will block.
    /// </summary>
    /// <param name="name">Name that uniquely identifies the custom <see cref="StardewValley.Objects.Chest" />.</param>
    /// <param name="filters">Search terms of context tags to select allowed items.</param>
    /// <returns>True if custom chest exists.</returns>
    public bool SetItemFilters(string name, HashSet<string> filters);
}