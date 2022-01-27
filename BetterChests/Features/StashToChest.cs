namespace BetterChests.Features;

using System.Linq;
using BetterChests.Enums;
using BetterChests.Interfaces;
using Common.Extensions;
using Common.Helpers;
using FuryCore.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

/// <inheritdoc />
internal class StashToChest : Feature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StashToChest"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    public StashToChest(IConfigModel config, IModHelper helper, IServiceLocator services)
        : base(config, helper, services)
    {
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
            this.Config.ControlScheme.StashItems.Suppress();
        }
    }

    private bool StashItems()
    {
        var eligibleChests = (
            from managedChest in this.ManagedChests.AccessibleChests
            where managedChest.CollectionType switch
            {
                ItemCollectionType.GameLocation when managedChest.StashToChest == FeatureOptionRange.World => true,
                ItemCollectionType.GameLocation when managedChest.StashToChestDistance == -1 =>
                    managedChest.StashToChest >= FeatureOptionRange.Location
                    && ReferenceEquals(managedChest.Location, Game1.currentLocation),
                ItemCollectionType.GameLocation when managedChest.StashToChestDistance >= 1 =>
                    managedChest.StashToChest >= FeatureOptionRange.Location
                    && ReferenceEquals(managedChest.Location, Game1.currentLocation)
                    && Utility.withinRadiusOfPlayer((int)managedChest.Position.X * 64, (int)managedChest.Position.Y * 64, managedChest.StashToChestDistance, Game1.player),
                ItemCollectionType.PlayerInventory =>
                    managedChest.StashToChest >= FeatureOptionRange.Inventory
                    && ReferenceEquals(managedChest.Player, Game1.player),
                ItemCollectionType.ChestInventory => false,
                _ => false,
            }
            select managedChest).ToList();

        if (!eligibleChests.Any())
        {
            Log.Trace("No eligible chests found to stash items into");
            return false;
        }

        for (var index = Game1.player.Items.Count - 1; index >= 0; index--)
        {
            if (this.Config.SlotLock && this.Config.LockedSlots.ElementAtOrDefault(index))
            {
                continue;
            }

            var item = Game1.player.Items[index];
            if (item is null)
            {
                continue;
            }

            foreach (var eligibleChest in eligibleChests)
            {
                item = eligibleChest.StashItem(item);
                if (item is null)
                {
                    Game1.player.Items[index] = null;
                    break;
                }
            }
        }

        Game1.playSound("Ship");
        return true;
    }
}