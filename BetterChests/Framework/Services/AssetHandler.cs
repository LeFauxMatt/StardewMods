namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly IDataHelper dataHelper;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="theming">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IDataHelper dataHelper,
        EventManager eventManager,
        ILog log,
        IManifest manifest,
        ITheming theming)
        : base(log, manifest)
    {
        // Init
        this.dataHelper = dataHelper;
        this.HslTexturePath = this.ModId + "/HueBar";
        this.IconTexturePath = this.ModId + "/Icons";
        this.TabTexturePath = this.ModId + "/Tabs/Texture";
        this.TabDataPath = this.ModId + "/Tabs";
        theming.AddAssets([this.IconTexturePath, this.TabTexturePath]);

        // Events
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    /// <summary>Gets the game path to the hsl texture.</summary>
    public string HslTexturePath { get; }

    /// <summary>Gets the game path to the icon texture.</summary>
    public string IconTexturePath { get; }

    /// <summary>Gets the game path to the tab texture.</summary>
    public string TabTexturePath { get; }

    /// <summary>Gets the game path to tab data.</summary>
    public string TabDataPath { get; }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.HslTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/hue.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.IconTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.TabTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/tabs.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.TabDataPath))
        {
            e.LoadFrom(this.GetTabData, AssetLoadPriority.Exclusive);
        }
    }

    private Dictionary<string, InventoryTabData> GetTabData()
    {
        var tabData = this.dataHelper.ReadJsonFile<Dictionary<string, InventoryTabData>>("assets/tabs.json");
        if (tabData is not null && tabData.Any())
        {
            return tabData;
        }

        tabData = new Dictionary<string, InventoryTabData>
        {
            {
                "Clothing",
                new InventoryTabData(
                    "Clothing",
                    this.TabTexturePath,
                    2,
                    ["category_clothing", "category_boots", "category_hat"])
            },
            {
                "Cooking",
                new InventoryTabData(
                    "Cooking",
                    this.TabTexturePath,
                    3,
                    [
                        "category_syrup",
                        "category_artisan_goods",
                        "category_ingredients",
                        "category_sell_at_pierres_and_marnies",
                        "category_sell_at_pierres",
                        "category_meat",
                        "category_cooking",
                        "category_milk",
                        "category_egg",
                    ])
            },
            {
                "Crops",
                new InventoryTabData(
                    "Crops",
                    this.TabTexturePath,
                    4,
                    ["category_greens", "category_flowers", "category_fruits", "category_vegetable"])
            },
            {
                "Equipment",
                new InventoryTabData(
                    "Equipment",
                    this.TabTexturePath,
                    5,
                    ["category_equipment", "category_ring", "category_tool", "category_weapon"])
            },
            {
                "Fishing",
                new InventoryTabData(
                    "Fishing",
                    this.TabTexturePath,
                    6,
                    ["category_bait", "category_fish", "category_tackle", "category_sell_at_fish_shop"])
            },
            {
                "Materials",
                new InventoryTabData(
                    "Materials",
                    this.TabTexturePath,
                    7,
                    [
                        "category_monster_loot",
                        "category_metal_resources",
                        "category_building_resources",
                        "category_minerals",
                        "category_crafting",
                        "category_gem",
                    ])
            },
            {
                "Misc",
                new InventoryTabData(
                    "Misc",
                    this.TabTexturePath,
                    8,
                    ["category_big_craftable", "category_furniture", "category_junk"])
            },
            {
                "Seeds",
                new InventoryTabData("Seeds", this.TabTexturePath, 9, ["category_seeds", "category_fertilizer"])
            },
        };

        this.dataHelper.WriteJsonFile("assets/tabs.json", tabData);
        return tabData;
    }
}