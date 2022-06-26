namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.PatternPatcher;
using StardewMods.Common.UI;
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

    private readonly PerScreen<DisplayedItems?> _inventory = new();

    private readonly PerScreen<ItemGrabMenu?> _itemGrabMenu = new();

    private readonly PerScreen<DisplayedItems?> _itemsToGrabMenu = new();

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
            });
    }

    /// <summary>
    ///     Gets or sets the bottom inventory menu.
    /// </summary>
    public static DisplayedItems? Inventory
    {
        get => BetterItemGrabMenu.Instance!._inventory.Value;
        set => BetterItemGrabMenu.Instance!._inventory.Value = value;
    }

    /// <summary>
    ///     Gets or sets the top inventory menu.
    /// </summary>
    public static DisplayedItems? ItemsToGrabMenu
    {
        get => BetterItemGrabMenu.Instance!._itemsToGrabMenu.Value;
        set => BetterItemGrabMenu.Instance!._itemsToGrabMenu.Value = value;
    }

    private static BetterItemGrabMenu? Instance { get; set; }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    private ItemGrabMenu? Menu
    {
        get => this._itemGrabMenu.Value;
        set => this._itemGrabMenu.Value = value;
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

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            HarmonyHelper.ApplyPatches(BetterItemGrabMenu.Id);
            this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
            this.Helper.Events.Display.RenderedActiveMenu += BetterItemGrabMenu.OnRenderedActiveMenu;
            this.Helper.Events.Display.RenderedActiveMenu += BetterItemGrabMenu.OnRenderedActiveMenu_Low;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            this.Helper.Events.Input.CursorMoved += BetterItemGrabMenu.OnCursorMoved;
            this.Helper.Events.Input.MouseWheelScrolled += BetterItemGrabMenu.OnMouseWheelScrolled;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            HarmonyHelper.UnapplyPatches(BetterItemGrabMenu.Id);
            this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
            this.Helper.Events.Display.RenderedActiveMenu -= BetterItemGrabMenu.OnRenderedActiveMenu;
            this.Helper.Events.Display.RenderedActiveMenu -= BetterItemGrabMenu.OnRenderedActiveMenu_Low;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            this.Helper.Events.Input.CursorMoved -= BetterItemGrabMenu.OnCursorMoved;
            this.Helper.Events.Input.MouseWheelScrolled -= BetterItemGrabMenu.OnMouseWheelScrolled;
        }
    }

    private static IList<Item> ActualInventory(IList<Item> actualInventory, InventoryMenu inventoryMenu)
    {
        return BetterItemGrabMenu.Instance!.GetItems(actualInventory, inventoryMenu);
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

    private static void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        BetterItemGrabMenu.Inventory?.Hover(x, y);
        BetterItemGrabMenu.ItemsToGrabMenu?.Hover(x, y);
    }

    private static void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (BetterItemGrabMenu.Inventory?.Menu.isWithinBounds(x, y) == true)
        {
            BetterItemGrabMenu.Inventory.Offset += e.Delta > 0 ? -1 : 1;
        }

        if (BetterItemGrabMenu.ItemsToGrabMenu?.Menu.isWithinBounds(x, y) == true)
        {
            BetterItemGrabMenu.ItemsToGrabMenu.Offset += e.Delta > 0 ? -1 : 1;
        }

        if (itemGrabMenu is { chestColorPicker: HslColorPicker colorPicker })
        {
            colorPicker.receiveScrollWheelAction(e.Delta > 0 ? -10 : 10);
        }
    }

    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu)
        {
            return;
        }

        BetterItemGrabMenu.ItemsToGrabMenu?.Draw(e.SpriteBatch);
        BetterItemGrabMenu.Inventory?.Draw(e.SpriteBatch);
    }

    [EventPriority(EventPriority.Low)]
    private static void OnRenderedActiveMenu_Low(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu || Game1.activeClickableMenu is ItemSelectionMenu)
        {
            return;
        }

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

        itemGrabMenu.drawMouse(e.SpriteBatch);
    }

    private IList<Item> GetItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: { } context, inventory: { } inventory, ItemsToGrabMenu: { } itemsToGrabMenu } itemGrabMenu || !StorageHelper.TryGetOne(context, out _))
        {
            return actualInventory;
        }

        if (!ReferenceEquals(itemGrabMenu, this.Menu))
        {
            if (ReferenceEquals(context, this.Menu?.context))
            {
                BetterItemGrabMenu.Inventory = new(inventory)
                {
                    Offset = BetterItemGrabMenu.Inventory?.Offset ?? 0,
                };
                BetterItemGrabMenu.ItemsToGrabMenu = new(itemsToGrabMenu)
                {
                    Offset = BetterItemGrabMenu.ItemsToGrabMenu?.Offset ?? 0,
                };
            }
            else
            {
                BetterItemGrabMenu.Inventory = new(inventory);
                BetterItemGrabMenu.ItemsToGrabMenu = new(itemsToGrabMenu);
            }

            this.Menu = itemGrabMenu;
        }

        if (ReferenceEquals(inventoryMenu, BetterItemGrabMenu.Inventory?.Menu))
        {
            return BetterItemGrabMenu.Inventory.Items;
        }

        if (ReferenceEquals(inventoryMenu, BetterItemGrabMenu.ItemsToGrabMenu?.Menu))
        {
            return BetterItemGrabMenu.ItemsToGrabMenu.Items;
        }

        return actualInventory;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        BetterItemGrabMenu.Inventory?.LeftClick(x, y);
        BetterItemGrabMenu.ItemsToGrabMenu?.LeftClick(x, y);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu || BetterItemGrabMenu.ItemsToGrabMenu is null)
        {
            return;
        }

        if (this.Config.ControlScheme.ScrollUp.JustPressed())
        {
            BetterItemGrabMenu.ItemsToGrabMenu.Offset--;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.ScrollUp);
        }

        if (this.Config.ControlScheme.ScrollUp.JustPressed())
        {
            BetterItemGrabMenu.ItemsToGrabMenu.Offset++;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.ScrollUp);
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu)
        {
            this.Menu = null;
        }
    }
}