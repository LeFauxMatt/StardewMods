namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Draw chest label to the screen.</summary>
internal sealed class LabelChest : BaseFeature
{
    private readonly ContainerFactory containers;
    private readonly IModEvents events;

    /// <summary>Initializes a new instance of the <see cref="LabelChest" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="containers">Dependency used for accessing containers.</param>
    public LabelChest(ILogging logging, ModConfig modConfig, IModEvents events, ContainerFactory containers)
        : base(logging, modConfig)
    {
        this.events = events;
        this.containers = containers;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.Default.LabelChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.events.Display.RenderedHud += this.OnRenderedHud;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.events.Display.RenderedHud -= this.OnRenderedHud;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            }
            || !this.containers.TryGetOne(chest, out var storage)
            || storage.Options.LabelChest != FeatureOption.Enabled
            || string.IsNullOrWhiteSpace(storage.Options.ChestLabel))
        {
            return;
        }

        var bounds = Game1.smallFont.MeasureString(storage.Options.ChestLabel).ToPoint();
        var overrideX = Game1.activeClickableMenu.xPositionOnScreen - bounds.X - IClickableMenu.borderWidth;
        var overrideY = Game1.activeClickableMenu.yPositionOnScreen - IClickableMenu.borderWidth - Game1.tileSize;

        IClickableMenu.drawHoverText(e.SpriteBatch, storage.Options.ChestLabel, Game1.smallFont, overrideX: overrideX, overrideY: overrideY);
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile();
        if ((!Game1.currentLocation.Objects.TryGetValue(pos, out var obj) && !Game1.currentLocation.Objects.TryGetValue(pos - new Vector2(0, -1), out obj))
            || !this.containers.TryGetOne(obj, out var storage)
            || storage.Options.LabelChest != FeatureOption.Enabled
            || string.IsNullOrWhiteSpace(storage.Options.ChestLabel))
        {
            return;
        }

        IClickableMenu.drawHoverText(e.SpriteBatch, storage.Options.ChestLabel, Game1.smallFont);
    }
}
