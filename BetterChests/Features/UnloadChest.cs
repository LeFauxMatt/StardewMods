namespace StardewMods.BetterChests.Features;

using System.Globalization;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewValley.Locations;

/// <summary>
///     Unload a held chest's contents into another chest.
/// </summary>
internal sealed class UnloadChest : IFeature
{
    private static UnloadChest? Instance;

    private readonly IModHelper _helper;

    private bool _isActivated;

    private UnloadChest(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Initializes <see cref="UnloadChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="UnloadChest" /> class.</returns>
    public static UnloadChest Init(IModHelper helper)
    {
        return UnloadChest.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    [EventPriority(EventPriority.Normal + 10)]
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
         || !e.Button.IsUseToolButton()
         || this._helper.Input.IsSuppressed(e.Button)
         || Storages.CurrentItem is null or { UnloadChest: not FeatureOption.Enabled }
         || (!Storages.CurrentItem.Items.Any() && Storages.CurrentItem.UnloadChestCombine is not FeatureOption.Enabled)
         || (Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine")))
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
         || !Storages.TryGetOne(Game1.currentLocation, pos, out var toStorage))
        {
            return;
        }

        // Add source capacity to target
        var combined = false;
        if (toStorage.UnloadChestCombine is FeatureOption.Enabled
         && Storages.CurrentItem.UnloadChestCombine is FeatureOption.Enabled)
        {
            var currentCapacity = toStorage.ActualCapacity;
            var addedCapacity = Storages.CurrentItem.ActualCapacity;
            if (currentCapacity < int.MaxValue - addedCapacity)
            {
                combined = true;
                toStorage.ResizeChestCapacity = currentCapacity + addedCapacity;
            }
        }

        // Stash items into target chest
        for (var index = Storages.CurrentItem.Items.Count - 1; index >= 0; index--)
        {
            var item = Storages.CurrentItem.Items[index];
            if (item is null)
            {
                continue;
            }

            var stack = item.Stack;
            var tmp = toStorage.AddItem(item);
            if (tmp is not null)
            {
                continue;
            }

            Log.Trace(
                $"UnloadChest: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {Storages.CurrentItem}, To: {toStorage}");
            Storages.CurrentItem.Items[index] = null;
        }

        if (combined && !Storages.CurrentItem.Items.OfType<Item>().Any())
        {
            Game1.player.Items[Game1.player.CurrentToolIndex] = null;
            Game1.playSound("Ship");
        }
        else
        {
            Storages.CurrentItem.ClearNulls();
        }

        CarryChest.CheckForOverburdened();
        this._helper.Input.Suppress(e.Button);
    }
}