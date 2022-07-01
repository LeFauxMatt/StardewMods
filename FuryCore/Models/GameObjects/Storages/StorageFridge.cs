#nullable disable

namespace StardewMods.FuryCore.Models.GameObjects.Storages;

using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

/// <inheritdoc />
internal class StorageFridge : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageFridge" /> class.
    /// </summary>
    /// <param name="location">The farmhouse or island farmhouse location.</param>
    public StorageFridge(GameLocation location)
        : base(location)
    {
        this.Location = location;
    }

    /// <inheritdoc />
    public override int Capacity
    {
        get => this.Chest.GetActualCapacity();
    }

    /// <inheritdoc />
    public override IList<Item> Items
    {
        get => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.Location.modData;
    }

    private Chest Chest
    {
        get => this.Location switch
        {
            FarmHouse farmHouse => farmHouse.fridge.Value,
            IslandFarmHouse islandFarmHouse => islandFarmHouse.fridge.Value,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private GameLocation Location { get; }
}