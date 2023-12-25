namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Transfer all items into or out from a chest.</summary>
internal sealed class TransferItems : BaseFeature
{
    private const string IconPath = "furyx639.BetterChests/Icons";

    private readonly PerScreen<ClickableTextureComponent> downArrow;
    private readonly IInputHelper inputHelper;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly IModEvents modEvents;
    private readonly PerScreen<ClickableTextureComponent> upArrow;

    /// <summary>Initializes a new instance of the <see cref="TransferItems" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public TransferItems(
        ILog log,
        ModConfig modConfig,
        IGameContentHelper gameContentHelper,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        IModEvents modEvents)
        : base(log, modConfig)
    {
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.modEvents = modEvents;

        this.downArrow = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
                gameContentHelper.Load<Texture2D>(TransferItems.IconPath),
                new Rectangle(84, 0, 7, 16),
                Game1.pixelZoom)
            {
                hoverText = I18n.Button_TransferDown_Name(),
                myID = 5318010,
            });

        this.upArrow = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
                gameContentHelper.Load<Texture2D>(TransferItems.IconPath),
                new Rectangle(100, 0, 7, 16),
                Game1.pixelZoom)
            {
                hoverText = I18n.Button_TransferUp_Name(),
                myID = 5318011,
            });
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.TransferItems != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    private void Transfer(IContainer containerFrom, IContainer containerTo)
    {
        if (!containerFrom.Transfer(containerTo, out var amounts))
        {
            return;
        }

        foreach (var (name, amount) in amounts)
        {
            if (amount > 0)
            {
                this.Log.Trace(
                    "{0}: {{ Item: {1}, Quantity: {2}, From: {3}, To: {4} }}",
                    this.Id,
                    name,
                    amount,
                    containerFrom,
                    containerTo);
            }
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft
            || this.itemGrabMenuManager.Top.Container is null
            || this.itemGrabMenuManager.Bottom.Container is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.upArrow.Value.visible && this.upArrow.Value.containsPoint(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            this.Transfer(this.itemGrabMenuManager.Bottom.Container, this.itemGrabMenuManager.Top.Container);
            return;
        }

        if (this.downArrow.Value.visible && this.downArrow.Value.containsPoint(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            this.Transfer(this.itemGrabMenuManager.Top.Container, this.itemGrabMenuManager.Bottom.Container);
        }
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Menu is null
            || this.itemGrabMenuManager.Bottom.Menu is null
            || this.itemGrabMenuManager.Top.Container is null
            || this.itemGrabMenuManager.Bottom.Container is null)
        {
            this.upArrow.Value.visible = false;
            this.downArrow.Value.visible = false;
            return;
        }

        var top = this.itemGrabMenuManager.Top;
        var bottom = this.itemGrabMenuManager.Bottom;
        this.upArrow.Value.visible = top.Container?.Options.TransferItems == Option.Enabled;
        this.downArrow.Value.visible = bottom.Container?.Options.TransferItems == Option.Enabled;

        if (this.upArrow.Value.visible)
        {
            var topLeft = bottom.Menu.inventory[0];
            this.upArrow.Value.bounds.X = topLeft.bounds.X - 24;
            this.upArrow.Value.bounds.Y = topLeft.bounds.Y - 32;
        }

        if (this.downArrow.Value.visible)
        {
            var bottomLeft = top.Menu.inventory[top.Capacity - top.Columns];
            this.downArrow.Value.bounds.X = bottomLeft.bounds.X - 24;
            this.downArrow.Value.bounds.Y = bottomLeft.bounds.Y + 32;
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.downArrow.Value.visible)
        {
            this.downArrow.Value.draw(e.SpriteBatch);
            if (this.downArrow.Value.containsPoint(mouseX, mouseY))
            {
                this.itemGrabMenuManager.CurrentMenu.hoverText = this.downArrow.Value.hoverText;
            }
        }

        if (this.upArrow.Value.visible)
        {
            this.upArrow.Value.draw(e.SpriteBatch);
            if (this.upArrow.Value.containsPoint(mouseX, mouseY))
            {
                this.itemGrabMenuManager.CurrentMenu.hoverText = this.upArrow.Value.hoverText;
            }
        }
    }
}