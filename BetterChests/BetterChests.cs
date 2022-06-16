namespace StardewMods.BetterChests;

using System;
using Common.Helpers;
using Common.Integrations.FuryCore;
using Common.Integrations.ToolbarIcons;
using CommonHarmony.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Models.Config;
using StardewMods.BetterChests.Services;
using StardewMods.FuryCore.Services;

/// <inheritdoc />
public class BetterChests : Mod
{
    private ConfigModel? _config;

    /// <summary>
    ///     Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ConfigModel Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            IConfigData? config = null;
            try
            {
                config = this.Helper.ReadConfig<ConfigData>();
            }
            catch (Exception)
            {
                // ignored
            }

            this._config = new(config ?? new ConfigData(), this.Helper, this.Services);
            return this._config;
        }
    }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        BetterChests.ModUniqueId = this.ModManifest.UniqueID;
        I18n.Init(helper.Translation);
        Log.Monitor = this.Monitor;

        // Core Services
        this.Services.Add(
            new AssetHandler(this.Config, this.Helper),
            new CommandHandler(this.Config, this.Helper, this.Services),
            new ManagedObjects(this.Config, this.Services),
            new ModConfigMenu(this.Config, this.Helper, this.ModManifest, this.Services),
            new ModIntegrations(this.Helper, this.Services));

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new BetterChestsApi(this.Services);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var harmony = new HarmonyHelper();
        var furyCore = new FuryCoreIntegration(this.Helper.ModRegistry);
        var toolbarIcons = new ToolbarIconsIntegration(this.Helper.ModRegistry);
        furyCore.API!.AddFuryCoreServices(this.Services);

        // Features
        this.Services.Add(
            new AutoOrganize(this.Config, this.Helper, this.Services),
            new CarryChest(this.Config, this.Helper, this.Services, harmony),
            //new CategorizeChest(this.Config, this.Helper, this.Services),
            //new ChestMenuTabs(this.Config, this.Helper, this.Services),
            new CollectItems(this.Config, this.Helper, this.Services, harmony),
            new Configurator(this.Config, this.Helper, this.Services),
            new CraftFromChest(this.Config, this.Helper, this.Services, harmony, toolbarIcons),
            new CustomColorPicker(this.Config, this.Helper, this.Services, harmony),
            new FilterItems(this.Config, this.Helper, this.Services, harmony),
            new InventoryProviderForBetterCrafting(this.Config, this.Helper, this.Services),
            new MenuForShippingBin(this.Config, this.Helper, this.Services),
            new OpenHeldChest(this.Config, this.Helper, this.Services, harmony),
            new OrganizeChest(this.Config, this.Helper, this.Services, harmony),
            new ResizeChest(this.Config, this.Helper, this.Services, harmony),
            new ResizeChestMenu(this.Config, this.Helper, this.Services, harmony),
            new SearchItems(this.Config, this.Helper, this.Services, harmony),
            new SlotLock(this.Config, this.Helper, this.Services, harmony),
            new StashToChest(this.Config, this.Helper, this.Services, toolbarIcons),
            new UnloadChest(this.Config, this.Helper, this.Services));

        // Activate Features
        foreach (var feature in this.Services.FindServices<Feature>())
        {
            feature.Toggle();
        }
    }
}