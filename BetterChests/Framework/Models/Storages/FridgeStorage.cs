namespace StardewMods.BetterChests.Framework.Models.Storages;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc />
internal sealed class FridgeStorage : IStorage
{
    private readonly IStorage storage;

    /// <summary>Initializes a new instance of the <see cref="FridgeStorage" /> class.</summary>
    /// <param name="location">The game location where the fridge storage is located.</param>
    /// <param name="storage">The storage object used for storing items in the fridge.</param>
    public FridgeStorage(GameLocation location, IStorage storage)
    {
        this.Location = location;
        this.storage = storage;
    }

    /// <inheritdoc />
    public GameLocation Location { get; }

    /// <inheritdoc />
    public Vector2 TileLocation => this.Location.GetFridgePosition()?.ToVector2() ?? Vector2.Zero;

    /// <inheritdoc />
    public ModDataDictionary ModData => this.storage.ModData;

    /// <inheritdoc />
    public Dictionary<string, string>? CustomFields => this.Location.GetData().CustomFields;

    /// <inheritdoc />
    public IEnumerable<Item> Items => this.storage.Items;

    /// <inheritdoc />
    public NetMutex Mutex => this.storage.Mutex;
}
