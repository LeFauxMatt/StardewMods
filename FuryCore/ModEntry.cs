namespace StardewMods.FuryCore;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.Common.Interfaces;
using StardewMods.FuryCore.Framework;

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
    public override object GetApi(IModInfo mod) => new FuryCoreApi();

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var config = this.Helper.ReadConfig<ModConfig>();

        // Init
        this.container = new();
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
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();

        // Services
        this.container.RegisterSingleton<Logging>();

        this.container.Verify();
    }
}
