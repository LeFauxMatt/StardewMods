namespace StardewMods.BetterChests.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Draw chest label to the screen.
/// </summary>
internal class LabelChest : IFeature
{
    private LabelChest(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static LabelChest? Instance { get; set; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

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
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            this.Helper.Events.Display.RenderedActiveMenu += LabelChest.OnRenderedActiveMenu;
            this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            this.Helper.Events.Display.RenderedActiveMenu -= LabelChest.OnRenderedActiveMenu;
            this.Helper.Events.Display.RenderedHud -= this.OnRenderedHud;
        }
    }

    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: { } context }
            || !StorageHelper.TryGetOne(context, out var storage)
            || string.IsNullOrWhiteSpace(storage.ChestLabel))
        {
            return;
        }

        var bounds = Game1.smallFont.MeasureString(storage.ChestLabel).ToPoint();

        // draw texture
        IClickableMenu.drawTextureBox(
            e.SpriteBatch,
            Game1.menuTexture,
            new(0, 256, 60, 60),
            5,
            5,
            bounds.X + 32,
            bounds.Y + Game1.tileSize / 3 + Game1.tileSize / 16,
            Color.White,
            drawShadow: true);

        Utility.drawTextWithShadow(
            e.SpriteBatch,
            storage.ChestLabel,
            Game1.smallFont,
            new(21, 21),
            Game1.textColor);
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree || !(this.Helper.Input.IsDown(SButton.LeftShift) || this.Helper.Input.IsDown(SButton.RightShift)))
        {
            return;
        }

        var pos = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;

        // Object exists at pos, is within reach of player, and is a Chest
        pos.X = (int)pos.X;
        pos.Y = (int)pos.Y;
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj) || !StorageHelper.TryGetOne(obj, out var storage) || string.IsNullOrWhiteSpace(storage.ChestLabel))
        {
            return;
        }

        IClickableMenu.drawHoverText(e.SpriteBatch, storage.ChestLabel, Game1.smallFont);
    }
}