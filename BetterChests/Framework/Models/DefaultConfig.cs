namespace StardewMods.BetterChests.Framework.Models;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <summary>Mod config data for Better Chests.</summary>
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions { get; set; } = new();

    /// <inheritdoc />
    public int CarryChestLimit { get; set; } = 3;

    /// <inheritdoc />
    public int CarryChestSlowLimit { get; set; } = 1;

    /// <inheritdoc />
    public Controls Controls { get; set; } = new();

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public RangeOption CraftFromWorkbench { get; set; } = RangeOption.Location;

    /// <inheritdoc />
    public int CraftFromWorkbenchDistance { get; set; } = -1;

    /// <inheritdoc />
    public FilterMethod InventoryTabMethod { get; set; } = FilterMethod.Hidden;

    /// <inheritdoc />
    public FeatureOption LockItem { get; set; }

    /// <inheritdoc />
    public bool LockItemHold { get; set; } = true;

    /// <inheritdoc />
    public FilterMethod SearchItemsMethod { get; set; } = FilterMethod.GrayedOut;

    /// <inheritdoc />
    public char SearchTagSymbol { get; set; } = '#';

    /// <inheritdoc />
    public char SearchNegationSymbol { get; set; } = '!';

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations { get; set; } = [];
}