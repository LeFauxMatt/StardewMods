namespace StardewMods.HelpfulSpouses.Chores;

using StardewValley.Extensions;

/// <inheritdoc />
internal sealed class PetTheAnimals : IChore
{
    private readonly Config config;

    private int animalsPetted;

    /// <summary>
    /// Initializes a new instance of the <see cref="PetTheAnimals"/> class.
    /// </summary>
    /// <param name="config">Config data for <see cref="PetTheAnimals"/>.</param>
    public PetTheAnimals(Config config) => this.config = config;

    /// <inheritdoc/>
    public void AddTokens(Dictionary<string, object> tokens)
    {
        var animals = Game1.getFarm()
            .getAllFarmAnimals()
            .Where(animal => !animal.wasAutoPet.Value)
            .ToList();
        var animal = Game1.random.ChooseFrom(animals);
        if (animal is not null)
        {
            tokens["AnimalName"] = animal.Name;
        }

        tokens["AnimalsPetted"] = this.animalsPetted;
    }

    /// <inheritdoc/>
    public bool IsPossibleForSpouse(NPC spouse)
    {
        var farm = Game1.getFarm();
        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction()
                || building.GetIndoors() is not AnimalHouse animalHouse
                || animalHouse.characters.Count == 0)
            {
                continue;
            }

            var data = building.GetData();
            if (data.ValidOccupantTypes is null
                || !data.ValidOccupantTypes.Any(this.config.ValidOccupantTypes.Contains))
            {
                continue;
            }

            if (animalHouse.Objects.Values.Any(@object => @object.QualifiedItemId == "(BC)272"))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool TryPerformChore(NPC spouse)
    {
        this.animalsPetted = 0;
        var farm = Game1.getFarm();
        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction()
                || building.GetIndoors() is not AnimalHouse animalHouse)
            {
                continue;
            }

            var data = building.GetData();
            if (data.ValidOccupantTypes is null
                || !data.ValidOccupantTypes.Any(this.config.ValidOccupantTypes.Contains))
            {
                continue;
            }

            if (animalHouse.Objects.Values.Any(@object => @object.QualifiedItemId == "(BC)272"))
            {
                continue;
            }

            foreach (var animal in animalHouse.animals.Values)
            {
                animal.pet(Game1.player);
                this.animalsPetted++;
                if (this.config.AnimalLimit > 0 && this.animalsPetted >= this.config.AnimalLimit)
                {
                    return true;
                }
            }
        }

        return this.animalsPetted > 0;
    }

    /// <summary>
    /// Config data for <see cref="PetTheAnimals" />.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// Gets or sets the limit to the number of animals that will be pet.
        /// </summary>
        public int AnimalLimit { get; set; } = 0;

        /// <summary>
        /// Gets or sets the occupant types.
        /// </summary>
        public List<string> ValidOccupantTypes { get; set; } = new()
        {
            "Barn",
            "Coop",
        };
    }
}
