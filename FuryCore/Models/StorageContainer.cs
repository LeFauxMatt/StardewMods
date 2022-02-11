namespace StardewMods.FuryCore.Models;

using System;
using System.Collections.Generic;
using StardewMods.FuryCore.Interfaces;
using StardewValley;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.IStorageContainer" />
internal class StorageContainer : GameObject, IStorageContainer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageContainer" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    /// <param name="getItems">A get method for the item in storage.</param>
    /// <param name="getModData">A get method for the mod data of the object.</param>
    public StorageContainer(object context, Func<IList<Item>> getItems, Func<ModDataDictionary> getModData)
        : base(context)
    {
        this.GetItems = getItems;
        this.GetModData = getModData;
    }

    /// <inheritdoc />
    public IList<Item> Items
    {
        get => this.GetItems.Invoke();
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.GetModData.Invoke();
    }

    private Func<IList<Item>> GetItems { get; }

    private Func<ModDataDictionary> GetModData { get; }
}