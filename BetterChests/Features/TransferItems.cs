namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Transfer all items into or out from a chest.
/// </summary>
internal class TransferItems : IFeature
{
    private static TransferItems? Instance;

    private readonly PerScreen<ClickableTextureComponent> _downArrow = new(
        () => new(
            new(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
            Game1.content.Load<Texture2D>("furyx639.BetterChests/Icons"),
            new(84, 0, 7, 16),
            Game1.pixelZoom)
        {
            hoverText = I18n.Button_TransferDown_Name(),
            myID = 5318010,
        });

    private readonly IModHelper _helper;

    private readonly PerScreen<ClickableTextureComponent> _upArrow = new(
        () => new(
            new(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
            Game1.content.Load<Texture2D>("furyx639.BetterChests/Icons"),
            new(100, 0, 7, 16),
            Game1.pixelZoom)
        {
            hoverText = I18n.Button_TransferUp_Name(),
            myID = 5318011,
        });

    private bool _isActivated;

    private TransferItems(IModHelper helper)
    {
        this._helper = helper;
    }

    private ClickableTextureComponent DownArrow => this._downArrow.Value;

    private ClickableTextureComponent UpArrow => this._upArrow.Value;

    /// <summary>
    ///     Initializes <see cref="TransferItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="SlotLock" /> class.</returns>
    public static TransferItems Init(IModHelper helper)
    {
        return TransferItems.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        BetterItemGrabMenu.ConstructMenu += TransferItems.OnConstructMenu;
        this._helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this._helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        BetterItemGrabMenu.ConstructMenu -= TransferItems.OnConstructMenu;
        this._helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this._helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static void OnConstructMenu(object? sender, ItemGrabMenu itemGrabMenu)
    {
        if (itemGrabMenu.context is null || !StorageHelper.TryGetOne(itemGrabMenu.context, out _))
        {
            return;
        }

        BetterItemGrabMenu.TopPadding = 24;
    }

    private static void TransferDown()
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: { } context }
         || !StorageHelper.TryGetOne(context, out var storage))
        {
            return;
        }

        var items = new Queue<Item>(storage.Items.OfType<Item>());
        while (items.Count > 0)
        {
            var item = items.Dequeue();
            if (item.modData.ContainsKey("furyx639.BetterChests/LockedSlot"))
            {
                continue;
            }

            if (Game1.player.addItemToInventoryBool(item))
            {
                storage.Items.Remove(item);
            }
        }

        storage.ClearNulls();
    }

    private static void TransferUp()
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: { } context }
         || !StorageHelper.TryGetOne(context, out var storage))
        {
            return;
        }

        var items = new Queue<Item>(Game1.player.Items.Where(item => item is not null));
        while (items.Count > 0)
        {
            var item = items.Dequeue();
            if (item.modData.ContainsKey("furyx639.BetterChests/LockedSlot"))
            {
                continue;
            }

            var tmp = storage.AddItem(item);
            if (tmp is null)
            {
                Game1.player.removeItemFromInventory(item);
            }
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
         || !this.DownArrow.visible
         || e.Button is not SButton.MouseLeft)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.DownArrow.containsPoint(x, y))
        {
            TransferItems.TransferDown();
            this._helper.Input.Suppress(e.Button);
            return;
        }

        if (this.UpArrow.containsPoint(x, y))
        {
            TransferItems.TransferUp();
            this._helper.Input.Suppress(e.Button);
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu { context: { } context, ItemsToGrabMenu: { } itemsToGrabMenu }
         || !StorageHelper.TryGetOne(context, out _))
        {
            this.DownArrow.visible = false;
            this.UpArrow.visible = false;
            return;
        }

        this.DownArrow.visible = true;
        this.DownArrow.bounds.X = itemsToGrabMenu.xPositionOnScreen + itemsToGrabMenu.width - 60;
        this.DownArrow.bounds.Y = itemsToGrabMenu.yPositionOnScreen - Game1.tileSize;
        this.UpArrow.visible = true;
        this.UpArrow.bounds.X = itemsToGrabMenu.xPositionOnScreen + itemsToGrabMenu.width - 24;
        this.UpArrow.bounds.Y = itemsToGrabMenu.yPositionOnScreen - Game1.tileSize;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu || !this.DownArrow.visible)
        {
            return;
        }

        this.DownArrow.draw(e.SpriteBatch);
        this.UpArrow.draw(e.SpriteBatch);

        var (x, y) = Game1.getMousePosition(true);
        if (this.DownArrow.containsPoint(x, y))
        {
            itemGrabMenu.hoverText = this.DownArrow.hoverText;
            return;
        }

        if (this.UpArrow.containsPoint(x, y))
        {
            itemGrabMenu.hoverText = this.UpArrow.hoverText;
        }
    }
}