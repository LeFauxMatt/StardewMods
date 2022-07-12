namespace StardewMods.BetterChests.Storages;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;

/// <inheritdoc />
internal class FarmerStorage : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FarmerStorage"/> class.
    /// </summary>
    /// <param name="farmer">The source farmer.</param>
    /// <param name="defaultChest">Config options for <see cref="ModConfig.DefaultChest" />.</param>
    public FarmerStorage(Farmer farmer, IStorageData defaultChest)
        : base(farmer, defaultChest)
    {
        this.Farmer = farmer;
    }

    /// <summary>
    ///     Gets the source farmer.
    /// </summary>
    public Farmer Farmer { get; }

    /// <inheritdoc/>
    public override IList<Item?> Items
    {
        get => this.Farmer.Items;
    }

    /// <inheritdoc/>
    public override ModDataDictionary ModData
    {
        get => this.Farmer.modData;
    }

    /// <inheritdoc/>
    public override object? Parent
    {
        get => this.Farmer.currentLocation;
    }

    /// <inheritdoc/>
    public override Vector2 Position
    {
        get => this.Farmer.getTileLocation();
    }
}