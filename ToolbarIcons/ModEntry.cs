namespace StardewMods.ToolbarIcons;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Services;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;
using StardewValley.Menus;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private Container container;
#nullable enable

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) =>
        new ToolbarIconsApi(
            this.container.GetInstance<EventsManager>(),
            this.container.GetInstance<ILog>(),
            mod,
            this.container.GetInstance<ToolbarManager>());

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Init
        this.container = new Container();

        // Configuration
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
        this.container.RegisterInstance(new Dictionary<string, ClickableTextureComponent>());
        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<EventsManager>();
        this.container.RegisterSingleton<IntegrationManager>();
        this.container.RegisterSingleton<ILog, LogService>();
        this.container.RegisterSingleton<ITheming, ThemingService>();
        this.container.RegisterSingleton<ToolbarManager>();

        this.container.Collection.Register<ICustomIntegration>(
            typeof(AlwaysScrollMap),
            typeof(CjbCheatsMenu),
            typeof(CjbItemSpawner),
            typeof(DailyQuests),
            typeof(DynamicGameAssets),
            typeof(GenericModConfigMenu),
            typeof(SpecialOrders),
            typeof(StardewAquarium),
            typeof(ToDew));

        // Verify
        this.container.Verify();
    }
}