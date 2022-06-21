namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Storages;
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

    private readonly PerScreen<List<EligibleChest>> _cachedEligibleChests = new(() => new());
    private readonly PerScreen<int> _timeOut = new();

    private CraftFromChest(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static IEnumerable<EligibleChest> EligibleChests
    {
        get
        {
            foreach (var (storage, location, position) in StorageHelper.World)
            {
                if (storage is not ChestStorage { Chest: { } chest } || chest is { SpecialChestType: Chest.SpecialChestTypes.JunimoChest })
                {
                    continue;
                }

                // Disabled in config or by location name
                if (storage.CraftFromChest == FeatureOptionRange.Disabled || storage.CraftFromChestDisableLocations?.Contains(Game1.player.currentLocation.Name) == true)
                {
                    continue;
                }

                // Disabled in mines
                if (storage.CraftFromChestDisableLocations?.Contains("UndergroundMine") == true && Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine"))
                {
                    continue;
                }

                var (pX, pY) = Game1.player.getTileLocation();
                switch (storage.CraftFromChest)
                {
                    // Disabled if not current location for location chest
                    case FeatureOptionRange.Location when !location.Equals(Game1.currentLocation):
                        continue;
                    case FeatureOptionRange.World:
                    case FeatureOptionRange.Location when storage.CraftFromChestDistance == -1:
                    case FeatureOptionRange.Location when Math.Abs(position.X - pX) + Math.Abs(position.Y - pY) <= storage.CraftFromChestDistance:
                        yield return new(chest, location);
                        continue;
                    case FeatureOptionRange.Default:
                    case FeatureOptionRange.Disabled:
                    case FeatureOptionRange.Inventory:
                    default:
                        continue;
                }
            }

            foreach (var storage in StorageHelper.Inventory)
            {
                if (storage is not ChestStorage { Chest: { } chest } || chest is { SpecialChestType: Chest.SpecialChestTypes.JunimoChest })
                {
                    continue;
                }

                // Disabled in config
                if (storage.CraftFromChest == FeatureOptionRange.Disabled || storage.OpenHeldChest == FeatureOption.Disabled)
                {
                    continue;
                }

                yield return new(chest, Game1.currentLocation);
            }
        }
    }

    private static CraftFromChest? Instance { get; set; }

    private List<EligibleChest> CachedEligibleChests
    {
        get => this._cachedEligibleChests.Value;
    }

    private IModHelper Helper { get; }

    private int TimeOut
    {
        get => this._timeOut.Value;
        set => this._timeOut.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="CraftFromChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="CraftFromChest" /> class.</returns>
    public static CraftFromChest Init(IModHelper helper)
    {
        return CraftFromChest.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (Integrations.ToolbarIcons.IsLoaded)
        {
            Integrations.ToolbarIcons.API.AddToolbarIcon(
                "BetterChests.CraftFromChest",
                "furyx638.BetterChests/Icons",
                new(32, 0, 16, 16),
                I18n.Button_CraftFromChest_Name());
            Integrations.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
        }

        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (Integrations.ToolbarIcons.IsLoaded)
        {
            Integrations.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.CraftFromChest");
            Integrations.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
        }

        this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private void ExitFunction()
    {
        foreach (var mutex in this.CachedEligibleChests.Select(eligible => eligible.Chest.mutex).Where(mutex => mutex.IsLockHeld()))
        {
            mutex.ReleaseLock();
        }

        this.CachedEligibleChests.Clear();
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !Config.ControlScheme.OpenCrafting.JustPressed())
        {
            return;
        }

        this.Helper.Input.SuppressActiveKeybinds(Config.ControlScheme.OpenCrafting);
        this.OpenCrafting();
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        CraftingPage craftingPage;
        switch (e.NewMenu)
        {
            case GameMenu { currentTab: var currentTab } gameMenu when gameMenu.pages[currentTab] is CraftingPage tab:
                craftingPage = tab;
                break;
            case CraftingPage menu:
                craftingPage = menu;
                break;
            case { } when this.CachedEligibleChests.Any():
                this.ExitFunction();
                return;
            default:
                return;
        }

        // Add player inventory chests to crafting page
        craftingPage._materialContainers ??= new();
        craftingPage._materialContainers.AddRange(
            from storage in StorageHelper.Inventory.OfType<ChestStorage>()
            where storage.CraftFromChest >= FeatureOptionRange.Inventory
                  && storage.OpenHeldChest == FeatureOption.Enabled
                  && storage.Chest is not { SpecialChestType: Chest.SpecialChestTypes.JunimoChest }
            select storage.Chest);
        craftingPage._materialContainers = craftingPage._materialContainers.Distinct().ToList();
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
        // No current attempt to lock chests
        if (!this.CachedEligibleChests.Any() || this.TimeOut == 0)
        {
            return;
        }

        // Chest locking timed out
        if (--this.TimeOut == 0 || this.CachedEligibleChests.All(eligibleChest => eligibleChest.Chest.mutex.IsLockHeld()))
        {
            this.TimeOut = 0;
            var width = 800 + IClickableMenu.borderWidth * 2;
            var height = 600 + IClickableMenu.borderWidth * 2;
            var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            Game1.activeClickableMenu = new CraftingPage((int)x, (int)y, width, height, false, true, this.CachedEligibleChests.Select(eligible => eligible.Chest).Where(chest => chest.mutex.IsLockHeld()).ToList())
            {
                exitFunction = this.ExitFunction,
            };
            return;
        }

        // Attempt to lock chests
        foreach (var (chest, location) in this.CachedEligibleChests)
        {
            chest.mutex.Update(location);
        }
    }

    private void OpenCrafting()
    {
        this.CachedEligibleChests.Clear();
        this.CachedEligibleChests.AddRange(CraftFromChest.EligibleChests);
        if (!this.CachedEligibleChests.Any())
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        if (Integrations.BetterCrafting.IsLoaded)
        {
            Integrations.BetterCrafting.API.OpenCraftingMenu(
                false,
                false,
                null,
                null,
                null,
                false,
                this.CachedEligibleChests.Select(storage => new Tuple<object, GameLocation>(storage.Chest, storage.Location)).ToList());
            this.CachedEligibleChests.Clear();
        }

        this.TimeOut = CraftFromChest.MaxTimeOut;
        foreach (var mutex in this.CachedEligibleChests.Select(eligible => eligible.Chest.mutex))
        {
            mutex.RequestLock();
        }
    }

    private record EligibleChest(Chest Chest, GameLocation Location);
}