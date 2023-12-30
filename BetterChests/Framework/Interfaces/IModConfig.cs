namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;

/// <summary>Mod config data for Better Chests.</summary>
internal interface IModConfig
{
    /// <summary>Gets a value containing the default storage options.</summary>
    public DefaultStorageOptions DefaultOptions { get; }

    /// <summary>Gets a value indicating how many chests can be carried at once.</summary>
    public int CarryChestLimit { get; }

    /// <summary>Gets a value indicating how many chests can be carried before applying a slowness effect.</summary>
    public int CarryChestSlowLimit { get; }

    /// <summary>Gets a value indicating how categorized items will be displayed.</summary>
    public Method CategorizeChestMethod { get; }

    /// <summary>Gets the control scheme.</summary>
    public Controls Controls { get; }

    /// <summary>
    /// Gets a value indicating if the chest cannot be remotely crafted from while the player is in one of the listed
    /// locations.
    /// </summary>
    public HashSet<string> CraftFromChestDisableLocations { get; }

    /// <summary>Gets a value indicating the distance in tiles that the chest can be remotely crafted from.</summary>
    public int CraftFromChestDistance { get; }

    /// <summary>Gets a value indicating the range which workbenches will craft from.</summary>
    public RangeOption CraftFromWorkbench { get; }

    /// <summary>Gets a value indicating the distance in tiles that the workbench can be remotely crafted from.</summary>
    public int CraftFromWorkbenchDistance { get; }

    /// <summary>Gets a value indicating whether experimental features will be enabled.</summary>
    public bool Experimental { get; }

    /// <summary>Gets a value indicating how tab items will be displayed.</summary>
    public Method InventoryTabMethod { get; }

    /// <summary>Gets a value indicating whether chests can be labeled.</summary>
    public bool LabelChest { get; }

    /// <summary>Gets a value indicating whether the slot lock feature is enabled.</summary>
    public Option LockItem { get; }

    /// <summary>Gets a value indicating whether the slot lock button needs to be held down.</summary>
    public bool LockItemHold { get; }

    /// <summary>Gets a value indicating how searched items will be displayed.</summary>
    public Method SearchItemsMethod { get; }

    /// <summary>Gets the symbol used to denote context tags in searches.</summary>
    public char SearchTagSymbol { get; }

    /// <summary>Gets the symbol used to denote negative searches.</summary>
    public char SearchNegationSymbol { get; }

    /// <summary>
    /// Gets a value indicating if the chest cannot be remotely crafted from while the player is in one of the listed
    /// locations.
    /// </summary>
    public HashSet<string> StashToChestDisableLocations { get; }

    /// <summary>Gets a value indicating the distance in tiles that the chest can be remotely stashed into.</summary>
    public int StashToChestDistance { get; }

    /// <summary>Gets a value indicating whether stashing into the chest will fill existing item stacks.</summary>
    public bool StashToChestStacks { get; }

    /// <summary>Gets a value indicating whether chests will swap out with unloaded chests.</summary>
    public bool UnloadChestSwap { get; }
}