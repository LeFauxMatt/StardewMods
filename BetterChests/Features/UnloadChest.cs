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
internal class UnloadChest : IFeature
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
         || StorageHelper.CurrentItem is null or { UnloadChest: not FeatureOption.Enabled }
         || (Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine")))
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1);
        if (!StorageHelper.TryGetOne(Game1.currentLocation, pos, out var toStorage))
        {
            return;
        }

        // Add source capacity to target
        var combined = false;
        if (toStorage.UnloadChestCombine is FeatureOption.Enabled
         && StorageHelper.CurrentItem.UnloadChestCombine is FeatureOption.Enabled)
        {
            var currentCapacity = toStorage.ActualCapacity;
            var addedCapacity = StorageHelper.CurrentItem.ActualCapacity;
            if (currentCapacity < int.MaxValue - addedCapacity)
            {
                combined = true;
                toStorage.ResizeChestCapacity = currentCapacity + addedCapacity;
            }
        }

        // Stash items into target chest
        for (var index = StorageHelper.CurrentItem.Items.Count - 1; index >= 0; index--)
        {
            var item = StorageHelper.CurrentItem.Items[index];
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
                $"UnloadChest: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {StorageHelper.CurrentItem}, To: {toStorage}");
            StorageHelper.CurrentItem.Items[index] = null;
        }

        if (combined && !StorageHelper.CurrentItem.Items.OfType<Item>().Any())
        {
            Game1.player.Items[Game1.player.CurrentToolIndex] = null;
            Game1.playSound("Ship");
        }
        else
        {
            StorageHelper.CurrentItem.ClearNulls();
        }

        CarryChest.CheckForOverburdened();
        this._helper.Input.Suppress(e.Button);
    }
}