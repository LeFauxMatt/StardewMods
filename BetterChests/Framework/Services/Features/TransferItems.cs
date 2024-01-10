namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Transfer all items into or out from a chest.</summary>
internal sealed class TransferItems : BaseFeature<TransferItems>
{
    private readonly ContainerHandler containerHandler;
    private readonly PerScreen<ClickableTextureComponent> downArrow;
    private readonly IInputHelper inputHelper;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<ClickableTextureComponent> upArrow;

    /// <summary>Initializes a new instance of the <see cref="TransferItems" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public TransferItems(
        AssetHandler assetHandler,
        ContainerHandler containerHandler,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.containerHandler = containerHandler;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;

        this.downArrow = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
                gameContentHelper.Load<Texture2D>(assetHandler.IconTexturePath),
                new Rectangle(84, 0, 7, 16),
                Game1.pixelZoom)
            {
                hoverText = I18n.Button_TransferDown_Name(),
                myID = 5318010,
            });

        this.upArrow = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
                gameContentHelper.Load<Texture2D>(assetHandler.IconTexturePath),
                new Rectangle(100, 0, 7, 16),
                Game1.pixelZoom)
            {
                hoverText = I18n.Button_TransferUp_Name(),
                myID = 5318011,
            });
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.TransferItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
    }

    private void Transfer(IStorageContainer containerFrom, IStorageContainer containerTo)
    {
        if (!this.containerHandler.Transfer(containerFrom, containerTo, out var amounts, true))
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

    private void OnButtonPressed(ButtonPressedEventArgs e)
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

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
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
        this.upArrow.Value.visible = top.Container?.Options.TransferItems == FeatureOption.Enabled;
        this.downArrow.Value.visible = bottom.Container?.Options.TransferItems == FeatureOption.Enabled;

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

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
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