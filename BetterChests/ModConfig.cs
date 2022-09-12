namespace StardewMods.BetterChests;

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;

/// <summary>
///     Mod config data.
/// </summary>
internal sealed class ModConfig : StorageData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModConfig" /> class.
    /// </summary>
    public ModConfig()
    {
        this.Reset();
    }

    /// <summary>
    ///     Gets or sets a value indicating whether shipping bin will be relaunched as a regular chest inventory menu.
    /// </summary>
    public bool BetterShippingBin { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating how many chests containing items can be carried at once.
    /// </summary>
    public int CarryChestLimit { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether carrying a chest containing items will apply a slowness effect.
    /// </summary>
    public int CarryChestSlowAmount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether chests can be searched for.
    /// </summary>
    public bool ChestFinder { get; set; }

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    public Controls ControlScheme { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating the range which workbenches will craft from.
    /// </summary>
    public FeatureOptionRange CraftFromWorkbench { get; set; } = FeatureOptionRange.Default;

    /// <summary>
    ///     Gets or sets a value indicating the distance in tiles that the workbench can be remotely crafted from.
    /// </summary>
    public int CraftFromWorkbenchDistance { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="ComponentArea" /> that the <see cref="BetterColorPicker" /> will be aligned to.
    /// </summary>
    public ComponentArea CustomColorPickerArea { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether experimental features will be enabled.
    /// </summary>
    public bool Experimental { get; set; }

    /// <summary>
    ///     Gets or sets the symbol used to denote context tags in searches.
    /// </summary>
    public char SearchTagSymbol { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock feature is enabled.
    /// </summary>
    public bool SlotLock { get; set; }

    /// <summary>
    ///     Gets or sets the color of locked slots.
    /// </summary>
    public Colors SlotLockColor { get; set; } = Colors.Red;

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock button needs to be held down.
    /// </summary>
    public bool SlotLockHold { get; set; }

    /// <summary>
    ///     Gets or sets storage data for vanilla storage types.
    /// </summary>
    public Dictionary<string, StorageData> VanillaStorages { get; set; }

    /// <summary>
    ///     Populates all default values with specific values.
    /// </summary>
    public void FillDefaults()
    {
        if (this.AutoOrganize is FeatureOption.Default)
        {
            this.AutoOrganize = FeatureOption.Disabled;
        }

        if (this.CarryChest is FeatureOption.Default)
        {
            this.CarryChest = FeatureOption.Enabled;
        }

        if (this.CarryChestSlow is FeatureOption.Default)
        {
            this.CarryChestSlow = FeatureOption.Enabled;
        }

        if (this.ChestInfo is FeatureOption.Default)
        {
            this.ChestInfo = FeatureOption.Disabled;
        }

        if (this.ChestMenuTabs is FeatureOption.Default)
        {
            this.ChestMenuTabs = FeatureOption.Enabled;
        }

        if (this.CollectItems is FeatureOption.Default)
        {
            this.CollectItems = FeatureOption.Disabled;
        }

        if (this.Configurator is FeatureOption.Default)
        {
            this.Configurator = FeatureOption.Enabled;
        }

        if (this.ConfigureMenu is InGameMenu.Default)
        {
            this.ConfigureMenu = InGameMenu.Simple;
        }

        if (this.CraftFromChest is FeatureOptionRange.Default)
        {
            this.CraftFromChest = FeatureOptionRange.Location;
        }

        if (this.CraftFromChestDistance == 0)
        {
            this.CraftFromChestDistance = -1;
        }

        if (this.CraftFromWorkbench is FeatureOptionRange.Default)
        {
            this.CraftFromWorkbench = FeatureOptionRange.Location;
        }

        if (this.CraftFromWorkbenchDistance == 0)
        {
            this.CraftFromWorkbenchDistance = -1;
        }

        if (this.CustomColorPicker is FeatureOption.Default)
        {
            this.CustomColorPicker = FeatureOption.Enabled;
        }

        if (this.CustomColorPickerArea is not (ComponentArea.Left or ComponentArea.Right))
        {
            this.CustomColorPickerArea = ComponentArea.Right;
        }

        if (this.FilterItems is FeatureOption.Default)
        {
            this.FilterItems = FeatureOption.Enabled;
        }

        if (this.HideItems is FeatureOption.Default)
        {
            this.HideItems = FeatureOption.Disabled;
        }

        if (this.LabelChest is FeatureOption.Default)
        {
            this.LabelChest = FeatureOption.Enabled;
        }

        if (this.OpenHeldChest is FeatureOption.Default)
        {
            this.OpenHeldChest = FeatureOption.Enabled;
        }

        if (this.OrganizeChest is FeatureOption.Default)
        {
            this.OrganizeChest = FeatureOption.Disabled;
        }

        if (this.ResizeChest is FeatureOption.Default)
        {
            this.ResizeChest = FeatureOption.Enabled;
        }

        if (this.ResizeChestCapacity == 0)
        {
            this.ResizeChestCapacity = 60;
        }

        if (this.ResizeChestMenu is FeatureOption.Default)
        {
            this.ResizeChestMenu = FeatureOption.Enabled;
        }

        if (this.ResizeChestMenuRows == 0)
        {
            this.ResizeChestMenuRows = 5;
        }

        if (this.SearchItems is FeatureOption.Default)
        {
            this.SearchItems = FeatureOption.Enabled;
        }

        if (string.IsNullOrWhiteSpace(this.SearchTagSymbol.ToString()))
        {
            this.SearchTagSymbol = '#';
        }

        if (this.StashToChest is FeatureOptionRange.Default)
        {
            this.StashToChest = FeatureOptionRange.Location;
        }

        if (this.StashToChestDistance == 0)
        {
            this.StashToChestDistance = -1;
        }

        if (this.StashToChestStacks is FeatureOption.Default)
        {
            this.StashToChestStacks = FeatureOption.Enabled;
        }

        if (this.TransferItems is FeatureOption.Default)
        {
            this.TransferItems = FeatureOption.Enabled;
        }

        if (this.UnloadChest is FeatureOption.Default)
        {
            this.UnloadChest = FeatureOption.Disabled;
        }

        if (this.UnloadChestCombine is FeatureOption.Default)
        {
            this.UnloadChestCombine = FeatureOption.Disabled;
        }
    }

    /// <summary>
    ///     Resets <see cref="ModConfig" /> to default options.
    /// </summary>
    [MemberNotNull(nameof(ModConfig.ControlScheme), nameof(ModConfig.VanillaStorages))]
    public void Reset()
    {
        this.FillDefaults();
        this.BetterShippingBin = true;
        this.CarryChestLimit = 1;
        this.CarryChestSlowAmount = 1;
        this.ChestFinder = true;
        this.ControlScheme = new();
        this.SlotLock = true;
        this.SlotLockColor = Colors.Red;
        this.SlotLockHold = true;
        this.VanillaStorages = new();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(" Main Config".PadLeft(50, '=')[^50..]);
        sb.AppendLine($"BetterShippingBin: {this.BetterShippingBin.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CarryChestLimit: {this.CarryChestLimit.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CarryChestSlowAmount: {this.CarryChestSlowAmount.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"ChestFinder: {this.ChestFinder.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CraftFromWorkbench: {this.CraftFromWorkbench.ToStringFast()}");
        sb.AppendLine(
            $"CraftFromWorkbenchDistance: {this.CraftFromWorkbenchDistance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CustomColorPickerArea: {this.CustomColorPickerArea.ToStringFast()}");
        sb.AppendLine($"SearchTagSymbol: {this.SearchTagSymbol.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SlotLock: {this.SlotLock.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SlotLockColor: {this.SlotLockColor.ToStringFast()}");
        sb.AppendLine($"SlotLockHold: {this.SlotLockHold.ToString(CultureInfo.InvariantCulture)}");

        sb.AppendLine(" Control Scheme".PadLeft(50, '=')[^50..]);
        sb.Append(this.ControlScheme);

        sb.AppendLine(" Default Storage".PadLeft(50, '=')[^50..]);
        sb.Append(base.ToString());

        foreach (var (key, data) in this.VanillaStorages)
        {
            var output = data.ToString();
            if (string.IsNullOrWhiteSpace(output))
            {
                continue;
            }

            sb.AppendLine($" {key}".PadLeft(50, '=')[^50..]);
            sb.Append(data);
        }

        return sb.ToString();
    }
}