namespace StardewMods.BetterChests.Framework.Models.Storages;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc />
internal sealed class FarmerStorage : IStorage
{
    private readonly Farmer farmer;
    private readonly IStorage storage;

    /// <summary>Initializes a new instance of the <see cref="FarmerStorage" /> class.</summary>
    /// <param name="farmer">The farmer object associated with the storage.</param>
    /// <param name="storage">The storage object held by the farmer.</param>
    public FarmerStorage(Farmer farmer, IStorage storage)
    {
        this.farmer = farmer;
        this.storage = storage;
    }

    /// <summary
    public GameLocation Location => this.farmer.currentLocation;

    /// <inheritdoc />
    public Vector2 TileLocation => this.farmer.Tile;

    /// <inheritdoc />
    public ModDataDictionary ModData => this.storage.ModData;

    /// <inheritdoc />
    public Dictionary<string, string>? CustomFields => this.storage.CustomFields;

    /// <inheritdoc />
    public IEnumerable<Item> Items => this.storage.Items;

    /// <inheritdoc />
    public NetMutex Mutex => this.storage.Mutex;
}
