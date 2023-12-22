namespace StardewMods.CustomBush;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.CustomBush.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private AssetHandler assetHandler;
    private ModConfig config;
    private Logging logging;
#nullable enable

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        this.config = this.Helper.ReadConfig<ModConfig>();
        this.logging = new(this.config, this.Monitor);
        this.assetHandler = new(this.Helper.Events, this.Helper.GameContent, this.logging);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);
        _ = new ConfigMenu(this.config, this.Helper, this.ModManifest);
        _ = new BushManager(this.assetHandler, this.Helper.GameContent, harmony, this.logging);
    }
}
