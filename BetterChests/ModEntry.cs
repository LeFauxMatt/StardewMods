namespace StardewMods.BetterChests;

using HarmonyLib;
using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Features;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.Automate;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private Container container;
#nullable enable

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(this.Helper.Translation);
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Init
        this.container = new Container();

        // Configuration
        this.container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
        this.container.RegisterInstance(this.Helper);
        this.container.RegisterInstance(this.ModManifest);
        this.container.RegisterInstance(this.Monitor);
        this.container.RegisterInstance(this.Helper.Data);
        this.container.RegisterInstance(this.Helper.Events);
        this.container.RegisterInstance(this.Helper.GameContent);
        this.container.RegisterInstance(this.Helper.Input);
        this.container.RegisterInstance(this.Helper.ModContent);
        this.container.RegisterInstance(this.Helper.ModRegistry);
        this.container.RegisterInstance(this.Helper.Reflection);
        this.container.RegisterInstance(this.Helper.Translation);
        this.container.RegisterInstance<Func<CategorizeOption>>(this.GetCategorizeOption);
        this.container.RegisterInstance<Func<Dictionary<string, InventoryTabData>>>(this.GetInventoryTabData);
        this.container.RegisterInstance<Func<IModConfig>>(this.GetConfig);
        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<AutomateIntegration>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.container.RegisterSingleton<ContainerFactory>();
        this.container.RegisterSingleton<ContainerHandler>();
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<IEventPublisher, EventManager>();
        this.container.RegisterSingleton<IEventSubscriber, EventManager>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<InventoryTabFactory>();
        this.container.RegisterSingleton<ItemGrabMenuManager>();
        this.container.RegisterSingleton<ItemMatcherFactory>();
        this.container.RegisterSingleton<LocalizedTextManager>();
        this.container.RegisterSingleton<ILog, FuryLogger>();
        this.container.RegisterSingleton<IThemeHelper, FuryThemer>();
        this.container.RegisterSingleton<ProxyChestFactory>();
        this.container.RegisterSingleton<StatusEffectManager>();
        this.container.RegisterSingleton<ToolbarIconsIntegration>();
        this.container.Register<CategorizeOption>();

        this.container.Collection.Register<IFeature>(
            new[]
            {
                typeof(AutoOrganize),
                typeof(CarryChest),
                typeof(CategorizeChest),
                typeof(ChestFinder),
                typeof(ChestInfo),
                typeof(CollectItems),
                typeof(ConfigureChest),
                typeof(CraftFromChest),
                typeof(HslColorPicker),
                typeof(InventoryTabs),
                typeof(LabelChest),
                typeof(LockItem),
                typeof(OpenHeldChest),
                typeof(OrganizeItems),
                typeof(ResizeChest),
                typeof(SearchItems),
                typeof(StashToChest),
                typeof(TransferItems),
                typeof(UnloadChest),
            },
            Lifestyle.Singleton);

        // Verify
        this.container.Verify();

        var configManager = this.container.GetInstance<ConfigManager>();
        configManager.Init();
    }

    private IModConfig GetConfig() => this.container.GetInstance<IModConfig>();

    private Dictionary<string, InventoryTabData> GetInventoryTabData()
    {
        var assetHandler = this.container.GetInstance<AssetHandler>();
        var gameContentHelper = this.container.GetInstance<IGameContentHelper>();
        return gameContentHelper.Load<Dictionary<string, InventoryTabData>>(assetHandler.TabDataPath);
    }

    private CategorizeOption GetCategorizeOption() => this.container.GetInstance<CategorizeOption>();
}