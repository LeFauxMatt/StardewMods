namespace StardewMods.CustomBush;

using HarmonyLib;
using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.CustomBush.Framework.Models;
using StardewMods.CustomBush.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private Container container = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

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
        this.container.RegisterInstance<Func<Dictionary<string, BushModel>>>(this.GetData);
        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<BushManager>();
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<IEventPublisher, EventManager>();
        this.container.RegisterSingleton<IEventSubscriber, EventManager>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<ILog, FuryLogger>();

        // Verify
        this.container.Verify();
    }

    private Dictionary<string, BushModel> GetData()
    {
        var assetHandler = this.container.GetInstance<AssetHandler>();
        var gameContentHelper = this.container.GetInstance<IGameContentHelper>();
        return gameContentHelper.Load<Dictionary<string, BushModel>>(assetHandler.DataPath);
    }
}