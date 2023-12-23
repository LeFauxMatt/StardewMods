namespace StardewMods.ExpandedStorage;

using HarmonyLib;
using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Services;
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
        var config = this.Helper.ReadConfig<ModConfig>();

        // Init
        this.container = new Container();
        this.container.RegisterInstance(config);
        this.container.RegisterInstance<IConfigWithLogLevel>(config);
        this.container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));

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
        this.container.RegisterSingleton<ContentPatcherIntegration>();

        // Services
        this.container.RegisterSingleton<Logging>();
        this.container.RegisterSingleton<ConfigMenu>();
        this.container.RegisterSingleton<ManagedStorages>();
        this.container.RegisterSingleton<ModPatches>();

        this.container.Verify();
    }
}
