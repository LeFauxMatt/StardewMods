namespace HelpForHire.Chores;

using System.Collections.Generic;
using System.Linq;
using StardewValley;

internal class PetAnimals : GenericChore
{
    public PetAnimals(ServiceLocator serviceLocator)
        : base("pet-animals", serviceLocator)
    {
    }

    protected override bool DoChore()
    {
        var animalsPetted = false;

        foreach (var farmAnimal in PetAnimals.GetFarmAnimals())
        {
            farmAnimal.pet(Game1.player);
            animalsPetted = true;
        }

        return animalsPetted;
    }

    protected override bool TestChore()
    {
        return PetAnimals.GetFarmAnimals().Any();
    }

    private static IEnumerable<FarmAnimal> GetFarmAnimals()
    {
        return
            from farmAnimal in Game1.getFarm().getAllFarmAnimals()
            where !farmAnimal.wasPet.Value
            select farmAnimal;
    }
}