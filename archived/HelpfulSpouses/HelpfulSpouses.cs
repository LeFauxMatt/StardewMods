namespace StardewMods.HelpfulSpouses;

using System;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.HelpfulSpouses.Chores;
using StardewMods.HelpfulSpouses.Helpers;

/// <inheritdoc />
public class HelpfulSpouses : Mod
{
    private ModConfig? _config;

    private ModConfig Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            ModConfig? config = null;
            try
            {
                config = this.Helper.ReadConfig<ModConfig>();
            }
            catch (Exception)
            {
                // ignored
            }

            this._config = config ?? new ModConfig();
            Log.Trace(this._config.ToString());
            return this._config;
        }
    }

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        Integrations.Init(this.Helper, this.ModManifest);
        Tokens.Init(this.Helper);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        BirthdayShopping.Init(this.Helper, this.Config);
    }
}