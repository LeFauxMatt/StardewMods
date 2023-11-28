namespace StardewMods.BetterChests.Framework.Features;

using System.Globalization;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewValley.Locations;

/// <summary>
///     Unload a held chest's contents into another chest.
/// </summary>
internal sealed class UnloadChest : Feature
{
#nullable disable
    private static Feature instance;
#nullable enable

    private readonly IModHelper helper;

    private UnloadChest(IModHelper helper)
    {
        this.helper = helper;
    }

    /// <summary>
    ///     Initializes <see cref="UnloadChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="UnloadChest" /> class.</returns>
    public static Feature Init(IModHelper helper)
    {
        return UnloadChest.instance ??= new UnloadChest(helper);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    [EventPriority(EventPriority.Normal + 10)]
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsUseToolButton()
            || this.helper.Input.IsSuppressed(e.Button)
            || Storages.CurrentItem is null or { UnloadChest: not FeatureOption.Enabled }
            || Storages.CurrentItem.Data is not Storage storageObject
            || (!storageObject.Items.Any() && Storages.CurrentItem.UnloadChestCombine is not FeatureOption.Enabled)
            || (Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine")))
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
            || !Storages.TryGetOne(Game1.currentLocation, pos, out var toStorage)
            || toStorage is not { Data: Storage toStorageObject })
        {
            return;
        }

        // Add source capacity to target
        var combined = false;
        if (toStorage.UnloadChestCombine is FeatureOption.Enabled
            && Storages.CurrentItem.UnloadChestCombine is FeatureOption.Enabled)
        {
            var currentCapacity = toStorageObject.ActualCapacity;
            var addedCapacity = storageObject.ActualCapacity;
            if (currentCapacity < int.MaxValue - addedCapacity)
            {
                combined = true;
                toStorage.ResizeChestCapacity = currentCapacity + addedCapacity;
            }
        }

        // Stash items into target chest
        for (var index = storageObject.Items.Count - 1; index >= 0; --index)
        {
            var item = storageObject.Items[index];
            if (item is null)
            {
                continue;
            }

            var stack = item.Stack;
            var tmp = toStorageObject.AddItem(item);
            if (tmp is not null)
            {
                continue;
            }

            Log.Trace(
                $"UnloadChest: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {Storages.CurrentItem}, To: {toStorage}");
            storageObject.Items[index] = null;
        }

        if (combined && !storageObject.Items.OfType<Item>().Any())
        {
            Game1.player.Items[Game1.player.CurrentToolIndex] = null;
            Game1.playSound("Ship");
        }
        else
        {
            storageObject.ClearNulls();
        }

        CarryChest.CheckForOverburdened();
        this.helper.Input.Suppress(e.Button);
    }
}