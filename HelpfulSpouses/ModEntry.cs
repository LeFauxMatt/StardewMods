namespace StardewMods.HelpfulSpouses;

using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.HelpfulSpouses.Chores;
using StardewMods.HelpfulSpouses.Helpers;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private ModConfig? _config;

    private ModConfig Config => this._config ??= CommonHelpers.GetConfig<ModConfig>(this.Helper);

    /// <inheritdoc />
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