namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Draw chest label to the screen.</summary>
internal sealed class LabelChest : BaseFeature<LabelChest>
{
    private readonly ContainerFactory containerFactory;

    private readonly PerScreen<IStorageContainer?> containerFacing = new();

    /// <summary>Initializes a new instance of the <see cref="LabelChest" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public LabelChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig) =>
        this.containerFactory = containerFactory;

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.LabelChest;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.Events.Subscribe<CursorMovedEventArgs>(this.OnCursorMoved);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.Events.Unsubscribe<CursorMovedEventArgs>(this.OnCursorMoved);
    }

    private void OnCursorMoved(CursorMovedEventArgs e)
    {
        this.containerFacing.Value = null;

        if (!Context.IsPlayerFree)
        {
            return;
        }

        if (!this.containerFactory.TryGetOneFromLocation(
            Game1.currentLocation,
            e.NewPosition.GrabTile,
            out var container))
        {
            if (!this.containerFactory.TryGetOneFromLocation(Game1.currentLocation, e.NewPosition.Tile, out container))
            {
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(container.Options.ChestLabel))
        {
            return;
        }

        this.containerFacing.Value = container;
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (!this.containerFactory.TryGetOneFromMenu(out var container)
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

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree || this.containerFacing.Value is null)
        {
            return;
        }

        IClickableMenu.drawHoverText(e.SpriteBatch, this.containerFacing.Value.Options.ChestLabel, Game1.smallFont);
    }
}