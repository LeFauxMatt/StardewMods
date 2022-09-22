namespace StardewMods.ExpandedStorage.Models;

using System.Collections.Generic;
using System.Linq;
using StardewMods.Common.Enums;
using StardewValley.Objects;

/// <summary>
///     Legacy data for an Expanded Storage chest.
/// </summary>
internal sealed class LegacyStorageData
{
    /// <summary>
    ///     Gets or sets a value indicating whether the chest can be opened while it's being carried in the players inventory.
    /// </summary>
    public bool AccessCarried { get; set; }

    /// <summary>
    ///     Convert this storage into the updated format.
    /// </summary>
    /// <returns>Returns the LegacyStorage in CustomStorageData format.</returns>
    public CustomStorageData AsCustomStorage
    {
        get
        {
            var betterChestsData = new BetterChestsData
            {
                CarryChest = this.CanCarry ? FeatureOption.Enabled : FeatureOption.Default,
                FilterItems = this.FilterItems.Any() ? FeatureOption.Enabled : FeatureOption.Default,
                OpenHeldChest = this.AccessCarried ? FeatureOption.Enabled : FeatureOption.Default,
                ResizeChest = this.Capacity != 0 ? FeatureOption.Enabled : FeatureOption.Default,
                ResizeChestCapacity = this.Capacity,
            };

            var customStorage = new CustomStorageData(betterChestsData)
            {
                CloseNearbySound = this.CloseNearbySound,
                Depth = this.Depth,
                Description = this.Description,
                DisplayName = this.DisplayName,
                Image = this.Image,
                IsFridge = this.IsFridge,
                IsPlaceable = this.IsPlaceable,
                ModData = this.ModData,
                OpenNearby = this.OpenNearby,
                OpenNearbySound = this.OpenNearbySound,
                OpenSound = this.OpenSound,
                PlaceSound = this.PlaceSound,
                PlayerColor = this.PlayerColor,
                PlayerConfig = this.PlayerConfig,
                SpecialChestType = this.SpecialChestType,
            };

            foreach (var (tag, enable) in this.FilterItems)
            {
                if (enable)
                {
                    betterChestsData.FilterItemsList.Add(tag);
                    continue;
                }

                betterChestsData.FilterItemsList.Add($"!{tag}");
            }

            foreach (var feature in this.EnabledFeatures)
            {
                switch (feature)
                {
                    case "AccessCarried":
                        betterChestsData.OpenHeldChest = FeatureOption.Enabled;
                        continue;
                    case "CarryChest":
                        betterChestsData.CarryChest = FeatureOption.Enabled;
                        continue;
                    case "CategorizeChest":
                        betterChestsData.Configurator = FeatureOption.Enabled;
                        continue;
                    case "ColorPicker":
                        betterChestsData.CustomColorPicker = FeatureOption.Enabled;
                        continue;
                    case "CraftFromChest":
                        betterChestsData.CraftFromChest = FeatureOptionRange.Default;
                        continue;
                    case "ExpandedMenu":
                        betterChestsData.ResizeChestMenu = FeatureOption.Enabled;
                        continue;
                    case "InventoryTabs":
                        betterChestsData.ChestMenuTabs = FeatureOption.Enabled;
                        continue;
                    case "OpenNearby":
                        continue;
                    case "SearchItems":
                        betterChestsData.SearchItems = FeatureOption.Enabled;
                        continue;
                    case "StashToChest":
                        betterChestsData.StashToChest = FeatureOptionRange.Default;
                        continue;
                    case "Unbreakable":
                        // To be implemented
                        continue;
                    case "VacuumItems":
                        betterChestsData.CollectItems = FeatureOption.Enabled;
                        continue;
                }
            }

            foreach (var feature in this.DisabledFeatures)
            {
                switch (feature)
                {
                    case "AccessCarried":
                        betterChestsData.OpenHeldChest = FeatureOption.Disabled;
                        continue;
                    case "CarryChest":
                        betterChestsData.CarryChest = FeatureOption.Disabled;
                        continue;
                    case "CategorizeChest":
                        betterChestsData.Configurator = FeatureOption.Disabled;
                        continue;
                    case "ColorPicker":
                        betterChestsData.CustomColorPicker = FeatureOption.Disabled;
                        continue;
                    case "CraftFromChest":
                        betterChestsData.CraftFromChest = FeatureOptionRange.Disabled;
                        continue;
                    case "ExpandedMenu":
                        betterChestsData.ResizeChestMenu = FeatureOption.Disabled;
                        continue;
                    case "InventoryTabs":
                        betterChestsData.ChestMenuTabs = FeatureOption.Disabled;
                        continue;
                    case "OpenNearby":
                        continue;
                    case "SearchItems":
                        betterChestsData.SearchItems = FeatureOption.Disabled;
                        continue;
                    case "StashToChest":
                        betterChestsData.StashToChest = FeatureOptionRange.Disabled;
                        continue;
                    case "Unbreakable":
                        // To be implemented
                        continue;
                    case "VacuumItems":
                        betterChestsData.CollectItems = FeatureOption.Disabled;
                        continue;
                }
            }

            return customStorage;
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the chest can be carried by the player.
    /// </summary>
    public bool CanCarry { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating the number of item stacks that the chest can hold.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    ///     Gets or sets the sound to play when the lid closing animation plays.
    /// </summary>
    public string CloseNearbySound { get; set; } = "doorCreakReverse";

    /// <summary>
    ///     Gets or sets the pixel depth.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    ///     Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the list of features which will be disabled.
    /// </summary>
    public HashSet<string> DisabledFeatures { get; set; } = new();

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the list of features which will be enabled.
    /// </summary>
    public HashSet<string> EnabledFeatures { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating what categories of items are allowed in the chest.
    /// </summary>
    public Dictionary<string, bool> FilterItems { get; set; } = new();

    /// <summary>
    ///     Gets or sets the number of frames in the lid opening animation.
    /// </summary>
    public int Frames { get; set; } = 5;

    /// <summary>
    ///     Gets or sets the texture path.
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the storage is a fridge.
    /// </summary>
    public bool IsFridge { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the storage can be placed.
    /// </summary>
    public bool IsPlaceable { get; set; } = true;

    /// <summary>
    ///     Gets or sets additional mod data that will be added to storage when it is created.
    /// </summary>
    public Dictionary<string, string> ModData { get; set; } = new();

    /// <summary>
    ///     Gets or sets the distance from the player that the storage will play it's lid opening animation.
    /// </summary>
    public float OpenNearby { get; set; }

    /// <summary>
    ///     Gets or sets the sound to play when the lid opening animation plays.
    /// </summary>
    public string OpenNearbySound { get; set; } = "doorCreak";

    /// <summary>
    ///     Gets or sets the sound to play when the storage is opened.
    /// </summary>
    public string OpenSound { get; set; } = "openChest";

    /// <summary>
    ///     Gets or sets the sound to play when the chest is placed.
    /// </summary>
    public string PlaceSound { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether player color is enabled.
    /// </summary>
    public bool PlayerColor { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether player config is enabled.
    /// </summary>
    public bool PlayerConfig { get; set; } = true;

    /// <summary>
    ///     Gets or sets the special chest type.
    /// </summary>
    public Chest.SpecialChestTypes SpecialChestType { get; set; } = Chest.SpecialChestTypes.None;

    /// <summary>
    ///     Gets or sets a value indicating whether the chest can collect dropped items.
    /// </summary>
    public bool VacuumItems { get; set; }
}