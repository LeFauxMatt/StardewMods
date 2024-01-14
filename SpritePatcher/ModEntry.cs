namespace StardewMods.SpritePatcher;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Services;
using StardewMods.SpritePatcher.Framework.Services.Factory;
using StardewMods.SpritePatcher.Framework.Services.TextureMangers.Buildings;
using StardewMods.SpritePatcher.Framework.Services.TextureMangers.Characters;
using StardewMods.SpritePatcher.Framework.Services.TextureMangers.Items;
using StardewMods.SpritePatcher.Framework.Services.TextureMangers.TerrainFeatures;
using StardewMods.SpritePatcher.Framework.Services.TextureMangers.Tools;

/// <inheritdoc />
internal sealed class ModEntry : Mod
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
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.container.RegisterSingleton<ContentPatcherIntegration>();
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<IEventPublisher, EventManager>();
        this.container.RegisterSingleton<IEventSubscriber, EventManager>();
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<ILog, FuryLogger>();
        this.container.RegisterSingleton<IPatchManager, FuryPatcher>();
        this.container.RegisterSingleton<DelegateManager>();
        this.container.RegisterSingleton<ManagedObjectFactory>();
        this.container.RegisterSingleton<TextureBuilder>();

        this.container.Collection.Register<ITextureManager>(
            new[]
            {
                typeof(BootsManager),
                typeof(BuildingManager),
                typeof(BushManager),
                typeof(ChestManager),
                typeof(ChildManager),
                typeof(ClothingManager),
                typeof(ColoredObjectManager),
                typeof(CombinedRingManager),
                typeof(CosmeticPlantManager),
                typeof(CrabPotManager),
                typeof(FarmAnimalManager),
                typeof(FenceManager),
                typeof(FishingRodManager),
                typeof(FishPondManager),
                typeof(FishTankFurnitureManager),
                typeof(FlooringManager),
                typeof(FruitTreeManager),
                typeof(FurnitureManager),
                typeof(GiantCropManager),
                typeof(GrassManager),
                typeof(HatManager),
                typeof(HoeDirtManager),
                typeof(HorseManager),
                typeof(IndoorPotManager),
                typeof(ItemPedestalManager),
                typeof(JunimoHarvesterManager),
                typeof(JunimoHutManager),
                typeof(JunimoManager),
                typeof(MeleeWeaponManager),
                typeof(ObjectManager),
                typeof(PetManager),
                typeof(PetBowlManager),
                typeof(ResourceClumpManager),
                typeof(RingManager),
                typeof(ShippingBinManager),
                typeof(SlingshotManager),
                typeof(TreeManager),
                typeof(WallpaperManager),
                typeof(WateringCanManager),
                typeof(WoodChipperManager),
            },
            Lifestyle.Singleton);

        // Verify
        this.container.Verify();

        var configManager = this.container.GetInstance<ConfigManager>();
        configManager.Init();
    }
}