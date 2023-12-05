namespace StardewMods.BetterChests.Framework.Features;

using System.Globalization;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewValley.Locations;

/// <summary>Unload a held chest's contents into another chest.</summary>
internal sealed class UnloadChest : BaseFeature
{
    private readonly IModEvents events;
    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="UnloadChest" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public UnloadChest(IMonitor monitor, ModConfig config, IModEvents events, IInputHelper input)
        : base(monitor, nameof(UnloadChest), () => config.UnloadChest is not FeatureOption.Disabled)
    {
        this.events = events;
        this.input = input;
    }

    /// <inheritdoc />
    protected override void Activate() => this.events.Input.ButtonPressed += this.OnButtonPressed;

    /// <inheritdoc />
    protected override void Deactivate() => this.events.Input.ButtonPressed -= this.OnButtonPressed;

    [EventPriority(EventPriority.Normal + 10)]
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsUseToolButton()
            || this.input.IsSuppressed(e.Button)
            || StorageService.CurrentItem is null
                or
                {
                    UnloadChest: not FeatureOption.Enabled,
                }
            || StorageService.CurrentItem.Data is not Storage storageObject
            || (!storageObject.Inventory.HasAny()
                && StorageService.CurrentItem.UnloadChestCombine is not FeatureOption.Enabled)
            || (Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
            || !StorageService.TryGetOne(Game1.currentLocation, pos, out var toStorage)
            || toStorage is not
            {
                Data: Storage toStorageObject,
            })
        {
            return;
        }

        // Add source capacity to target
        var combined = false;
        if (toStorage.UnloadChestCombine is FeatureOption.Enabled
            && StorageService.CurrentItem.UnloadChestCombine is FeatureOption.Enabled)
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
        for (var index = storageObject.Inventory.Count - 1; index >= 0; --index)
        {
            var item = storageObject.Inventory[index];
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

            this.Monitor.Log(
                $"UnloadChest: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {StorageService.CurrentItem}, To: {toStorage}");

            storageObject.Inventory[index] = null;
        }

        if (combined && !storageObject.Inventory.HasAny())
        {
            Game1.player.Items[Game1.player.CurrentToolIndex] = null;
            Game1.playSound("Ship");
        }
        else
        {
            storageObject.ClearNulls();
        }

        CarryChest.CheckForOverburdened();
        this.input.Suppress(e.Button);
    }
}
