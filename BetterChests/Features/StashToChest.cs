namespace BetterChests.Features;

using System.Linq;
using BetterChests.Enums;
using Common.Helpers;
using FuryCore.Services;
using Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

/// <inheritdoc />
internal class StashToChest : Feature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StashToChest"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public StashToChest(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    public override void Deactivate()
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
            from item in this.ManagedChests.AccessibleChests
            where item.Value.Config.StashingRange switch
            {
                FeatureOptionRange.Inventory => ReferenceEquals(item.Key.Player, Game1.player),
                FeatureOptionRange.Location => ReferenceEquals(item.Key.Location, Game1.currentLocation),
                FeatureOptionRange.World => true,
                _ => false,
            }
            select item.Value)
        .ToList();

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
                item = eligibleChest.StashItem(item);
                if (item is null)
                {
                    break;
                }
            }
        }

        Game1.playSound("Ship");
        this.Helper.Input.SuppressActiveKeybinds(this.Config.StashItems);
    }
}