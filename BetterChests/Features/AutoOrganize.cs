namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class AutoOrganize : Feature
{
    private readonly Lazy<OrganizeChest> _organizeChest;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoOrganize" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public AutoOrganize(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._organizeChest = services.Lazy<OrganizeChest>();
    }

    private List<KeyValuePair<IGameObjectType, IManagedStorage>> EligibleStorages
    {
        get
        {
            var storages = new List<KeyValuePair<IGameObjectType, IManagedStorage>>();
            storages.AddRange(
                from inventoryStorage in this.ManagedObjects.InventoryStorages
                where inventoryStorage.Value.AutoOrganize == FeatureOption.Enabled
                    && (inventoryStorage.Value.Context as Chest)?.SpecialChestType is null or Chest.SpecialChestTypes.JunimoChest
                select new KeyValuePair<IGameObjectType, IManagedStorage>(inventoryStorage.Key, inventoryStorage.Value));
            storages.AddRange(
                from locationStorage in this.ManagedObjects.LocationStorages
                where locationStorage.Value.AutoOrganize == FeatureOption.Enabled
                      && (locationStorage.Value.Context as Chest)?.SpecialChestType is null or Chest.SpecialChestTypes.JunimoChest
                select new KeyValuePair<IGameObjectType, IManagedStorage>(locationStorage.Key, locationStorage.Value));
            return storages;
        }
    }

    private OrganizeChest OrganizeChest
    {
        get => this._organizeChest.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Helper.Events.GameLoop.DayEnding += this.OnDayEnding;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Helper.Events.GameLoop.DayEnding -= this.OnDayEnding;
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

    private void OnDayEnding(object sender, DayEndingEventArgs e)
    {
        var allStorages = this.EligibleStorages;
        foreach (var (fromGameObjectType, fromStorage) in allStorages)
        {
            this.StashItems(allStorages, fromGameObjectType, fromStorage);
        }
    }

    private void StashItems(IEnumerable<KeyValuePair<IGameObjectType, IManagedStorage>> allStorages, IGameObjectType fromGameObjectType, IManagedStorage fromStorage)
    {
        var toStorages = (
            from storage in allStorages
            where storage.Value.StashToChest != FeatureOptionRange.Disabled
                  && storage.Value.StashToChestPriority > fromStorage.StashToChestPriority
            orderby storage.Value.StashToChestPriority descending
            select storage).ToList();

        if (!toStorages.Any())
        {
            this.OrganizeChest.OrganizeItems(fromStorage, true);
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

        this.OrganizeChest.OrganizeItems(fromStorage, true);
    }
}