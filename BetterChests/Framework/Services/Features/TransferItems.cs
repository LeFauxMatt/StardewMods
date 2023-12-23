namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Transfer all items into or out from a chest.</summary>
internal sealed class TransferItems : BaseFeature
{
    private const string IconPath = "furyx639.BetterChests/Icons";
    private readonly ContainerFactory containers;
    private readonly PerScreen<ClickableTextureComponent> downArrow;

    private readonly IModEvents events;
    private readonly IInputHelper input;
    private readonly PerScreen<ClickableTextureComponent> upArrow;

    /// <summary>Initializes a new instance of the <see cref="TransferItems" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    /// <param name="containers">Dependency used for accessing containers.</param>
    public TransferItems(
        ILog log,
        ModConfig modConfig,
        IModEvents events,
        IGameContentHelper gameContent,
        IInputHelper input,
        ContainerFactory containers)
        : base(log, modConfig)
    {
        this.events = events;
        this.input = input;
        this.containers = containers;

        this.downArrow = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
                gameContent.Load<Texture2D>(TransferItems.IconPath),
                new Rectangle(84, 0, 7, 16),
                Game1.pixelZoom)
            {
                hoverText = I18n.Button_TransferDown_Name(),
                myID = 5318010,
            });

        this.upArrow = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
                gameContent.Load<Texture2D>(TransferItems.IconPath),
                new Rectangle(100, 0, 7, 16),
                Game1.pixelZoom)
            {
                hoverText = I18n.Button_TransferUp_Name(),
                myID = 5318011,
            });
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.TransferItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Display.MenuChanged += this.OnMenuChanged;
        this.events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Display.MenuChanged += this.OnMenuChanged;
        this.events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private void TransferDown()
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            }
            || !this.containers.TryGetOne(chest, out var container)
            || container.Options.TransferItems != FeatureOption.Enabled
            || !this.containers.TryGetOne(Game1.player, out var farmerContainer)
            || farmerContainer.Options.TransferItems != FeatureOption.Enabled)
        {
            return;
        }

        foreach (var item in container.Items)
        {
            if (item is null)
            {
                continue;
            }

            var stack = item.Stack;
            if (container.Transfer(item, farmerContainer, out var remaining))
            {
                var amount = stack - (remaining?.Stack ?? 0);
                this.Log.Trace(
                    "TransferItems: {{ Item: {0}, Quantity: {1}, From: {2}, To: {3} }}",
                    item.Name,
                    amount.ToString(CultureInfo.InvariantCulture),
                    container,
                    farmerContainer);
            }
        }
    }

    private void TransferUp()
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            }
            || !this.containers.TryGetOne(chest, out var container)
            || container.Options.TransferItems != FeatureOption.Enabled
            || !this.containers.TryGetOne(Game1.player, out var farmerContainer)
            || farmerContainer.Options.TransferItems != FeatureOption.Enabled)
        {
            return;
        }

        foreach (var item in container.Items)
        {
            if (item is null)
            {
                continue;
            }

            var stack = item.Stack;
            if (farmerContainer.Transfer(item, container, out var remaining))
            {
                var amount = stack - (remaining?.Stack ?? 0);
                this.Log.Trace(
                    "TransferItems: {{ Item: {0}, Quantity: {1}, From: {2}, To: {3} }}",
                    item.Name,
                    amount.ToString(CultureInfo.InvariantCulture),
                    farmerContainer,
                    container);
            }
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu || e.Button is not SButton.MouseLeft)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.downArrow.Value.visible && this.downArrow.Value.containsPoint(x, y))
        {
            this.TransferDown();
            this.input.Suppress(e.Button);
            return;
        }

        if (this.upArrow.Value.visible && this.upArrow.Value.containsPoint(x, y))
        {
            this.TransferUp();
            this.input.Suppress(e.Button);
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        this.downArrow.Value.visible = false;
        this.upArrow.Value.visible = false;

        if (e.NewMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        if (itemGrabMenu.context is Chest chest
            && this.containers.TryGetOne(chest, out var container)
            && container.Options.TransferItems == FeatureOption.Enabled)
        {
            this.upArrow.Value.visible = true;
            this.upArrow.Value.bounds.Y = itemGrabMenu.ItemsToGrabMenu.yPositionOnScreen - Game1.tileSize;
            this.upArrow.Value.bounds.X = itemGrabMenu.ItemsToGrabMenu.xPositionOnScreen
                + itemGrabMenu.ItemsToGrabMenu.width
                - 24;
        }

        if (this.containers.TryGetOne(Game1.player, out container)
            && container.Options.TransferItems == FeatureOption.Enabled)
        {
            this.downArrow.Value.visible = true;
            this.downArrow.Value.bounds.Y = itemGrabMenu.ItemsToGrabMenu.yPositionOnScreen - Game1.tileSize;
            this.downArrow.Value.bounds.X = itemGrabMenu.ItemsToGrabMenu.xPositionOnScreen
                + itemGrabMenu.ItemsToGrabMenu.width
                - 60;
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.downArrow.Value.visible)
        {
            this.downArrow.Value.draw(e.SpriteBatch);
            if (this.downArrow.Value.containsPoint(x, y))
            {
                itemGrabMenu.hoverText = this.downArrow.Value.hoverText;
                return;
            }
        }

        if (this.upArrow.Value.visible)
        {
            this.upArrow.Value.draw(e.SpriteBatch);
            if (this.upArrow.Value.containsPoint(x, y))
            {
                itemGrabMenu.hoverText = this.upArrow.Value.hoverText;
            }
        }
    }
}