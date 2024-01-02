namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Draw chest label to the screen.</summary>
internal sealed class LabelChest : BaseFeature<LabelChest>
{
    private readonly ContainerFactory containerFactory;
    private readonly IModEvents modEvents;

    private readonly PerScreen<IStorageContainer?> containerFacing = new();

    /// <summary>Initializes a new instance of the <see cref="LabelChest" /> class.</summary>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public LabelChest(
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ILog log,
        IManifest manifest,
        IModEvents modEvents)
        : base(log, manifest, configManager)
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
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.modEvents.Display.RenderedHud -= this.OnRenderedHud;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        this.containerFacing.Value = null;

        if (!Context.IsPlayerFree)
        {
            return;
        }

        if (!this.containerFactory.TryGetOneFromLocation(Game1.currentLocation, e.Cursor.GrabTile, out var container))
        {
            if (!this.containerFactory.TryGetOneFromLocation(Game1.currentLocation, e.Cursor.Tile, out container))
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

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
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

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree || this.containerFacing.Value is null)
        {
            return;
        }

        IClickableMenu.drawHoverText(e.SpriteBatch, this.containerFacing.Value.Options.ChestLabel, Game1.smallFont);
    }
}