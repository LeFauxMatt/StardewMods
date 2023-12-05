namespace StardewMods.BetterChests;

using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework;
using StardewMods.BetterChests.Framework.Features;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.Automate;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

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

        this.container = new();

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
        this.container.Register<AutomateIntegration>(Lifestyle.Singleton);
        this.container.Register<BetterCraftingIntegration>(Lifestyle.Singleton);
        this.container.Register<GenericModConfigMenuIntegration>(Lifestyle.Singleton);
        this.container.Register<ToolbarIconsIntegration>(Lifestyle.Singleton);
        this.container.Register<IntegrationService>(Lifestyle.Singleton);

        // Services
        this.container.RegisterSingleton(() => this.Helper.ReadConfig<ModConfig>());
        this.container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
        this.container.Register<StorageService>(Lifestyle.Singleton);
        this.container.RegisterSingleton<ThemeHelper>(
            () => new(this.Helper, "furyx639.BetterChests/Icons", "furyx639.BetterChests/Tabs/Texture"));

        this.container.Register<FormatService>(Lifestyle.Singleton);

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

        this.container.Register<ConfigService>(Lifestyle.Singleton);
        this.container.Verify();

        // Events
        this.Helper.Events.Content.AssetRequested += ModEntry.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi() => new Api();

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.BetterChests/HueBar"))
        {
            e.LoadFromModFile<Texture2D>("assets/hue.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.BetterChests/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.BetterChests/Tabs/Texture"))
        {
            e.LoadFromModFile<Texture2D>("assets/tabs.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var config = this.container.GetInstance<ModConfig>();
        var features = this.container.GetAllInstances<IFeature>();
        foreach (var feature in features)
        {
            feature.SetActivated(true);
        }

        StorageService.StorageTypeRequested += this.OnStorageTypeRequested;

        config.VanillaStorages.TryAdd("Auto-Grabber", new() { CustomColorPicker = FeatureOption.Disabled });
        config.VanillaStorages.TryAdd("Chest", new());
        config.VanillaStorages.TryAdd("Fridge", new() { CustomColorPicker = FeatureOption.Disabled });
        config.VanillaStorages.TryAdd("Junimo Chest", new() { CustomColorPicker = FeatureOption.Disabled });
        config.VanillaStorages.TryAdd("Junimo Hut", new() { CustomColorPicker = FeatureOption.Disabled });
        config.VanillaStorages.TryAdd("Mini-Fridge", new() { CustomColorPicker = FeatureOption.Disabled });
        config.VanillaStorages.TryAdd("Mini-Shipping Bin", new() { CustomColorPicker = FeatureOption.Disabled });
        config.VanillaStorages.TryAdd("Shipping Bin", new() { CustomColorPicker = FeatureOption.Disabled });
        config.VanillaStorages.TryAdd("Stone Chest", new());
    }

    private void OnStorageTypeRequested(object? sender, IStorageTypeRequestedEventArgs e)
    {
        var config = this.container.GetInstance<ModConfig>();
        switch (e.Context)
        {
            // Auto-Grabber
            case SObject
            {
                ParentSheetIndex: 165,
            } when config.VanillaStorages.TryGetValue("Auto-Grabber", out var autoGrabberData):
                e.Load(autoGrabberData, -1);
                return;

            // Chest
            case Chest
            {
                playerChest.Value: true,
                SpecialChestType: Chest.SpecialChestTypes.None,
                ParentSheetIndex: 130,
            } when config.VanillaStorages.TryGetValue("Chest", out var chestData):
                e.Load(chestData, -1);
                return;

            // Fridge
            case FarmHouse or IslandFarmHouse when config.VanillaStorages.TryGetValue("Fridge", out var fridgeData):
                e.Load(fridgeData, -1);
                return;

            // Junimo Chest
            case Chest
            {
                playerChest.Value: true,
                SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            } when config.VanillaStorages.TryGetValue("Junimo Chest", out var junimoChestData):
                e.Load(junimoChestData, -1);
                return;

            // Junimo Hut
            case JunimoHut when config.VanillaStorages.TryGetValue("Junimo Hut", out var junimoHutData):
                e.Load(junimoHutData, -1);
                return;

            // Mini-Fridge
            case Chest
            {
                fridge.Value: true,
                playerChest.Value: true,
            } when config.VanillaStorages.TryGetValue("Mini-Fridge", out var miniFridgeData):
                e.Load(miniFridgeData, -1);
                return;

            // Mini-Shipping Bin
            case Chest
            {
                playerChest.Value: true,
                SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin,
            } when config.VanillaStorages.TryGetValue("Mini-Shipping Bin", out var miniShippingBinData):
                e.Load(miniShippingBinData, -1);
                return;

            // Shipping Bin
            case ShippingBin or Farm or IslandWest
                when config.VanillaStorages.TryGetValue("Shipping Bin", out var shippingBinData):
                e.Load(shippingBinData, -1);
                return;

            // Stone Chest
            case Chest
            {
                playerChest.Value: true,
                SpecialChestType: Chest.SpecialChestTypes.None,
                ParentSheetIndex: 232,
            } when config.VanillaStorages.TryGetValue("Stone Chest", out var stoneChestData):
                e.Load(stoneChestData, -1);
                return;
        }
    }
}
