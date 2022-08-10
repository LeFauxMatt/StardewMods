namespace StardewMods.BetterChests.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewValley.Menus;

/// <summary>
///     Draw chest label to the screen.
/// </summary>
internal class LabelChest : IFeature
{
    private static LabelChest? Instance;

    private readonly IModHelper _helper;

    private bool _isActivated;

    private LabelChest(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Initializes <see cref="LabelChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="LabelChest" /> class.</returns>
    public static LabelChest Init(IModHelper helper)
    {
        return LabelChest.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        this._helper.Events.Display.RenderedActiveMenu += LabelChest.OnRenderedActiveMenu;
        this._helper.Events.Display.RenderedHud += LabelChest.OnRenderedHud;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        this._helper.Events.Display.RenderedActiveMenu -= LabelChest.OnRenderedActiveMenu;
        this._helper.Events.Display.RenderedHud -= LabelChest.OnRenderedHud;
    }

    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: { } context } itemGrabMenu
         || !StorageHelper.TryGetOne(context, out var storage)
         || string.IsNullOrWhiteSpace(storage.ChestLabel))
        {
            return;
        }

        IClickableMenu.drawHoverText(
            e.SpriteBatch,
            storage.ChestLabel,
            Game1.smallFont,
            overrideX: itemGrabMenu.xPositionOnScreen,
            overrideY: itemGrabMenu.yPositionOnScreen
                     - IClickableMenu.spaceToClearSideBorder
                     - Game1.tileSize
                     - (storage.SearchItems is not FeatureOption.Disabled ? 14 * Game1.pixelZoom : 0));
    }

    private static void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        var pos = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y)
                / Game1.tileSize;

        pos.X = (int)pos.X;
        pos.Y = (int)pos.Y;
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
         || !StorageHelper.TryGetOne(obj, out var storage)
         || string.IsNullOrWhiteSpace(storage.ChestLabel))
        {
            return;
        }

        IClickableMenu.drawHoverText(e.SpriteBatch, storage.ChestLabel, Game1.smallFont);
    }
}