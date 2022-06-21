namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Common.Helpers;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Helpers.PatternPatcher;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Enhances the <see cref="ItemGrabMenu" /> to support filters, sorting, and scrolling..
/// </summary>
internal class BetterItemGrabMenu : IFeature
{
    private const string Id = "BetterChests.BetterItemGrabMenu";

    private readonly PerScreen<object?> _context = new();

    private readonly PerScreen<InventoryMenu?> _inventory = new();

    private readonly PerScreen<List<Item>?> _inventoryItems = new();

    private readonly PerScreen<int> _inventoryOffset = new();

    private readonly PerScreen<InventoryMenu?> _itemsToGrabMenu = new();

    private readonly PerScreen<List<Item>?> _itemsToGrabMenuItems = new();

    private readonly PerScreen<int> _itemsToGrabMenuOffset = new();

    private BetterItemGrabMenu(IModHelper helper)
    {
        this.Helper = helper;
        HarmonyHelper.AddPatches(
            BetterItemGrabMenu.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw)),
                    typeof(BetterItemGrabMenu),
                    nameof(BetterItemGrabMenu.InventoryMenu_draw_transpiler),
                    PatchType.Transpiler),
            });
    }

    private static BetterItemGrabMenu? Instance { get; set; }

    private object? Context
    {
        get => this._context.Value;
        set => this._context.Value = value;
    }

    private IModHelper? Helper { get; }

    private InventoryMenu? Inventory
    {
        get => this._inventory.Value;
        set => this._inventory.Value = value;
    }

    private List<Item>? InventoryItems
    {
        get => this._inventoryItems.Value;
        set => this._inventoryItems.Value = value;
    }

    private int InventoryOffset
    {
        get => this._inventoryOffset.Value;
        set => this._inventoryOffset.Value = value;
    }

    private InventoryMenu? ItemsToGrabMenu
    {
        get => this._itemsToGrabMenu.Value;
        set => this._itemsToGrabMenu.Value = value;
    }

    private List<Item>? ItemsToGrabMenuItems
    {
        get => this._itemsToGrabMenuItems.Value;
        set => this._itemsToGrabMenuItems.Value = value;
    }

    private int ItemsToGrabMenuOffset
    {
        get => this._itemsToGrabMenuOffset.Value;
        set => this._itemsToGrabMenuOffset.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="BetterItemGrabMenu" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="BetterItemGrabMenu" /> class.</returns>
    public static BetterItemGrabMenu Init(IModHelper helper)
    {
        return BetterItemGrabMenu.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        HarmonyHelper.ApplyPatches(BetterItemGrabMenu.Id);
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(BetterItemGrabMenu.Id);
    }

    private static IList<Item> GetItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
    {
        if (ReferenceEquals(inventoryMenu, BetterItemGrabMenu.Instance!.Inventory))
        {
            return BetterItemGrabMenu.Instance.InventoryItems!;
        }

        if (ReferenceEquals(inventoryMenu, BetterItemGrabMenu.Instance.ItemsToGrabMenu))
        {
            return BetterItemGrabMenu.Instance.ItemsToGrabMenuItems!;
        }

        if (Game1.activeClickableMenu is not ItemGrabMenu { context: { } context, inventory: { } inventory, ItemsToGrabMenu: { } itemsToGrabMenu })
        {
            return actualInventory;
        }

        if (!ReferenceEquals(BetterItemGrabMenu.Instance.Context, context))
        {
            BetterItemGrabMenu.Instance.Context = context;
            BetterItemGrabMenu.Instance.InventoryOffset = 0;
            BetterItemGrabMenu.Instance.ItemsToGrabMenuOffset = 0;
        }

        BetterItemGrabMenu.Instance.Inventory = inventory;
        BetterItemGrabMenu.Instance.ItemsToGrabMenu = itemsToGrabMenu;
        BetterItemGrabMenu.Instance.InventoryItems = inventory.actualInventory.Take(inventory.capacity).ToList();
        BetterItemGrabMenu.Instance.ItemsToGrabMenuItems = itemsToGrabMenu.actualInventory.Take(itemsToGrabMenu.capacity).ToList();
        return actualInventory;
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(InventoryMenu)}.{nameof(InventoryMenu.draw)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Actual Inventory Patch
        // Replaces all actualInventory with ItemsDisplayed.DisplayedItems(actualInventory)
        // which can filter/sort items separately from the actual inventory.
        patcher.AddPatchLoop(
            code =>
            {
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.GetItems))));
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
}