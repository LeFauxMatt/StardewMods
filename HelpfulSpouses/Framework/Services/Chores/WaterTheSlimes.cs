namespace StardewMods.HelpfulSpouses.Framework.Services.Chores;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IChore" />
internal sealed class WaterTheSlimes : BaseChore<WaterTheSlimes>
{
    private int slimesWatered;

    /// <summary>Initializes a new instance of the <see cref="WaterTheSlimes" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public WaterTheSlimes(ILog log, IManifest manifest, IModConfig modConfig)
        : base(log, manifest, modConfig) { }

    /// <inheritdoc />
    public override ChoreOption Option => ChoreOption.WaterTheSlimes;

    /// <inheritdoc />
    public override void AddTokens(Dictionary<string, object> tokens) => tokens["SlimesWatered"] = this.slimesWatered;

    /// <inheritdoc />
    public override bool IsPossibleForSpouse(NPC spouse)
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
    public override bool TryPerformChore(NPC spouse)
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
                if (this.Config.WaterTheSlimes.SlimeLimit > 0
                    && this.slimesWatered >= this.Config.WaterTheSlimes.SlimeLimit)
                {
                    return true;
                }
            }
        }

        return this.slimesWatered > 0;
    }
}