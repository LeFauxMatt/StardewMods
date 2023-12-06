namespace StardewMods.BetterChests.Framework.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewValley.Menus;

/// <summary>Draw chest label to the screen.</summary>
internal sealed class LabelChest : BaseFeature
{
    private readonly IModEvents events;

    /// <summary>Initializes a new instance of the <see cref="LabelChest" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    public LabelChest(IMonitor monitor, ModConfig config, IModEvents events)
        : base(monitor, nameof(LabelChest), () => config.LabelChest is not FeatureOption.Disabled) =>
        this.events = events;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Display.RenderedActiveMenu += LabelChest.OnRenderedActiveMenu;
        this.events.Display.RenderedHud += LabelChest.OnRenderedHud;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Display.RenderedActiveMenu -= LabelChest.OnRenderedActiveMenu;
        this.events.Display.RenderedHud -= LabelChest.OnRenderedHud;
    }

    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu
            || string.IsNullOrWhiteSpace(BetterItemGrabMenu.Context?.ChestLabel))
        {
            return;
        }

        var bounds = Game1.smallFont.MeasureString(BetterItemGrabMenu.Context.ChestLabel).ToPoint();
        var overrideY = itemGrabMenu.yPositionOnScreen
            - IClickableMenu.borderWidth
            - BetterItemGrabMenu.TopPadding
            - Game1.tileSize;

        IClickableMenu.drawHoverText(
            e.SpriteBatch,
            BetterItemGrabMenu.Context.ChestLabel,
            Game1.smallFont,
            overrideX: itemGrabMenu.xPositionOnScreen - bounds.X - IClickableMenu.borderWidth,
            overrideY: overrideY);
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
            || !StorageHandler.TryGetOne(obj, out var storage)
            || string.IsNullOrWhiteSpace(storage.ChestLabel))
        {
            return;
        }

        IClickableMenu.drawHoverText(e.SpriteBatch, storage.ChestLabel, Game1.smallFont);
    }
}
