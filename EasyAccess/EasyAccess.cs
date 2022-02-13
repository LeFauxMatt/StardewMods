namespace StardewMods.EasyAccess;

using System;
using Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.EasyAccess.Features;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Models.Config;
using StardewMods.EasyAccess.Services;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Services;

/// <inheritdoc />
public class EasyAccess : Mod
{
    /// <summary>
    ///     Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ConfigModel Config { get; set; }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        EasyAccess.ModUniqueId = this.ModManifest.UniqueID;
        I18n.Init(helper.Translation);
        Log.Monitor = this.Monitor;

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

        // Services
        this.Services.Add(
            new AssetHandler(this.Config, this.Helper),
            new CommandHandler(this.Config, this.Helper, this.Services),
            new ManagedObjects(this.Config, this.Services),
            new ModConfigMenu(this.Config, this.Helper, this.ModManifest, this.Services),
            new CollectOutputs(this.Config, this.Helper, this.Services),
            new DispenseInputs(this.Config, this.Helper, this.Services));

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var furyCore = this.Helper.ModRegistry.GetApi<IModServices>("furyx639.FuryCore");
        this.Services.Add((IModService)furyCore);

        // Activate Features
        foreach (var feature in this.Services.FindServices<Feature>())
        {
            feature.Toggle();
        }
    }
}