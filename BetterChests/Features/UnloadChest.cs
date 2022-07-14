namespace StardewMods.BetterChests.Features;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>
///     Unload a held chest's contents into another chest.
/// </summary>
internal class UnloadChest : IFeature
{
    private UnloadChest(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static UnloadChest? Instance { get; set; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

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
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        }
    }

    [EventPriority(EventPriority.Normal + 10)]
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsUseToolButton() || this.Helper.Input.IsSuppressed(e.Button) || Game1.player.CurrentItem is Chest { SpecialChestType: Chest.SpecialChestTypes.JunimoChest } or not Chest or null || (Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine")))
        {
            return;
        }

        var pos = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;
        if (!Game1.wasMouseVisibleThisFrame || Game1.mouseCursorTransparency == 0f || !Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player))
        {
            pos = Game1.player.GetGrabTile();
        }

        pos.X = (int)pos.X;
        pos.Y = (int)pos.Y;
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj) || !StorageHelper.TryGetOne(obj, out var toStorage))
        {
            return;
        }

        // Disabled for held object
        if (!StorageHelper.TryGetOne(Game1.player.CurrentItem, out var fromStorage) || fromStorage.UnloadChest == FeatureOption.Disabled)
        {
            return;
        }

        // Stash items into target chest
        for (var index = fromStorage.Items.Count - 1; index >= 0; index--)
        {
            var item = fromStorage.Items[index];
            if (item is null)
            {
                continue;
            }

            var stack = item.Stack;
            var tmp = toStorage.AddItem(item);
            if (tmp is null)
            {
                Log.Trace($"UnloadChest: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");
                fromStorage.Items[index] = null;
            }
        }

        fromStorage.ClearNulls();
        CarryChest.CheckForOverburdened();
        this.Helper.Input.Suppress(e.Button);
    }
}