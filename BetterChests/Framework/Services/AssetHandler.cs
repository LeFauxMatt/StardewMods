namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Interfaces;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    /// <summary>The game path to the hsl texture.</summary>
    public const string HslTexturePath = BaseService.ModPrefix + "/HueBar";

    /// <summary>The game path to the icon texture.</summary>
    public const string IconTexturePath = BaseService.ModPrefix + "/Icons";

    /// <summary>The game path to the tab texture.</summary>
    public const string TabTexturePath = BaseService.ModPrefix + "/Tabs/Texture";

    /// <summary>The game path to tab data.</summary>
    public const string TabDataPath = BaseService.ModPrefix + "/Tabs";

    private readonly IDataHelper data;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="data">Dependency used for storing and retrieving data.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(ILogging logging, IModEvents events, IDataHelper data, IThemeHelper themeHelper)
        : base(logging)
    {
        // Init
        this.data = data;
        themeHelper.AddAssets(AssetHandler.IconTexturePath, AssetHandler.TabTexturePath);

        // Events
        events.Content.AssetRequested += this.OnAssetRequested;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(AssetHandler.HslTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/hue.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(AssetHandler.IconTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(AssetHandler.TabTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/tabs.png", AssetLoadPriority.Exclusive);
        }

        if (e.Name.IsEquivalentTo(AssetHandler.TabDataPath))
        {
            e.LoadFrom(this.GetTabData, AssetLoadPriority.Exclusive);
        }
    }

    private Dictionary<string, InventoryTabData> GetTabData()
    {
        var tabData = this.data.ReadJsonFile<Dictionary<string, InventoryTabData>>("assets/tabs.json");
        if (tabData is not null && tabData.Any())
        {
            return tabData;
        }

        tabData = new Dictionary<string, InventoryTabData>
        {
            { "Clothing", new InventoryTabData("Clothing", AssetHandler.TabTexturePath, 2, ["category_clothing", "category_boots", "category_hat"]) },
            {
                "Cooking",
                new InventoryTabData(
                    "Cooking",
                    AssetHandler.TabTexturePath,
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
            { "Crops", new InventoryTabData("Crops", AssetHandler.TabTexturePath, 4, ["category_greens", "category_flowers", "category_fruits", "category_vegetable"]) },
            { "Equipment", new InventoryTabData("Equipment", AssetHandler.TabTexturePath, 5, ["category_equipment", "category_ring", "category_tool", "category_weapon"]) },
            { "Fishing", new InventoryTabData("Fishing", AssetHandler.TabTexturePath, 6, ["category_bait", "category_fish", "category_tackle", "category_sell_at_fish_shop"]) },
            {
                "Materials",
                new InventoryTabData(
                    "Materials",
                    AssetHandler.TabTexturePath,
                    7,
                    ["category_monster_loot", "category_metal_resources", "category_building_resources", "category_minerals", "category_crafting", "category_gem"])
            },
            { "Misc", new InventoryTabData("Misc", AssetHandler.TabTexturePath, 8, ["category_big_craftable", "category_furniture", "category_junk"]) },
            { "Seeds", new InventoryTabData("Seeds", AssetHandler.TabTexturePath, 9, ["category_seeds", "category_fertilizer"]) },
        };

        this.data.WriteJsonFile("assets/tabs.json", tabData);
        return tabData;
    }
}
