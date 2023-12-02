namespace StardewMods.BetterChests.Framework.StorageObjects;

using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Inventories;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class JunimoHutStorage : Storage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JunimoHutStorage" /> class.
    /// </summary>
    /// <param name="junimoHut">The junimo hut.</param>
    /// <param name="source">The context where the source object is contained.</param>
    /// <param name="position">The position of the source object.</param>
    public JunimoHutStorage(JunimoHut junimoHut, object? source, Vector2 position)
        : base(junimoHut, source, position)
    {
        this.JunimoHut = junimoHut;
    }

    /// <inheritdoc />
    public override IInventory Inventory => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

    /// <summary>
    ///     Gets the Junimo Hut building.
    /// </summary>
    public JunimoHut JunimoHut { get; }

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.JunimoHut.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.Chest.GetMutex();

    private Chest Chest => this.JunimoHut.GetOutputChest();
}