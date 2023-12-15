namespace StardewMods.BetterChests.Framework.Models.Storages;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.GameData.BigCraftables;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc />
internal abstract class ObjectStorage : IStorage
{
    private readonly SObject obj;

    /// <summary>Initializes a new instance of the <see cref="ObjectStorage" /> class.</summary>
    /// <param name="obj">The storage object.</param>
    protected ObjectStorage(SObject obj) => this.obj = obj;

    /// <inheritdoc />
    public GameLocation Location => this.obj.Location;

    /// <inheritdoc />
    public Vector2 TileLocation => this.obj.TileLocation;

    /// <inheritdoc />
    public ModDataDictionary ModData => this.obj.modData;

    /// <inheritdoc />
    public Dictionary<string, string>? CustomFields =>
        (ItemRegistry.GetData(this.obj.QualifiedItemId).RawData as BigCraftableData)?.CustomFields;

    /// <inheritdoc />
    public abstract IEnumerable<Item> Items { get; }

    /// <inheritdoc />
    public abstract NetMutex Mutex { get; }
}
