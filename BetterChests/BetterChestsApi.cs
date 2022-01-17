namespace BetterChests;

using System;
using System.Collections.Generic;
using BetterChests.Enums;
using Common.Integrations.BetterChests;
using FuryCore.Services;
using Models;
using Services;
using StardewModdingAPI;

/// <inheritdoc />
public class BetterChestsApi : IBetterChestsApi
{
    private readonly Lazy<ModConfigMenu> _configMenu;

    /// <summary>
    /// Initializes a new instance of the <see cref="BetterChestsApi"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="services"></param>
    internal BetterChestsApi(ModConfig config, ServiceCollection services)
    {
        this.Config = config;
        this._configMenu = services.Lazy<ModConfigMenu>();
    }

    private ModConfig Config { get; }

    private ModConfigMenu ConfigMenu
    {
        get => this._configMenu.Value;
    }

    /// <inheritdoc />
    public bool RegisterCustomChest(string name)
    {
        if (!this.Config.ChestConfigs.TryGetValue(name, out var chestConfig))
        {
            chestConfig = new();
        }

        return true;
    }

    /// <inheritdoc />
    public bool AddGMCMOptions(string name)
    {
        if (this.Config.ChestConfigs.TryGetValue(name, out var chestConfig))
        {
            this.ConfigMenu.GenerateChestConfigOptions(chestConfig);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool AddGMCMOptions(string name, IManifest manifest, string[] options)
    {
        if (this.Config.ChestConfigs.TryGetValue(name, out var chestConfig))
        {
            this.ConfigMenu.GenerateChestConfigOptions(chestConfig, manifest, options);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool SetCapacity(string name, int capacity)
    {
        if (this.Config.ChestConfigs.TryGetValue(name, out var chestConfig))
        {
            chestConfig.Capacity = capacity;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool SetCollectItems(string name, bool enabled)
    {
        if (this.Config.ChestConfigs.TryGetValue(name, out var chestConfig))
        {
            chestConfig.CollectItems = enabled ? FeatureOption.Enabled : FeatureOption.Disabled;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool SetCraftingRange(string name, string range)
    {
        if (this.Config.ChestConfigs.TryGetValue(name, out var chestConfig) && Enum.TryParse(range, out FeatureOptionRange craftingRange))
        {
            chestConfig.CraftingRange = craftingRange;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool SetStashingRange(string name, string range)
    {
        if (this.Config.ChestConfigs.TryGetValue(name, out var chestConfig) && Enum.TryParse(range, out FeatureOptionRange stashingRange))
        {
            chestConfig.StashingRange = stashingRange;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool SetItemFilters(string name, HashSet<string> filters)
    {
        if (this.Config.ChestConfigs.TryGetValue(name, out var chestConfig))
        {
            chestConfig.FilterItems = filters;
            return true;
        }

        return false;
    }
}