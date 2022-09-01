namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>
///     Craft using items from placed chests and chests in the farmer's inventory.
/// </summary>
internal class CraftFromChest : IFeature
{
    private static CraftFromChest? Instance;

    private readonly ModConfig _config;
    private readonly IModHelper _helper;

    private bool _isActivated;

    private CraftFromChest(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
    }

    private static IEnumerable<IStorageObject> Eligible =>
        from storage in Storages.All
        where storage.CraftFromChest is not (FeatureOptionRange.Disabled or FeatureOptionRange.Default)
           && storage.CraftFromChestDisableLocations?.Contains(Game1.player.currentLocation.Name) != true
           && !(storage.CraftFromChestDisableLocations?.Contains("UndergroundMine") == true
             && Game1.player.currentLocation is MineShaft mineShaft
             && mineShaft.Name.StartsWith("UndergroundMine"))
           && storage.CraftFromChest.WithinRangeOfPlayer(
                  storage.CraftFromChestDistance,
                  storage.Location,
                  storage.Position)
        select storage;

    /// <summary>
    ///     Initializes <see cref="CraftFromChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="CraftFromChest" /> class.</returns>
    public static CraftFromChest Init(IModHelper helper, ModConfig config)
    {
        return CraftFromChest.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        this._helper.Events.Display.MenuChanged += CraftFromChest.OnMenuChanged;
        this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

        if (Integrations.ToolbarIcons.IsLoaded)
        {
            Integrations.ToolbarIcons.API.AddToolbarIcon(
                "BetterChests.CraftFromChest",
                "furyx639.BetterChests/Icons",
                new(32, 0, 16, 16),
                I18n.Button_CraftFromChest_Name());
            Integrations.ToolbarIcons.API.ToolbarIconPressed += CraftFromChest.OnToolbarIconPressed;
        }

        if (Integrations.BetterCrafting.IsLoaded)
        {
            Integrations.BetterCrafting.API.RegisterInventoryProvider(typeof(StorageWrapper), new StorageProvider());
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        this._helper.Events.Display.MenuChanged -= CraftFromChest.OnMenuChanged;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

        if (Integrations.ToolbarIcons.IsLoaded)
        {
            Integrations.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.CraftFromChest");
            Integrations.ToolbarIcons.API.ToolbarIconPressed -= CraftFromChest.OnToolbarIconPressed;
        }

        if (Integrations.BetterCrafting.IsLoaded)
        {
            Integrations.BetterCrafting.API.UnregisterInventoryProvider(typeof(StorageWrapper));
        }
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is GameMenu)
        {
            BetterCrafting.AddMaterials(CraftFromChest.Eligible);
        }
    }

    private static void OnToolbarIconPressed(object? sender, string id)
    {
        if (id != "BetterChests.CraftFromChest")
        {
            return;
        }

        if (!BetterCrafting.ShowCraftingPage(CraftFromChest.Eligible))
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this._config.ControlScheme.OpenCrafting.JustPressed())
        {
            return;
        }

        this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.OpenCrafting);
        if (!BetterCrafting.ShowCraftingPage(CraftFromChest.Eligible))
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
        }
    }
}