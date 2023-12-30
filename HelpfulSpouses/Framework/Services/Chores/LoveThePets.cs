namespace StardewMods.HelpfulSpouses.Framework.Services.Chores;

using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IChore" />
internal sealed class LoveThePets : BaseChore<LoveThePets>
{
    private int petsFed;
    private int petsPetted;

    /// <summary>Initializes a new instance of the <see cref="LoveThePets" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public LoveThePets(ILog log, IManifest manifest, IModConfig modConfig)
        : base(log, manifest, modConfig) { }

    /// <inheritdoc />
    public override ChoreOption Option => ChoreOption.LoveThePets;

    /// <inheritdoc />
    public override void AddTokens(Dictionary<string, object> tokens)
    {
        var pet = Game1.random.ChooseFrom(Game1.getFarm().characters.OfType<Pet>().ToList());
        tokens["PetName"] = pet.Name;
        tokens["PetsFed"] = this.petsFed;
        tokens["PetsPetted"] = this.petsPetted;
    }

    /// <inheritdoc />
    public override bool IsPossibleForSpouse(NPC spouse) =>
        (this.Config.LoveThePets.FillWaterBowl || this.Config.LoveThePets.EnablePetting)
        && Game1.getFarm().characters.OfType<Pet>().Any();

    /// <inheritdoc />
    public override bool TryPerformChore(NPC spouse)
    {
        this.petsFed = 0;
        this.petsPetted = 0;
        var farm = Game1.getFarm();

        if (this.Config.LoveThePets.FillWaterBowl)
        {
            foreach (var petBowl in farm.buildings.OfType<PetBowl>())
            {
                petBowl.watered.Value = true;
                this.petsFed++;
            }
        }

        if (!this.Config.LoveThePets.EnablePetting)
        {
            return this.petsFed > 0;
        }

        foreach (var pet in farm.characters.OfType<Pet>())
        {
            if (pet.lastPetDay.TryGetValue(Game1.player.UniqueMultiplayerID, out var curLastPetDay)
                && curLastPetDay == Game1.Date.TotalDays)
            {
                continue;
            }

            pet.lastPetDay[Game1.player.UniqueMultiplayerID] = Game1.Date.TotalDays;
            pet.mutex.RequestLock(
                () =>
                {
                    if (!pet.grantedFriendshipForPet.Value)
                    {
                        pet.grantedFriendshipForPet.Set(newValue: true);
                        pet.friendshipTowardFarmer.Set(Math.Min(1000, pet.friendshipTowardFarmer.Value + 12));
                    }

                    pet.mutex.ReleaseLock();
                });

            this.petsPetted++;
        }

        return this.petsFed > 0 || this.petsPetted > 0;
    }
}