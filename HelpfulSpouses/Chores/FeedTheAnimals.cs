namespace StardewMods.HelpfulSpouses.Chores;

using StardewValley.Extensions;

/// <inheritdoc />
internal sealed class FeedTheAnimals : IChore
{
    private readonly Config config;

    private int animalsFed;

    /// <summary>Initializes a new instance of the <see cref="FeedTheAnimals" /> class.</summary>
    /// <param name="config">Config data for <see cref="FeedTheAnimals" />.</param>
    public FeedTheAnimals(Config config) => this.config = config;

    /// <inheritdoc />
    public void AddTokens(Dictionary<string, object> tokens)
    {
        var animals = Game1.getFarm().getAllFarmAnimals().Where(animal => !animal.currentLocation.HasMapPropertyWithValue("AutoFeed")).ToList();
        var animal = Game1.random.ChooseFrom(animals);
        if (animal is not null)
        {
            tokens["AnimalName"] = animal.Name;
        }
    }

    /// <inheritdoc />
    public bool IsPossibleForSpouse(NPC spouse)
    {
        var farm = Game1.getFarm();
        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction() || building.GetIndoors() is not AnimalHouse animalHouse || animalHouse.characters.Count == 0 || animalHouse.HasMapPropertyWithValue("AutoFeed"))
            {
                continue;
            }

            var data = building.GetData();
            if (data.ValidOccupantTypes is null || !data.ValidOccupantTypes.Any(this.config.ValidOccupantTypes.Contains))
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
        this.animalsFed = 0;
        var farm = Game1.getFarm();
        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction() || building.GetIndoors() is not AnimalHouse animalHouse || animalHouse.characters.Count == 0 || animalHouse.HasMapPropertyWithValue("AutoFeed"))
            {
                continue;
            }

            var data = building.GetData();
            if (data.ValidOccupantTypes is null || !data.ValidOccupantTypes.Any(this.config.ValidOccupantTypes.Contains))
            {
                continue;
            }

            animalHouse.feedAllAnimals();
            this.animalsFed += animalHouse.animals.Length;
            if (this.config.AnimalLimit > 0 && this.animalsFed >= this.config.AnimalLimit)
            {
                return true;
            }
        }

        return this.animalsFed > 0;
    }

    /// <summary>Config data for <see cref="FeedTheAnimals" />.</summary>
    public sealed class Config
    {
        /// <summary>Gets or sets the limit to the number of animals that will be fed.</summary>
        public int AnimalLimit { get; set; }

        /// <summary>Gets or sets the occupant types.</summary>
        public List<string> ValidOccupantTypes { get; set; } = new()
        {
            "Barn",
            "Coop",
        };
    }
}
