namespace StardewMods.FuryCore.Models.GameObjects.Terrains;

using StardewValley;
using StardewValley.TerrainFeatures;

/// <inheritdoc />
public class GenericTerrain : BaseTerrain
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GenericTerrain" /> class.
    /// </summary>
    /// <param name="terrainFeature">The source terrain feature.</param>
    public GenericTerrain(TerrainFeature terrainFeature)
        : base(terrainFeature)
    {
        this.TerrainFeature = terrainFeature;
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.TerrainFeature.modData;
    }

    private TerrainFeature TerrainFeature { get; }

    /// <inheritdoc />
    public override bool TryDropItem()
    {
        return this.TerrainFeature.performUseAction(Game1.player.getTileLocation(), Game1.currentLocation);
    }
}