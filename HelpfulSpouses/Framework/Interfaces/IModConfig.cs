namespace StardewMods.HelpfulSpouses.Framework.Interfaces;

using StardewMods.HelpfulSpouses.Framework.Models;
using StardewMods.HelpfulSpouses.Framework.Models.Chores;

/// <summary>Mod config data for Helpful Spouses.</summary>
internal interface IModConfig
{
    /// <summary>Gets a value indicating the daily limit for the number of chores a spouse will perform.</summary>
    public int DailyLimit { get; }

    /// <summary>Gets the chance that any spouse will perform a chore.</summary>
    public double GlobalChance { get; }

    /// <summary>Gets the minimum number of hearts required before a spouse will begin performing chores.</summary>
    public int HeartsNeeded { get; }

    /// <summary>Gets the default chance that a chore will be done if no individual option was provided.</summary>
    public CharacterOptions DefaultOptions { get; }

    /// <summary>Gets the config options for <see cref="Services.Chores.BirthdayGift" />.</summary>
    public BirthdayGiftOptions BirthdayGift { get; }

    /// <summary>Gets the config options for <see cref="Services.Chores.FeedTheAnimals" />.</summary>
    public FeedTheAnimalsOptions FeedTheAnimals { get; }

    /// <summary>Gets the config options for <see cref="Services.Chores.LoveThePets" />.</summary>
    public LoveThePetsOptions LoveThePets { get; }

    /// <summary>Gets the config options for <see cref="Services.Chores.PetTheAnimals" />.</summary>
    public PetTheAnimalsOptions PetTheAnimals { get; }

    /// <summary>Gets the config options for <see cref="Services.Chores.RepairTheFences" />.</summary>
    public RepairTheFencesOptions RepairTheFences { get; }

    /// <summary>Gets the config options for <see cref="Services.Chores.WaterTheCrops" />.</summary>
    public WaterTheCropsOptions WaterTheCrops { get; }

    /// <summary>Gets the config options for <see cref="Services.Chores.WaterTheSlimes" />.</summary>
    public WaterTheSlimesOptions WaterTheSlimes { get; }
}