#nullable disable

namespace StardewMods.FuryCore;

using Common.Helpers;
using CommonHarmony.Services;
using StardewModdingAPI;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Services;

/// <inheritdoc />
public class FuryCore : Mod
{
    /// <summary>
    ///     Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ConfigData Config { get; set; }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        FuryCore.ModUniqueId = this.ModManifest.UniqueID;
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);
        this.Config = this.Helper.ReadConfig<ConfigData>();

        var harmony = new HarmonyHelper();
        this.Services.Add(
            new AssetHandler(this.Helper),
            new CommandHandler(this.Helper, this.Services),
            new ConfigureGameObject(this.Config, this.Helper, this.ModManifest, this.Services, harmony),
            new CustomEvents(this.Helper, this.Services, harmony),
            new CustomTags(this.Config, harmony),
            new GameObjects(this.Helper, this.Services),
            new MenuComponents(this.Helper, this.Services, harmony),
            new MenuItems(this.Config, this.Helper, this.Services, harmony),
            new ModConfigMenu(this.Config, this.Helper, this.ModManifest),
            new HudComponents(this.Config, this.Helper, this.Services));
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new FuryCoreApi(this.Services);
    }
}