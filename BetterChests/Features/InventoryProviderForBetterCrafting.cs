namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using Common.Integrations.BetterCrafting;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc cref="Feature" />
internal class InventoryProviderForBetterCrafting : Feature, IInventoryProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InventoryProviderForBetterCrafting" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public InventoryProviderForBetterCrafting(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this.BetterCrafting = new(this.Helper.ModRegistry);
    }

    private BetterCraftingIntegration BetterCrafting { get; }

    /// <inheritdoc />
    public bool CanExtractItems(object obj, GameLocation? location, Farmer? who)
    {
        return true;
    }

    /// <inheritdoc />
    public bool CanInsertItems(object obj, GameLocation? location, Farmer? who)
    {
        return true;
    }

    /// <inheritdoc />
    public void CleanInventory(object obj, GameLocation? location, Farmer? who)
    {
        if (obj is KeyValuePair<IGameObjectType, IManagedStorage> pair)
        {
            pair.Value.ClearNulls();
        }
    }

    /// <inheritdoc />
    public int GetActualCapacity(object obj, GameLocation? location, Farmer? who)
    {
        return obj is not KeyValuePair<IGameObjectType, IManagedStorage> pair ? 1 : pair.Value.Capacity;
    }

    /// <inheritdoc />
    public IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who)
    {
        return obj is not KeyValuePair<IGameObjectType, IManagedStorage> pair ? null : pair.Value.Items;
    }

    /// <inheritdoc />
    public Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who)
    {
        return null;
    }

    /// <inheritdoc />
    public NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who)
    {
        return obj is not KeyValuePair<IGameObjectType, IManagedStorage> { Key: LocationObject, Value.Context: Chest chest } ? null : chest.GetMutex();
    }

    /// <inheritdoc />
    public Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who)
    {
        if (obj is not KeyValuePair<IGameObjectType, IManagedStorage> { Key: LocationObject locationObject })
        {
            return null;
        }

        return locationObject.Position;
    }

    /// <inheritdoc />
    public bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item)
    {
        return obj is KeyValuePair<IGameObjectType, IManagedStorage> pair && pair.Value.ItemMatcher.Matches(item);
    }

    /// <inheritdoc />
    public bool IsMutexRequired(object obj, GameLocation? location, Farmer? who)
    {
        if (obj is not KeyValuePair<IGameObjectType, IManagedStorage> pair)
        {
            return true;
        }

        return pair.Key switch
        {
            LocationObject => true,
            InventoryItem => false,
            _ => true,
        };
    }

    /// <inheritdoc />
    public bool IsValid(object obj, GameLocation? location, Farmer? who)
    {
        return obj is KeyValuePair<IGameObjectType, IManagedStorage>;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        if (this.BetterCrafting.IsLoaded)
        {
            this.BetterCrafting.API.RegisterInventoryProvider(typeof(KeyValuePair<IGameObjectType, IManagedStorage>), this);
        }
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        if (this.BetterCrafting.IsLoaded)
        {
            this.BetterCrafting.API.UnregisterInventoryProvider(typeof(KeyValuePair<IGameObjectType, IManagedStorage>));
        }
    }
}