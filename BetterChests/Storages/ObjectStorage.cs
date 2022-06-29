namespace StardewMods.BetterChests.Storages;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ObjectStorage : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ObjectStorage" /> class.
    /// </summary>
    /// <param name="obj">The source object.</param>
    /// <param name="parent">The context where the source object is contained.</param>
    /// <param name="defaultChest">Config options for <see cref="ModConfig.DefaultChest" />.</param>
    /// <param name="position">The position of the source object.</param>
    public ObjectStorage(SObject obj, object? parent, IStorageData defaultChest, Vector2 position)
        : base(obj, parent, defaultChest, position)
    {
        this.Object = obj;
    }

    /// <inheritdoc />
    public override IList<Item?> Items
    {
        get => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.Object.modData;
    }

    /// <inheritdoc />
    public override NetMutex? Mutex
    {
        get => this.Chest.GetMutex();
    }

    /// <summary>
    ///     Gets the source object.
    /// </summary>
    public SObject Object { get; }

    private Chest Chest
    {
        get => (Chest)this.Object.heldObject.Value;
    }
}