namespace StardewMods.ToolbarIcons;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework;
using StardewMods.ToolbarIcons.Framework.Integrations;
using StardewMods.ToolbarIcons.Framework.Integrations.Mods;
using StardewMods.ToolbarIcons.Framework.Integrations.Vanilla;
using StardewMods.ToolbarIcons.Framework.Services;
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
    public override object GetApi(IModInfo mod)
    {
        var customEvents = this.container.GetInstance<EventsManager>();
        var toolbar = this.container.GetInstance<ToolbarHandler>();
        return new ToolbarIconsApi(mod, customEvents, toolbar);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.container = new();

        // Init
        this.container.RegisterSingleton(() => this.Helper.ReadConfig<ModConfig>());
        this.container.RegisterSingleton<EventsManager>();

        // SMAPI
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

        // Integrations
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<SimpleIntegration>();
        this.container.RegisterSingleton<ComplexIntegration>();
        this.container.Collection.Register<ICustomIntegration>(
            typeof(AlwaysScrollMap),
            typeof(CjbCheatsMenu),
            typeof(CjbItemSpawner),
            typeof(DynamicGameAssets),
            typeof(GenericCustomConfigMenu),
            typeof(StardewAquarium),
            typeof(ToDew),
            typeof(DailyQuests),
            typeof(SpecialOrders));

        // Services
        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<IntegrationsManager>();
        this.container.RegisterSingleton<ConfigMenu>();
        this.container.RegisterSingleton<ThemeHelper>();
        this.container.RegisterSingleton<ToolbarHandler>();

        this.container.Verify();

        var integrations = this.container.GetAllInstances<ICustomIntegration>();
        foreach (var integration in integrations)
        {
            integration.AddIntegration();
        }
    }
}
