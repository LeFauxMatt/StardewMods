namespace StardewMods.BetterChests;

using HarmonyLib;
using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Features;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Interfaces;
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
        var config = this.Helper.ReadConfig<ModConfig>();

        // Init
        this.container = new Container();
        this.container.RegisterInstance(config);
        this.container.RegisterInstance<IConfigWithLogLevel>(config);
        this.container.Register(() => new Harmony(this.ModManifest.UniqueID), Lifestyle.Singleton);

        // SMAPI
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

        // Integrations
        this.container.Register<FuryCoreIntegration>(Lifestyle.Singleton);
        this.container.Register<AutomateIntegration>(Lifestyle.Singleton);
        this.container.Register<GenericModConfigMenuIntegration>(Lifestyle.Singleton);
        this.container.Register<ToolbarIconsIntegration>(Lifestyle.Singleton);

        // Services
        this.container.Register(
            () =>
            {
                var furyCore = this.container.GetInstance<FuryCoreIntegration>();
                var monitor = this.container.GetInstance<IMonitor>();
                return furyCore.Api!.GetLogger(monitor);
            },
            Lifestyle.Singleton);

        this.container.Register(
            () =>
            {
                var furyCore = this.container.GetInstance<FuryCoreIntegration>();
                return furyCore.Api!.GetThemeHelper();
            },
            Lifestyle.Singleton);

        this.container.Register<ItemMatcher>(Lifestyle.Transient);
        this.container.Register(
            () =>
            {
                var logging = this.container.GetInstance<ILogging>();
                return new ItemMatcherFactory(logging, this.container.GetInstance<ItemMatcher>);
            },
            Lifestyle.Singleton);

        this.container.Register<AssetHandler>(Lifestyle.Singleton);
        this.container.Register<ContainerFactory>(Lifestyle.Singleton);
        this.container.Register<LocalizedTextManager>(Lifestyle.Singleton);
        this.container.Register<InventoryTabFactory>(Lifestyle.Singleton);
        this.container.Register<ItemGrabMenuManager>(Lifestyle.Singleton);
        this.container.Register<StatusEffectManager>(Lifestyle.Singleton);
        this.container.Register<VirtualizedChestFactory>(Lifestyle.Singleton);

        // Features
        this.container.Collection.Register<IFeature>(
            new[]
            {
                Lifestyle.Singleton.CreateRegistration<AutoOrganize>(this.container),
                Lifestyle.Singleton.CreateRegistration<CarryChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<ChestFinder>(this.container),
                Lifestyle.Singleton.CreateRegistration<ChestInfo>(this.container),
                Lifestyle.Singleton.CreateRegistration<CollectItems>(this.container),
                Lifestyle.Singleton.CreateRegistration<ConfigureChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<CraftFromChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<FilterItems>(this.container),
                Lifestyle.Singleton.CreateRegistration<HslColorPicker>(this.container),
                Lifestyle.Singleton.CreateRegistration<InventoryTabs>(this.container),
                Lifestyle.Singleton.CreateRegistration<LabelChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<OpenHeldChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<OrganizeChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<ResizeChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<SearchItems>(this.container),
                Lifestyle.Singleton.CreateRegistration<SlotLock>(this.container),
                Lifestyle.Singleton.CreateRegistration<StashToChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<TransferItems>(this.container),
                Lifestyle.Singleton.CreateRegistration<UnloadChest>(this.container),
            });

        this.container.Verify();

        var features = this.container.GetAllInstances<IFeature>();
        foreach (var feature in features)
        {
            feature.SetActivated(true);
        }
    }
}
