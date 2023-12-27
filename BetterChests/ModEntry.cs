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
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.container.RegisterSingleton<FeatureManager>();
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
        this.container.RegisterSingleton<ConfigMenuManager>();
        this.container.RegisterSingleton<ContainerFactory>();
        this.container.RegisterSingleton<ContainerOperations>();
        this.container.RegisterSingleton<InventoryTabFactory>();
        this.container.RegisterSingleton<ItemGrabMenuManager>();
        this.container.RegisterSingleton<LocalizedTextManager>();
        this.container.RegisterSingleton<ProxyChestFactory>();
        this.container.RegisterSingleton<StatusEffectManager>();
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

        this.container.Verify();

        var featureManager = this.container.GetInstance<FeatureManager>();
        featureManager.Activate();
    }
}