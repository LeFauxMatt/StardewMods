namespace StardewMods.HelpfulSpouses.Framework.Models;

using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewMods.HelpfulSpouses.Framework.Models.Chores;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public BirthdayGiftOptions BirthdayGift { get; set; } = new();

    /// <inheritdoc />
    public int DailyLimit { get; set; } = 1;

    /// <inheritdoc />
    public CharacterOptions DefaultOptions { get; set; } = new()
    {
        [ChoreOption.BirthdayGift] = 0,
        [ChoreOption.FeedTheAnimals] = 0,
        [ChoreOption.LoveThePets] = 0,
        [ChoreOption.MakeBreakfast] = 0,
        [ChoreOption.PetTheAnimals] = 0,
        [ChoreOption.RepairTheFences] = 0,
        [ChoreOption.WaterTheCrops] = 0,
        [ChoreOption.WaterTheSlimes] = 0,
    };

    /// <inheritdoc />
    public FeedTheAnimalsOptions FeedTheAnimals { get; set; } = new();

    /// <inheritdoc />
    public double GlobalChance { get; set; } = 1.0;

    /// <inheritdoc />
    public int HeartsNeeded { get; set; } = 12;

    /// <inheritdoc />
    public LoveThePetsOptions LoveThePets { get; set; } = new();

    /// <inheritdoc />
    public PetTheAnimalsOptions PetTheAnimals { get; set; } = new();

    /// <inheritdoc />
    public RepairTheFencesOptions RepairTheFences { get; set; } = new();

    /// <inheritdoc />
    public WaterTheCropsOptions WaterTheCrops { get; set; } = new();

    /// <inheritdoc />
    public WaterTheSlimesOptions WaterTheSlimes { get; set; } = new();
}