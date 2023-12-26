namespace StardewMods.ExpandedStorage;

using HarmonyLib;
using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ExpandedStorage.Framework.Services;

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

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Init
        this.container = new Container();
        this.container.Register(this.Helper.ReadConfig<ModConfig>, Lifestyle.Singleton);
        this.container.Register(() => new Harmony(this.ModManifest.UniqueID), Lifestyle.Singleton);

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
        this.container.Register<FuryCoreIntegration>(Lifestyle.Singleton);
        this.container.Register<GenericModConfigMenuIntegration>(Lifestyle.Singleton);
        this.container.Register<ContentPatcherIntegration>(Lifestyle.Singleton);

        // Services
        this.container.Register(
            () =>
            {
                var furyCore = this.container.GetInstance<FuryCoreIntegration>();
                var monitor = this.container.GetInstance<IMonitor>();
                return furyCore.Api!.CreateLogService(monitor);
            },
            Lifestyle.Singleton);

        this.container.Register<ConfigMenu>(Lifestyle.Singleton);
        this.container.Register<ManagedStorages>(Lifestyle.Singleton);
        this.container.Register<ModPatches>(Lifestyle.Singleton);

        this.container.Verify();
    }
}