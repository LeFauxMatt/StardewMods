namespace StardewMods.HelpfulSpouses.Chores;

using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

/// <inheritdoc />
internal sealed class WaterTheCrops : IChore
{
    private readonly Config config;

    private int cropsWatered;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaterTheCrops"/> class.
    /// </summary>
    /// <param name="config">Config data for <see cref="WaterTheCrops"/>.</param>
    public WaterTheCrops(Config config)
    {
        this.config = config;
    }

    /// <inheritdoc/>
    public void AddTokens(Dictionary<string, object> tokens)
    {
        tokens["CropsWatered"] = this.cropsWatered;
    }

    /// <inheritdoc/>
    public bool IsPossibleForSpouse(NPC spouse)
    {
        var farm = Game1.getFarm();
        if (farm.IsRainingHere() || Game1.GetSeasonForLocation(farm) == Season.Winter)
        {
            return false;
        }

        var spots = new HashSet<Vector2>(farm.terrainFeatures.Pairs
            .Where(spot => spot.Value is HoeDirt hoeDirt && hoeDirt.needsWatering())
            .Select(spot => spot.Key));
        if (!spots.Any())
        {
            return false;
        }

        if (Game1.player.team.SpecialOrderActive("NO_SPRINKLER"))
        {
            return true;
        }

        foreach (var sprinkler in farm.Objects.Values.Where(@object => @object.IsSprinkler()))
        {
            var sprinklerTiles = sprinkler.GetSprinklerTiles()
                .Where(tile => farm.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "NoSprinklers", "Back") != "T");
            foreach (var tile in sprinklerTiles)
            {
                spots.Remove(tile);
            }
        }

        return spots.Any();
    }

    /// <inheritdoc/>
    public bool TryPerformChore(NPC spouse)
    {
        this.cropsWatered = 0;
        var farm = Game1.getFarm();

        var spots = farm.terrainFeatures.Values
            .OfType<HoeDirt>()
            .Where(hoeDirt => hoeDirt.needsWatering());

        foreach (var spot in spots)
        {
            spot.state.Value = HoeDirt.watered;
            this.cropsWatered++;
            if (this.config.CropLimit > 0 && this.cropsWatered >= this.config.CropLimit)
            {
                return true;
            }
        }

        return this.cropsWatered > 0;
    }

    /// <summary>
    /// Config data for <see cref="WaterTheCrops" />.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// Gets or sets the limit to the number of crops that will be watered.
        /// </summary>
        public int CropLimit { get; set; } = 0;
    }
}