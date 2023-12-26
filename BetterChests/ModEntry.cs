namespace StardewMods.BetterChests;

using HarmonyLib;
using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Features;
using StardewMods.BetterChests.Framework.Services.Transient;
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
        // Init
        I18n.Init(this.Helper.Translation);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Init
        this.container = new Container();

        // Configuration
        this.container.Register(() => new Harmony(this.ModManifest.UniqueID), Lifestyle.Singleton);
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
        this.container.RegisterSingleton<FeatureManager>();
        this.container.RegisterSingleton<ConfigManager>();
        this.container.RegisterSingleton<AutomateIntegration>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<ToolbarIconsIntegration>();
        this.container.Register(
            () =>
            {
                var furyCore = this.container.GetInstance<FuryCoreIntegration>();
                var monitor = this.container.GetInstance<IMonitor>();
                return furyCore.Api!.CreateLogService(monitor);
            },
            Lifestyle.Singleton);

        this.container.Register(
            () =>
            {
                var furyCore = this.container.GetInstance<FuryCoreIntegration>();
                return furyCore.Api!.CreateThemingService();
            },
            Lifestyle.Singleton);

        this.container.Register<ItemMatcher>(Lifestyle.Transient);
        this.container.Register(
            () =>
            {
                var log = this.container.GetInstance<ILog>();
                return new ItemMatcherFactory(log, this.container.GetInstance<ItemMatcher>);
            },
            Lifestyle.Singleton);

        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<ContainerFactory>();
        this.container.RegisterSingleton<LocalizedTextManager>();
        this.container.RegisterSingleton<InventoryTabFactory>();
        this.container.RegisterSingleton<ItemGrabMenuManager>();
        this.container.RegisterSingleton<StatusEffectManager>();
        this.container.RegisterSingleton<ProxyChestFactory>();
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

        // this.container.Collection.Register<IFeature>(
        //     new[]
        //     {
        //         Lifestyle.Singleton.CreateRegistration<AutoOrganize>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<CarryChest>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<CategorizeChest>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<ChestFinder>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<ChestInfo>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<CollectItems>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<ConfigureChest>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<CraftFromChest>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<HslColorPicker>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<InventoryTabs>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<LabelChest>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<LockItem>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<OpenHeldChest>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<OrganizeItems>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<ResizeChest>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<SearchItems>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<StashToChest>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<TransferItems>(this.container),
        //         Lifestyle.Singleton.CreateRegistration<UnloadChest>(this.container),
        //     });

        this.container.Verify();

        var configManager = this.container.GetInstance<ConfigManager>();
        configManager.Reload();
    }
}