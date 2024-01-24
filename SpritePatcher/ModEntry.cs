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
using StardewMods.SpritePatcher.Framework.Services.NetEvents;
using StardewMods.SpritePatcher.Framework.Services.Patchers.Buildings;
using StardewMods.SpritePatcher.Framework.Services.Patchers.Characters;
using StardewMods.SpritePatcher.Framework.Services.Patchers.Items;
using StardewMods.SpritePatcher.Framework.Services.Patchers.TerrainFeatures;
using StardewMods.SpritePatcher.Framework.Services.Patchers.Tools;

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
        this.container.RegisterSingleton<CodeManager>();
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
        this.container.RegisterSingleton<SpriteFactory>();
        this.container.RegisterSingleton<INetEventManager, NetEventManager>();
        this.container.RegisterSingleton<ISpriteSheetManager, SpriteSheetManager>();

        this.container.Collection.Register<ISpritePatcher>(
            new[]
            {
                typeof(AnimatedSpritePatcher),
                typeof(BootsPatcher),
                typeof(BuildingPatcher),
                typeof(BushPatcher),
                typeof(ChestPatcher),
                typeof(ChildPatcher),
                typeof(ClothingPatcher),
                typeof(ColoredObjectPatcher),
                typeof(CombinedRingPatcher),
                typeof(CosmeticPlantPatcher),
                typeof(CrabPotPatcher),
                typeof(FarmAnimalPatcher),
                typeof(FencePatcher),
                typeof(FishingRodPatcher),
                typeof(FishPondPatcher),
                typeof(FishTankFurniturePatcher),
                typeof(FlooringPatcher),
                typeof(FruitTreePatcher),
                typeof(FurniturePatcher),
                typeof(GiantCropPatcher),
                typeof(GrassPatcher),
                typeof(HatPatcher),
                typeof(HoeDirtPatcher),
                typeof(HorsePatcher),
                typeof(IndoorPotPatcher),
                typeof(ItemPedestalPatcher),
                typeof(JunimoHarvesterPatcher),
                typeof(JunimoHutPatcher),
                typeof(JunimoPatcher),
                typeof(MeleeWeaponPatcher),
                typeof(ObjectPatcher),
                typeof(PetPatcher),
                typeof(PetBowlPatcher),
                typeof(ResourceClumpPatcher),
                typeof(RingPatcher),
                typeof(ShippingBinPatcher),
                typeof(SlingshotPatcher),
                typeof(TreePatcher),
                typeof(WallpaperPatcher),
                typeof(WateringCanPatcher),
                typeof(WoodChipperPatcher),
            },
            Lifestyle.Singleton);

        // Verify
        this.container.Verify();

        var configManager = this.container.GetInstance<ConfigManager>();
        configManager.Init();
    }
}