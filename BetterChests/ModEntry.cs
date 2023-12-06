namespace StardewMods.BetterChests;

using HarmonyLib;
using SimpleInjector;
using StardewMods.BetterChests.Framework;
using StardewMods.BetterChests.Framework.Features;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.Automate;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.Common.Integrations.ToolbarIcons;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private readonly Container container = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);

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
        this.container.RegisterSingleton<AutomateIntegration>();
        this.container.RegisterSingleton<BetterCraftingIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<ToolbarIconsIntegration>();
        this.container.RegisterSingleton<Integrations>();

        // Services
        this.container.RegisterSingleton(() => this.Helper.ReadConfig<ModConfig>());
        this.container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<BuffHandler>();
        this.container.RegisterSingleton<ThemeHelper>(() => new(this.Helper, AssetHandler.IconPath, AssetHandler.TabsPath));
        this.container.RegisterSingleton<Formatting>();

        // Features
        this.container.Collection.Register<IFeature>(
            new[]
            {
                Lifestyle.Singleton.CreateRegistration<AutoOrganize>(this.container),
                Lifestyle.Singleton.CreateRegistration<BetterColorPicker>(this.container),
                Lifestyle.Singleton.CreateRegistration<BetterCrafting>(this.container),
                Lifestyle.Singleton.CreateRegistration<BetterItemGrabMenu>(this.container),
                Lifestyle.Singleton.CreateRegistration<CarryChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<ChestFinder>(this.container),
                Lifestyle.Singleton.CreateRegistration<ChestInfo>(this.container),
                Lifestyle.Singleton.CreateRegistration<ChestMenuTabs>(this.container),
                Lifestyle.Singleton.CreateRegistration<CollectItems>(this.container),
                Lifestyle.Singleton.CreateRegistration<Configurator>(this.container),
                Lifestyle.Singleton.CreateRegistration<CraftFromChest>(this.container),
                Lifestyle.Singleton.CreateRegistration<FilterItems>(this.container),
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

        this.container.RegisterSingleton<ConfigMenu>();
        this.container.RegisterSingleton<StorageHandler>();
        this.container.Verify();
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        var configMenu = this.container.GetInstance<ConfigMenu>();
        var storages = this.container.GetInstance<StorageHandler>();
        return new Api(configMenu, storages);
    }
}
