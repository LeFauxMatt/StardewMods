﻿namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley;
using StardewValley.Locations;

/// <inheritdoc />
internal class StashToChest : Feature
{
    private readonly PerScreen<IManagedStorage> _currentStorage = new();
    private readonly PerScreen<IClickableComponent> _stashButton = new();
    private readonly Lazy<IHudComponents> _toolbarIcons;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StashToChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public StashToChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._toolbarIcons = services.Lazy<IHudComponents>();
    }

    /// <summary>
    ///     Gets a value indicating which storages are eligible for stashing into.
    /// </summary>
    public IEnumerable<IManagedStorage> EligibleStorages
    {
        get
        {
            IList<IManagedStorage> eligibleStorages =
                this.ManagedObjects.InventoryStorages
                    .Select(inventoryStorage => inventoryStorage.Value)
                    .Where(playerChest => playerChest.StashToChest >= FeatureOptionRange.Inventory && playerChest.OpenHeldChest == FeatureOption.Enabled)
                    .ToList();
            foreach (var ((location, (x, y)), locationStorage) in this.ManagedObjects.LocationStorages)
            {
                // Disabled in config or by location name
                if (locationStorage.StashToChest == FeatureOptionRange.Disabled || locationStorage.StashToChestDisableLocations.Contains(Game1.player.currentLocation.Name))
                {
                    continue;
                }

                // Disabled in mines
                if (locationStorage.StashToChestDisableLocations.Contains("UndergroundMine") && Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine"))
                {
                    continue;
                }

                switch (locationStorage.StashToChest)
                {
                    // Disabled if not current location for location chest
                    case FeatureOptionRange.Location when !location.Equals(Game1.currentLocation):
                        continue;
                    case FeatureOptionRange.World:
                    case FeatureOptionRange.Location when locationStorage.StashToChestDistance == -1:
                    case FeatureOptionRange.Location when Utility.withinRadiusOfPlayer((int)x * 64, (int)y * 64, locationStorage.StashToChestDistance, Game1.player):
                        eligibleStorages.Add(locationStorage);
                        continue;
                    case FeatureOptionRange.Default:
                    case FeatureOptionRange.Disabled:
                    case FeatureOptionRange.Inventory:
                    default:
                        continue;
                }
            }

            return eligibleStorages.OrderByDescending(eligibleStorage => eligibleStorage.StashToChestPriority);
        }
    }

    private IManagedStorage CurrentStorage
    {
        get => this._currentStorage.Value;
        set => this._currentStorage.Value = value;
    }

    private IHudComponents HudComponents
    {
        get => this._toolbarIcons.Value;
    }

    private IClickableComponent StashButton
    {
        get => this._stashButton.Value ??= new CustomClickableComponent(
            new(
                new(0, 0, 32, 32),
                this.Helper.Content.Load<Texture2D>($"{BetterChests.ModUniqueId}/Icons", ContentSource.GameContent),
                new(16, 0, 16, 16),
                2f)
            {
                name = "Stash to Chest",
                hoverText = I18n.Button_StashToChest_Name(),
            },
            ComponentArea.Right);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.HudComponents.AddToolbarIcon(this.StashButton);
        this.CustomEvents.ClickableMenuChanged += this.OnClickableMenuChanged;
        this.CustomEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.CustomEvents.HudComponentPressed += this.OnHudComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.HudComponents.RemoveToolbarIcon(this.StashButton);
        this.CustomEvents.ClickableMenuChanged -= this.OnClickableMenuChanged;
        this.CustomEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
        this.CustomEvents.HudComponentPressed -= this.OnHudComponentPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!this.Config.ControlScheme.StashItems.JustPressed())
        {
            return;
        }

        // Stash to current
        if (this.CurrentStorage is not null)
        {
            var stashedAny = false;
            for (var index = 0; index < Game1.player.MaxItems; index++)
            {
                var item = Game1.player.Items[index];
                if (item?.modData.ContainsKey($"{BetterChests.ModUniqueId}/LockedSlot") != false)
                {
                    continue;
                }

                item = this.CurrentStorage.StashItem(item);
                if (item is null)
                {
                    stashedAny = true;
                    Game1.player.Items[index] = null;
                }
            }

            if (stashedAny)
            {
                Game1.playSound("Ship");
                this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.StashItems);
            }

            return;
        }

        // Stash to all
        if (Context.IsPlayerFree && this.StashItems())
        {
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.StashItems);
        }
    }

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        this.CurrentStorage = e.Context is not null && this.ManagedObjects.TryGetManagedStorage(e.Context, out var managedStorage) && managedStorage.StashToChest != FeatureOptionRange.Disabled
            ? managedStorage
            : null;
    }

    private void OnHudComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (ReferenceEquals(this.StashButton, e.Component))
        {
            this.StashItems();
            e.SuppressInput();
        }
    }

    private void OnMenuComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (this.CurrentStorage is null || e.Component.ComponentType is not ComponentType.FillStacksButton || e.Button != SButton.MouseLeft && !e.Button.IsActionButton())
        {
            return;
        }

        var stashedAny = false;
        for (var index = 0; index < Game1.player.MaxItems; index++)
        {
            var item = Game1.player.Items[index];
            if (item?.modData.ContainsKey($"{BetterChests.ModUniqueId}/LockedSlot") != false)
            {
                continue;
            }

            item = this.CurrentStorage.StashItem(item);
            if (item is null)
            {
                stashedAny = true;
                Game1.player.Items[index] = null;
            }
        }

        if (stashedAny)
        {
            Game1.playSound("Ship");
            e.SuppressInput();
        }
    }

    private bool StashItems()
    {
        Log.Trace("Stashing items into storages");
        var stashedAny = false;
        foreach (var eligibleStorage in this.EligibleStorages)
        {
            for (var index = 0; index < Game1.player.MaxItems; index++)
            {
                var item = Game1.player.Items[index];
                if (item?.modData.ContainsKey($"{BetterChests.ModUniqueId}/LockedSlot") != false)
                {
                    continue;
                }

                item = eligibleStorage.StashItem(item);
                if (item is null)
                {
                    stashedAny = true;
                    Game1.player.Items[index] = null;
                }
            }
        }

        if (stashedAny)
        {
            Game1.playSound("Ship");
            return true;
        }

        Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
        return false;
    }
}