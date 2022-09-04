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

/// <inheritdoc />
public class BetterChests : Mod
{
    private readonly Dictionary<IFeature, Func<bool>> _features = new();
    private readonly Dictionary<Func<object, bool>, IStorageData> _storageTypes = new();

    private ModConfig? _config;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        Formatting.Translations = this.Helper.Translation;
        CommonHelpers.Multiplayer = this.Helper.Multiplayer;
        I18n.Init(this.Helper.Translation);
        this._config = Config.Init(this.Helper, this.ModManifest, this._features);
        Integrations.Init(this.Helper);
        Storages.Init(this._config, this._storageTypes);
        ThemeHelper.Init(this.Helper, "furyx639.BetterChests/Icons", "furyx639.BetterChests/Tabs/Texture");

        // Events
        this.Helper.Events.Content.AssetRequested += BetterChests.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // Features
        this._features.Add(
            AutoOrganize.Init(this.Helper),
            () => this._config.AutoOrganize is not FeatureOption.Disabled);
        this._features.Add(
            BetterColorPicker.Init(this.Helper, this._config),
            () => this._config.CustomColorPicker is not FeatureOption.Disabled);
        this._features.Add(BetterCrafting.Init(this.Helper, this._config), () => true);
        this._features.Add(BetterItemGrabMenu.Init(this.Helper, this._config), () => true);
        this._features.Add(BetterShippingBin.Init(this.Helper), () => this._config.BetterShippingBin);
        this._features.Add(
            CarryChest.Init(this.Helper, this._config),
            () => this._config.CarryChest is not FeatureOption.Disabled);
        this._features.Add(LabelChest.Init(this.Helper), () => this._config.LabelChest is not FeatureOption.Disabled);
        this._features.Add(ChestFinder.Init(this.Helper, this._config), () => this._config.ChestFinder);
        this._features.Add(
            ChestInfo.Init(this.Helper, this._config),
            () => this._config.ChestInfo is not FeatureOption.Disabled);
        this._features.Add(
            ChestMenuTabs.Init(this.Helper, this._config),
            () => this._config.ChestMenuTabs is not FeatureOption.Disabled);
        this._features.Add(
            CollectItems.Init(this.Helper),
            () => this._config.CollectItems is not FeatureOption.Disabled);
        this._features.Add(
            Configurator.Init(this.Helper, this._config, this.ModManifest),
            () => this._config.Configurator is not FeatureOption.Disabled && Integrations.GMCM.IsLoaded);
        this._features.Add(
            CraftFromChest.Init(this.Helper, this._config),
            () => this._config.CraftFromChest is not FeatureOptionRange.Disabled);
        this._features.Add(FilterItems.Init(this.Helper), () => this._config.FilterItems is not FeatureOption.Disabled);
        this._features.Add(
            OpenHeldChest.Init(this.Helper),
            () => this._config.OpenHeldChest is not FeatureOption.Disabled);
        this._features.Add(
            OrganizeChest.Init(this.Helper),
            () => this._config.OrganizeChest is not FeatureOption.Disabled);
        this._features.Add(ResizeChest.Init(), () => this._config.ResizeChest is not FeatureOption.Disabled);
        this._features.Add(
            ResizeChestMenu.Init(this.Helper),
            () => this._config.ResizeChestMenu is not FeatureOption.Disabled);
        this._features.Add(
            SearchItems.Init(this.Helper, this._config),
            () => this._config.SearchItems is not FeatureOption.Disabled);
        this._features.Add(SlotLock.Init(this.Helper, this._config), () => this._config.SlotLock);
        this._features.Add(
            StashToChest.Init(this.Helper, this._config),
            () => this._config.StashToChest is not FeatureOptionRange.Disabled);
        this._features.Add(
            TransferItems.Init(this.Helper),
            () => this._config.TransferItems is not FeatureOption.Disabled);
        this._features.Add(UnloadChest.Init(this.Helper), () => this._config.UnloadChest is not FeatureOption.Disabled);
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new BetterChestsApi(this._storageTypes, this._config!);
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
    }
}