namespace StardewMods.BetterChests;

using System;
using Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Services;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Services;

/// <inheritdoc />
public class BetterChests : Mod
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
        BetterChests.ModUniqueId = this.ModManifest.UniqueID;
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
            new ManagedStorages(this.Config, this.Services),
            new ModConfigMenu(this.Config, this.Helper, this.ModManifest, this.Services),
            new ModIntegrations(this.Helper, this.Services),
            new CarryChest(this.Config, this.Helper, this.Services),
            new CategorizeChest(this.Config, this.Helper, this.Services),
            new ChestMenuTabs(this.Config, this.Helper, this.Services),
            new CollectItems(this.Config, this.Helper, this.Services),
            new CraftFromChest(this.Config, this.Helper, this.Services),
            new CustomColorPicker(this.Config, this.Helper, this.Services),
            new FilterItems(this.Config, this.Helper, this.Services),
            new OpenHeldChest(this.Config, this.Helper, this.Services),
            new ResizeChest(this.Config, this.Helper, this.Services),
            new ResizeChestMenu(this.Config, this.Helper, this.Services),
            new SearchItems(this.Config, this.Helper, this.Services),
            new SlotLock(this.Config, this.Helper, this.Services),
            new StashToChest(this.Config, this.Helper, this.Services),
            new UnloadChest(this.Config, this.Helper, this.Services));

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new BetterChestsApi(this.Services);
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