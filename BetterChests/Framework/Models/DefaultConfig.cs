namespace StardewMods.BetterChests.Framework.Models;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;

/// <summary>Mod config data for Better Chests.</summary>
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public IStorageOptions DefaultOptions { get; set; } = new DefaultStorageOptions();

    /// <inheritdoc />
    public int CarryChestLimit { get; set; } = 3;

    /// <inheritdoc />
    public int CarryChestSlowLimit { get; set; } = 1;

    /// <inheritdoc />
    public Method CategorizeChestMethod { get; set; } = Method.GrayedOut;

    /// <inheritdoc />
    public Controls Controls { get; set; } = new();

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public int CraftFromChestDistance { get; set; } = -1;

    /// <inheritdoc />
    public RangeOption CraftFromWorkbench { get; set; } = RangeOption.Location;

    /// <inheritdoc />
    public int CraftFromWorkbenchDistance { get; set; } = -1;

    /// <inheritdoc />
    public bool Experimental { get; set; }

    /// <inheritdoc />
    public Method InventoryTabMethod { get; set; } = Method.Hidden;

    /// <inheritdoc />
    public bool LabelChest { get; set; } = true;

    /// <inheritdoc />
    public Option LockItem { get; set; }

    /// <inheritdoc />
    public bool LockItemHold { get; set; } = true;

    /// <inheritdoc />
    public Method SearchItemsMethod { get; set; } = Method.GrayedOut;

    /// <inheritdoc />
    public char SearchTagSymbol { get; set; } = '#';

    /// <inheritdoc />
    public char SearchNegationSymbol { get; set; } = '!';

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public int StashToChestDistance { get; set; } = -1;

    /// <inheritdoc />
    public bool StashToChestStacks { get; set; } = true;
}