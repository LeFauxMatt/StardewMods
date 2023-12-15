namespace StardewMods.BetterChests.Framework.Models.Storages;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc />
internal sealed class StoredStorage : IStorage
{
    private readonly IStorage child;
    private readonly IStorage parent;

    /// <summary>Initializes a new instance of the <see cref="StoredStorage" /> class.\</summary>
    /// <param name="parent">The parent storage where the data is initially stored.</param>
    /// <param name="child">The child storage where the data is also stored.</param>
    public StoredStorage(IStorage parent, IStorage child)
    {
        this.parent = parent;
        this.child = child;
    }

    /// <inheritdoc />
    public GameLocation Location => this.parent.Location;

    /// <inheritdoc />
    public Vector2 TileLocation => this.parent.TileLocation;

    /// <inheritdoc />
    public ModDataDictionary ModData => this.child.ModData;

    /// <inheritdoc />
    public Dictionary<string, string>? CustomFields => this.child.CustomFields;

    /// <inheritdoc />
    public IEnumerable<Item> Items => this.child.Items;

    /// <inheritdoc />
    public NetMutex Mutex => this.child.Mutex;
}
