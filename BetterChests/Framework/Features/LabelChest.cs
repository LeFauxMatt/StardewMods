namespace StardewMods.BetterChests.Framework.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewValley.Menus;

/// <summary>
///     Draw chest label to the screen.
/// </summary>
internal sealed class LabelChest : Feature
{
#nullable disable
    private static Feature Instance;
#nullable enable

    private readonly IModHelper _helper;

    private LabelChest(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Initializes <see cref="LabelChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="LabelChest" /> class.</returns>
    public static Feature Init(IModHelper helper)
    {
        return LabelChest.Instance ??= new LabelChest(helper);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this._helper.Events.Display.RenderedActiveMenu += LabelChest.OnRenderedActiveMenu;
        this._helper.Events.Display.RenderedHud += LabelChest.OnRenderedHud;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this._helper.Events.Display.RenderedActiveMenu -= LabelChest.OnRenderedActiveMenu;
        this._helper.Events.Display.RenderedHud -= LabelChest.OnRenderedHud;
    }

    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu
         || string.IsNullOrWhiteSpace(BetterItemGrabMenu.Context?.ChestLabel))
        {
            return;
        }

        var bounds = Game1.smallFont.MeasureString(BetterItemGrabMenu.Context.ChestLabel).ToPoint();

        IClickableMenu.drawHoverText(
            e.SpriteBatch,
            BetterItemGrabMenu.Context.ChestLabel,
            Game1.smallFont,
            overrideX: itemGrabMenu.xPositionOnScreen - bounds.X - IClickableMenu.borderWidth,
            overrideY: itemGrabMenu.yPositionOnScreen
                     - IClickableMenu.borderWidth
                     - BetterItemGrabMenu.TopPadding
                     - Game1.tileSize);
    }

    private static void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile();
        if ((!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
          && !Game1.currentLocation.Objects.TryGetValue(pos - new Vector2(0, -1), out obj))
         || !Storages.TryGetOne(obj, out var storage)
         || string.IsNullOrWhiteSpace(storage.ChestLabel))
        {
            return;
        }

        IClickableMenu.drawHoverText(e.SpriteBatch, storage.ChestLabel, Game1.smallFont);
    }
}