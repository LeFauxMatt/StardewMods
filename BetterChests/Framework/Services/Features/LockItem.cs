namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Locks items in inventory so they cannot be stashed.</summary>
internal sealed class LockItem : BaseFeature<LockItem>
{
    private readonly ContainerOperations containerOperations;
    private readonly IInputHelper inputHelper;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="LockItem" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerOperations">Dependency used for handling operations between containers.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public LockItem(
        ILog log,
        IModConfig modConfig,
        ContainerOperations containerOperations,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        IModEvents modEvents)
        : base(log, modConfig)
    {
        this.containerOperations = containerOperations;
        this.modEvents = modEvents;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.LockItem != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.containerOperations.ItemTransferring += this.OnItemTransferring;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.containerOperations.ItemTransferring -= this.OnItemTransferring;
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    private static bool TryGetMenu(int mouseX, int mouseY, [NotNullWhen(true)] out InventoryMenu? inventoryMenu)
    {
        inventoryMenu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu
            {
                inventory:
                { } inventory,
            } when inventory.isWithinBounds(mouseX, mouseY) => inventory,
            ItemGrabMenu
            {
                ItemsToGrabMenu:
                { } itemsToGrabMenu,
            } when itemsToGrabMenu.isWithinBounds(mouseX, mouseY) => itemsToGrabMenu,
            GameMenu gameMenu when gameMenu.GetCurrentPage() is InventoryPage
            {
                inventory:
                { } inventoryPage,
            } => inventoryPage,
            _ => null,
        };

        return inventoryMenu is not null;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Menu is not null)
        {
            this.DrawOverlay(e.SpriteBatch, this.itemGrabMenuManager.Top.Menu);
        }

        if (this.itemGrabMenuManager.Bottom.Menu is not null)
        {
            this.DrawOverlay(e.SpriteBatch, this.itemGrabMenuManager.Bottom.Menu);
        }
    }

    private void DrawOverlay(SpriteBatch spriteBatch, InventoryMenu inventoryMenu)
    {
        foreach (var slot in inventoryMenu.inventory)
        {
            if (!int.TryParse(slot.name, out var index))
            {
                continue;
            }

            var item = inventoryMenu.actualInventory.ElementAtOrDefault(index);
            if (item is null || this.IsUnlocked(item))
            {
                continue;
            }

            var x = slot.bounds.X + slot.bounds.Width - 18;
            var y = slot.bounds.Y + slot.bounds.Height - 18;
            spriteBatch.Draw(
                Game1.mouseCursors,
                new Vector2(x - 40, y - 40),
                new Rectangle(107, 442, 7, 8),
                Color.White,
                0f,
                Vector2.Zero,
                2,
                SpriteEffects.None,
                1f);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.Config.LockItemHold || e.Button is not SButton.MouseLeft || !this.Config.Controls.LockSlot.IsDown())
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!LockItem.TryGetMenu(mouseX, mouseY, out var inventoryMenu))
        {
            return;
        }

        var slot = inventoryMenu.inventory.FirstOrDefault(slot => slot.containsPoint(mouseX, mouseY));
        if (slot is null || !int.TryParse(slot.name, out var index))
        {
            return;
        }

        var item = inventoryMenu.actualInventory.ElementAtOrDefault(index);
        if (item is null)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.ToggleLock(item);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.Config.LockItemHold || !this.Config.Controls.LockSlot.JustPressed())
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!LockItem.TryGetMenu(mouseX, mouseY, out var inventoryMenu))
        {
            return;
        }

        var slot = inventoryMenu.inventory.FirstOrDefault(slot => slot.containsPoint(mouseX, mouseY));
        if (slot is null || !int.TryParse(slot.name, out var index))
        {
            return;
        }

        var item = inventoryMenu.actualInventory.ElementAtOrDefault(index);
        if (item is null)
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.LockSlot);
        this.ToggleLock(item);
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        this.itemGrabMenuManager.Top.AddHighlightMethod(this.IsUnlocked);
        this.itemGrabMenuManager.Bottom.AddHighlightMethod(this.IsUnlocked);
    }

    private void OnItemTransferring(object? sender, ItemTransferringEventArgs e)
    {
        if (!this.IsUnlocked(e.Item))
        {
            e.PreventTransfer();
        }
    }

    private void ToggleLock(Item item)
    {
        if (this.IsUnlocked(item))
        {
            item.modData[this.UniqueId] = "Locked;";
        }
        else
        {
            item.modData.Remove(this.UniqueId);
        }
    }

    private bool IsUnlocked(Item item) => !item.modData.ContainsKey(this.UniqueId);
}