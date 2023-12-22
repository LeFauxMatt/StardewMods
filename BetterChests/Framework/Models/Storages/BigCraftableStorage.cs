namespace StardewMods.BetterChests.Framework.Models.Storages;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.GameData.BigCraftables;

/// <inheritdoc />
internal sealed class BigCraftableStorage : ChildStorage
{
    /// <summary>Initializes a new instance of the <see cref="BigCraftableStorage" /> class.</summary>
    /// <param name="default">The default storage options.</param>
    /// <param name="data">The big craftable data</param>
    public BigCraftableStorage(IStorage @default, BigCraftableData data)
        : base(@default, new CustomFieldsStorage(data.CustomFields)) =>
        this.Data = data;

    /// <summary>Gets the big craftable data.</summary>
    public BigCraftableData Data { get; }
}
