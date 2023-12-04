namespace StardewMods.BetterChests;

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StardewMods.BetterChests.Framework.Features;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;

/// <summary>
///     Mod config data for Better Chests.
/// </summary>
internal sealed class ModConfig : IStorageData
{
    /// <inheritdoc />
    public FeatureOption AutoOrganize { get; set; } = FeatureOption.Disabled;

    /// <summary>
    ///     Gets or sets a value indicating whether shipping bin will be relaunched as a regular chest inventory menu.
    /// </summary>
    public bool BetterShippingBin { get; set; } = true;

    /// <inheritdoc />
    public FeatureOption CarryChest { get; set; } = FeatureOption.Enabled;

    /// <summary>
    ///     Gets or sets a value indicating how many chests containing items can be carried at once.
    /// </summary>
    public int CarryChestLimit { get; set; } = 1;

    /// <inheritdoc />
    public FeatureOption CarryChestSlow { get; set; } = FeatureOption.Enabled;

    /// <summary>
    ///     Gets or sets a value indicating whether carrying a chest containing items will apply a slowness effect.
    /// </summary>
    public int CarryChestSlowAmount { get; set; } = 1;

    /// <summary>
    ///     Gets or sets a value indicating whether chests can be searched for.
    /// </summary>
    public bool ChestFinder { get; set; } = true;

    /// <inheritdoc />
    public FeatureOption ChestInfo { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public string ChestLabel { get; set; } = string.Empty;

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public HashSet<string> ChestMenuTabSet { get; set; } = new();

    /// <inheritdoc />
    public FeatureOption CollectItems { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public FeatureOption Configurator { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public InGameMenu ConfigureMenu { get; set; } = InGameMenu.Simple;

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    public Controls ControlScheme { get; set; } = new();

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest { get; set; } = FeatureOptionRange.Location;

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations { get; set; } = new();

    /// <inheritdoc />
    public int CraftFromChestDistance { get; set; } = -1;

    /// <summary>
    ///     Gets or sets a value indicating the range which workbenches will craft from.
    /// </summary>
    public FeatureOptionRange CraftFromWorkbench { get; set; } = FeatureOptionRange.Location;

    /// <summary>
    ///     Gets or sets a value indicating the distance in tiles that the workbench can be remotely crafted from.
    /// </summary>
    public int CraftFromWorkbenchDistance { get; set; } = -1;

    /// <inheritdoc />
    public FeatureOption CustomColorPicker { get; set; } = FeatureOption.Enabled;

    /// <summary>
    ///     Gets or sets the <see cref="ComponentArea" /> that the <see cref="BetterColorPicker" /> will be aligned to.
    /// </summary>
    public ComponentArea CustomColorPickerArea { get; set; } = ComponentArea.Right;

    /// <summary>
    ///     Gets or sets a value indicating whether experimental features will be enabled.
    /// </summary>
    public bool Experimental { get; set; }

    /// <inheritdoc />
    public FeatureOption FilterItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public HashSet<string> FilterItemsList { get; set; } = new();

    /// <inheritdoc />
    public FeatureOption HideItems { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public FeatureOption LabelChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption OpenHeldChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption OrganizeChest { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy { get; set; } = GroupBy.Default;

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy { get; set; } = SortBy.Default;

    /// <inheritdoc />
    public FeatureOption ResizeChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public int ResizeChestCapacity { get; set; } = 60;

    /// <inheritdoc />
    public FeatureOption SearchItems { get; set; } = FeatureOption.Enabled;

    /// <summary>
    ///     Gets or sets the symbol used to denote context tags in searches.
    /// </summary>
    public char SearchTagSymbol { get; set; } = '#';

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock feature is enabled.
    /// </summary>
    public bool SlotLock { get; set; } = true;

    /// <summary>
    ///     Gets or sets the color of locked slots.
    /// </summary>
    public string SlotLockColor { get; set; } = "Red";

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock button needs to be held down.
    /// </summary>
    public bool SlotLockHold { get; set; } = true;

    /// <inheritdoc />
    public FeatureOptionRange StashToChest { get; set; } = FeatureOptionRange.Location;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations { get; set; } = new();

    /// <inheritdoc />
    public int StashToChestDistance { get; set; } = -1;

    /// <inheritdoc />
    public int StashToChestPriority { get; set; }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption TransferItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption UnloadChest { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public FeatureOption UnloadChestCombine { get; set; } = FeatureOption.Disabled;

    /// <summary>
    ///     Gets or sets storage data for vanilla storage types.
    /// </summary>
    public Dictionary<string, StorageData> VanillaStorages { get; set; } = new();

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(" Main Config".PadLeft(50, '=')[^50..]);
        sb.AppendLine($"CarryChestLimit: {this.CarryChestLimit.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CarryChestSlowAmount: {this.CarryChestSlowAmount.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"ChestFinder: {this.ChestFinder.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CraftFromWorkbench: {this.CraftFromWorkbench.ToStringFast()}");
        sb.AppendLine($"CraftFromWorkbenchDistance: {this.CraftFromWorkbenchDistance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CustomColorPickerArea: {this.CustomColorPickerArea.ToStringFast()}");
        sb.AppendLine($"SearchTagSymbol: {this.SearchTagSymbol.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SlotLock: {this.SlotLock.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SlotLockColor: {this.SlotLockColor}");
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