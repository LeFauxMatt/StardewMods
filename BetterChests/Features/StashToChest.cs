namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Storages;
using StardewMods.Common.Enums;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>
///     Stash items into placed chests and chests in the farmer's inventory.
/// </summary>
internal class StashToChest : IFeature
{
    private StashToChest(IModHelper helper, ModConfig config)
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
                // Disabled in config or by location name
                if (storage.StashToChest == FeatureOptionRange.Disabled || storage.StashToChestDisableLocations?.Contains(Game1.player.currentLocation.Name) == true)
                {
                    continue;
                }

                // Disabled in mines
                if (storage.StashToChestDisableLocations?.Contains("UndergroundMine") == true && Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine"))
                {
                    continue;
                }

                if (RangeHelper.IsWithinRangeOfPlayer(storage.StashToChest, storage.StashToChestDistance, storage.Location, storage.Position))
                {
                    yield return storage;
                }
            }

            foreach (var storage in StorageHelper.Inventory)
            {
                // Disabled in config
                if (storage.StashToChest == FeatureOptionRange.Disabled || storage.OpenHeldChest == FeatureOption.Disabled)
                {
                    continue;
                }

                yield return storage;
            }
        }
    }

    private static StashToChest? Instance { get; set; }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Initializes <see cref="StashToChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="StashToChest" /> class.</returns>
    public static StashToChest Init(IModHelper helper, ModConfig config)
    {
        return StashToChest.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (IntegrationHelper.ToolbarIcons.IsLoaded)
        {
            IntegrationHelper.ToolbarIcons.API.AddToolbarIcon(
                "BetterChests.StashToChest",
                "furyx639.BetterChests/Icons",
                new(16, 0, 16, 16),
                I18n.Button_StashToChest_Name());
            IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed += StashToChest.OnToolbarIconPressed;
        }

        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.ButtonPressed += StashToChest.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (IntegrationHelper.ToolbarIcons.IsLoaded)
        {
            IntegrationHelper.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.StashToChest");
            IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed -= StashToChest.OnToolbarIconPressed;
        }

        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.Helper.Events.Input.ButtonPressed -= StashToChest.OnButtonPressed;
    }

    private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft || Game1.activeClickableMenu is not ItemGrabMenu { context: { } context } itemGrabMenu)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.fillStacksButton?.containsPoint(x, y) == true
            && StorageHelper.TryGetOne(context, out var storage)
            && storage.StashToChest != FeatureOptionRange.Disabled)
        {
            StashToChest.StashIntoStorage(storage);
        }
    }

    private static void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == "BetterChests.StashToChest")
        {
            StashToChest.StashIntoAll();
        }
    }

    private static void StashIntoAll()
    {
        var storages = StashToChest.Eligible.OrderByDescending(storage => storage.StashToChestPriority).ToList();
        var stashedAny = false;
        foreach (var unused in storages.Where(StashToChest.StashIntoStorage))
        {
            stashedAny = true;
        }

        if (stashedAny)
        {
            Game1.playSound("Ship");
            return;
        }

        Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
    }

    private static bool StashIntoStorage(BaseStorage storage)
    {
        var stashedAny = false;

        for (var index = 0; index < Game1.player.MaxItems; index++)
        {
            if (Game1.player.Items[index] is null || Game1.player.Items[index].modData.ContainsKey("furyx639.BetterChests/LockedSlot"))
            {
                continue;
            }

            var stack = Game1.player.Items[index].Stack;

            // First pass to categorized storages
            Game1.player.Items[index] = storage.StashItem(Game1.player.Items[index]);
            if (!stashedAny && (Game1.player.Items[index] is null || stack != Game1.player.Items[index].Stack))
            {
                stashedAny = true;
            }

            // Second pass to existing stacks
            if (Game1.player.Items[index] is not null)
            {
                stack = Game1.player.Items[index].Stack;
                Game1.player.Items[index] = storage.StashItem(Game1.player.Items[index], true);
                if (!stashedAny && (Game1.player.Items[index] is null || stack != Game1.player.Items[index].Stack))
                {
                    stashedAny = true;
                }
            }
        }

        return stashedAny;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.Config.ControlScheme.StashItems.JustPressed())
        {
            return;
        }

        // Stash to Current
        if (Game1.activeClickableMenu is ItemGrabMenu { context: { } context }
            && StorageHelper.TryGetOne(context, out var storage)
            && storage.StashToChest != FeatureOptionRange.Disabled)
        {
            StashToChest.StashIntoStorage(storage);
            return;
        }

        // Stash to all
        if (Context.IsPlayerFree)
        {
            StashToChest.StashIntoAll();
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.StashItems);
        }
    }
}