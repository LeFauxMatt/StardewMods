namespace StardewMods.FuryCore.Events;

using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class RenderedClickableMenu : SortedEventHandler<RenderedActiveMenuEventArgs>
{
    private readonly Lazy<GameObjects> _gameObjects;
    private readonly PerScreen<IClickableMenu> _menu = new();
    private readonly PerScreen<int> _screenId = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderedClickableMenu" /> class.
    /// </summary>
    /// <param name="display">SMAPI events related to UI and drawing to the screen.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public RenderedClickableMenu(IDisplayEvents display, IModServices services)
    {
        this._gameObjects = services.Lazy<GameObjects>();
        services.Lazy<CustomEvents>(events => events.ClickableMenuChanged += this.OnClickableMenuChanged);
        display.RenderedActiveMenu += this.OnRenderedActiveMenu;
    }

    private GameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private IClickableMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private int ScreenId
    {
        get => this._screenId.Value;
        set => this._screenId.Value = value;
    }

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        switch (e.Menu)
        {
            case ItemGrabMenu { context: { } context } itemGrabMenu when this.GameObjects.TryGetGameObject(context, out var gameObject) && gameObject is IStorageContainer:
                this.Menu = e.Menu;
                this.ScreenId = e.ScreenId;
                itemGrabMenu.setBackgroundTransparency(false);
                break;
            case PurchaseAnimalsMenu:
                this.Menu = e.Menu;
                this.ScreenId = e.ScreenId;
                break;
            default:
                this.Menu = null;
                this.ScreenId = -1;
                break;
        }
    }

    [EventPriority(EventPriority.Low - 1000)]
    private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        if (this.HandlerCount == 0 || this.Menu is null || this.ScreenId != Context.ScreenId)
        {
            return;
        }

        // Draw render items below foreground
        this.InvokeAll(e);

        // Draw hover elements
        switch (this.Menu)
        {
            case ItemGrabMenu itemGrabMenu:
                if (itemGrabMenu.hoveredItem is not null)
                {
                    IClickableMenu.drawToolTip(e.SpriteBatch, itemGrabMenu.hoveredItem.getDescription(), itemGrabMenu.hoveredItem.DisplayName, itemGrabMenu.hoveredItem, itemGrabMenu.heldItem != null);
                }
                else if (!string.IsNullOrWhiteSpace(itemGrabMenu.hoverText))
                {
                    if (itemGrabMenu.hoverAmount > 0)
                    {
                        IClickableMenu.drawToolTip(e.SpriteBatch, itemGrabMenu.hoverText, string.Empty, null, true, -1, 0, -1, -1, null, itemGrabMenu.hoverAmount);
                    }
                    else
                    {
                        IClickableMenu.drawHoverText(e.SpriteBatch, itemGrabMenu.hoverText, Game1.smallFont);
                    }
                }

                itemGrabMenu.heldItem?.drawInMenu(e.SpriteBatch, new(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);

                break;
            case PurchaseAnimalsMenu purchaseAnimalsMenu:
                // Redraw foreground components
                if (purchaseAnimalsMenu.hovered?.item is SObject { Type: not null } obj)
                {
                    IClickableMenu.drawHoverText(e.SpriteBatch, Game1.parseText(obj.Type, Game1.dialogueFont, 320), Game1.dialogueFont);
                }
                else if (purchaseAnimalsMenu.hovered is not null)
                {
                    var displayName = PurchaseAnimalsMenu.getAnimalTitle(purchaseAnimalsMenu.hovered.hoverText);
                    var description = PurchaseAnimalsMenu.getAnimalDescription(purchaseAnimalsMenu.hovered.hoverText);
                    SpriteText.drawStringWithScrollBackground(e.SpriteBatch, displayName, purchaseAnimalsMenu.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 64, purchaseAnimalsMenu.yPositionOnScreen + purchaseAnimalsMenu.height + -32 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "Truffle Pig");
                    SpriteText.drawStringWithScrollBackground(e.SpriteBatch, "$" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", purchaseAnimalsMenu.hovered.item.salePrice()), purchaseAnimalsMenu.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128, purchaseAnimalsMenu.yPositionOnScreen + purchaseAnimalsMenu.height + 64 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "$99999999g", Game1.player.Money >= purchaseAnimalsMenu.hovered.item.salePrice() ? 1f : 0.5f);
                    IClickableMenu.drawHoverText(e.SpriteBatch, Game1.parseText(description, Game1.smallFont, 320), Game1.smallFont, 0, 0, -1, displayName);
                }

                break;
        }

        // Draw cursor
        Game1.mouseCursorTransparency = 1f;
        this.Menu.drawMouse(e.SpriteBatch);
    }
}