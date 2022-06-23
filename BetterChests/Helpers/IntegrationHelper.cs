﻿namespace StardewMods.BetterChests.Helpers;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI;
using StardewMods.BetterChests.Storages;
using StardewMods.Common.Integrations.Automate;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal class IntegrationHelper
{
    private const string HorseOverhaulId = "Goldenrevolver.HorseOverhaul";
    private const string WearMoreRingsId = "bcmpinc.WearMoreRings";

    private readonly AutomateIntegration _automate;
    private readonly BetterCraftingIntegration _betterCrafting;
    private readonly ToolbarIconsIntegration _toolbarIcons;

    private IntegrationHelper(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
        this._automate = new(helper.ModRegistry);
        this._betterCrafting = new(helper.ModRegistry);
        this._toolbarIcons = new(helper.ModRegistry);
    }

    /// <summary>
    ///     Gets Automate integration.
    /// </summary>
    public static AutomateIntegration Automate
    {
        get => IntegrationHelper.Instance!._automate;
    }

    /// <summary>
    ///     Gets Better Craft integration.
    /// </summary>
    public static BetterCraftingIntegration BetterCrafting
    {
        get => IntegrationHelper.Instance!._betterCrafting;
    }

    /// <summary>
    ///     Gets Toolbar Icons integration.
    /// </summary>
    public static ToolbarIconsIntegration ToolbarIcons
    {
        get => IntegrationHelper.Instance!._toolbarIcons;
    }

    private static IntegrationHelper? Instance { get; set; }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Gets all storages placed in a particular location.
    /// </summary>
    /// <param name="location">The location to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <returns>An enumerable of all placed storages at the location.</returns>
    public static IEnumerable<BaseStorage> FromLocation(GameLocation location, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();

        foreach (var storage in IntegrationHelper.Instance!.HorseOverhaul_FromLocation(location, excluded))
        {
            yield return storage;
        }

        if (IntegrationHelper.Instance.Helper.ModRegistry.IsLoaded(IntegrationHelper.WearMoreRingsId)
            && location is Farm
            && location.Objects.TryGetValue(new(0, -50), out var obj))
        {
            excluded.Add(obj);
        }
    }

    /// <summary>
    ///     Gets all storages placed in a particular farmer's inventory.
    /// </summary>
    /// <param name="player">The farmer to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <returns>An enumerable of all held storages in the farmer's inventory.</returns>
    public static IEnumerable<BaseStorage> FromPlayer(Farmer player, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();

        foreach (var storage in IntegrationHelper.Instance!.HorseOverhaul_FromPlayer(player, excluded))
        {
            yield return storage;
        }
    }

    /// <summary>
    ///     Initializes <see cref="IntegrationHelper" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="IntegrationHelper" /> class.</returns>
    public static IntegrationHelper Init(IModHelper helper, ModConfig config)
    {
        return IntegrationHelper.Instance ??= new(helper, config);
    }

    /// <summary>
    ///     Attempts to retrieve a storage based on a context object.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="storage">The storage object.</param>
    /// <returns>Returns true if a storage could be found for the context object.</returns>
    public static bool TryGetOne(object? context, [NotNullWhen(true)] out BaseStorage? storage)
    {
        if (IntegrationHelper.Instance!.HorseOverhaul_TryGetOne(context, out storage))
        {
            return true;
        }

        storage = default;
        return false;
    }

    private IEnumerable<BaseStorage> HorseOverhaul_FromLocation(GameLocation location, ISet<object> excluded)
    {
        if (!this.Helper.ModRegistry.IsLoaded(IntegrationHelper.HorseOverhaulId))
        {
            yield break;
        }

        var farm = Game1.getFarm();
        foreach (var stable in farm.buildings.OfType<Stable>())
        {
            if (!stable.modData.TryGetValue($"{IntegrationHelper.HorseOverhaulId}/stableID", out var stableId)
                || !int.TryParse(stableId, out var x)
                || !farm.Objects.TryGetValue(new(x, 0), out var obj)
                || obj is not Chest chest
                || !chest.modData.ContainsKey($"{IntegrationHelper.HorseOverhaulId}/isSaddleBag"))
            {
                continue;
            }

            var horse = Game1.player.mount;
            if (horse?.HorseId == stable.HorseId && Game1.player.currentLocation.Equals(location))
            {
                excluded.Add(chest);
                yield return new ChestStorage(chest, this.Config.DefaultChest, Game1.player.currentLocation, Game1.player.getTileLocation());
            }

            horse = stable.getStableHorse();
            if (horse?.getOwner() == Game1.player && horse.currentLocation.Equals(location))
            {
                excluded.Add(chest);
                yield return new ChestStorage(chest, this.Config.DefaultChest, horse.currentLocation, horse.getTileLocation());
            }
        }
    }

    private IEnumerable<BaseStorage> HorseOverhaul_FromPlayer(Farmer player, ISet<object> excluded)
    {
        if (!this.Helper.ModRegistry.IsLoaded(IntegrationHelper.HorseOverhaulId))
        {
            yield break;
        }

        if (player.mount is not null)
        {
            var farm = Game1.getFarm();
            var stable = farm.buildings
                             .OfType<Stable>()
                             .FirstOrDefault(stable => stable.HorseId == player.mount.HorseId);
            if (stable is null
                || !stable.modData.TryGetValue($"{IntegrationHelper.HorseOverhaulId}/stableID", out var stableId)
                || !int.TryParse(stableId, out var x)
                || !farm.Objects.TryGetValue(new(x, 0), out var obj)
                || obj is not Chest chest
                || !chest.modData.ContainsKey($"{IntegrationHelper.HorseOverhaulId}/isSaddleBag"))
            {
                yield break;
            }

            excluded.Add(chest);
            yield return new ChestStorage(chest, this.Config.DefaultChest, player.currentLocation, player.getTileLocation());
        }
    }

    private bool HorseOverhaul_TryGetOne(object? context, [NotNullWhen(true)] out BaseStorage? storage)
    {
        if (!this.Helper.ModRegistry.IsLoaded(IntegrationHelper.HorseOverhaulId)
            || context is not Chest chest
            || !chest.modData.ContainsKey($"{IntegrationHelper.HorseOverhaulId}/isSaddleBag"))
        {
            storage = default;
            return false;
        }

        var farm = Game1.getFarm();
        foreach (var stable in farm.buildings.OfType<Stable>())
        {
            if (!stable.modData.TryGetValue($"{IntegrationHelper.HorseOverhaulId}/stableID", out var stableId)
                || !int.TryParse(stableId, out var x)
                || !farm.Objects.TryGetValue(new(x, 0), out var obj)
                || !ReferenceEquals(chest, obj))
            {
                continue;
            }

            var horse = Game1.player.mount;
            if (horse?.HorseId == stable.HorseId)
            {
                storage = new ChestStorage(chest, this.Config.DefaultChest);
                return true;
            }

            horse = stable.getStableHorse();
            if (horse?.getOwner() == Game1.player)
            {
                storage = new ChestStorage(chest, this.Config.DefaultChest);
                return true;
            }
        }

        storage = default;
        return false;
    }
}