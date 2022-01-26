namespace BetterChests.Features;

using System.Linq;
using BetterChests.Enums;
using BetterChests.Interfaces;
using Common.Helpers;
using FuryCore.Services;
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

    /// <summary>Stash inventory items into all supported chests.</summary>
    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Config.StashItems.JustPressed())
        {
            return;
        }
        var eligibleChests = (
            from managedChest in this.ManagedChests.AccessibleChests
            where managedChest.CollectionType switch
            {
                ItemCollectionType.GameLocation when managedChest.StashToChest == FeatureOptionRange.World => true,
                ItemCollectionType.GameLocation when this.Config.StashToChestDistance == -1 =>
                    managedChest.StashToChest >= FeatureOptionRange.Location
                    && ReferenceEquals(managedChest.Location, Game1.currentLocation),
                ItemCollectionType.GameLocation when this.Config.StashToChestDistance >= 1 =>
                    managedChest.StashToChest >= FeatureOptionRange.Location
                    && ReferenceEquals(managedChest.Location, Game1.currentLocation)
                    && Utility.withinRadiusOfPlayer((int)managedChest.Position.X * 64, (int)managedChest.Position.Y * 64, this.Config.StashToChestDistance, Game1.player),
                ItemCollectionType.PlayerInventory =>
                    managedChest.StashToChest >= FeatureOptionRange.Inventory
                    && ReferenceEquals(managedChest.Player, Game1.player),
            }
            select managedChest).ToList();

        if (!eligibleChests.Any())
        {
            Log.Trace("No eligible chests found to stash items into");
            return;
        }

        for (var i = Game1.player.Items.Count - 1; i >= 0; i--)
        {
            var item = Game1.player.Items[i];
            if (item is null)
            {
                continue;
            }

            foreach (var eligibleChest in eligibleChests)
            {
                item = eligibleChest.StashItem(item, this.Config.FillStacks);
                if (item is null)
                {
                    Game1.player.Items[i] = null;
                    break;
                }
            }
        }

        Game1.playSound("Ship");
        this.Helper.Input.SuppressActiveKeybinds(this.Config.StashItems);
    }
}