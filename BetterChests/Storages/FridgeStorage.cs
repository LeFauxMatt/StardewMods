namespace StardewMods.BetterChests.Storages;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal class FridgeStorage : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FridgeStorage" /> class.
    /// </summary>
    /// <param name="location">The farmhouse or island farmhouse location.</param>
    /// <param name="defaultChest">Config options for <see cref="ModConfig.DefaultChest" />.</param>
    /// <param name="position">The position of the source object.</param>
    public FridgeStorage(GameLocation location, IStorageData defaultChest, Vector2 position)
        : base(location, location, defaultChest, position)
    {
        this.Location = location;
    }

    /// <inheritdoc />
    public override IList<Item?> Items => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

    /// <summary>
    ///     Gets the location of the fridge.
    /// </summary>
    public GameLocation Location { get; }

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Chest.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.Chest.GetMutex();

    private Chest Chest =>
        this.Location switch
        {
            FarmHouse farmHouse => farmHouse.fridge.Value,
            IslandFarmHouse islandFarmHouse => islandFarmHouse.fridge.Value,
            _ => throw new ArgumentOutOfRangeException(),
        };
}