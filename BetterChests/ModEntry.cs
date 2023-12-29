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
        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<AutomateIntegration>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.container.RegisterSingleton<ConfigMenuManager>();
        this.container.RegisterSingleton<ContainerFactory>();
        this.container.RegisterSingleton<ContainerOperations>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<InventoryTabFactory>();
        this.container.RegisterSingleton<ItemGrabMenuManager>();
        this.container.RegisterSingleton<ItemMatcherFactory>();
        this.container.RegisterSingleton<LocalizedTextManager>();
        this.container.RegisterSingleton<ILog, LogService>();
        this.container.RegisterSingleton<ProxyChestFactory>();
        this.container.RegisterSingleton<StatusEffectManager>();
        this.container.RegisterSingleton<ITheming, ThemingService>();
        this.container.RegisterSingleton<ToolbarIconsIntegration>();
        this.container.Register<ItemMatcher>();
        this.container.Register<Func<ItemMatcher>>(() => this.container.GetInstance<ItemMatcher>);

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
}