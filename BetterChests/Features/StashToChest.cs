namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewValley;
using StardewValley.Locations;

/// <inheritdoc />
internal class StashToChest : Feature
{
    private readonly PerScreen<IManagedStorage> _currentStorage = new();
    private readonly Lazy<IMenuComponents> _menuComponents;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StashToChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public StashToChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._menuComponents = services.Lazy<IMenuComponents>();
    }

    /// <summary>
    ///     Gets a value indicating which chests are eligible for stashing into.
    /// </summary>
    public IEnumerable<IManagedStorage> EligibleStorages
    {
        get
        {
            IList<IManagedStorage> eligibleStorages =
                this.ManagedStorages.PlayerStorages
                    .Where(playerChest => playerChest.StashToChest >= FeatureOptionRange.Inventory && playerChest.OpenHeldChest == FeatureOption.Enabled)
                    .ToList();
            foreach (var ((location, (x, y)), locationStorage) in this.ManagedStorages.LocationStorages)
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

            return eligibleStorages;
        }
    }

    private IManagedStorage CurrentStorage
    {
        get => this._currentStorage.Value;
        set => this._currentStorage.Value = value;
    }

    private IMenuComponents MenuComponents
    {
        get => this._menuComponents.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.CustomEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.CustomEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.CustomEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.CustomEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
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

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.MenuComponents.Menu is null || e.Context is null)
        {
            this.CurrentStorage = null;
            return;
        }

        if (this.ManagedStorages.FindStorage(e.Context, out var managedStorage))
        {
            this.CurrentStorage = managedStorage;
        }
    }

    private void OnMenuComponentPressed(object sender, MenuComponentPressedEventArgs e)
    {
        if (this.CurrentStorage is null || e.Component.ComponentType is not ComponentType.FillStacksButton)
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
        Log.Trace("Stashing items into chests");
        var stashedAny = false;
        foreach (var eligibleChest in this.EligibleStorages)
        {
            for (var index = 0; index < Game1.player.MaxItems; index++)
            {
                var item = Game1.player.Items[index];
                if (item?.modData.ContainsKey($"{BetterChests.ModUniqueId}/LockedSlot") != false)
                {
                    continue;
                }

                item = eligibleChest.StashItem(item);
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