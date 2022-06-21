namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using Common.Enums;
using Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Services;

/// <inheritdoc />
public class BetterChests : Mod
{
    private IDictionary<string, IFeature> Features { get; } = new Dictionary<string, IFeature>();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        Integrations.Init(helper);
        Log.Monitor = this.Monitor;

        // Config
        ModConfig? config = null;
        try
        {
            config = helper.ReadConfig<ModConfig>();
        }
        catch (Exception)
        {
            // ignored
        }

        Config.Data = config ?? new ModConfig();
        StorageHelper.Init(this.Helper.Multiplayer);

        // Core Services
        this.Services.Add(
            new AssetHandler(this.Config, this.Helper),
            new CommandHandler(this.Config, this.Helper, this.Services),
            new ManagedObjects(this.Config, this.Services),
            new ModConfigMenu(this.Config, this.Helper, this.ModManifest, this.Services),
            new ModIntegrations(this.Helper, this.Services));

        // Features
        AutoOrganize.Init(this.Helper);
        this.Features.Add(nameof(AutoOrganize), AutoOrganize.Init(this.Helper));
        this.Features.Add(nameof(BetterItemGrabMenu), BetterItemGrabMenu.Init(this.Helper));
        this.Features.Add(nameof(BetterShippingBin), BetterShippingBin.Init(this.Helper));
        this.Features.Add(nameof(CarryChest), CarryChest.Init(this.Helper));
        this.Features.Add(nameof(CollectItems), CollectItems.Init(this.Helper));
        this.Features.Add(nameof(CraftFromChest), CraftFromChest.Init(this.Helper));
        this.Features.Add(nameof(CustomColorPicker), CustomColorPicker.Init(this.Helper));
        this.Features.Add(nameof(FilterItems), FilterItems.Init(this.Helper));
        this.Features.Add(nameof(OpenHeldChest), OpenHeldChest.Init(this.Helper));
        this.Features.Add(nameof(OrganizeChest), OrganizeChest.Init(this.Helper));
        this.Features.Add(nameof(ResizeChest), ResizeChest.Init(this.Helper));
        this.Features.Add(nameof(ResizeChestMenu), ResizeChestMenu.Init(this.Helper));
        this.Features.Add(nameof(SearchItems), SearchItems.Init(this.Helper));
        this.Features.Add(nameof(SlotLock), SlotLock.Init(this.Helper));
        this.Features.Add(nameof(StashToChest), StashToChest.Init(this.Helper));
        this.Features.Add(nameof(UnloadChest), UnloadChest.Init(this.Helper));

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new BetterChestsApi(this.Services);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Features
        this.Services.Add(
            //new CategorizeChest(this.Config, this.Helper, this.Services),
            //new ChestMenuTabs(this.Config, this.Helper, this.Services),
            new Configurator(this.Config, this.Helper, this.Services));

        // Activate Features
        if (Config.DefaultChest.AutoOrganize != FeatureOption.Disabled)
        {
            this.Features[nameof(AutoOrganize)].Activate();
        }

        this.Features[nameof(BetterItemGrabMenu)].Activate();
        this.Features[nameof(BetterShippingBin)].Activate();

        if (Config.DefaultChest.CarryChest != FeatureOption.Disabled)
        {
            this.Features[nameof(CarryChest)].Activate();
        }

        if (Config.DefaultChest.CollectItems != FeatureOption.Disabled)
        {
            this.Features[nameof(CollectItems)].Activate();
        }

        if (Config.DefaultChest.CraftFromChest != FeatureOptionRange.Disabled)
        {
            this.Features[nameof(CraftFromChest)].Activate();
        }

        if (Config.DefaultChest.CustomColorPicker != FeatureOption.Disabled)
        {
            this.Features[nameof(CustomColorPicker)].Activate();
        }

        if (Config.DefaultChest.FilterItems != FeatureOption.Disabled)
        {
            this.Features[nameof(FilterItems)].Activate();
        }

        if (Config.DefaultChest.OpenHeldChest != FeatureOption.Disabled)
        {
            this.Features[nameof(OpenHeldChest)].Activate();
        }

        if (Config.DefaultChest.OrganizeChest != FeatureOption.Disabled)
        {
            this.Features[nameof(OrganizeChest)].Activate();
        }

        if (Config.DefaultChest.ResizeChest != FeatureOption.Disabled)
        {
            this.Features[nameof(ResizeChest)].Activate();
        }

        if (Config.DefaultChest.ResizeChestMenu != FeatureOption.Disabled)
        {
            this.Features[nameof(ResizeChestMenu)].Activate();
        }

        if (Config.DefaultChest.SearchItems != FeatureOption.Disabled)
        {
            this.Features[nameof(SearchItems)].Activate();
        }

        if (Config.SlotLock)
        {
            this.Features[nameof(SlotLock)].Activate();
        }

        if (Config.DefaultChest.StashToChest != FeatureOptionRange.Disabled)
        {
            this.Features[nameof(StashToChest)].Activate();
        }

        if (Config.DefaultChest.UnloadChest != FeatureOption.Disabled)
        {
            this.Features[nameof(UnloadChest)].Activate();
        }
    }
}