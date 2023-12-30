namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(IModHelper modHelper)
        : base(modHelper) { }

    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions => this.Config.DefaultOptions;

    /// <inheritdoc />
    public int CarryChestLimit => this.Config.CarryChestLimit;

    /// <inheritdoc />
    public int CarryChestSlowLimit => this.Config.CarryChestSlowLimit;

    /// <inheritdoc />
    public Method CategorizeChestMethod => this.Config.CategorizeChestMethod;

    /// <inheritdoc />
    public Controls Controls => this.Config.Controls;

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations => this.Config.CraftFromChestDisableLocations;

    /// <inheritdoc />
    public int CraftFromChestDistance => this.Config.CraftFromChestDistance;

    /// <inheritdoc />
    public RangeOption CraftFromWorkbench => this.Config.CraftFromWorkbench;

    /// <inheritdoc />
    public int CraftFromWorkbenchDistance => this.Config.CraftFromWorkbenchDistance;

    /// <inheritdoc />
    public bool Experimental => this.Config.Experimental;

    /// <inheritdoc />
    public Method InventoryTabMethod => this.Config.InventoryTabMethod;

    /// <inheritdoc />
    public bool LabelChest => this.Config.LabelChest;

    /// <inheritdoc />
    public Option LockItem => this.Config.LockItem;

    /// <inheritdoc />
    public bool LockItemHold => this.Config.LockItemHold;

    /// <inheritdoc />
    public Method SearchItemsMethod => this.Config.SearchItemsMethod;

    /// <inheritdoc />
    public char SearchTagSymbol => this.Config.SearchTagSymbol;

    /// <inheritdoc />
    public char SearchNegationSymbol => this.Config.SearchNegationSymbol;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations => this.Config.StashToChestDisableLocations;

    /// <inheritdoc />
    public int StashToChestDistance => this.Config.StashToChestDistance;

    /// <inheritdoc />
    public bool StashToChestStacks => this.Config.StashToChestStacks;

    /// <inheritdoc />
    public bool UnloadChestSwap => this.Config.UnloadChestSwap;
}