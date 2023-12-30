namespace StardewMods.HelpfulSpouses.Framework.Services.Chores;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewValley.TerrainFeatures;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IChore" />
internal sealed class WaterTheCrops : BaseChore<WaterTheCrops>
{
    private int cropsWatered;

    /// <summary>Initializes a new instance of the <see cref="WaterTheCrops" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public WaterTheCrops(ILog log, IManifest manifest, IModConfig modConfig)
        : base(log, manifest, modConfig) { }

    /// <inheritdoc />
    public override ChoreOption Option => ChoreOption.WaterTheCrops;

    /// <inheritdoc />
    public override void AddTokens(Dictionary<string, object> tokens) => tokens["CropsWatered"] = this.cropsWatered;

    /// <inheritdoc />
    public override bool IsPossibleForSpouse(NPC spouse)
    {
        var farm = Game1.getFarm();
        if (farm.IsRainingHere() || Game1.GetSeasonForLocation(farm) == Season.Winter)
        {
            return false;
        }

        var spots = new HashSet<Vector2>(
            farm
                .terrainFeatures.Pairs.Where(spot => spot.Value is HoeDirt hoeDirt && hoeDirt.needsWatering())
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
            var sprinklerTiles = sprinkler
                .GetSprinklerTiles()
                .Where(
                    tile => farm.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "NoSprinklers", "Back") != "T");

            foreach (var tile in sprinklerTiles)
            {
                spots.Remove(tile);
            }
        }

        return spots.Any();
    }

    /// <inheritdoc />
    public override bool TryPerformChore(NPC spouse)
    {
        this.cropsWatered = 0;
        var farm = Game1.getFarm();

        var spots = farm.terrainFeatures.Values.OfType<HoeDirt>().Where(hoeDirt => hoeDirt.needsWatering());

        foreach (var spot in spots)
        {
            spot.state.Value = HoeDirt.watered;
            this.cropsWatered++;
            if (this.Config.WaterTheCrops.CropLimit > 0 && this.cropsWatered >= this.Config.WaterTheCrops.CropLimit)
            {
                return true;
            }
        }

        return this.cropsWatered > 0;
    }
}