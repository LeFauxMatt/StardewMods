namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.GameData.Locations;

/// <inheritdoc />
internal sealed class LocationStorageOptions : ChildStorageOptions
{
    /// <summary>Initializes a new instance of the <see cref="LocationStorageOptions" /> class.</summary>
    /// <param name="default">The default storage options.</param>
    /// <param name="data">The location data.</param>
    public LocationStorageOptions(IStorageOptions @default, LocationData data)
        : base(@default, new CustomFieldsStorageOptions(data.CustomFields)) =>
        this.Data = data;

    /// <summary>Gets the location data.</summary>
    public LocationData Data { get; }

    /// <inheritdoc />
    public override string GetDescription() => I18n.Storage_Fridge_Tooltip();

    /// <inheritdoc />
    public override string GetDisplayName() => I18n.Storage_Fridge_Name();
}