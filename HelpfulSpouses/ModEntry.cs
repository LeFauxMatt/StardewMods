namespace StardewMods.HelpfulSpouses;

using HarmonyLib;
using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.Services.Integrations.ProjectFluent;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewMods.HelpfulSpouses.Framework.Services;
using StardewMods.HelpfulSpouses.Framework.Services.Chores;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private Container container = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper) => this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Init
        this.container = new Container();

        // Configuration
        this.container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
        this.container.RegisterInstance(this.Helper);
        this.container.RegisterInstance(this.ModManifest);
        this.container.RegisterInstance(this.Monitor);
        this.container.RegisterInstance(this.Helper.Data);
        this.container.RegisterInstance(this.Helper.Events);
        this.container.RegisterInstance(this.Helper.GameContent);
        this.container.RegisterInstance(this.Helper.Input);
        this.container.RegisterInstance(this.Helper.ModContent);
        this.container.RegisterInstance(this.Helper.ModRegistry);
        this.container.RegisterInstance(this.Helper.Reflection);
        this.container.RegisterInstance(this.Helper.Translation);
        this.container.RegisterSingleton<ActionHandler>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<IEventPublisher, EventManager>();
        this.container.RegisterSingleton<IEventSubscriber, EventManager>();
        this.container.RegisterSingleton<ILog, FuryLogger>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<ProjectFluentIntegration>();

        this.container.Collection.Register<IChore>(
            new[]
            {
                typeof(BirthdayGift),
                typeof(FeedTheAnimals),
                typeof(LoveThePets),
                typeof(MakeBreakfast),
                typeof(PetTheAnimals),
                typeof(RepairTheFences),
                typeof(WaterTheCrops),
                typeof(WaterTheSlimes),
            },
            Lifestyle.Singleton);

        // Verify
        this.container.Verify();
    }
}