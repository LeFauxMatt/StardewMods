namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.PatternPatcher;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Enhances the <see cref="StardewValley.Menus.ItemGrabMenu" /> to support filters, sorting, and scrolling..
/// </summary>
internal class BetterItemGrabMenu : IFeature
{
    private const string Id = "furyx639.BetterChests/BetterItemGrabMenu";

    private readonly PerScreen<ItemGrabMenu?> _currentMenu = new();
    private readonly PerScreen<DisplayedItems?> _inventory = new();
    private readonly PerScreen<DisplayedItems?> _itemsToGrabMenu = new();
    private readonly PerScreen<Stack<IClickableMenu>> _overlaidMenus = new(() => new());
    private readonly PerScreen<bool> _refreshInventory = new();
    private readonly PerScreen<bool> _refreshItemsToGrabMenu = new();

    private BetterItemGrabMenu(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
        HarmonyHelper.AddPatches(
            BetterItemGrabMenu.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
                    typeof(BetterItemGrabMenu),
                    nameof(BetterItemGrabMenu.InventoryMenu_draw_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Constructor(typeof(ItemGrabMenu), new[] { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) }),
                    typeof(BetterItemGrabMenu),
                    nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeItemsInList)),
                    typeof(BetterItemGrabMenu),
                    nameof(BetterItemGrabMenu.ItemGrabMenu_organizeItemsInList_postfix),
                    PatchType.Postfix),
            });
    }

    /// <summary>
    ///     Gets the bottom inventory menu.
    /// </summary>
    public static DisplayedItems? Inventory
    {
        get => BetterItemGrabMenu.Instance!._inventory.Value;
        private set => BetterItemGrabMenu.Instance!._inventory.Value = value;
    }

    /// <summary>
    ///     Gets the top inventory menu.
    /// </summary>
    public static DisplayedItems? ItemsToGrabMenu
    {
        get => BetterItemGrabMenu.Instance!._itemsToGrabMenu.Value;
        private set => BetterItemGrabMenu.Instance!._itemsToGrabMenu.Value = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to refresh inventory items on the next tick.
    /// </summary>
    public static bool RefreshInventory
    {
        get => BetterItemGrabMenu.Instance!._refreshInventory.Value;
        set => BetterItemGrabMenu.Instance!._refreshInventory.Value = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to refresh chest items on the next tick.
    /// </summary>
    public static bool RefreshItemsToGrabMenu
    {
        get => BetterItemGrabMenu.Instance!._refreshItemsToGrabMenu.Value;
        set => BetterItemGrabMenu.Instance!._refreshItemsToGrabMenu.Value = value;
    }

    private static BetterItemGrabMenu? Instance { get; set; }

    private ModConfig Config { get; }

    private ItemGrabMenu? CurrentMenu
    {
        get => this._currentMenu.Value;
        set => this._currentMenu.Value = value;
    }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    private Stack<IClickableMenu> OverlaidMenus
    {
        get => this._overlaidMenus.Value;
    }

    /// <summary>
    ///     Adds an overlay to the current <see cref="ItemGrabMenu" />.
    /// </summary>
    /// <param name="menu">The <see cref="IClickableMenu" /> to add.</param>
    public static void AddOverlay(IClickableMenu menu)
    {
        BetterItemGrabMenu.Instance!.OverlaidMenus.Push(menu);
    }

    /// <summary>
    ///     Initializes <see cref="BetterItemGrabMenu" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="BetterItemGrabMenu" /> class.</returns>
    public static BetterItemGrabMenu Init(IModHelper helper, ModConfig config)
    {
        return BetterItemGrabMenu.Instance ??= new(helper, config);
    }

    /// <summary>
    ///     Removes an overlay from the current <see cref="ItemGrabMenu" />.
    /// </summary>
    /// <returns>Returns the removed overlay.</returns>
    public static IClickableMenu RemoveOverlay()
    {
        return BetterItemGrabMenu.Instance!.OverlaidMenus.Pop();
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            HarmonyHelper.ApplyPatches(BetterItemGrabMenu.Id);
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu_Low;
            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
            this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
            this.Helper.Events.Player.InventoryChanged += BetterItemGrabMenu.OnInventoryChanged;
            this.Helper.Events.World.ChestInventoryChanged += BetterItemGrabMenu.OnChestInventoryChanged;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            HarmonyHelper.UnapplyPatches(BetterItemGrabMenu.Id);
            this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
            this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu_Low;
            this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            this.Helper.Events.Input.CursorMoved -= this.OnCursorMoved;
            this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
            this.Helper.Events.Player.InventoryChanged -= BetterItemGrabMenu.OnInventoryChanged;
            this.Helper.Events.World.ChestInventoryChanged -= BetterItemGrabMenu.OnChestInventoryChanged;
        }
    }

    private static IList<Item> ActualInventory(IList<Item> actualInventory, InventoryMenu inventoryMenu)
    {
        return ReferenceEquals(inventoryMenu, BetterItemGrabMenu.Inventory?.Menu)
            ? BetterItemGrabMenu.Inventory.Items
            : ReferenceEquals(inventoryMenu, BetterItemGrabMenu.ItemsToGrabMenu?.Menu)
                ? BetterItemGrabMenu.ItemsToGrabMenu.Items
                : actualInventory;
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(InventoryMenu)}.{nameof(InventoryMenu.draw)} from {nameof(BetterItemGrabMenu)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Actual Inventory Patch
        // Replaces all actualInventory with ItemsDisplayed.DisplayedItems(actualInventory)
        // which can filter/sort items separately from the actual inventory.
        patcher.AddPatchLoop(
            code =>
            {
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ActualInventory))));
            },
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))));

        // Fill code buffer
        foreach (var inCode in instructions)
        {
            // Return patched code segments
            foreach (var outCode in patcher.From(inCode))
            {
                yield return outCode;
            }
        }

        // Return remaining code
        foreach (var outCode in patcher.FlushBuffer())
        {
            yield return outCode;
        }

        Log.Trace($"{patcher.AppliedPatches.ToString()} / {patcher.TotalPatches.ToString()} patches applied.");
        if (patcher.AppliedPatches < patcher.TotalPatches)
        {
            Log.Warn("Failed to applied all patches!");
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
    {
        if (__instance is not { context: { } context, inventory: { } inventory, ItemsToGrabMenu: { } itemsToGrabMenu } itemGrabMenu || !StorageHelper.TryGetOne(context, out _))
        {
            BetterItemGrabMenu.Inventory = null;
            BetterItemGrabMenu.ItemsToGrabMenu = null;
            return;
        }

        if (!ReferenceEquals(itemGrabMenu, BetterItemGrabMenu.Instance!.CurrentMenu))
        {
            BetterItemGrabMenu.Instance.CurrentMenu = itemGrabMenu;
            if (ReferenceEquals(context, BetterItemGrabMenu.Instance.CurrentMenu?.context))
            {
                BetterItemGrabMenu.Inventory = new(inventory, false)
                {
                    Offset = BetterItemGrabMenu.Inventory?.Offset ?? 0,
                };
                BetterItemGrabMenu.ItemsToGrabMenu = new(itemsToGrabMenu, true)
                {
                    Offset = BetterItemGrabMenu.ItemsToGrabMenu?.Offset ?? 0,
                };
            }
            else
            {
                BetterItemGrabMenu.Inventory = new(inventory, false);
                BetterItemGrabMenu.ItemsToGrabMenu = new(itemsToGrabMenu, true);
            }
        }
    }

    private static void ItemGrabMenu_organizeItemsInList_postfix(IList<Item> items)
    {
        if (BetterItemGrabMenu.Instance!.CurrentMenu is null)
        {
            return;
        }

        BetterItemGrabMenu.RefreshInventory |= ReferenceEquals(BetterItemGrabMenu.Instance.CurrentMenu.inventory.actualInventory, items);
        BetterItemGrabMenu.RefreshItemsToGrabMenu |= ReferenceEquals(BetterItemGrabMenu.Instance.CurrentMenu.ItemsToGrabMenu.actualInventory, items);
    }

    private static void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
    {
        BetterItemGrabMenu.RefreshItemsToGrabMenu |= Game1.activeClickableMenu is ItemGrabMenu;
        BetterItemGrabMenu.RefreshInventory |= Game1.activeClickableMenu is ItemGrabMenu;
    }

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        BetterItemGrabMenu.RefreshItemsToGrabMenu |= Game1.activeClickableMenu is ItemGrabMenu;
        BetterItemGrabMenu.RefreshInventory |= Game1.activeClickableMenu is ItemGrabMenu && e.IsLocalPlayer;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft when this.OverlaidMenus.Any():
                this.OverlaidMenus.Last().receiveLeftClick(x, y);
                break;
            case SButton.MouseRight when this.OverlaidMenus.Any():
                this.OverlaidMenus.Last().receiveRightClick(x, y);
                break;
            case SButton.MouseLeft when BetterItemGrabMenu.Inventory?.LeftClick(x, y) == true:
                break;
            case SButton.MouseLeft when BetterItemGrabMenu.ItemsToGrabMenu?.LeftClick(x, y) == true:
                break;
            default:
                return;
        }

        this.Helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.CurrentMenu is null || this.OverlaidMenus.Any())
        {
            return;
        }

        var displayedItems = BetterItemGrabMenu.Inventory is not null
                             && this.CurrentMenu.currentlySnappedComponent is not null
                             && BetterItemGrabMenu.Inventory.Menu.inventory.Contains(this.CurrentMenu.currentlySnappedComponent)
            ? BetterItemGrabMenu.Inventory
            : BetterItemGrabMenu.ItemsToGrabMenu;
        if (displayedItems is null)
        {
            return;
        }

        var offset = displayedItems.Offset;
        if (this.Config.ControlScheme.ScrollUp.JustPressed()
            && (this.CurrentMenu.currentlySnappedComponent is null
                || displayedItems.Menu.inventory.Take(12).Contains(this.CurrentMenu.currentlySnappedComponent)))
        {
            displayedItems.Offset--;
            if (offset != displayedItems.Offset)
            {
                this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.ScrollUp);
            }
        }

        if (this.Config.ControlScheme.ScrollDown.JustPressed()
            && (this.CurrentMenu.currentlySnappedComponent is null
                || displayedItems.Menu.inventory.TakeLast(12).Contains(this.CurrentMenu.currentlySnappedComponent)))
        {
            displayedItems.Offset++;
            if (offset != displayedItems.Offset)
            {
                this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.ScrollDown);
            }
        }
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.OverlaidMenus.Any())
        {
            this.OverlaidMenus.Last().performHoverAction(x, y);
            return;
        }

        BetterItemGrabMenu.Inventory?.Hover(x, y);
        BetterItemGrabMenu.ItemsToGrabMenu?.Hover(x, y);
    }

    private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.OverlaidMenus.Any())
        {
            this.OverlaidMenus.Last().receiveScrollWheelAction(e.Delta);
            return;
        }

        if (BetterItemGrabMenu.Inventory?.Menu.isWithinBounds(x, y) == true)
        {
            BetterItemGrabMenu.Inventory.Offset += e.Delta > 0 ? -1 : 1;
        }

        if (BetterItemGrabMenu.ItemsToGrabMenu?.Menu.isWithinBounds(x, y) == true)
        {
            BetterItemGrabMenu.ItemsToGrabMenu.Offset += e.Delta > 0 ? -1 : 1;
        }

        if (this.CurrentMenu is { chestColorPicker: HslColorPicker colorPicker })
        {
            colorPicker.receiveScrollWheelAction(e.Delta > 0 ? -10 : 10);
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        BetterItemGrabMenu.ItemsToGrabMenu?.Draw(e.SpriteBatch);
        BetterItemGrabMenu.Inventory?.Draw(e.SpriteBatch);
    }

    [EventPriority(EventPriority.Low)]
    private void OnRenderedActiveMenu_Low(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        if (this.OverlaidMenus.Any())
        {
            foreach (var overlay in this.OverlaidMenus)
            {
                overlay.draw(e.SpriteBatch);
            }

            this.CurrentMenu.drawMouse(e.SpriteBatch);
            return;
        }

        if (this.CurrentMenu.hoveredItem is not null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, this.CurrentMenu.hoveredItem.getDescription(), this.CurrentMenu.hoveredItem.DisplayName, this.CurrentMenu.hoveredItem, this.CurrentMenu.heldItem != null);
        }
        else if (!string.IsNullOrWhiteSpace(this.CurrentMenu.hoverText))
        {
            if (this.CurrentMenu.hoverAmount > 0)
            {
                IClickableMenu.drawToolTip(e.SpriteBatch, this.CurrentMenu.hoverText, string.Empty, null, true, -1, 0, -1, -1, null, this.CurrentMenu.hoverAmount);
            }
            else
            {
                IClickableMenu.drawHoverText(e.SpriteBatch, this.CurrentMenu.hoverText, Game1.smallFont);
            }
        }

        this.CurrentMenu.drawMouse(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        var menu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu itemGrabMenu => itemGrabMenu,
            { } clickableMenu when clickableMenu.GetChildMenu() is ItemGrabMenu itemGrabMenu => itemGrabMenu,
            _ => null,
        };

        if (!ReferenceEquals(menu, this.CurrentMenu))
        {
            if (menu is null or { context: null })
            {
                this.CurrentMenu = null;
                this.OverlaidMenus.Clear();
            }
        }

        if (!BetterItemGrabMenu.RefreshInventory && !BetterItemGrabMenu.RefreshItemsToGrabMenu)
        {
            return;
        }

        var refreshInventory = BetterItemGrabMenu.RefreshInventory;
        var refreshItemsToGrabMenu = BetterItemGrabMenu.RefreshItemsToGrabMenu;
        BetterItemGrabMenu.RefreshInventory = false;
        BetterItemGrabMenu.RefreshItemsToGrabMenu = false;
        if (menu is null)
        {
            return;
        }

        if (refreshInventory)
        {
            BetterItemGrabMenu.Inventory?.RefreshItems();
        }

        if (refreshItemsToGrabMenu)
        {
            BetterItemGrabMenu.ItemsToGrabMenu?.RefreshItems();
        }
    }
}