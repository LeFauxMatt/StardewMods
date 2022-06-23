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
using StardewMods.BetterChests.Storages;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.PatternPatcher;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Adds additional rows to the <see cref="ItemGrabMenu" />.
/// </summary>
internal class ResizeChestMenu : IFeature
{
    private const string Id = "BetterChests.ResizeChestMenu";

    private readonly PerScreen<object?> _context = new();
    private readonly PerScreen<BaseStorage?> _storage = new();

    private ResizeChestMenu(IModHelper helper)
    {
        this.Helper = helper;
        var ctorItemGrabMenu = new[]
        {
            typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object),
        };

        var drawMenuWithInventory = new[]
        {
            typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int),
        };

        HarmonyHelper.AddPatches(
            ResizeChestMenu.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Constructor(typeof(ItemGrabMenu), ctorItemGrabMenu),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.ItemGrabMenu_constructor_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(
                        typeof(ItemGrabMenu),
                        nameof(ItemGrabMenu.draw),
                        new[]
                        {
                            typeof(SpriteBatch),
                        }),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.ItemGrabMenu_draw_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), drawMenuWithInventory),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.MenuWithInventory_draw_transpiler),
                    PatchType.Transpiler),
            });
    }

    private static ResizeChestMenu? Instance { get; set; }

    private object? Context
    {
        get => this._context.Value;
        set => this._context.Value = value;
    }

    private IModHelper Helper { get; }

    private BaseStorage? Storage
    {
        get => this._storage.Value;
        set => this._storage.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="ResizeChestMenu" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="ResizeChestMenu" /> class.</returns>
    public static ResizeChestMenu Init(IModHelper helper)
    {
        return ResizeChestMenu.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        HarmonyHelper.ApplyPatches(ResizeChestMenu.Id);
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(ResizeChestMenu.Id);
        this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
    }

    private static int GetCapacity(ItemGrabMenu menu, object? context = null)
    {
        if (context is not null && !ReferenceEquals(ResizeChestMenu.Instance!.Context, context))
        {
            ResizeChestMenu.Instance.Context = context;
            ResizeChestMenu.Instance.Storage = StorageHelper.TryGetOne(context, out var storage) ? storage : null;
        }

        return ResizeChestMenu.Instance?.Storage?.MenuCapacity ?? -1;
    }

    private static int GetExtraSpace(MenuWithInventory menu)
    {
        return menu is ItemGrabMenu { context: { } context } && ReferenceEquals(ResizeChestMenu.Instance?.Context, context) && ResizeChestMenu.Instance.Storage is not null
            ? ResizeChestMenu.Instance.Storage.MenuExtraSpace
            : 0;
    }

    private static int? GetRows(ItemGrabMenu menu, object? context = null)
    {
        if (context is not null && !ReferenceEquals(ResizeChestMenu.Instance!.Context, context))
        {
            ResizeChestMenu.Instance.Context = context;
            ResizeChestMenu.Instance.Storage = StorageHelper.TryGetOne(context, out var storage) ? storage : null;
        }

        return ResizeChestMenu.Instance?.Storage?.MenuRows ?? 3;
    }

    /// <summary>Generate additional slots/rows for top inventory menu.</summary>
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
    private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(ItemGrabMenu)}.ctor from {nameof(ResizeChestMenu)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Jump Condition Patch
        // Original:
        //      if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).GetActualCapacity() != 36)
        // Patched:
        //      if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).GetActualCapacity() >= 10)
        //
        // This forces (InventoryMenu) ItemsToGrabMenu to be instantiated with the a capacity of 36
        // and prevents large capacity chests from freezing the game and leaking memory
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Changing jump condition from Beq 36 to Bge 10.", true);
                var top = code[^1];
                code.RemoveAt(code.Count - 1);
                code.RemoveAt(code.Count - 1);
                code.Add(new(OpCodes.Ldc_I4_S, (sbyte)10));
                code.Add(new(OpCodes.Bge_S, top.operand));
            },
            new(OpCodes.Isinst, typeof(Chest)),
            new(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))),
            new(OpCodes.Ldc_I4_S, (sbyte)36),
            new(OpCodes.Beq_S));

        // Original:
        //      ResizeChestMenu.ItemsToGrabMenu = new InventoryMenu(base.xPositionOnScreen + 32, base.yPositionOnScreen, false, inventory, highlightFunction, -1, 3, 0, 0, true);
        // Patched:
        //      ResizeChestMenu.ItemsToGrabMenu = new InventoryMenu(base.xPositionOnScreen + 32, base.yPositionOnScreen, false, inventory, highlightFunction, ResizeChestMenu.GetMenuCapacity(), ResizeChestMenu.GetMenuRows(), 0, 0, true);
        //
        // This replaces the default capacity/rows of -1 and 3 with ResizeChestMenu methods to
        // allow customized capacity and rows
        patcher.AddSeek(
            new(
                OpCodes.Newobj,
                AccessTools.Constructor(
                    typeof(InventoryMenu),
                    new[]
                    {
                        typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool),
                    })),
            new(OpCodes.Stfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))));

        patcher.AddPatch(
            code =>
            {
                Log.Trace("Overriding default values for capacity and rows.", true);
                code.RemoveAt(code.Count - 1);
                code.RemoveAt(code.Count - 1);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Ldarg_S, (short)15));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetCapacity))));
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Ldarg_S, (short)15));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetRows))));
            },
            new(OpCodes.Ldc_I4_M1),
            new(OpCodes.Ldc_I4_3));

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

    /// <summary>Move/resize backpack by expanded menu height.</summary>
    private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(ItemGrabMenu)}.{nameof(ItemGrabMenu.draw)} from {nameof(ResizeChestMenu)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Draw Backpack Patch
        // This adds ResizeChestMenu.GetExtraSpace() to the y-coordinate of the backpack sprite
        patcher.AddSeek(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))));
        patcher.AddPatch(
                   code =>
                   {
                       Log.Trace("Moving backpack icon down by expanded menu extra height.", true);
                       code.Add(new(OpCodes.Ldarg_0));
                       code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetExtraSpace))));
                       code.Add(new(OpCodes.Add));
                   },
                   new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
               .Repeat(2);

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

    /// <summary>Move/resize bottom dialogue box by search bar height.</summary>
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
    private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(MenuWithInventory)}.{nameof(MenuWithInventory.draw)} from {nameof(ResizeChestMenu)}", true);
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Move Dialogue Patch
        // This adds ResizeChestMenu.GetExtraSpace() to the y-coordinate of the inventory dialogue
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Moving bottom dialogue box down by expanded menu height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetExtraSpace))));
                code.Add(new(OpCodes.Add));
            },
            new(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
            new(OpCodes.Add),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
            new(OpCodes.Add),
            new(OpCodes.Ldc_I4_S, (sbyte)64),
            new(OpCodes.Add));

        // ****************************************************************************************
        // Shrink Dialogue Patch
        // This subtracts ResizeChestMenu.GetExtraSpace() from the height of the inventory dialogue
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Shrinking bottom dialogue box height by expanded menu height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetExtraSpace))));
                code.Add(new(OpCodes.Add));
            },
            new(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
            new(OpCodes.Add),
            new(OpCodes.Ldc_I4, 192),
            new(OpCodes.Add));

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

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu { ItemsToGrabMenu.inventory: { } topRow, inventory.inventory: { } bottomRow })
        {
            return;
        }

        // Set upNeighborId for first row of player inventory
        topRow = topRow.Take(12).ToList();
        bottomRow = bottomRow.TakeLast(12).ToList();
        for (var index = 0; index < 12; index++)
        {
            var topSlot = topRow.ElementAtOrDefault(index);
            var bottomSlot = bottomRow.ElementAtOrDefault(index);
            if (topSlot is not null && bottomSlot is not null)
            {
                topSlot.upNeighborID = bottomSlot.myID;
            }
        }
    }
}