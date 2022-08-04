namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Storages;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Craft using items from placed chests and chests in the farmer's inventory.
/// </summary>
internal class CraftFromChest : IFeature
{
    private const int MaxTimeOut = 60;
    private readonly PerScreen<List<IStorageObject>> _cachedEligible = new(() => new());
    private readonly PerScreen<int> _currentTab = new();
    private readonly PerScreen<List<IStorageObject>> _lockedEligible = new(() => new());
    private readonly PerScreen<int> _timeOut = new();

    private CraftFromChest(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
    }

    private static IEnumerable<IStorageObject> Eligible =>
        from storage in StorageHelper.All
        where storage is not ChestStorage { Chest: { SpecialChestType: Chest.SpecialChestTypes.JunimoChest } }
           && storage.CraftFromChest != FeatureOptionRange.Disabled
           && storage.CraftFromChestDisableLocations?.Contains(Game1.player.currentLocation.Name) != true
           && !(storage.CraftFromChestDisableLocations?.Contains("UndergroundMine") == true
             && Game1.player.currentLocation is MineShaft mineShaft
             && mineShaft.Name.StartsWith("UndergroundMine"))
           && storage.Parent is not null
           && RangeHelper.IsWithinRangeOfPlayer(
                  storage.CraftFromChest,
                  storage.CraftFromChestDistance,
                  storage.Parent,
                  storage.Position)
        select storage;

    private static CraftFromChest? Instance { get; set; }

    private List<IStorageObject> CachedEligible => this._cachedEligible.Value;

    private ModConfig Config { get; }

    private int CurrentTab
    {
        get => this._currentTab.Value;
        set => this._currentTab.Value = value;
    }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    private List<IStorageObject> LockedEligible => this._lockedEligible.Value;

    private int TimeOut
    {
        get => this._timeOut.Value;
        set => this._timeOut.Value = value;
    }

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
        if (this.IsActivated)
        {
            return;
        }

        this.IsActivated = true;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

        if (IntegrationHelper.ToolbarIcons.IsLoaded)
        {
            IntegrationHelper.ToolbarIcons.API.AddToolbarIcon(
                "BetterChests.CraftFromChest",
                "furyx639.BetterChests/Icons",
                new(32, 0, 16, 16),
                I18n.Button_CraftFromChest_Name());
            IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
        }

        if (IntegrationHelper.BetterCrafting.IsLoaded)
        {
            IntegrationHelper.BetterCrafting.API.RegisterInventoryProvider(
                typeof(StorageWrapper),
                new StorageProvider());
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this.IsActivated)
        {
            return;
        }

        this.IsActivated = false;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.GameLoop.UpdateTicking -= this.OnUpdateTicking;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

        if (IntegrationHelper.ToolbarIcons.IsLoaded)
        {
            IntegrationHelper.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.CraftFromChest");
            IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
        }

        if (IntegrationHelper.BetterCrafting.IsLoaded)
        {
            IntegrationHelper.BetterCrafting.API.UnregisterInventoryProvider(typeof(StorageWrapper));
        }
    }

    private void ExitFunction()
    {
        foreach (var storage in this.LockedEligible)
        {
            storage.Mutex?.ReleaseLock();
        }

        this.LockedEligible.Clear();
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Config.ControlScheme.OpenCrafting.JustPressed())
        {
            return;
        }

        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.OpenCrafting);
        this.OpenCrafting();
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == "BetterChests.CraftFromChest")
        {
            this.OpenCrafting();
        }
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (this.TimeOut == 0)
        {
            return;
        }

        // Chest locking timed out
        if (this.CachedEligible.Count != 0 && --this.TimeOut != 0)
        {
            return;
        }

        for (var index = this.CachedEligible.Count - 1; index >= 0; index--)
        {
            this.CachedEligible[index].Mutex?.ReleaseLock();
        }

        this.CachedEligible.Clear();
        this.TimeOut = 0;
        var width = 800 + IClickableMenu.borderWidth * 2;
        var height = 600 + IClickableMenu.borderWidth * 2;
        var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height).ToPoint();
        Game1.activeClickableMenu = new CraftingPage(
            x,
            y,
            width,
            height,
            false,
            true,
            this.LockedEligible.OfType<ChestStorage>().Select(storage => storage.Chest).ToList())
        {
            exitFunction = this.ExitFunction,
        };
    }

    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        for (var index = this.CachedEligible.Count - 1; index >= 0; index--)
        {
            var storage = this.CachedEligible[index];
            storage.Mutex?.Update(storage.Parent as GameLocation ?? Game1.currentLocation);
        }

        if (Game1.activeClickableMenu is not GameMenu { currentTab: var currentTab } gameMenu
         || currentTab == this.CurrentTab)
        {
            return;
        }

        this.CurrentTab = currentTab;
        if (gameMenu.pages[currentTab] is not CraftingPage craftingPage)
        {
            return;
        }

        craftingPage._materialContainers ??= new();
        craftingPage._materialContainers.AddRange(
            CraftFromChest.Eligible.OfType<ChestStorage>().Select(storage => storage.Chest));
        craftingPage._materialContainers = craftingPage._materialContainers.Distinct().ToList();
    }

    private void OpenCrafting()
    {
        this.CachedEligible.Clear();
        this.CachedEligible.AddRange(CraftFromChest.Eligible);
        if (!this.CachedEligible.Any())
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        if (IntegrationHelper.BetterCrafting.IsLoaded)
        {
            IntegrationHelper.BetterCrafting.API.OpenCraftingMenu(
                false,
                false,
                null,
                null,
                null,
                false,
                this.CachedEligible.Select(
                    storage => new Tuple<object, GameLocation>(
                        new StorageWrapper(storage),
                        storage.Parent as GameLocation ?? Game1.currentLocation)).ToList());
            return;
        }

        this.TimeOut = CraftFromChest.MaxTimeOut;
        for (var index = this.CachedEligible.Count - 1; index >= 0; index--)
        {
            var storage = this.CachedEligible[index];
            storage.Mutex?.RequestLock(
                () => this.LockedEligible.Add(storage),
                () => this.CachedEligible.Remove(storage));
        }
    }
}