namespace StardewMods.HelpfulSpouses.Chores;

using Microsoft.Xna.Framework;

/// <inheritdoc />
internal sealed class WaterTheSlimes : IChore
{
    private readonly Config config;

    private int slimesWatered;

    /// <summary>Initializes a new instance of the <see cref="WaterTheSlimes" /> class.</summary>
    /// <param name="config">Config data for <see cref="WaterTheSlimes" />.</param>
    public WaterTheSlimes(Config config) => this.config = config;

    /// <inheritdoc />
    public void AddTokens(Dictionary<string, object> tokens) => tokens["SlimesWatered"] = this.slimesWatered;

    /// <inheritdoc />
    public bool IsPossibleForSpouse(NPC spouse)
    {
        var farm = Game1.getFarm();
        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction()
                || building.GetIndoors() is not SlimeHutch slimeHutch
                || slimeHutch.characters.Count == 0)
            {
                continue;
            }

            var spots = new HashSet<Vector2>(
                Enumerable.Range(0, slimeHutch.waterSpots.Count).Select(i => new Vector2(16f, 6 + i)).ToList());

            foreach (var sprinkler in slimeHutch.Objects.Values.Where(@object => @object.IsSprinkler()))
            {
                foreach (var tile in sprinkler.GetSprinklerTiles())
                {
                    spots.Remove(tile);
                }
            }

            if (!spots.Any())
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool TryPerformChore(NPC spouse)
    {
        this.slimesWatered = 0;
        var farm = Game1.getFarm();

        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction() || building.GetIndoors() is not SlimeHutch slimeHutch)
            {
                continue;
            }

            for (var i = 0; i < slimeHutch.waterSpots.Count; i++)
            {
                slimeHutch.waterSpots[i] = true;
                this.slimesWatered++;
                if (this.config.SlimeLimit > 0 && this.slimesWatered >= this.config.SlimeLimit)
                {
                    return true;
                }
            }
        }

        return this.slimesWatered > 0;
    }

    /// <summary>Config data for <see cref="WaterTheSlimes" />.</summary>
    public sealed class Config
    {
        /// <summary>Gets or sets the limit to the number of slimes that will be watered.</summary>
        public int SlimeLimit { get; set; }
    }
}