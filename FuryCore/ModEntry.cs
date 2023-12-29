namespace StardewMods.FuryCore;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.FuryCore.Framework;
using StardewMods.FuryCore.Framework.Interfaces;
using StardewMods.FuryCore.Framework.Models;
using StardewMods.FuryCore.Framework.Services;

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
        var config = this.container.GetInstance<IConfigWithLogLevel>();
        var theming = this.container.GetInstance<ITheming>();
        return new FuryCoreApi(mod, config, theming);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var config = this.Helper.ReadConfig<DefaultConfig>();

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
        this.container.RegisterSingleton<IConfigWithLogLevel, ConfigManager>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<ILog, Log>();
        this.container.RegisterSingleton<ITheming, Theming>();

        // Verify
        this.container.Verify();
    }
}