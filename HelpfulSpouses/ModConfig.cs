namespace StardewMods.HelpfulSpouses;

using StardewMods.HelpfulSpouses.Chores;

/// <summary>
/// Mod config data for Helpful Spouses.
/// </summary>
internal sealed class ModConfig
{
    /// <summary>
    /// Gets or sets a the config options for <see cref="BirthdayGift" />.
    /// </summary>
    public BirthdayGift.Config BirthdayGiftOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating the daily limit for the number of chores a spouse will perform.
    /// </summary>
    public int DailyLimit { get; set; } = 1;

    /// <summary>
    /// Gets or sets the default chance that a chore will be done if no individual option was provided.
    /// </summary>
    public Dictionary<ChoreOption, double> DefaultChance { get; set; } = new()
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

    /// <summary>
    /// Gets or sets a the config options for <see cref="FeedTheAnimals" />.
    /// </summary>
    public FeedTheAnimals.Config FeedTheAnimalsOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the chance that any spouse will perform a chore.
    /// </summary>
    public double GlobalChance { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the minimum number of hearts required before a spouse will begin performing chores.
    /// </summary>
    public int HeartsNeeded { get; set; } = 12;

    /// <summary>
    /// Gets or sets a the config options for <see cref="LoveThePets" />.
    /// </summary>
    public LoveThePets.Config LoveThePetsOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets a the config options for <see cref="PetTheAnimals" />.
    /// </summary>
    public PetTheAnimals.Config PetTheAnimalsOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets a the config options for <see cref="RepairTheFences" />.
    /// </summary>
    public RepairTheFences.Config RepairTheFencesOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets a the config options for <see cref="WaterTheCrops" />.
    /// </summary>
    public WaterTheCrops.Config WaterTheCropsOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets a the config options for <see cref="WaterTheSlimes" />.
    /// </summary>
    public WaterTheSlimes.Config WaterTheSlimesOptions { get; set; } = new();
}