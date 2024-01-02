namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewValley.Buildings;
using StardewValley.Mods;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class BuildingContainer : ChestContainer
{
    private readonly WeakReference<Building> building;

    /// <summary>Initializes a new instance of the <see cref="BuildingContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="building">The building to which the storage is connected.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public BuildingContainer(IStorageOptions baseOptions, Building building, Chest chest)
        : base(baseOptions, chest) =>
        this.building = new WeakReference<Building>(building);

    /// <summary>Gets the source building of the container.</summary>
    public Building Building =>
        this.building.TryGetTarget(out var target)
            ? target
            : throw new ObjectDisposedException(nameof(BuildingContainer));

    /// <inheritdoc />
    public override GameLocation Location => this.Building.GetParentLocation();

    /// <inheritdoc />
    public override Vector2 TileLocation =>
        new(
            this.Building.tileX.Value + (this.Building.tilesWide.Value / 2f),
            this.Building.tileY.Value + (this.Building.tilesHigh.Value / 2f));

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Building.modData;

    /// <summary>Gets a value indicating whether the source object is still alive.</summary>
    public override bool IsAlive => this.building.TryGetTarget(out _);
}