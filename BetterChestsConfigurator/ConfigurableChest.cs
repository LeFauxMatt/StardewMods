namespace BetterChestsConfigurator;

using System;
using System.Collections.Generic;
using System.Linq;
using Mod.BetterChests.Enums;
using Mod.BetterChests.Helpers;
using Mod.BetterChests.Interfaces;
using Mod.BetterChests.Models;
using StardewValley.Objects;

/// <summary>
/// Class to support loading/save chest data from a chests mod data.
/// </summary>
internal class ConfigurableChest : IChestData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurableChest"/> class.
    /// </summary>
    /// <param name="chest">The chest to configure.</param>
    /// <param name="modPrefix">Prefix to use for mod data.</param>
    public ConfigurableChest(Chest chest, string modPrefix)
    {
        this.Chest = chest;
        this.ModPrefix = modPrefix;
        this.ChestData = new()
        {
            CarryChest = this.Chest.modData.TryGetValue($"{this.ModPrefix}/CarryChest", out var value) && Enum.TryParse(value, out FeatureOption option)
                ? option
                : FeatureOption.Default,
            ChestMenuTabs = this.Chest.modData.TryGetValue($"{this.ModPrefix}/ChestMenuTabs", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            CollectItems = this.Chest.modData.TryGetValue($"{this.ModPrefix}/CollectItems", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            CraftFromChest = this.Chest.modData.TryGetValue($"{this.ModPrefix}/CraftFromChest", out value) && Enum.TryParse(value, out FeatureOptionRange range)
                ? range
                : FeatureOptionRange.Default,
            CustomColorPicker = this.Chest.modData.TryGetValue($"{this.ModPrefix}/CustomColorPicker", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            FilterItems = this.Chest.modData.TryGetValue($"{this.ModPrefix}/FilterItems", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            OpenHeldChest = this.Chest.modData.TryGetValue($"{this.ModPrefix}/OpenHeldChest", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            ResizeChest = this.Chest.modData.TryGetValue($"{this.ModPrefix}/ResizeChest", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            ResizeChestMenu = this.Chest.modData.TryGetValue($"{this.ModPrefix}/ResizeChestMenu", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            SearchItems = this.Chest.modData.TryGetValue($"{this.ModPrefix}/SearchItems", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            StashToChest = this.Chest.modData.TryGetValue($"{this.ModPrefix}/StashToChest", out value) && Enum.TryParse(value, out range)
                ? range
                : FeatureOptionRange.Default,
            UnloadChest = this.Chest.modData.TryGetValue($"{this.ModPrefix}/UnloadChest", out value) && Enum.TryParse(value, out option)
                ? option
                : FeatureOption.Default,
            CraftFromChestDistance = this.Chest.modData.TryGetValue($"{this.ModPrefix}/CraftFromChestDistance", out value) && int.TryParse(value, out var distance)
                ? distance
                : 0,
            ChestMenuTabSet = this.Chest.modData.TryGetValue($"{this.ModPrefix}/ChestMenuTabSet", out value) && !string.IsNullOrWhiteSpace(value)
                ? new(value.Split(','))
                : new(),
            FilterItemsList = this.Chest.modData.TryGetValue($"{this.ModPrefix}/FilterItemsList", out value) && !string.IsNullOrWhiteSpace(value)
                ? new(value.Split(','))
                : new(),
            ResizeChestCapacity = this.Chest.modData.TryGetValue($"{this.ModPrefix}/ResizeChestCapacity", out value) && int.TryParse(value, out var capacity)
                ? capacity
                : 0,
            ResizeChestMenuRows = this.Chest.modData.TryGetValue($"{this.ModPrefix}/ResizeChestMenuRows", out value) && int.TryParse(value, out var rows)
                ? rows
                : 0,
            StashToChestDistance = this.Chest.modData.TryGetValue($"{this.ModPrefix}/StashToChestDistance", out value) && int.TryParse(value, out distance)
                ? distance
                : 0,
            StashToChestStacks = !this.Chest.modData.TryGetValue($"{this.ModPrefix}/StashToChestStacks", out value) || !bool.TryParse(value, out var stacks) || stacks,
        };
    }

    /// <inheritdoc/>
    public FeatureOption CarryChest
    {
        get => this.ChestData.CarryChest;
        set => this.ChestData.CarryChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs
    {
        get => this.ChestData.ChestMenuTabs;
        set => this.ChestData.ChestMenuTabs = value;
    }

    /// <inheritdoc/>
    public FeatureOption CollectItems
    {
        get => this.ChestData.CollectItems;
        set => this.ChestData.CollectItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest
    {
        get => this.ChestData.CraftFromChest;
        set => this.ChestData.CraftFromChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker
    {
        get => this.ChestData.CustomColorPicker;
        set => this.ChestData.CustomColorPicker = value;
    }

    /// <inheritdoc/>
    public FeatureOption FilterItems
    {
        get => this.ChestData.FilterItems;
        set => this.ChestData.FilterItems = value;
    }

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest
    {
        get => this.ChestData.OpenHeldChest;
        set => this.ChestData.OpenHeldChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChest
    {
        get => this.ChestData.ResizeChest;
        set => this.ChestData.ResizeChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu
    {
        get => this.ChestData.ResizeChestMenu;
        set => this.ChestData.ResizeChestMenu = value;
    }

    /// <inheritdoc/>
    public FeatureOption SearchItems
    {
        get => this.ChestData.SearchItems;
        set => this.ChestData.SearchItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest
    {
        get => this.ChestData.StashToChest;
        set => this.ChestData.StashToChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption UnloadChest
    {
        get => this.ChestData.UnloadChest;
        set => this.ChestData.UnloadChest = value;
    }

    /// <inheritdoc/>
    public int CraftFromChestDistance
    {
        get => this.ChestData.CraftFromChestDistance;
        set => this.ChestData.CraftFromChestDistance = value;
    }

    /// <inheritdoc/>
    public HashSet<string> ChestMenuTabSet
    {
        get => this.ChestData.ChestMenuTabSet;
        set => this.ChestData.ChestMenuTabSet = value;
    }

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList
    {
        get => this.ChestData.FilterItemsList;
        set => this.ChestData.FilterItemsList = value;
    }

    /// <inheritdoc/>
    public int ResizeChestCapacity
    {
        get => this.ChestData.ResizeChestCapacity;
        set => this.ChestData.ResizeChestCapacity = value;
    }

    /// <inheritdoc/>
    public int ResizeChestMenuRows
    {
        get => this.ChestData.ResizeChestMenuRows;
        set => this.ChestData.ResizeChestMenuRows = value;
    }

    /// <inheritdoc/>
    public int StashToChestDistance
    {
        get => this.ChestData.StashToChestDistance;
        set => this.ChestData.StashToChestDistance = value;
    }

    /// <inheritdoc/>
    public bool StashToChestStacks
    {
        get => this.ChestData.StashToChestStacks;
        set => this.ChestData.StashToChestStacks = value;
    }

    private Chest Chest { get; }

    private string ModPrefix { get; }

    private ChestData ChestData { get; }

    /// <summary>
    /// Saves Chest Data back to Chest Mod Data.
    /// </summary>
    public void Save()
    {
        // Carry Chest
        if (this.ChestData.CarryChest == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/CarryChest");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/CarryChest"] = FormatHelper.GetOptionString(this.ChestData.CarryChest);
        }

        // Chest Menu Tabs
        if (this.ChestData.ChestMenuTabs == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/ChestMenuTabs");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/ChestMenuTabs"] = FormatHelper.GetOptionString(this.ChestData.ChestMenuTabs);
        }

        // Collect Items
        if (this.ChestData.CollectItems == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/CollectItems");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/CollectItems"] = FormatHelper.GetOptionString(this.ChestData.CollectItems);
        }

        // Craft From Chest
        if (this.ChestData.CraftFromChest == FeatureOptionRange.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/CraftFromChest");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/CraftFromChest"] = FormatHelper.GetRangeString(this.ChestData.CraftFromChest);
        }

        // Custom Color Picker
        if (this.ChestData.CustomColorPicker == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/CustomColorPicker");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/CustomColorPicker"] = FormatHelper.GetOptionString(this.ChestData.CustomColorPicker);
        }

        // Filter Items
        if (this.ChestData.FilterItems == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/FilterItems");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/FilterItems"] = FormatHelper.GetOptionString(this.ChestData.FilterItems);
        }

        // Open Held Chest
        if (this.ChestData.OpenHeldChest == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/OpenHeldChest");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/OpenHeldChest"] = FormatHelper.GetOptionString(this.ChestData.OpenHeldChest);
        }

        // Resize Chest
        if (this.ChestData.ResizeChest == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/ResizeChest");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/ResizeChest"] = FormatHelper.GetOptionString(this.ChestData.ResizeChest);
        }

        // Resize Chest Menu
        if (this.ChestData.ResizeChestMenu == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/ResizeChestMenu");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/ResizeChestMenu"] = FormatHelper.GetOptionString(this.ChestData.ResizeChestMenu);
        }

        // Search Items
        if (this.ChestData.SearchItems == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/SearchItems");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/SearchItems"] = FormatHelper.GetOptionString(this.ChestData.SearchItems);
        }

        // Stash To Chest
        if (this.ChestData.StashToChest == FeatureOptionRange.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/StashToChest");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/StashToChest"] = FormatHelper.GetRangeString(this.ChestData.StashToChest);
        }

        // Unload Chest
        if (this.ChestData.UnloadChest == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/UnloadChest");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/UnloadChest"] = FormatHelper.GetOptionString(this.ChestData.UnloadChest);
        }

        // Craft From Chest Distance
        if (this.ChestData.CraftFromChestDistance == 0)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/CraftFromChestDistance");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/CraftFromChestDistance"] = this.ChestData.CraftFromChestDistance.ToString();
        }

        // Chest Menu Tab Set
        if (!this.ChestData.ChestMenuTabSet.Any())
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/ChestMenuTabSet");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/ChestMenuTabSet"] = string.Join(",", this.ChestData.ChestMenuTabSet);
        }

        // Filter Items List
        if (!this.ChestData.FilterItemsList.Any())
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/FilterItemsList");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/FilterItemsList"] = string.Join(",", this.ChestData.FilterItemsList);
        }

        // Resize Chest Capacity
        if (this.ChestData.ResizeChestCapacity == 0)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/ResizeChestCapacity");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/ResizeChestCapacity"] = this.ChestData.ResizeChestCapacity.ToString();
        }

        // Resize Chest Menu Rows
        if (this.ChestData.ResizeChestMenuRows == 0)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/ResizeChestMenuRows");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/ResizeChestMenuRows"] = this.ChestData.ResizeChestMenuRows.ToString();
        }

        // Stash to Chest Distance
        if (this.ChestData.StashToChestDistance == 0)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/StashToChestDistance");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/StashToChestDistance"] = this.ChestData.StashToChestDistance.ToString();
        }

        // Stash to Chest Stacks
        if (this.ChestData.ChestMenuTabs == FeatureOption.Default)
        {
            this.Chest.modData.Remove($"{this.ModPrefix}/CarryChest");
        }
        else
        {
            this.Chest.modData[$"{this.ModPrefix}/StashToChestStacks"] = this.ChestData.StashToChestStacks.ToString();
        }
    }
}