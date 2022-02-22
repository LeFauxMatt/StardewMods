namespace StardewMods.EasyAccess;

using System;
using Common.Helpers;
using Common.Integrations.FuryCore;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.EasyAccess.Features;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Models.Config;
using StardewMods.EasyAccess.Services;
using StardewMods.FuryCore.Services;

/// <inheritdoc />
public class EasyAccess : Mod
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
        EasyAccess.ModUniqueId = this.ModManifest.UniqueID;
        I18n.Init(helper.Translation);
        Log.Monitor = this.Monitor;
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

        // Core Services
        this.Services.Add(
            new AssetHandler(this.Config, this.Helper),
            new CommandHandler(this.Config, this.Helper, this.Services),
            new ManagedObjects(this.Config, this.Services),
            new ModConfigMenu(this.Config, this.Helper, this.ModManifest, this.Services));

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new EasyAccessApi(this.Services);
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.FuryCore.API.AddFuryCoreServices(this.Services);

        // Features
        this.Services.Add(
            new CollectOutputs(this.Config, this.Helper, this.Services),
            new Configurator(this.Config, this.Helper, this.Services),
            new DispenseInputs(this.Config, this.Helper, this.Services));

        // Activate Features
        foreach (var feature in this.Services.FindServices<Feature>())
        {
            feature.Toggle();
        }
    }
}