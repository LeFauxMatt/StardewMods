namespace StardewMods.ItemIconOverlays;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ItemIconOverlays.Framework.Services;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
#nullable disable
    private Container container;
#nullable enable

    /// <inheritdoc />
    public override void Entry(IModHelper helper) => this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

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
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<IEventPublisher, EventManager>();
        this.container.RegisterSingleton<IEventSubscriber, EventManager>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<IconManager>();
        this.container.RegisterSingleton<ItemPropertyManager>();
        this.container.RegisterSingleton<ILog, LogService>();
        this.container.RegisterSingleton<OverlayManager>();

        // Verify
        this.container.Verify();
    }
}