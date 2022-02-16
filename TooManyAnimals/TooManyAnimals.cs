namespace StardewMods.TooManyAnimals;

using System;
using Common.Helpers;
using Common.Integrations.FuryCore;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Services;
using StardewMods.TooManyAnimals.Interfaces;
using StardewMods.TooManyAnimals.Models;
using StardewMods.TooManyAnimals.Services;

/// <inheritdoc />
public class TooManyAnimals : Mod
{
    /// <summary>
    ///     Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ConfigModel Config { get; set; }

    private FuryCoreIntegration FuryCore { get; set; }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        TooManyAnimals.ModUniqueId = this.ModManifest.UniqueID;
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);
        this.FuryCore = new(this.Helper.ModRegistry);

        // Mod Config
        IConfigData config = null;
        try
        {
            config = this.Helper.ReadConfig<ConfigData>();
        }
        catch (Exception)
        {
            // ignored
        }

        this.Config = new(config ?? new ConfigData(), this.Helper, this.Services);

        this.Services.Add(
            new AnimalMenuHandler(this.Config, this.Helper, this.Services),
            new ModConfigMenu(this.Config, this.Helper, this.ModManifest));

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.FuryCore.API.AddFuryCoreServices(this.Services);
    }
}