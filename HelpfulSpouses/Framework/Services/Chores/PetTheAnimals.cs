namespace StardewMods.HelpfulSpouses.Framework.Services.Chores;

using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewValley.Extensions;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IChore" />
internal sealed class PetTheAnimals : BaseChore<PetTheAnimals>
{
    private int animalsPetted;

    /// <summary>Initializes a new instance of the <see cref="PetTheAnimals" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public PetTheAnimals(ILog log, IManifest manifest, IModConfig modConfig)
        : base(log, manifest, modConfig) { }

    /// <inheritdoc />
    public override ChoreOption Option => ChoreOption.PetTheAnimals;

    /// <inheritdoc />
    public override void AddTokens(Dictionary<string, object> tokens)
    {
        var animals = Game1.getFarm().getAllFarmAnimals().Where(animal => !animal.wasAutoPet.Value).ToList();
        var animal = Game1.random.ChooseFrom(animals);
        if (animal is not null)
        {
            tokens["AnimalName"] = animal.Name;
        }

        tokens["AnimalsPetted"] = this.animalsPetted;
    }

    /// <inheritdoc />
    public override bool IsPossibleForSpouse(NPC spouse)
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
                || !data.ValidOccupantTypes.Any(this.Config.PetTheAnimals.ValidOccupantTypes.Contains))
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

    /// <inheritdoc />
    public override bool TryPerformChore(NPC spouse)
    {
        this.animalsPetted = 0;
        var farm = Game1.getFarm();
        foreach (var building in farm.buildings)
        {
            if (building.isUnderConstruction() || building.GetIndoors() is not AnimalHouse animalHouse)
            {
                continue;
            }

            var data = building.GetData();
            if (data.ValidOccupantTypes is null
                || !data.ValidOccupantTypes.Any(this.Config.PetTheAnimals.ValidOccupantTypes.Contains))
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
                if (this.Config.PetTheAnimals.AnimalLimit > 0
                    && this.animalsPetted >= this.Config.PetTheAnimals.AnimalLimit)
                {
                    return true;
                }
            }
        }

        return this.animalsPetted > 0;
    }
}