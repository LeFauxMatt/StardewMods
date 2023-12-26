namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Draw chest label to the screen.</summary>
internal sealed class LabelChest : BaseFeature<LabelChest>
{
    private readonly ContainerFactory containerFactory;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="LabelChest" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public LabelChest(ILog log, IModConfig modConfig, ContainerFactory containerFactory, IModEvents modEvents)
        : base(log, modConfig)
    {
        this.containerFactory = containerFactory;
        this.modEvents = modEvents;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.LabelChest;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.Display.RenderedHud += this.OnRenderedHud;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.modEvents.Display.RenderedHud -= this.OnRenderedHud;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            }
            || !this.containerFactory.TryGetOne(chest, out var container)
            || string.IsNullOrWhiteSpace(container.Options.ChestLabel))
        {
            return;
        }

        var bounds = Game1.smallFont.MeasureString(container.Options.ChestLabel).ToPoint();
        var overrideX = Game1.activeClickableMenu.xPositionOnScreen - bounds.X - IClickableMenu.borderWidth;
        var overrideY = Game1.activeClickableMenu.yPositionOnScreen - IClickableMenu.borderWidth - Game1.tileSize;

        IClickableMenu.drawHoverText(
            e.SpriteBatch,
            container.Options.ChestLabel,
            Game1.smallFont,
            overrideX: overrideX,
            overrideY: overrideY);
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile();
        if ((!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
                && !Game1.currentLocation.Objects.TryGetValue(pos - new Vector2(0, -1), out obj))
            || !this.containerFactory.TryGetOne(obj, out var storage)
            || string.IsNullOrWhiteSpace(storage.Options.ChestLabel))
        {
            return;
        }

        IClickableMenu.drawHoverText(e.SpriteBatch, storage.Options.ChestLabel, Game1.smallFont);
    }
}