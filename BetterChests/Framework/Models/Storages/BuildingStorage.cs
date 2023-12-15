namespace StardewMods.BetterChests.Framework.Models.Storages;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Buildings;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc />
internal sealed class BuildingStorage : IStorage
{
    private readonly Building building;
    private readonly IStorage storage;

    /// <summary>Initializes a new instance of the <see cref="BuildingStorage" /> class.</summary>
    /// <param name="building">The building to which the storage is connected.</param>
    /// <param name="storage">The storage implementation.</param>
    public BuildingStorage(Building building, IStorage storage)
    {
        this.building = building;
        this.storage = storage;
    }

    /// <inheritdoc />
    public GameLocation Location => this.building.GetParentLocation();

    /// <inheritdoc />
    public Vector2 TileLocation =>
        new(
            this.building.tileX.Value + (this.building.tilesWide.Value / 2f),
            this.building.tileY.Value + (this.building.tilesHigh.Value / 2f));

    /// <inheritdoc />
    public ModDataDictionary ModData => this.building.modData;

    /// <inheritdoc />
    public Dictionary<string, string>? CustomFields => this.building.GetData().CustomFields;

    /// <inheritdoc />
    public IEnumerable<Item> Items => this.storage.Items;

    /// <inheritdoc />
    public NetMutex Mutex => this.storage.Mutex;
}
