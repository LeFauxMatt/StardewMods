namespace StardewMods.TooManyAnimals;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.TooManyAnimals.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private ModConfig config;
    private Logging logging;
#nullable enable

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        this.config = this.Helper.ReadConfig<ModConfig>();
        this.logging = new Logging(this.config, this.Monitor);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);
        _ = new ConfigMenu(this.config, this.Helper, this.ModManifest);
        _ = new AnimalsMenuHandler(this.config, harmony, this.Helper.Input, this.Helper.Events);
    }
}