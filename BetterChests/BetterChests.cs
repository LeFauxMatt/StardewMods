namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;

/// <inheritdoc />
public class BetterChests : Mod
{
    private ModConfig? Config { get; set; }

    private Dictionary<string, (IFeature Feature, Func<bool> Condition)> Features { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        I18n.Init(helper.Translation);
        this.Config = ConfigHelper.Init(this.Helper, this.ModManifest, this.Features);
        IntegrationHelper.Init(this.Helper, this.Config);
        StorageHelper.Init(this.Helper.Multiplayer, this.Config);

        // Events
        this.Helper.Events.Content.AssetRequested += BetterChests.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // Features
        this.Features.Add(nameof(AutoOrganize), new(AutoOrganize.Init(this.Helper), () => this.Config.DefaultChest.AutoOrganize != FeatureOption.Disabled));
        this.Features.Add(nameof(BetterColorPicker), new(BetterColorPicker.Init(this.Helper, this.Config), () => this.Config.DefaultChest.CustomColorPicker != FeatureOption.Disabled));
        this.Features.Add(nameof(BetterItemGrabMenu), new(BetterItemGrabMenu.Init(this.Helper, this.Config), () => true));
        this.Features.Add(nameof(BetterShippingBin), new(BetterShippingBin.Init(this.Helper), () => true));
        this.Features.Add(nameof(CarryChest), new(CarryChest.Init(this.Helper, this.Config), () => this.Config.DefaultChest.CarryChest != FeatureOption.Disabled));
        this.Features.Add(nameof(CategorizeChest), new(CategorizeChest.Init(this.Helper), () => this.Config.CategorizeChest));
        this.Features.Add(nameof(ChestMenuTabs), new(ChestMenuTabs.Init(this.Helper, this.Config), () => this.Config.DefaultChest.ChestMenuTabs != FeatureOption.Disabled));
        this.Features.Add(nameof(CollectItems), new(CollectItems.Init(this.Helper), () => this.Config.DefaultChest.CollectItems != FeatureOption.Disabled));
        this.Features.Add(nameof(Configurator), new(Configurator.Init(this.Helper, this.Config), () => this.Config.Configurator));
        this.Features.Add(nameof(CraftFromChest), new(CraftFromChest.Init(this.Helper, this.Config), () => this.Config.DefaultChest.CraftFromChest != FeatureOptionRange.Disabled));
        this.Features.Add(nameof(FilterItems), new(FilterItems.Init(this.Helper), () => this.Config.DefaultChest.FilterItems != FeatureOption.Disabled));
        this.Features.Add(nameof(OpenHeldChest), new(OpenHeldChest.Init(this.Helper), () => this.Config.DefaultChest.OpenHeldChest != FeatureOption.Disabled));
        this.Features.Add(nameof(OrganizeChest), new(OrganizeChest.Init(this.Helper), () => this.Config.DefaultChest.OrganizeChest != FeatureOption.Disabled));
        this.Features.Add(nameof(ResizeChest), new(ResizeChest.Init(this.Helper), () => this.Config.DefaultChest.ResizeChest != FeatureOption.Disabled));
        this.Features.Add(nameof(ResizeChestMenu), new(ResizeChestMenu.Init(this.Helper), () => this.Config.DefaultChest.ResizeChestMenu != FeatureOption.Disabled));
        this.Features.Add(nameof(SearchItems), new(SearchItems.Init(this.Helper, this.Config), () => this.Config.DefaultChest.SearchItems != FeatureOption.Disabled));
        this.Features.Add(nameof(SlotLock), new(SlotLock.Init(this.Helper, this.Config), () => this.Config.SlotLock));
        this.Features.Add(nameof(StashToChest), new(StashToChest.Init(this.Helper, this.Config), () => this.Config.DefaultChest.StashToChest != FeatureOptionRange.Disabled));
        this.Features.Add(nameof(UnloadChest), new(UnloadChest.Init(this.Helper), () => this.Config.DefaultChest.UnloadChest != FeatureOption.Disabled));
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.BetterChests/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo("furyx639.BetterChests/Tabs"))
        {
            e.LoadFromModFile<Dictionary<string, string>>("assets/tabs.json", AssetLoadPriority.Exclusive);
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
        foreach (var (feature, condition) in this.Features.Values)
        {
            if (condition())
            {
                feature.Activate();
            }
        }
    }
}