namespace StardewMods.BetterChests.Storages;

using System.Collections.Generic;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

/// <inheritdoc />
internal class JunimoHutStorage : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JunimoHutStorage" /> class.
    /// </summary>
    /// <param name="junimoHut">The junimo hut.</param>
    public JunimoHutStorage(JunimoHut junimoHut)
        : base(junimoHut)
    {
        this.JunimoHut = junimoHut;
    }

    /// <inheritdoc />
    public override int Capacity
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

    private Chest Chest
    {
        get => this.JunimoHut.output.Value;
    }
}