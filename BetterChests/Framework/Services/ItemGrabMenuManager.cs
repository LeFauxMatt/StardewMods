namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Extensions;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Manages the item grab menu in the game.</summary>
internal sealed class ItemGrabMenuManager : BaseService
{
#nullable disable
    private static ItemGrabMenuManager instance;
#nullable enable
    private readonly PerScreen<InventoryMenuManager> bottomMenu;

    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<IContainer?> currentContainer = new();
    private readonly PerScreen<IClickableMenu?> currentMenu = new();
    private readonly PerScreen<InventoryMenuManager> topMenu;

    private EventHandler<ItemGrabMenuChangedEventArgs>? itemGrabMenuChanged;

    /// <summary>Initializes a new instance of the <see cref="ItemGrabMenuManager" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    public ItemGrabMenuManager(ILogging logging, IModEvents modEvents, Harmony harmony, ContainerFactory containerFactory)
        : base(logging)
    {
        // Init
        ItemGrabMenuManager.instance = this;
        this.containerFactory = containerFactory;
        this.topMenu = new PerScreen<InventoryMenuManager>(() => new InventoryMenuManager(logging));
        this.bottomMenu = new PerScreen<InventoryMenuManager>(() => new InventoryMenuManager(logging));

        // Events
        modEvents.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
        modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        modEvents.GameLoop.UpdateTicked += this.OnUpdateTicked;
        modEvents.GameLoop.UpdateTicking += this.OnUpdateTicking;

        // Patches
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
            new HarmonyMethod(typeof(ItemGrabMenuManager), nameof(ItemGrabMenuManager.InventoryMenu_draw_prefix)),
            new HarmonyMethod(typeof(ItemGrabMenuManager), nameof(ItemGrabMenuManager.InventoryMenu_draw_postfix)));
    }

    /// <summary>Gets the current container for the item grab menu.</summary>
    public IContainer? CurrentContainer => Game1.activeClickableMenu?.Equals(this.currentMenu.Value) == true ? this.currentContainer.Value : null;

    /// <summary>Gets the current item grab menu.</summary>
    public ItemGrabMenu? CurrentMenu => Game1.activeClickableMenu?.Equals(this.currentMenu.Value) == true ? this.currentMenu.Value as ItemGrabMenu : null;

    /// <summary>Gets the inventory menu manager for the top inventory menu.</summary>
    public IInventoryMenuManager TopMenu => this.topMenu.Value;

    /// <summary>Gets the inventory menu manager for the bottom inventory menu.</summary>
    public IInventoryMenuManager BottomMenu => this.bottomMenu.Value;

    /// <summary>Event raised when the item grab menu has changed.</summary>
    public event EventHandler<ItemGrabMenuChangedEventArgs> ItemGrabMenuChanged
    {
        add => this.itemGrabMenuChanged += value;
        remove => this.itemGrabMenuChanged -= value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_prefix(InventoryMenu __instance, ref InventoryMenuManager? __state)
    {
        __state = __instance.Equals(ItemGrabMenuManager.instance.topMenu.Value.Source)
            ? ItemGrabMenuManager.instance.topMenu.Value
            : __instance.Equals(ItemGrabMenuManager.instance.bottomMenu.Value.Source)
                ? ItemGrabMenuManager.instance.bottomMenu.Value
                : null;

        if (__state?.Context is null)
        {
            return;
        }

        // Apply operations
        __instance.actualInventory = __state.ApplyOperation(__state.Context.Items).ToList();
        for (var index = 0; index < __instance.inventory.Count; ++index)
        {
            if (index >= __instance.actualInventory.Count)
            {
                __instance.inventory[index].name = __instance.actualInventory.Count.ToString(CultureInfo.InvariantCulture);
                continue;
            }

            __instance.inventory[index].name = __state.Context.Items.IndexOf(__instance.actualInventory[index]).ToString(CultureInfo.InvariantCulture);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_postfix(InventoryMenu __instance, ref InventoryMenuManager? __state)
    {
        __state = __instance.Equals(ItemGrabMenuManager.instance.topMenu.Value.Source)
            ? ItemGrabMenuManager.instance.topMenu.Value
            : __instance.Equals(ItemGrabMenuManager.instance.bottomMenu.Value.Source)
                ? ItemGrabMenuManager.instance.bottomMenu.Value
                : null;

        if (__state?.Context is null)
        {
            return;
        }

        // Restore original
        __instance.actualInventory = __state.Context.Items;
    }

    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e) => this.UpdateMenu();

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e) => this.UpdateMenu();

    private void UpdateHighlightMethods()
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        if (this.CurrentMenu.ItemsToGrabMenu.highlightMethod != this.topMenu.Value.HighlightMethod)
        {
            this.topMenu.Value.OriginalHighlightMethod = this.CurrentMenu.ItemsToGrabMenu.highlightMethod;
            this.CurrentMenu.ItemsToGrabMenu.highlightMethod = this.topMenu.Value.HighlightMethod;
        }

        if (this.CurrentMenu.inventory.highlightMethod != this.bottomMenu.Value.HighlightMethod)
        {
            this.bottomMenu.Value.OriginalHighlightMethod = this.CurrentMenu.inventory.highlightMethod;
            this.CurrentMenu.inventory.highlightMethod = this.bottomMenu.Value.HighlightMethod;
        }
    }

    private void UpdateMenu()
    {
        if (Game1.activeClickableMenu?.Equals(this.currentMenu.Value) == true)
        {
            this.UpdateHighlightMethods();
            return;
        }

        this.currentMenu.Value = Game1.activeClickableMenu;
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            } itemGrabMenu
            || !this.containerFactory.TryGetOne(chest, out var container))
        {
            this.currentContainer.Value = null;
            this.itemGrabMenuChanged.InvokeAll(this, new ItemGrabMenuChangedEventArgs());
            return;
        }

        this.currentContainer.Value = container;
        var eventArgs = new ItemGrabMenuChangedEventArgs(container, itemGrabMenu);

        // Update top menu
        this.topMenu.Value.Reset();
        this.topMenu.Value.Source = itemGrabMenu.ItemsToGrabMenu;
        this.topMenu.Value.Context = container;

        // Update bottom menu
        this.bottomMenu.Value.Reset();
        this.bottomMenu.Value.Source = itemGrabMenu.inventory;
        if (itemGrabMenu.inventory.actualInventory.Equals(Game1.player.Items) && this.containerFactory.TryGetOne(Game1.player, out container))
        {
            this.bottomMenu.Value.Context = container;
        }

        // Reset filters
        this.UpdateHighlightMethods();
        this.itemGrabMenuChanged.InvokeAll(this, eventArgs);

        // Disable background fade
        itemGrabMenu.drawBG = false;
    }

    [EventPriority((EventPriority)int.MaxValue)]
    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        // Redraw background
        if (this.CurrentMenu is not null)
        {
            e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
        }
    }

    [EventPriority((EventPriority)int.MinValue)]
    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        // Redraw foreground
        if (this.CurrentMenu is null)
        {
            return;
        }

        if (this.CurrentMenu.hoverText != null && (this.CurrentMenu.hoveredItem == null || this.CurrentMenu.ItemsToGrabMenu == null))
        {
            if (this.CurrentMenu.hoverAmount > 0)
            {
                IClickableMenu.drawToolTip(e.SpriteBatch, this.CurrentMenu.hoverText, string.Empty, null, true, -1, 0, null, -1, null, this.CurrentMenu.hoverAmount);
            }
            else
            {
                IClickableMenu.drawHoverText(e.SpriteBatch, this.CurrentMenu.hoverText, Game1.smallFont);
            }
        }

        if (this.CurrentMenu.hoveredItem != null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, this.CurrentMenu.hoveredItem.getDescription(), this.CurrentMenu.hoveredItem.DisplayName, this.CurrentMenu.hoveredItem, this.CurrentMenu.heldItem != null);
        }
        else if (this.CurrentMenu.hoveredItem != null && this.CurrentMenu.ItemsToGrabMenu != null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, this.CurrentMenu.ItemsToGrabMenu.descriptionText, this.CurrentMenu.ItemsToGrabMenu.descriptionTitle, this.CurrentMenu.hoveredItem, this.CurrentMenu.heldItem != null);
        }

        this.CurrentMenu.heldItem?.drawInMenu(e.SpriteBatch, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
        Game1.mouseCursorTransparency = 1f;
        this.CurrentMenu.drawMouse(e.SpriteBatch);
    }
}
