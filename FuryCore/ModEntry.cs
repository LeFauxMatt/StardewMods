namespace StardewMods.FuryCore;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Framework.Interfaces;
using StardewMods.FuryCore.Framework.Services;
using StardewMods.FuryCore.Framework.Services.Integrations.FuryCore;
using StardewMods.FuryCore.Framework.Services.Integrations.GenericModConfigMenu;

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
        var apiFactory = this.container.GetInstance<ApiFactory>();
        return apiFactory.CreateApi(mod);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var config = this.Helper.ReadConfig<ModConfig>();

        // Init
        this.container = new Container();
        this.container.RegisterInstance(config);
        this.container.RegisterInstance<IConfigWithLogLevel>(config);

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

        // Integrations
        this.container.Register<FuryCoreIntegration>(Lifestyle.Transient);
        this.container.Register<GenericModConfigMenuIntegration>(Lifestyle.Transient);

        // Services
        this.container.Register<ILogger, Logger>(Lifestyle.Singleton);
        this.container.Register<IThemeHelper, ThemeHelper>(Lifestyle.Singleton);
        this.container.Register<ApiFactory>(Lifestyle.Singleton);

        this.container.Verify();
    }
}
