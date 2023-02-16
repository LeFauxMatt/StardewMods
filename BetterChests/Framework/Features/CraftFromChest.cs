namespace StardewMods.BetterChests.Framework.Features;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewValley.Locations;

/// <summary>
///     Craft using items from placed chests and chests in the farmer's inventory.
/// </summary>
internal sealed class CraftFromChest : Feature
{
#nullable disable
    private static Feature Instance;
#nullable enable

    private readonly ModConfig _config;
    private readonly IModHelper _helper;

    private CraftFromChest(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
    }

    private static IEnumerable<StorageNode> Eligible
    {
        get
        {
            foreach (var storage in Storages.All)
            {
                if (storage.CraftFromChest is FeatureOptionRange.Disabled or FeatureOptionRange.Default
                    || storage.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
                    || (storage.CraftFromChestDisableLocations.Contains("UndergroundMine")
                        && Game1.player.currentLocation is MineShaft mineShaft
                        && mineShaft.Name.StartsWith("UndergroundMine"))
                    || storage is not { Data: Storage storageObject }
                    || !storage.CraftFromChest.WithinRangeOfPlayer(
                        storage.CraftFromChestDistance,
                        storageObject.Location,
                        storageObject.Position))
                {
                    continue;
                }

                yield return storage;
            }
        }
    }

    /// <summary>
    ///     Initializes <see cref="CraftFromChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="CraftFromChest" /> class.</returns>
    public static Feature Init(IModHelper helper, ModConfig config)
    {
        return CraftFromChest.Instance ??= new CraftFromChest(helper, config);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        BetterCrafting.CraftingStoragesLoading += CraftFromChest.OnCraftingStoragesLoading;
        this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

        // Integrations
        if (Integrations.ToolbarIcons.IsLoaded)
        {
            Integrations.ToolbarIcons.Api.AddToolbarIcon(
                "BetterChests.CraftFromChest",
                "furyx639.BetterChests/Icons",
                new(32, 0, 16, 16),
                I18n.Button_CraftFromChest_Name());
            Integrations.ToolbarIcons.Api.ToolbarIconPressed += CraftFromChest.OnToolbarIconPressed;
        }

        if (!Integrations.BetterCrafting.IsLoaded)
        {
            return;
        }

        Integrations.BetterCrafting.Api.RegisterInventoryProvider(typeof(StorageNode), new StorageProvider());
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterCrafting.CraftingStoragesLoading -= CraftFromChest.OnCraftingStoragesLoading;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

        // Integrations
        if (Integrations.ToolbarIcons.IsLoaded)
        {
            Integrations.ToolbarIcons.Api.RemoveToolbarIcon("BetterChests.CraftFromChest");
            Integrations.ToolbarIcons.Api.ToolbarIconPressed -= CraftFromChest.OnToolbarIconPressed;
        }

        if (!Integrations.BetterCrafting.IsLoaded)
        {
            return;
        }

        Integrations.BetterCrafting.Api.UnregisterInventoryProvider(typeof(ChestStorage));
        Integrations.BetterCrafting.Api.UnregisterInventoryProvider(typeof(FridgeStorage));
        Integrations.BetterCrafting.Api.UnregisterInventoryProvider(typeof(JunimoHutStorage));
        Integrations.BetterCrafting.Api.UnregisterInventoryProvider(typeof(ObjectStorage));
        Integrations.BetterCrafting.Api.UnregisterInventoryProvider(typeof(ShippingBinStorage));
    }

    private static void OnCraftingStoragesLoading(object? sender, CraftingStoragesLoadingEventArgs e)
    {
        e.AddStorages(CraftFromChest.Eligible);
    }

    private static void OnToolbarIconPressed(object? sender, string id)
    {
        if (id != "BetterChests.CraftFromChest")
        {
            return;
        }

        if (!CraftFromChest.Eligible.Any())
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        BetterCrafting.ShowCraftingPage();
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this._config.ControlScheme.OpenCrafting.JustPressed())
        {
            return;
        }

        this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.OpenCrafting);
        if (!CraftFromChest.Eligible.Any())
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        BetterCrafting.ShowCraftingPage();
    }
}