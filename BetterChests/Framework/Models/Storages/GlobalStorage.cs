namespace StardewMods.BetterChests.Framework.Models.Storages;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc />
internal sealed class GlobalStorage : IStorage
{
    private readonly string id;
    private readonly IStorage storage;

    /// <summary>Initializes a new instance of the <see cref="GlobalStorage" /> class.</summary>
    /// <param name="id">The unique identifier for the global storage.</param>
    /// <param name="storage">An object that implements the IStorage interface, used for actual storage and retrieval of data.</param>
    public GlobalStorage(string id, IStorage storage)
    {
        this.id = id;
        this.storage = storage;
    }

    /// <inheritdoc />
    public GameLocation Location => this.storage.Location;

    /// <inheritdoc />
    public Vector2 TileLocation => this.storage.TileLocation;

    /// <inheritdoc />
    public ModDataDictionary ModData => this.storage.ModData;

    /// <inheritdoc />
    public Dictionary<string, string>? CustomFields => this.storage.CustomFields;

    /// <inheritdoc />
    public IEnumerable<Item> Items => Game1.player.team.GetOrCreateGlobalInventory(this.id);

    /// <inheritdoc />
    public NetMutex Mutex => this.storage.Mutex;
}
