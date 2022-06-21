namespace StardewMods.BetterChests.Features;

using Common.Enums;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
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
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    [EventPriority(EventPriority.High)]
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsUseToolButton() || this.Helper.Input.IsSuppressed(e.Button) || Game1.player.CurrentItem is Chest { SpecialChestType: Chest.SpecialChestTypes.JunimoChest } or not Chest or null || (Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine")))
        {
            return;
        }

        var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64 : e.Cursor.Tile;
        var x = (int)pos.X;
        var y = (int)pos.Y;
        pos.X = x;
        pos.Y = y;

        // Object exists at pos and is within reach of player
        if (!Utility.withinRadiusOfPlayer(x * Game1.tileSize, y * Game1.tileSize, 1, Game1.player) || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
        {
            return;
        }

        if (!StorageHelper.TryGetOne(obj, out var target))
        {
            return;
        }

        // Disabled for object
        if (!StorageHelper.TryGetOne(Game1.player.CurrentItem, out var storage) || storage.UnloadChest == FeatureOption.Disabled)
        {
            return;
        }

        // Stash items into target chest
        for (var index = storage.Items.Count - 1; index >= 0; index--)
        {
            if (storage.Items[index] is null)
            {
                continue;
            }

            var tmp = target.AddItem(storage.Items[index]!);
            if (tmp is null)
            {
                storage.Items[index] = null;
            }
        }

        storage.ClearNulls();
        CarryChest.CheckForOverburdened();
        this.Helper.Input.Suppress(e.Button);
    }
}