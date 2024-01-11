namespace StardewMods.HelpfulSpouses.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewMods.HelpfulSpouses.Framework.Models;
using StardewMods.HelpfulSpouses.Framework.Models.Chores;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(IEventPublisher eventPublisher, IModHelper modHelper)
        : base(eventPublisher, modHelper) { }

    /// <inheritdoc />
    public BirthdayGiftOptions BirthdayGift => this.Config.BirthdayGift;

    /// <inheritdoc />
    public int DailyLimit => this.Config.DailyLimit;

    /// <inheritdoc />
    public CharacterOptions DefaultOptions => this.Config.DefaultOptions;

    /// <inheritdoc />
    public FeedTheAnimalsOptions FeedTheAnimals => this.Config.FeedTheAnimals;

    /// <inheritdoc />
    public double GlobalChance => this.Config.GlobalChance;

    /// <inheritdoc />
    public int HeartsNeeded => this.Config.HeartsNeeded;

    /// <inheritdoc />
    public LoveThePetsOptions LoveThePets => this.Config.LoveThePets;

    /// <inheritdoc />
    public PetTheAnimalsOptions PetTheAnimals => this.Config.PetTheAnimals;

    /// <inheritdoc />
    public RepairTheFencesOptions RepairTheFences => this.Config.RepairTheFences;

    /// <inheritdoc />
    public WaterTheCropsOptions WaterTheCrops => this.Config.WaterTheCrops;

    /// <inheritdoc />
    public WaterTheSlimesOptions WaterTheSlimes => this.Config.WaterTheSlimes;
}