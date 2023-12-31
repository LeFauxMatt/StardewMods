namespace StardewMods.GarbageDay;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.GarbageDay.Framework.Interfaces;
using StardewMods.GarbageDay.Framework.Models;
using StardewMods.GarbageDay.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);

        //ModPatches.Init(this.ModManifest);

        // Console Commands
        // TODO: Toolbar Icons
        //this.Helper.ConsoleCommands.Add("garbage_fill", I18n.Command_GarbageFill_Description(), this.GarbageFill);
        //this.Helper.ConsoleCommands.Add("garbage_hat", I18n.Command_GarbageHat_Description(), ModEntry.GarbageHat);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private static void GarbageHat(string command, string[] args) => GarbageCan.GarbageHat = true;

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
        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ContentPatcherIntegration>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<GarbageCanManager>();
        this.container.RegisterSingleton<ILog, LogService>();

        // Verify
        this.container.Verify();

        this.multiplayer = this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
    }
#nullable disable
    private Container container;
    private Multiplayer multiplayer;
}