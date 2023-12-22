namespace StardewMods.BetterChests.Framework.Models.Storages;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.GameData.Buildings;

/// <inheritdoc />
internal sealed class BuildingStorage : ChildStorage
{
    /// <summary>Initializes a new instance of the <see cref="BuildingStorage" /> class.</summary>
    /// <param name="default">The default storage options.</param>
    /// <param name="data">The building data.</param>
    public BuildingStorage(IStorage @default, BuildingData data)
        : base(@default, new CustomFieldsStorage(data.CustomFields)) =>
        this.Data = data;

    /// <summary>Gets the building data.</summary>
    public BuildingData Data { get; }
}
