namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using Common.Helpers;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.BetterChests.Storages;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;
using StardewValley.Objects;

/// <summary>
///     Automatically organizes items between chests during sleep.
/// </summary>
internal class AutoOrganize : IFeature
{
    private AutoOrganize(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static AutoOrganize? Instance { get; set; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Initializes <see cref="AutoOrganize" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="AutoOrganize" /> class.</returns>
    public static AutoOrganize Init(IModHelper helper)
    {
        return AutoOrganize.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        this.Helper.Events.GameLoop.DayEnding += AutoOrganize.OnDayEnding;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        this.Helper.Events.GameLoop.DayEnding -= AutoOrganize.OnDayEnding;
    }

    private static void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        var storages =
            StorageHelper.Inventory
                         .Where(storage => storage.AutoOrganize == FeatureOption.Enabled && storage is not ChestStorage { Chest.SpecialChestType: Chest.SpecialChestTypes.JunimoChest })
                         .Concat(StorageHelper.World.Select(placed => placed.Storage).Where(storage => storage.AutoOrganize == FeatureOption.Enabled && storage is not ChestStorage { Chest.SpecialChestType: Chest.SpecialChestTypes.JunimoChest }))
                         .OrderByDescending(storage => storage.StashToChestPriority)
                         .ToList();

        foreach (var storage in StorageHelper.Inventory.Where(storage => storages.Contains(storage)))
        {
            AutoOrganize.StashItems(storage);
        }

        foreach (var (storage, location, position) in StorageHelper.World.Where(placed => storages.Contains(placed.Storage)))
        {
            AutoOrganize.StashItems(storage);
        }
    }

    private static void StashItems(IManagedStorage fromStorage, GameLocation fromLocation, Vector2 fromPosition, IGameObjectType toGameObjectType, IManagedStorage toStorage)
    {
        var fromText = fromLocation.Equals(Game1.player.currentLocation) && fromPosition.Equals(Game1.player.getTileLocation())
            ? $"with farmer {Game1.player.Name}"
            : $"at location {fromLocation.NameOrUniqueName} at coordinates ({((int)fromPosition.X).ToString()},{((int)fromPosition.Y).ToString()})";

        GameLocation toLocation;
        Vector2 toPosition;
        switch (toGameObjectType)
        {
            case LocationObject(var gameLocation, var position):
                toLocation = gameLocation;
                toPosition = position;
                break;
            default:
                toLocation = Game1.player.currentLocation;
                toPosition = Game1.player.getTileLocation();
                break;
        }

        switch (toStorage.StashToChest)
        {
            // Disabled if not current location for location chest
            case FeatureOptionRange.Location when !toLocation.Equals(fromLocation):
                return;
            case FeatureOptionRange.World:
            case FeatureOptionRange.Location when toStorage.StashToChestDistance == -1:
            case FeatureOptionRange.Location when Math.Abs(fromPosition.X - toPosition.X) <= toStorage.StashToChestDistance && Math.Abs(fromPosition.Y - toPosition.Y) <= toStorage.StashToChestDistance:
            case FeatureOptionRange.Inventory when fromLocation.Equals(toLocation) && fromPosition.Equals(toPosition):
                break;
            case FeatureOptionRange.Default:
            case FeatureOptionRange.Disabled:
            default:
                return;
        }

        for (var index = fromStorage.Items.Count - 1; index >= 0; index--)
        {
            var item = fromStorage.Items[index];
            if (item is null)
            {
                continue;
            }

            var stack = item.Stack;
            var tmp = toStorage.StashItem(item);
            if (tmp is not null && stack == item.Stack)
            {
                continue;
            }

            switch (toGameObjectType)
            {
                case InventoryItem(var farmer, var i):
                    Log.Info($"Item {item.Name} transferred from {fromStorage.QualifiedItemId} {fromText} to  {toStorage.QualifiedItemId} with farmer {farmer.Name} at slot {i.ToString()}.\n");
                    break;
                case LocationObject(var gameLocation, var (x, y)):
                    Log.Info($"Item {item.Name} transferred from {fromStorage.QualifiedItemId} {fromText} to  \"{toStorage.QualifiedItemId}\" at location {gameLocation.NameOrUniqueName} at coordinates ({((int)x).ToString()},{((int)y).ToString()}).");
                    break;
            }

            if (tmp is null)
            {
                fromStorage.Items.RemoveAt(index);
            }
        }
    }

    private static void StashItems(BaseStorage storage)
    {
    }

    private static void StashItems(IEnumerable<KeyValuePair<IGameObjectType, IManagedStorage>> allStorages, IGameObjectType fromGameObjectType, IManagedStorage fromStorage)
    {
        var toStorages = (
            from storage in allStorages
            where storage.Value.StashToChest != FeatureOptionRange.Disabled
                  && storage.Value.StashToChestPriority > fromStorage.StashToChestPriority
            orderby storage.Value.StashToChestPriority descending
            select storage).ToList();

        if (!toStorages.Any())
        {
            OrganizeChest.OrganizeItems(fromStorage);
            return;
        }

        GameLocation fromLocation;
        Vector2 fromPosition;
        switch (fromGameObjectType)
        {
            case LocationObject(var gameLocation, var position):
                fromLocation = gameLocation;
                fromPosition = position;
                break;
            default:
                fromLocation = Game1.player.currentLocation;
                fromPosition = Game1.player.getTileLocation();
                break;
        }

        foreach (var (toGameObjectType, toStorage) in toStorages)
        {
            AutoOrganize.StashItems(fromStorage, fromLocation, fromPosition, toGameObjectType, toStorage);
            if (fromStorage.Items.All(item => item is null))
            {
                break;
            }
        }

        OrganizeChest.OrganizeItems(fromStorage);
    }
}