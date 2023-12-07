namespace StardewMods.CustomBush;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.CustomBush.Framework;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private AssetHandler assetHandler;
    private BushManager bushManager;
#nullable enable

    private bool wait;

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        this.assetHandler = new(this.Helper.Events, this.Helper.GameContent);
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.wait = true;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (this.wait)
        {
            this.wait = false;
            return;
        }

        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;

        var harmony = new Harmony(this.ModManifest.UniqueID);
        this.bushManager = new(this.Monitor, this.assetHandler, harmony);
    }
}
