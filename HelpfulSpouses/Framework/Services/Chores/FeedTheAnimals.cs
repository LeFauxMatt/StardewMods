namespace StardewMods.HelpfulSpouses.Framework.Services.Chores;

using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewValley.Extensions;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IChore" />
internal sealed class FeedTheAnimals : BaseChore<FeedTheAnimals>
{
    private int animalsFed;

    /// <summary>Initializes a new instance of the <see cref="FeedTheAnimals" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public FeedTheAnimals(ILog log, IManifest manifest, IModConfig modConfig)
        : base(log, manifest, modConfig) { }

    /// <inheritdoc />
    public override ChoreOption Option => ChoreOption.FeedTheAnimals;

    /// <inheritdoc />
    public override void AddTokens(Dictionary<string, object> tokens)
    {
        var animals =
            Game1
                .getFarm()
                .getAllFarmAnimals()
                .Where(animal => !animal.currentLocation.HasMapPropertyWithValue("AutoFeed"))
                .ToList();

        var animal = Game1.random.ChooseFrom(animals);
        if (animal is not null)
        {
            tokens["AnimalName"] = animal.Name;
        }
    }

    /// <inheritdoc />
    public override bool IsPossibleForSpouse(NPC spouse)
    {
        var farm = Game1.getFarm();
        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction()
                || building.GetIndoors() is not AnimalHouse animalHouse
                || animalHouse.characters.Count == 0
                || animalHouse.HasMapPropertyWithValue("AutoFeed"))
            {
                continue;
            }

            var data = building.GetData();
            if (data.ValidOccupantTypes is null
                || !data.ValidOccupantTypes.Any(this.Config.FeedTheAnimals.ValidOccupantTypes.Contains))
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
        this.animalsFed = 0;
        var farm = Game1.getFarm();
        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction()
                || building.GetIndoors() is not AnimalHouse animalHouse
                || animalHouse.characters.Count == 0
                || animalHouse.HasMapPropertyWithValue("AutoFeed"))
            {
                continue;
            }

            var data = building.GetData();
            if (data.ValidOccupantTypes is null
                || !data.ValidOccupantTypes.Any(this.Config.FeedTheAnimals.ValidOccupantTypes.Contains))
            {
                continue;
            }

            animalHouse.feedAllAnimals();
            this.animalsFed += animalHouse.animals.Length;
            if (this.Config.FeedTheAnimals.AnimalLimit > 0 && this.animalsFed >= this.Config.FeedTheAnimals.AnimalLimit)
            {
                return true;
            }
        }

        return this.animalsFed > 0;
    }
}