namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Interfaces;
using StardewValley;
using StardewValley.Locations;

/// <inheritdoc />
internal class StashToChest : Feature
{
    private readonly Lazy<SlotLock> _slotLock;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StashToChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public StashToChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._slotLock = services.Lazy<SlotLock>();
    }

    /// <summary>
    ///     Gets a value indicating which chests are eligible for stashing into.
    /// </summary>
    public IList<IManagedChest> EligibleChests
    {
        get
        {
            var eligibleChests = (
                from managedChest in this.ManagedChests.PlayerChests
                where managedChest.StashToChest >= FeatureOptionRange.Inventory
                      && managedChest.OpenHeldChest == FeatureOption.Enabled
                select managedChest).ToList();

            foreach (var (placedObject, managedChest) in this.ManagedChests.PlacedChests)
            {
                // Disabled in config or by location name
                if (managedChest.StashToChest == FeatureOptionRange.Disabled || managedChest.StashToChestDisableLocations.Contains(Game1.player.currentLocation.Name))
                {
                    continue;
                }

                // Disabled in mines
                if (managedChest.StashToChestDisableLocations.Contains("UndergroundMine") && Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine"))
                {
                    continue;
                }

                var (location, (x, y)) = placedObject;
                switch (managedChest.StashToChest)
                {
                    // Disabled if not current location for location chest
                    case FeatureOptionRange.Location when !location.Equals(Game1.currentLocation):
                        continue;
                    case FeatureOptionRange.World:
                    case FeatureOptionRange.Location when managedChest.StashToChestDistance == -1:
                    case FeatureOptionRange.Location when Utility.withinRadiusOfPlayer((int)x * 64, (int)y * 64, managedChest.StashToChestDistance, Game1.player):
                        eligibleChests.Add(managedChest);
                        continue;
                    case FeatureOptionRange.Default:
                    case FeatureOptionRange.Disabled:
                    case FeatureOptionRange.Inventory:
                    default:
                        continue;
                }
            }

            return eligibleChests.OrderByDescending(managedChest => managedChest.StashToChestPriority).ToList();
        }
    }

    private SlotLock SlotLock
    {
        get => this._slotLock.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (Context.IsPlayerFree && this.Config.ControlScheme.StashItems.JustPressed() && this.StashItems())
        {
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.StashItems);
        }
    }

    private bool StashItems()
    {
        var eligibleChests = this.EligibleChests;
        if (!eligibleChests.Any())
        {
            Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
            return false;
        }

        Log.Trace("Stashing items into chests");
        var lockedSlots = this.Config.SlotLock
            ? this.SlotLock.LockedSlots
            : Array.Empty<bool>();

        var stashedAny = false;
        foreach (var eligibleChest in eligibleChests)
        {
            for (var index = 0; index < Game1.player.MaxItems; index++)
            {
                if (lockedSlots.ElementAtOrDefault(index))
                {
                    continue;
                }

                var item = Game1.player.Items[index];
                if (item is null)
                {
                    continue;
                }

                item = eligibleChest.StashItem(item);
                if (item is null)
                {
                    stashedAny = true;
                    eligibleChest.Chest.shakeTimer = 100;
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