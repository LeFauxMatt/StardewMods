namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Storages;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Stash items into placed chests and chests in the farmer's inventory.
/// </summary>
internal class StashToChest : IFeature
{
    private StashToChest(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static IEnumerable<EligibleStorage> EligibleStorages
    {
        get
        {
            foreach (var (storage, location, position) in StorageHelper.World)
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

                var (pX, pY) = Game1.player.getTileLocation();
                switch (storage.StashToChest)
                {
                    // Disabled if not current location for location chest
                    case FeatureOptionRange.Location when !location.Equals(Game1.currentLocation):
                        continue;
                    case FeatureOptionRange.World:
                    case FeatureOptionRange.Location when storage.StashToChestDistance == -1:
                    case FeatureOptionRange.Location when Math.Abs(position.X - pX) + Math.Abs(position.Y - pY) <= storage.StashToChestDistance:
                        if (storage.FilterItems != FeatureOption.Disabled && storage.FilterItemsList is not null)
                        {
                            var itemMatcher = new ItemMatcher(true);
                            foreach (var filter in storage.FilterItemsList)
                            {
                                itemMatcher.Add(filter);
                            }

                            if (itemMatcher.Any() && !itemMatcher.All(filter => filter.StartsWith("!")))
                            {
                                yield return new(storage, itemMatcher);
                                continue;
                            }
                        }

                        yield return new(storage, null);
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
                // Disabled in config
                if (storage.StashToChest == FeatureOptionRange.Disabled || storage.OpenHeldChest == FeatureOption.Disabled)
                {
                    continue;
                }

                if (storage.FilterItems != FeatureOption.Disabled && storage.FilterItemsList is not null)
                {
                    var itemMatcher = new ItemMatcher(true);
                    foreach (var filter in storage.FilterItemsList)
                    {
                        itemMatcher.Add(filter);
                    }

                    if (itemMatcher.Any() && !itemMatcher.All(filter => filter.StartsWith("!")))
                    {
                        yield return new(storage, itemMatcher);
                        continue;
                    }
                }

                yield return new(storage, null);
            }
        }
    }

    private static StashToChest? Instance { get; set; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Initializes <see cref="StashToChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="StashToChest" /> class.</returns>
    public static StashToChest Init(IModHelper helper)
    {
        return StashToChest.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (Integrations.ToolbarIcons.IsLoaded)
        {
            Integrations.ToolbarIcons.API.AddToolbarIcon(
                "BetterChests.StashToChest",
                "furyx638.BetterChests/Icons",
                new(16, 0, 16, 16),
                I18n.Button_StashToChest_Name());
            Integrations.ToolbarIcons.API.ToolbarIconPressed += StashToChest.OnToolbarIconPressed;
        }

        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.ButtonPressed += StashToChest.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (Integrations.ToolbarIcons.IsLoaded)
        {
            Integrations.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.StashToChest");
            Integrations.ToolbarIcons.API.ToolbarIconPressed -= StashToChest.OnToolbarIconPressed;
        }

        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.Helper.Events.Input.ButtonPressed -= StashToChest.OnButtonPressed;
    }

    private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.fillStacksButton?.containsPoint(x, y) != true)
        {
            return;
        }

        StashToChest.StashIntoCurrent();
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
        var storages = StashToChest.EligibleStorages.OrderByDescending(eligible => eligible.Storage.StashToChestPriority).ToList();
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

    private static void StashIntoCurrent()
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: Chest chest })
        {
            return;
        }

        // Disabled for object
        if (!StorageHelper.TryGetOne(chest, out var storage) || storage.StashToChest == FeatureOptionRange.Disabled)
        {
            return;
        }

        if (storage.FilterItems != FeatureOption.Disabled && storage.FilterItemsList is not null)
        {
            var itemMatcher = new ItemMatcher(true);
            foreach (var filter in storage.FilterItemsList)
            {
                itemMatcher.Add(filter);
            }

            if (itemMatcher.Any() && !itemMatcher.All(filter => filter.StartsWith("!")))
            {
                for (var index = 0; index < Game1.player.MaxItems; index++)
                {
                    var item = Game1.player.Items[index];
                    if (item?.modData.ContainsKey("furyx639.BetterChests/LockedSlot") != false)
                    {
                        continue;
                    }

                    // Add if categorized
                    if (itemMatcher.Matches(item))
                    {
                        Game1.player.Items[index] = chest.addItem(item);
                    }
                }
            }
        }
    }

    private static bool StashIntoStorage(EligibleStorage eligibleStorage)
    {
        var (storage, itemMatcher) = eligibleStorage;
        var stashedAny = false;
        for (var index = 0; index < Game1.player.MaxItems; index++)
        {
            if (Game1.player.Items[index]?.modData.ContainsKey("furyx639.BetterChests/LockedSlot") != false)
            {
                continue;
            }

            var stack = Game1.player.Items[index].Stack;
            Item? tmp = null;

            // Add if categorized
            if (itemMatcher?.Matches(Game1.player.Items[index]) == true)
            {
                tmp = storage.AddItem(Game1.player.Items[index]);
            }

            // Add if stackable
            if (tmp is not null
                && storage.StashToChestStacks == FeatureOption.Enabled
                && storage.Items.Any(chestItem => chestItem!.canStackWith(Game1.player.Items[index])))
            {
                tmp = storage.AddItem(Game1.player.Items[index]);
            }

            if (tmp is not null && stack == Game1.player.Items[index].Stack)
            {
                continue;
            }

            stashedAny = true;
            if (tmp is null)
            {
                Game1.player.Items[index] = null;
            }
        }

        return stashedAny;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Config.ControlScheme.StashItems.JustPressed())
        {
            return;
        }

        // Stash to Current
        if (Game1.activeClickableMenu is ItemGrabMenu)
        {
            StashToChest.StashIntoCurrent();
            return;
        }

        // Stash to all
        if (Context.IsPlayerFree)
        {
            StashToChest.StashIntoAll();
            this.Helper.Input.SuppressActiveKeybinds(Config.ControlScheme.StashItems);
        }
    }

    private record EligibleStorage(BaseStorage Storage, ItemMatcher? Filter);
}