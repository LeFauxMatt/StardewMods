namespace StardewMods.BetterChests.Framework.StorageObjects;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class ObjectStorage : Storage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ObjectStorage" /> class.
    /// </summary>
    /// <param name="obj">The source object.</param>
    /// <param name="source">The context where the source object is contained.</param>
    /// <param name="position">The position of the source object.</param>
    public ObjectStorage(SObject obj, object? source, Vector2 position)
        : base(obj, source, position)
    {
        this.Object = obj;
    }

    /// <inheritdoc />
    public override IList<Item?> Items => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Object.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.Chest.GetMutex();

    /// <summary>
    ///     Gets the source object.
    /// </summary>
    public SObject Object { get; }

    private Chest Chest => (Chest)this.Object.heldObject.Value;
}