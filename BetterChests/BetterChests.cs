namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

/// <inheritdoc />
public sealed class BetterChests : Mod
{
    private readonly IList<Tuple<IFeature, Func<bool>>> _features = new List<Tuple<IFeature, Func<bool>>>();

    private ModConfig? _config;

    private ModConfig ModConfig => this._config ??= Config.Init(this.Helper, this.ModManifest, this._features);

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        Formatting.Translations = this.Helper.Translation;
        CommonHelpers.Multiplayer = this.Helper.Multiplayer;
        I18n.Init(this.Helper.Translation);
        Integrations.Init(this.Helper);
        Storages.Init(this.ModConfig);
        ThemeHelper.Init(this.Helper, "furyx639.BetterChests/Icons", "furyx639.BetterChests/Tabs/Texture");

        // Events
        this.Helper.Events.Content.AssetRequested += BetterChests.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // Features
        this.AddFeature(
            AutoOrganize.Init(this.Helper),
            () => this.ModConfig.AutoOrganize is not FeatureOption.Disabled);
        this.AddFeature(
            BetterColorPicker.Init(this.Helper, this.ModConfig),
            () => this.ModConfig.CustomColorPicker is not FeatureOption.Disabled);
        this.AddFeature(BetterCrafting.Init(this.Helper, this.ModConfig), () => true);
        this.AddFeature(BetterItemGrabMenu.Init(this.Helper, this.ModConfig), () => true);
        this.AddFeature(BetterShippingBin.Init(this.Helper), () => this.ModConfig.BetterShippingBin);
        this.AddFeature(
            CarryChest.Init(this.Helper, this.ModConfig),
            () => this.ModConfig.CarryChest is not FeatureOption.Disabled);
        this.AddFeature(LabelChest.Init(this.Helper), () => this.ModConfig.LabelChest is not FeatureOption.Disabled);
        this.AddFeature(ChestFinder.Init(this.Helper, this.ModConfig), () => this.ModConfig.ChestFinder);
        this.AddFeature(
            ChestInfo.Init(this.Helper, this.ModConfig),
            () => this.ModConfig.ChestInfo is not FeatureOption.Disabled);
        this.AddFeature(
            ChestMenuTabs.Init(this.Helper, this.ModConfig),
            () => this.ModConfig.ChestMenuTabs is not FeatureOption.Disabled);
        this.AddFeature(
            CollectItems.Init(this.Helper),
            () => this.ModConfig.CollectItems is not FeatureOption.Disabled);
        this.AddFeature(
            Configurator.Init(this.Helper, this.ModConfig, this.ModManifest),
            () => this.ModConfig.Configurator is not FeatureOption.Disabled && Integrations.GMCM.IsLoaded);
        this.AddFeature(
            CraftFromChest.Init(this.Helper, this.ModConfig),
            () => this.ModConfig.CraftFromChest is not FeatureOptionRange.Disabled);
        this.AddFeature(FilterItems.Init(this.Helper), () => this.ModConfig.FilterItems is not FeatureOption.Disabled);
        this.AddFeature(
            OpenHeldChest.Init(this.Helper),
            () => this.ModConfig.OpenHeldChest is not FeatureOption.Disabled);
        this.AddFeature(
            OrganizeChest.Init(this.Helper),
            () => this.ModConfig.OrganizeChest is not FeatureOption.Disabled);
        this.AddFeature(ResizeChest.Init(), () => this.ModConfig.ResizeChest is not FeatureOption.Disabled);
        this.AddFeature(
            ResizeChestMenu.Init(this.Helper),
            () => this.ModConfig.ResizeChestMenu is not FeatureOption.Disabled);
        this.AddFeature(
            SearchItems.Init(this.Helper, this.ModConfig),
            () => this.ModConfig.SearchItems is not FeatureOption.Disabled);
        this.AddFeature(SlotLock.Init(this.Helper, this.ModConfig), () => this.ModConfig.SlotLock);
        this.AddFeature(
            StashToChest.Init(this.Helper, this.ModConfig),
            () => this.ModConfig.StashToChest is not FeatureOptionRange.Disabled);
        this.AddFeature(
            TransferItems.Init(this.Helper),
            () => this.ModConfig.TransferItems is not FeatureOption.Disabled);
        this.AddFeature(UnloadChest.Init(this.Helper), () => this.ModConfig.UnloadChest is not FeatureOption.Disabled);
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new BetterChestsApi(this.ModConfig);
    }

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

    private void AddFeature(IFeature feature, Func<bool> condition)
    {
        this._features.Add(new(feature, condition));
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        foreach (var (feature, condition) in this._features)
        {
            var featureName = feature.GetType().Name;
            if (Integrations.TestConflicts(featureName, out var mods))
            {
                var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
                Log.Warn(string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{featureName}", modList));
            }
            else if (condition())
            {
                feature.Activate();
            }
        }

        Storages.StorageTypeRequested += this.OnStorageTypeRequested;
    }

    private void OnStorageTypeRequested(object? sender, IStorageTypeRequestedEventArgs e)
    {
        switch (e.Context)
        {
            // Auto-Grabber
            case SObject { ParentSheetIndex: 165 }
                when this.ModConfig.VanillaStorages.TryGetValue("Auto-Grabber", out var autoGrabberData):
                e.Load(autoGrabberData, -1);
                return;

            // Chest
            case Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None, ParentSheetIndex: 130,
            } when this.ModConfig.VanillaStorages.TryGetValue("Chest", out var chestData):
                e.Load(chestData, -1);
                return;

            // Fridge
            case FarmHouse or IslandFarmHouse
                when this.ModConfig.VanillaStorages.TryGetValue("Fridge", out var fridgeData):
                e.Load(fridgeData, -1);
                return;

            // Junimo Chest
            case Chest { playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.JunimoChest }
                when this.ModConfig.VanillaStorages.TryGetValue("Junimo Chest", out var junimoChestData):
                e.Load(junimoChestData, -1);
                return;

            // Junimo Hut
            case JunimoHut when this.ModConfig.VanillaStorages.TryGetValue("Junimo Hut", out var junimoHutData):
                e.Load(junimoHutData, -1);
                return;

            // Mini-Fridge
            case Chest { fridge.Value: true, playerChest.Value: true }
                when this.ModConfig.VanillaStorages.TryGetValue("Mini-Fridge", out var miniFridgeData):
                e.Load(miniFridgeData, -1);
                return;

            // Mini-Shipping Bin
            case Chest { playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin }
                when this.ModConfig.VanillaStorages.TryGetValue("Mini-Shipping Bin", out var miniShippingBinData):
                e.Load(miniShippingBinData, -1);
                return;

            // Shipping Bin
            case ShippingBin or Farm or IslandWest
                when this.ModConfig.VanillaStorages.TryGetValue("Shipping Bin", out var shippingBinData):
                e.Load(shippingBinData, -1);
                return;

            // Stone Chest
            case Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None, ParentSheetIndex: 232,
            } when this.ModConfig.VanillaStorages.TryGetValue("Stone Chest", out var stoneChestData):
                e.Load(stoneChestData, -1);
                return;
        }
    }
}