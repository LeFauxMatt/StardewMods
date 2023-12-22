namespace StardewMods.BetterChests.Framework.Models.Storages;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.GameData.Locations;

/// <inheritdoc />
internal sealed class LocationStorage : ChildStorage
{
    /// <summary>Initializes a new instance of the <see cref="LocationStorage" /> class.</summary>
    /// <param name="default">The default storage options.</param>
    /// <param name="data">The location data.</param>
    public LocationStorage(IStorage @default, LocationData data)
        : base(@default, new CustomFieldsStorage(data.CustomFields)) =>
        this.Data = data;

    /// <summary>Gets the location data.</summary>
    public LocationData Data { get; }

    /// <inheritdoc />
    public override string GetDescription() => I18n.Storage_Fridge_Tooltip();

    /// <inheritdoc />
    public override string GetDisplayName() => I18n.Storage_Fridge_Name();
}
