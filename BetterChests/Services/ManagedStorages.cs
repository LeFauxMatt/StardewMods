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
using StardewMods.BetterChests.Models.Storages;
using StardewMods.FuryCore.Interfaces;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ManagedStorages : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<IDictionary<object, IManagedStorage>> _cachedObjects = new(() => new Dictionary<object, IManagedStorage>());
    private readonly Lazy<ModIntegrations> _modIntegrations;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedStorages" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ManagedStorages(IConfigModel config, IModHelper helper, IModServices services)
    {
        this.Config = config;
        this.Helper = helper;
        this._assetHandler = services.Lazy<AssetHandler>();
        this._modIntegrations = services.Lazy<ModIntegrations>();
    }

    /// <summary>
    ///     Gets all placed chests in the world.
    /// </summary>
    public IEnumerable<KeyValuePair<KeyValuePair<GameLocation, Vector2>, IManagedStorage>> LocationStorages
    {
        get
        {
            IList<object> exclude = new List<object>();
            foreach (var location in this.AccessibleLocations)
            {
                IManagedStorage managedStorage;

                // Try loading storages from Mod Integrations
                foreach (var (gameLocation, position, chest, name) in this.Integrations.GetLocationChests(location))
                {
                    if (this.TryGetStorage(chest, name, exclude, out managedStorage))
                    {
                        yield return new(new(gameLocation, position), managedStorage);
                    }
                }

                // Try loading storages based on location
                switch (location)
                {
                    case FarmHouse farmHouse:
                        // Try loading Fridge from FarmHouse
                        if (this.TryGetStorage(farmHouse.fridge.Value, "Fridge", exclude, out managedStorage) && !farmHouse.fridgePosition.Equals(Point.Zero))
                        {
                            yield return new(new(location, farmHouse.fridgePosition.ToVector2()), managedStorage);
                        }

                        break;
                    case IslandFarmHouse islandFarmHouse:
                        // Try loading Fridge from IslandFarmHouse
                        if (this.TryGetStorage(islandFarmHouse.fridge.Value, "Fridge", exclude, out managedStorage) && !islandFarmHouse.fridgePosition.Equals(Point.Zero))
                        {
                            yield return new(new(location, islandFarmHouse.fridgePosition.ToVector2()), managedStorage);
                        }

                        break;
                }

                // Try loading storages from buildings
                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (var building in buildableLocation.buildings)
                    {
                        switch (building)
                        {
                            case JunimoHut junimoHut:
                                // Try loading JunimoHut storage
                                if (this.TryGetStorage(junimoHut, junimoHut.output.Value, "Junimo Hut", exclude, out managedStorage))
                                {
                                    yield return new(new(location, new(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2)), managedStorage);
                                }

                                break;
                        }
                    }
                }

                // Try loading storages from GameLocation.Objects
                foreach (var (position, obj) in location.Objects.Pairs)
                {
                    if (this.TryGetStorage(obj, exclude, out managedStorage))
                    {
                        yield return new(new(location, position), managedStorage);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Gets all chests in player inventory.
    /// </summary>
    public IEnumerable<IManagedStorage> PlayerStorages
    {
        get
        {
            IList<object> exclude = new List<object>();
            IManagedStorage managedStorage;

            // Try loading storages from Mod Integrations
            foreach (var (_, chest, name) in this.Integrations.GetPlayerChests(Game1.player))
            {
                if (this.TryGetStorage(chest, name, exclude, out managedStorage))
                {
                    yield return managedStorage;
                }
            }

            // Try loading storages from Player inventory
            foreach (var item in Game1.player.Items.Where(item => item is not null))
            {
                if (this.TryGetStorage(item, exclude, out managedStorage))
                {
                    yield return managedStorage;
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

    private IDictionary<object, IManagedStorage> CachedObjects
    {
        get => this._cachedObjects.Value;
    }

    private IDictionary<string, IStorageData> ChestConfigs { get; } = new Dictionary<string, IStorageData>();

    private IConfigModel Config { get; }

    private IModHelper Helper { get; }

    private ModIntegrations Integrations
    {
        get => this._modIntegrations.Value;
    }

    /// <summary>
    ///     Attempts to find a <see cref="IManagedStorage" /> that matches a storage context instance.
    /// </summary>
    /// <param name="context">The context object to find.</param>
    /// <param name="managedStorage">The <see cref="IManagedStorage" /> to return if it matches the context object.</param>
    /// <returns>Returns true if a matching <see cref="IManagedStorage" /> could be found.</returns>
    public bool FindStorage(object context, out IManagedStorage managedStorage)
    {
        if (context is null)
        {
            managedStorage = default;
            return false;
        }

        if (this.CachedObjects.TryGetValue(context, out managedStorage))
        {
            return managedStorage is not null;
        }

        foreach (var playerStorage in this.PlayerStorages)
        {
            if (ReferenceEquals(playerStorage.Context, context))
            {
                managedStorage = playerStorage;
                return true;
            }
        }

        foreach (var (_, placedStorage) in this.LocationStorages)
        {
            if (ReferenceEquals(placedStorage.Context, context))
            {
                managedStorage = placedStorage;
                return true;
            }
        }

        managedStorage = default;
        return false;
    }

    /// <summary>
    ///     Removes a no longer needed context from cached objects.
    /// </summary>
    /// <param name="context">The object context to remove.</param>
    public void RemoveStorage(object context)
    {
        if (context is not null)
        {
            this.CachedObjects.Remove(context);
        }
    }

    /// <summary>
    ///     Tries to get an IManagedStorage for a Chest.
    /// </summary>
    /// <param name="chest">The Chest to get an IManagedStorage for.</param>
    /// <param name="name">The name of the chest.</param>
    /// <param name="exclude">A list of objects to exclude to prevent infinite recursion.</param>
    /// <param name="managedStorage">The IManagedStorage if one could be returned.</param>
    /// <returns>True if a matching IManagedStorage could be found.</returns>
    public bool TryGetStorage(Chest chest, string name, ICollection<object> exclude, out IManagedStorage managedStorage)
    {
        return this.TryGetStorage(chest, chest, name, exclude, out managedStorage);
    }

    /// <summary>
    ///     Tries to get an IManagedStorage for any context object.
    /// </summary>
    /// <param name="context">The context object to get an IManagedStorage for.</param>
    /// <param name="chest">The actual chest containing the items.</param>
    /// <param name="name">The name of the chest.</param>
    /// <param name="exclude">A list of objects to exclude to prevent infinite recursion.</param>
    /// <param name="managedStorage">The IManagedStorage if one could be returned.</param>
    /// <returns>True if a matching IManagedStorage could be found.</returns>
    public bool TryGetStorage(object context, Chest chest, string name, ICollection<object> exclude, out IManagedStorage managedStorage)
    {
        if (context is null || exclude.Contains(context))
        {
            managedStorage = null;
            return false;
        }

        exclude.Add(chest);
        if (!this.CachedObjects.TryGetValue(chest, out managedStorage))
        {
            exclude.Add(context);
            if (!this.CachedObjects.TryGetValue(context, out managedStorage))
            {
                managedStorage = new StorageChest(chest, this.GetData(name), name);
                this.CachedObjects.Add(context, managedStorage);
            }

            if (!ReferenceEquals(chest, context))
            {
                exclude.Add(context);
                this.CachedObjects.Add(chest, managedStorage);
            }
        }

        return managedStorage is not null;
    }

    /// <summary>
    ///     Tries to get an IManagedStorage for any item.
    /// </summary>
    /// <param name="item">The item to get an IManagedStorage for.</param>
    /// <param name="exclude">A list of objects to exclude to prevent infinite recursion.</param>
    /// <param name="managedStorage">The IManagedStorage if one could be returned.</param>
    /// <returns>True if a matching IManagedStorage could be found.</returns>
    public bool TryGetStorage(Item item, ICollection<object> exclude, out IManagedStorage managedStorage)
    {
        if (item is null || exclude.Contains(item))
        {
            managedStorage = null;
            return false;
        }

        exclude.Add(item);
        if (!this.CachedObjects.TryGetValue(item, out managedStorage))
        {
            var chest = item switch
            {
                Chest { Stack: 1 } playerChest when playerChest.IsPlayerChest() => playerChest,
                SObject { Stack: 1, ParentSheetIndex: 165, heldObject.Value: Chest heldChest } => heldChest,
                _ => null,
            };

            if (chest is null)
            {
                this.CachedObjects.Add(item, null);
                managedStorage = null;
                return false;
            }

            if (!chest.playerChest.Value)
            {
                chest.playerChest.Value = true;
            }

            var name = this.Assets.GetStorageName(item);
            if (string.IsNullOrWhiteSpace(name))
            {
                this.CachedObjects.Add(item, null);
                managedStorage = null;
                return false;
            }

            managedStorage = new StorageChest(chest, this.GetData(name), name);
            this.CachedObjects.Add(item, managedStorage);

            if (!ReferenceEquals(chest, item) && !this.CachedObjects.ContainsKey(chest))
            {
                exclude.Add(chest);
                this.CachedObjects.Add(chest, managedStorage);
            }
        }

        return managedStorage is not null;
    }

    private IStorageData GetData(string name)
    {
        if (!this.ChestConfigs.TryGetValue(name, out var config))
        {
            if (!this.Assets.ChestData.TryGetValue(name, out var chestData))
            {
                chestData = new StorageData();
                this.Assets.AddChestData(name, chestData);
            }

            config = this.ChestConfigs[name] = new StorageModel(chestData, this.Config.DefaultChest);
        }

        return config;
    }
}