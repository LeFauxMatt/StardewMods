namespace StardewMods.BetterChests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewMods.FuryCore.Interfaces;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ManagedChests : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<IDictionary<Item, IManagedChest>> _cachedObjects = new(() => new Dictionary<Item, IManagedChest>());

    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedChests" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ManagedChests(IConfigModel config, IModHelper helper, IModServices services)
    {
        this.Config = config;
        this.Helper = helper;
        this._assetHandler = services.Lazy<AssetHandler>();
    }

    /// <summary>
    ///     Gets all placed chests in the world.
    /// </summary>
    public IEnumerable<KeyValuePair<KeyValuePair<GameLocation, Vector2>, IManagedChest>> PlacedChests
    {
        get
        {
            foreach (var location in this.AccessibleLocations)
            {
                // Return fridge if location is FarmHouse
                if (location is FarmHouse { fridge.Value: { } fridge, fridgePosition: var point } && !point.ToVector2().Equals(Vector2.Zero))
                {
                    if (!this.CachedObjects.TryGetValue(fridge, out var managedChest))
                    {
                        managedChest = new ManagedChest(fridge, this.GetChestData("Fridge"), "Fridge");
                        this.CachedObjects.Add(fridge, managedChest);
                    }

                    if (managedChest is not null)
                    {
                        yield return new(new(location, point.ToVector2()), managedChest);
                    }
                }

                foreach (var (position, obj) in location.Objects.Pairs)
                {
                    // Add untracked objects to cache
                    if (!this.CachedObjects.TryGetValue(obj, out var managedChest))
                    {
                        var chest = obj switch
                        {
                            Chest { Stack: 1 } playerChest when playerChest.IsPlayerChest() => playerChest,
                            { Stack: 1, heldObject.Value: Chest heldChest } when heldChest.IsPlayerChest() => heldChest,
                            _ => null,
                        };

                        if (chest is null)
                        {
                            this.CachedObjects.Add(obj, null);
                            continue;
                        }

                        var name = this.Assets.Craftables.SingleOrDefault(info => info.Key == obj.ParentSheetIndex).Value?[0];
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            this.CachedObjects.Add(obj, null);
                            continue;
                        }

                        managedChest = new ManagedChest(chest, this.GetChestData(name), name);
                        this.CachedObjects.Add(chest, managedChest);
                    }

                    if (managedChest is not null)
                    {
                        yield return new(new(location, position), managedChest);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Gets all chests in player inventory.
    /// </summary>
    public IEnumerable<IManagedChest> PlayerChests
    {
        get
        {
            foreach (var item in Game1.player.Items)
            {
                // Add untracked objects to cache
                if (!this.CachedObjects.TryGetValue(item, out var managedChest))
                {
                    var chest = item switch
                    {
                        Chest { Stack: 1 } playerChest when playerChest.IsPlayerChest() => playerChest,
                        SObject { Stack: 1, heldObject.Value: Chest heldChest } when heldChest.IsPlayerChest() => heldChest,
                        _ => null,
                    };

                    if (chest is null)
                    {
                        this.CachedObjects.Add(item, null);
                        continue;
                    }

                    var name = this.Assets.Craftables.SingleOrDefault(info => info.Key == item.ParentSheetIndex).Value?[0];
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        this.CachedObjects.Add(item, null);
                        continue;
                    }

                    managedChest = new ManagedChest(chest, this.GetChestData(name), name);
                    this.CachedObjects.Add(chest, managedChest);
                }

                if (managedChest is not null)
                {
                    yield return managedChest;
                }
            }
        }
    }

    private IEnumerable<GameLocation> AccessibleLocations
    {
        get => Context.IsMainPlayer
            ? Game1.locations.Concat(
                from location in Game1.locations.OfType<BuildableGameLocation>()
                from building in location.buildings
                where building.indoors.Value is not null
                select building.indoors.Value)
            : this.Helper.Multiplayer.GetActiveLocations();
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private IDictionary<Item, IManagedChest> CachedObjects
    {
        get => this._cachedObjects.Value;
    }

    private IDictionary<string, IChestData> ChestConfigs { get; } = new Dictionary<string, IChestData>();

    private IConfigModel Config { get; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Attempts to find a <see cref="ManagedChest" /> that matches a <see cref="Chest" /> instance.
    /// </summary>
    /// <param name="chest">The <see cref="Chest" /> to find.</param>
    /// <param name="managedChest">The <see cref="ManagedChest" /> to return if it matches the <see cref="Chest" />.</param>
    /// <returns>Returns true if a matching <see cref="ManagedChest" /> could be found.</returns>
    public bool FindChest(Chest chest, out IManagedChest managedChest)
    {
        if (chest is null)
        {
            managedChest = default;
            return false;
        }

        if (this.CachedObjects.TryGetValue(chest, out managedChest))
        {
            return managedChest is not null;
        }

        foreach (var playerChest in this.PlayerChests)
        {
            if (ReferenceEquals(playerChest.Chest, chest))
            {
                managedChest = playerChest;
                return true;
            }
        }

        foreach (var (_, placedChest) in this.PlacedChests)
        {
            if (ReferenceEquals(placedChest.Chest, chest))
            {
                managedChest = placedChest;
                return true;
            }
        }

        managedChest = default;
        return false;
    }

    private IChestData GetChestData(string name)
    {
        if (!this.ChestConfigs.TryGetValue(name, out var config))
        {
            if (!this.Assets.ChestData.TryGetValue(name, out var chestData))
            {
                chestData = new ChestData();
                this.Assets.AddChestData(name, chestData);
            }

            config = this.ChestConfigs[name] = new ChestModel(chestData, this.Config.DefaultChest);
        }

        return config;
    }
}