namespace StardewMods.BetterChests.Storages;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal class JunimoHutStorage : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JunimoHutStorage" /> class.
    /// </summary>
    /// <param name="junimoHut">The junimo hut.</param>
    /// <param name="defaultChest">Config options for <see cref="ModConfig.DefaultChest" />.</param>
    /// <param name="location">The location of the source object.</param>
    /// <param name="position">The position of the source object.</param>
    public JunimoHutStorage(JunimoHut junimoHut, IStorageData defaultChest, GameLocation? location = default, Vector2? position = default)
        : base(junimoHut, location, position, defaultChest)
    {
        this.JunimoHut = junimoHut;
    }

    /// <inheritdoc />
    public override int ActualCapacity
    {
        get => this.Chest.GetActualCapacity();
    }

    /// <inheritdoc />
    public override IList<Item?> Items
    {
        get => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
    }

    /// <summary>
    ///     Gets the Junimo Hut building.
    /// </summary>
    public JunimoHut JunimoHut { get; }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.JunimoHut.modData;
    }

    /// <inheritdoc/>
    public override NetMutex? Mutex
    {
        get => this.JunimoHut.output.Value.GetMutex();
    }

    private Chest Chest
    {
        get => this.JunimoHut.output.Value;
    }
}