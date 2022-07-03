namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
public class BetterChests : Mod
{
    private ModConfig? Config { get; set; }

    private Dictionary<string, (IFeature Feature, Func<bool> Condition)> Features { get; } = new();

    private Dictionary<KeyValuePair<string, string>, IStorageData> StorageTypes { get; } = new();

    private Dictionary<string, string> Tabs
    {
        get
        {
            var tabs = this.Helper.Data.ReadJsonFile<Dictionary<string, string>>("assets/tabs.json");
            if (tabs is null)
            {
                tabs = new()
                {
                    {
                        "Clothing",
                        "/furyx639.BetterChests\\Tabs\\Texture/0/category_clothing category_boots category_hat"
                    },
                    {
                        "Cooking",
                        "/furyx639.BetterChests\\Tabs\\Texture/1/category_syrup category_artisan_goods category_ingredients category_sell_at_pierres_and_marnies category_sell_at_pierres category_meat category_cooking category_milk category_egg"
                    },
                    {
                        "Crops",
                        "/furyx639.BetterChests\\Tabs\\Texture/2/category_greens category_flowers category_fruits category_vegetable"
                    },
                    {
                        "Equipment",
                        "/furyx639.BetterChests\\Tabs\\Texture/3/category_equipment category_ring category_tool category_weapon"
                    },
                    {
                        "Fishing",
                        "/furyx639.BetterChests\\Tabs\\Texture/4/category_bait category_fish category_tackle category_sell_at_fish_shop"
                    },
                    {
                        "Materials",
                        "/furyx639.BetterChests\\Tabs\\Texture/5/category_monster_loot category_metal_resources category_building_resources category_minerals category_crafting category_gem"
                    },
                    {
                        "Misc",
                        "/furyx639.BetterChests\\Tabs\\Texture/6/category_big_craftable category_furniture category_junk"
                    },
                    {
                        "Seeds",
                        "/furyx639.BetterChests\\Tabs\\Texture/7/category_seeds category_fertilizer"
                    },
                };

                this.Helper.Data.WriteJsonFile("assets/tabs.json", tabs);
            }

            return tabs;
        }
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        I18n.Init(helper.Translation);
        this.Config = ConfigHelper.Init(this.Helper, this.ModManifest, this.Features);
        IntegrationHelper.Init(this.Helper, this.Config);
        StorageHelper.Init(this.Helper.Multiplayer, this.Config, this.StorageTypes);

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // Features
        this.Features.Add(nameof(AutoOrganize), new(AutoOrganize.Init(this.Helper), () => this.Config.DefaultChest.AutoOrganize != FeatureOption.Disabled));
        this.Features.Add(nameof(BetterColorPicker), new(BetterColorPicker.Init(this.Helper, this.Config), () => this.Config.DefaultChest.CustomColorPicker != FeatureOption.Disabled));
        this.Features.Add(nameof(BetterItemGrabMenu), new(BetterItemGrabMenu.Init(this.Helper, this.Config), () => true));
        this.Features.Add(nameof(BetterShippingBin), new(BetterShippingBin.Init(this.Helper), () => this.Config.BetterShippingBin));
        this.Features.Add(nameof(CarryChest), new(CarryChest.Init(this.Helper, this.Config), () => this.Config.DefaultChest.CarryChest != FeatureOption.Disabled));
        this.Features.Add(nameof(CategorizeChest), new(CategorizeChest.Init(this.Helper), () => this.Config.CategorizeChest));
        this.Features.Add(nameof(LabelChest), new(LabelChest.Init(this.Helper), () => this.Config.LabelChest));
        this.Features.Add(nameof(ChestFinder), new(ChestFinder.Init(this.Helper), () => this.Config.ChestFinder));
        this.Features.Add(nameof(ChestMenuTabs), new(ChestMenuTabs.Init(this.Helper, this.Config), () => this.Config.DefaultChest.ChestMenuTabs != FeatureOption.Disabled));
        this.Features.Add(nameof(CollectItems), new(CollectItems.Init(this.Helper), () => this.Config.DefaultChest.CollectItems != FeatureOption.Disabled));
        this.Features.Add(nameof(Configurator), new(Configurator.Init(this.Helper, this.Config), () => this.Config.Configurator));
        this.Features.Add(nameof(CraftFromChest), new(CraftFromChest.Init(this.Helper, this.Config), () => this.Config.DefaultChest.CraftFromChest != FeatureOptionRange.Disabled));
        this.Features.Add(nameof(FilterItems), new(FilterItems.Init(this.Helper), () => this.Config.DefaultChest.FilterItems != FeatureOption.Disabled));
        this.Features.Add(nameof(OpenHeldChest), new(OpenHeldChest.Init(this.Helper), () => this.Config.DefaultChest.OpenHeldChest != FeatureOption.Disabled));
        this.Features.Add(nameof(OrganizeChest), new(OrganizeChest.Init(this.Helper), () => this.Config.DefaultChest.OrganizeChest != FeatureOption.Disabled));
        this.Features.Add(nameof(ResizeChest), new(ResizeChest.Init(), () => this.Config.DefaultChest.ResizeChest != FeatureOption.Disabled));
        this.Features.Add(nameof(ResizeChestMenu), new(ResizeChestMenu.Init(this.Helper), () => this.Config.DefaultChest.ResizeChestMenu != FeatureOption.Disabled));
        this.Features.Add(nameof(SearchItems), new(SearchItems.Init(this.Helper, this.Config), () => this.Config.DefaultChest.SearchItems != FeatureOption.Disabled));
        this.Features.Add(nameof(SlotLock), new(SlotLock.Init(this.Helper, this.Config), () => this.Config.SlotLock));
        this.Features.Add(nameof(StashToChest), new(StashToChest.Init(this.Helper, this.Config), () => this.Config.DefaultChest.StashToChest != FeatureOptionRange.Disabled));
        this.Features.Add(nameof(UnloadChest), new(UnloadChest.Init(this.Helper), () => this.Config.DefaultChest.UnloadChest != FeatureOption.Disabled));
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new BetterChestsApi(this.StorageTypes);
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.BetterChests/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo("furyx639.BetterChests/Tabs"))
        {
            e.LoadFrom(() => this.Tabs, AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo("furyx639.BetterChests/Tabs/Texture"))
        {
            e.LoadFromModFile<Texture2D>("assets/tabs.png", AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo("furyx639.FuryCore/ConfigTool"))
        {
            e.LoadFromModFile<Texture2D>("assets/wrench.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        foreach (var (featureName, (feature, condition)) in this.Features)
        {
            if (IntegrationHelper.TestConflicts(featureName, out var mods))
            {
                var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
                Log.Warn(string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{featureName}", modList));
            }
            else if (condition())
            {
                feature.Activate();
            }
        }
    }
}