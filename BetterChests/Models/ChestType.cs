namespace BetterChests.Models;

using System.Collections.Generic;
using System.Linq;
using BetterChests.Enums;
using FuryCore.Helpers;
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
            foreach (var filterItem in this.FilterItems)
            {
                this.ItemMatcher.Add(filterItem);
            }
        }
    }

    public ItemMatcher ItemMatcher { get; } = new(true);

    private static IChestConfig Default { get; set; }

    private ChestConfig ChestConfig { get; }

    private bool IsDefault { get; }
}