namespace StardewMods.BetterChests.Framework.Features;

using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewValley.Menus;

/// <summary>
///     Transfer all items into or out from a chest.
/// </summary>
internal sealed class TransferItems : Feature
{
#nullable disable
    private static Feature Instance;
#nullable enable

    private readonly PerScreen<ClickableTextureComponent> _downArrow;
    private readonly IModHelper _helper;
    private readonly PerScreen<ClickableTextureComponent> _upArrow;

    private TransferItems(IModHelper helper)
    {
        this._helper = helper;
        this._downArrow = new(
            () => new(
                new(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
                helper.GameContent.Load<Texture2D>("furyx639.BetterChests/Icons"),
                new(84, 0, 7, 16),
                Game1.pixelZoom)
            {
                hoverText = I18n.Button_TransferDown_Name(),
                myID = 5318010,
            });
        this._upArrow = new(
            () => new(
                new(0, 0, 7 * Game1.pixelZoom, Game1.tileSize),
                helper.GameContent.Load<Texture2D>("furyx639.BetterChests/Icons"),
                new(100, 0, 7, 16),
                Game1.pixelZoom)
            {
                hoverText = I18n.Button_TransferUp_Name(),
                myID = 5318011,
            });
    }

    private ClickableTextureComponent DownArrow => this._downArrow.Value;

    private ClickableTextureComponent UpArrow => this._upArrow.Value;

    /// <summary>
    ///     Initializes <see cref="TransferItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="TransferItems" /> class.</returns>
    public static Feature Init(IModHelper helper)
    {
        return TransferItems.Instance ??= new TransferItems(helper);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        BetterItemGrabMenu.Constructing += TransferItems.OnConstructing;
        this._helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this._helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterItemGrabMenu.Constructing -= TransferItems.OnConstructing;
        this._helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this._helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static void OnConstructing(object? sender, ItemGrabMenu itemGrabMenu)
    {
        if (BetterItemGrabMenu.TopPadding > 0
         || itemGrabMenu.context is null
         || itemGrabMenu.shippingBin
         || !Storages.TryGetOne(itemGrabMenu.context, out _))
        {
            return;
        }

        BetterItemGrabMenu.TopPadding = 24;
    }

    private static void TransferDown()
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: { } context, shippingBin: false }
         || !Storages.TryGetOne(context, out var storage)
         || storage is not { Data: Storage storageObject })
        {
            return;
        }

        var items = storageObject.Items.ToArray();
        foreach (var item in items)
        {
            if (item is null || item.modData.ContainsKey("furyx639.BetterChests/LockedSlot"))
            {
                continue;
            }

            if (Game1.player.addItemToInventoryBool(item))
            {
                storageObject.Items.Remove(item);
            }
        }

        storageObject.ClearNulls();
    }

    private static void TransferUp()
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: { } context, shippingBin: false }
         || !Storages.TryGetOne(context, out var storage)
         || storage is not { Data: Storage storageObject })
        {
            return;
        }

        var items = Game1.player.Items.ToArray();
        foreach (var item in items)
        {
            if (item is null || item.modData.ContainsKey("furyx639.BetterChests/LockedSlot"))
            {
                continue;
            }

            if (storageObject.AddItem(item) is null)
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
        if (e.NewMenu is not ItemGrabMenu
            {
                context: { } context, ItemsToGrabMenu: { } itemsToGrabMenu, shippingBin: false,
            }
         || !Storages.TryGetOne(context, out _))
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