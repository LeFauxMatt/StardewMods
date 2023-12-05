namespace StardewMods.HelpfulSpouses.Chores;

using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;

/// <inheritdoc />
internal sealed class LoveThePets : IChore
{
    private readonly Config config;

    private int petsFed;

    private int petsPetted;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoveThePets"/> class.
    /// </summary>
    /// <param name="config">Config data for <see cref="LoveThePets"/>.</param>
    public LoveThePets(Config config) => this.config = config;

    /// <inheritdoc/>
    public void AddTokens(Dictionary<string, object> tokens)
    {
        var pet = Game1.random.ChooseFrom(Game1.getFarm().characters.OfType<Pet>().ToList());
        tokens["PetName"] = pet.Name;
        tokens["PetsFed"] = this.petsFed;
        tokens["PetsPetted"] = this.petsPetted;
    }

    /// <inheritdoc/>
    public bool IsPossibleForSpouse(NPC spouse) =>
        (this.config.FillWaterBowl || this.config.EnablePetting)
        && Game1.getFarm().characters.OfType<Pet>().Any();

    /// <inheritdoc/>
    public bool TryPerformChore(NPC spouse)
    {
        this.petsFed = 0;
        this.petsPetted = 0;
        var farm = Game1.getFarm();

        if (this.config.FillWaterBowl)
        {
            foreach (var petBowl in farm.buildings.OfType<PetBowl>())
            {
                petBowl.watered.Value = true;
                this.petsFed++;
            }
        }

        if (!this.config.EnablePetting)
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
            pet.mutex.RequestLock(() =>
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

    /// <summary>
    /// Config data for <see cref="LoveThePets" />.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// Gets or sets a value indicating whether petting will be enabled.
        /// </summary>
        public bool EnablePetting { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the water bowl will be filled.
        /// </summary>
        public bool FillWaterBowl { get; set; } = true;
    }
}
