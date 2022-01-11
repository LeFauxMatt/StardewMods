namespace BetterChests.Models;

using System.Collections.Generic;
using System.Linq;
using BetterChests.Enums;
using Common.Helpers.ItemMatcher;
using Interfaces;

/// <inheritdoc />
internal class ChestType : IChestConfig
{
    public ChestType(ModConfig config, string name)
    {
        if (!config.ChestConfigs.TryGetValue(name, out var chestConfig))
        {
            chestConfig = new();
        }

        this.ChestConfig = chestConfig;
        if (ChestType.Default is null && string.IsNullOrWhiteSpace(name))
        {
            this.IsDefault = true;
            ChestType.Default = this;
        }
    }

    /// <inheritdoc />
    public FeatureOption AccessCarried
    {
        get
        {
            if (this.ChestConfig.AccessCarried != FeatureOption.Default)
            {
                return this.ChestConfig.AccessCarried;
            }

            return this.IsDefault ? FeatureOption.Enabled : ChestType.Default.AccessCarried;
        }
        set => this.ChestConfig.AccessCarried = value;
    }

    /// <inheritdoc />
    public int Capacity
    {
        get
        {
            if (this.ChestConfig.Capacity != 0)
            {
                return this.ChestConfig.Capacity;
            }

            return this.IsDefault ? 60 : ChestType.Default.Capacity;
        }
        set => this.ChestConfig.Capacity = value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get
        {
            if (this.ChestConfig.CarryChest != FeatureOption.Default)
            {
                return this.ChestConfig.CarryChest;
            }

            return this.IsDefault ? FeatureOption.Enabled : ChestType.Default.CarryChest;
        }
        set => this.ChestConfig.CarryChest = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get
        {
            if (this.ChestConfig.CategorizeChest != FeatureOption.Default)
            {
                return this.ChestConfig.CategorizeChest;
            }

            return this.IsDefault ? FeatureOption.Enabled : ChestType.Default.CategorizeChest;
        }
        set => this.ChestConfig.CategorizeChest = value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get
        {
            if (this.ChestConfig.CollectItems != FeatureOption.Default)
            {
                return this.ChestConfig.CollectItems;
            }

            return this.IsDefault ? FeatureOption.Enabled : ChestType.Default.CollectItems;
        }
        set => this.ChestConfig.CollectItems = value;
    }

    /// <inheritdoc />
    public FeatureOption ColorPicker
    {
        get
        {
            if (this.ChestConfig.ColorPicker != FeatureOption.Default)
            {
                return this.ChestConfig.ColorPicker;
            }

            return this.IsDefault ? FeatureOption.Enabled : ChestType.Default.ColorPicker;
        }
        set => this.ChestConfig.ColorPicker = value;
    }

    /// <inheritdoc/>
    public FeatureOption SearchItems
    {
        get
        {
            if (this.ChestConfig.SearchItems != FeatureOption.Default)
            {
                return this.ChestConfig.SearchItems;
            }

            return this.IsDefault ? FeatureOption.Enabled : ChestType.Default.SearchItems;
        }
        set => this.ChestConfig.SearchItems = value;
    }

    /// <inheritdoc/>
    public FeatureOption VacuumItems
    {
        get
        {
            if (this.ChestConfig.VacuumItems != FeatureOption.Default)
            {
                return this.ChestConfig.VacuumItems;
            }

            return this.IsDefault ? FeatureOption.Enabled : ChestType.Default.VacuumItems;
        }
        set => this.ChestConfig.VacuumItems = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftingRange
    {
        get
        {
            if (this.ChestConfig.CraftingRange != FeatureOptionRange.Default)
            {
                return this.ChestConfig.CraftingRange;
            }

            return this.IsDefault ? FeatureOptionRange.Location : ChestType.Default.CraftingRange;
        }
        set => this.ChestConfig.CraftingRange = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange StashingRange
    {
        get
        {
            if (this.ChestConfig.StashingRange != FeatureOptionRange.Default)
            {
                return this.ChestConfig.StashingRange;
            }

            return this.IsDefault ? FeatureOptionRange.Location : ChestType.Default.StashingRange;
        }
        set => this.ChestConfig.StashingRange = value;
    }

    /// <inheritdoc />
    public HashSet<string> FilterItems
    {
        get
        {
            if (this.ChestConfig.FilterItems.Any())
            {
                return this.ChestConfig.FilterItems;
            }

            return ChestType.Default.FilterItems ??= new();
        }

        set
        {
            this.ChestConfig.FilterItems = value;
            this.ItemMatcher.Clear();
            this.ItemMatcher.UnionWith(this.FilterItems);
        }
    }

    public ItemMatcher ItemMatcher { get; } = new(true);

    private static IChestConfig Default { get; set; }

    private ChestConfig ChestConfig { get; }

    private bool IsDefault { get; }
}