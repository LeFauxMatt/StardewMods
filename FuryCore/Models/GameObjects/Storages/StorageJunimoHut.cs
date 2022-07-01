#nullable disable

namespace StardewMods.FuryCore.Models.GameObjects.Storages;

using System.Collections.Generic;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

/// <inheritdoc />
internal class StorageJunimoHut : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageJunimoHut" /> class.
    /// </summary>
    /// <param name="junimoHut">The junimo hut.</param>
    public StorageJunimoHut(JunimoHut junimoHut)
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
    public override IList<Item> Items
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