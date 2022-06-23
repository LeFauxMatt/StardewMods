namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Storages;
using StardewMods.Common.Enums;
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

    private readonly PerScreen<List<BaseStorage>> _cachedEligible = new(() => new());
    private readonly PerScreen<int> _timeOut = new();

    private CraftFromChest(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
    }

    private static IEnumerable<BaseStorage> Eligible
    {
        get
        {
            foreach (var storage in StorageHelper.World)
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

                if (RangeHelper.IsWithinRangeOfPlayer(storage.CraftFromChest, storage.CraftFromChestDistance, storage.Location, storage.Position))
                {
                    yield return storage;
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

                yield return storage;
            }
        }
    }

    private static CraftFromChest? Instance { get; set; }

    private List<BaseStorage> CachedEligible
    {
        get => this._cachedEligible.Value;
    }

    private ModConfig Config { get; }

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
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="CraftFromChest" /> class.</returns>
    public static CraftFromChest Init(IModHelper helper, ModConfig config)
    {
        return CraftFromChest.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (IntegrationHelper.ToolbarIcons.IsLoaded)
        {
            IntegrationHelper.ToolbarIcons.API.AddToolbarIcon(
                "BetterChests.CraftFromChest",
                "furyx639.BetterChests/Icons",
                new(32, 0, 16, 16),
                I18n.Button_CraftFromChest_Name());
            IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
        }

        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (IntegrationHelper.ToolbarIcons.IsLoaded)
        {
            IntegrationHelper.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.CraftFromChest");
            IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
        }

        this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private void ExitFunction()
    {
        foreach (var storage in this.CachedEligible.Where(storage => storage.Mutex?.IsLockHeld() == true))
        {
            storage.Mutex?.ReleaseLock();
        }

        this.CachedEligible.Clear();
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
            case { } when this.CachedEligible.Any():
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
        if (!this.CachedEligible.Any() || this.TimeOut == 0)
        {
            return;
        }

        // Chest locking timed out
        if (--this.TimeOut == 0 || this.CachedEligible.All(storage => storage.Mutex?.IsLockHeld() == true))
        {
            this.TimeOut = 0;
            var width = 800 + IClickableMenu.borderWidth * 2;
            var height = 600 + IClickableMenu.borderWidth * 2;
            var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            Game1.activeClickableMenu = new CraftingPage(
                (int)x,
                (int)y,
                width,
                height,
                false,
                true,
                this.CachedEligible.Where(storage => storage.Mutex?.IsLockHeld() == true)
                    .OfType<ChestStorage>()
                    .Select(storage => storage.Chest)
                    .ToList())
            {
                exitFunction = this.ExitFunction,
            };
            return;
        }

        // Attempt to lock chests
        foreach (var storage in this.CachedEligible)
        {
            storage.Mutex?.Update(storage.Location);
        }
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
                this.CachedEligible.Select(storage => new Tuple<object, GameLocation>(storage, storage.Location)).ToList());
            this.CachedEligible.Clear();
        }

        this.TimeOut = CraftFromChest.MaxTimeOut;
        foreach (var storage in this.CachedEligible)
        {
            storage.Mutex?.RequestLock();
        }
    }
}