namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Common.Extensions;
using Common.Helpers;
using Common.Helpers.PatternPatcher;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChestMenu : Feature
{
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<int?> _menuCapacity = new();
    private readonly PerScreen<int?> _menuOffset = new();
    private readonly PerScreen<int?> _menuRows = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResizeChestMenu" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ResizeChestMenu(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        ResizeChestMenu.Instance = this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                var ctorItemGrabMenu = new[]
                {
                    typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object),
                };

                var drawMenuWithInventory = new[]
                {
                    typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int),
                };

                harmony.AddPatches(
                    this.Id,
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
            });
    }

    private static ResizeChestMenu Instance { get; set; }

    private IHarmonyHelper HarmonyHelper
    {
        get => this._harmony.Value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set
        {
            if (!ReferenceEquals(this._menu.Value, value))
            {
                this._menu.Value = value;
                this._menuCapacity.Value = null;
                this._menuRows.Value = null;
                this._menuOffset.Value = null;
            }
        }
    }

    private int MenuCapacity
    {
        get
        {
            if (this._menuCapacity.Value.HasValue)
            {
                return this._menuCapacity.Value ?? default;
            }

            if (this.Menu?.IsPlayerChestMenu(out var chest) != true || !this.ManagedChests.FindChest(chest, out var managedChest))
            {
                return this._menuCapacity.Value ??= -1; // Vanilla
            }

            var capacity = Math.Max(chest.items.Count(item => item is not null).RoundUp(12), chest.GetActualCapacity());
            return this._menuCapacity.Value ??= capacity switch
            {
                Chest.capacity => -1, // Vanilla
                < 72 => Math.Min(managedChest.ResizeChestMenuRows * 12, capacity.RoundUp(12)), // Variable
                _ => managedChest.ResizeChestMenuRows * 12, // Large
            };
        }
    }

    private int MenuOffset
    {
        get
        {
            return this._menuOffset.Value ??= Game1.tileSize * (this.MenuRows - 3);
        }
    }

    private int MenuRows
    {
        get
        {
            if (this._menuRows.Value.HasValue)
            {
                return this._menuRows.Value ?? default;
            }

            if (this.Menu?.IsPlayerChestMenu(out var chest) != true || !this.ManagedChests.FindChest(chest, out var managedChest))
            {
                return this._menuRows.Value ??= 3; // Vanilla
            }

            var capacity = Math.Max(chest.items.Count(item => item is not null).RoundUp(12), chest.GetActualCapacity());
            return this._menuRows.Value ??= capacity switch
            {
                < 72 => (int)Math.Min(managedChest.ResizeChestMenuRows, Math.Ceiling(capacity / 12f)), // Variable
                _ => managedChest.ResizeChestMenuRows, // Large
            };
        }
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.HarmonyHelper.ApplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.HarmonyHelper.UnapplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    private static int GetMenuCapacity(MenuWithInventory menu)
    {
        ResizeChestMenu.Instance.Menu = menu as ItemGrabMenu;
        return ResizeChestMenu.Instance.MenuCapacity;
    }

    private static int GetMenuOffset(MenuWithInventory menu)
    {
        ResizeChestMenu.Instance.Menu = menu as ItemGrabMenu;
        return ResizeChestMenu.Instance.MenuOffset;
    }

    private static int GetMenuRows(MenuWithInventory menu)
    {
        ResizeChestMenu.Instance.Menu = menu as ItemGrabMenu;
        return ResizeChestMenu.Instance.MenuRows;
    }

    /// <summary>Generate additional slots/rows for top inventory menu.</summary>
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
    private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(ItemGrabMenu)}.ctor");
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
                code.Add(new(OpCodes.Bge_S, top?.operand));
            },
            new(OpCodes.Isinst, typeof(Chest)),
            new(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))),
            new(OpCodes.Ldc_I4_S, (sbyte)36),
            new(OpCodes.Beq_S));

        // Original:
        //      this.ItemsToGrabMenu = new InventoryMenu(base.xPositionOnScreen + 32, base.yPositionOnScreen, false, inventory, highlightFunction, -1, 3, 0, 0, true);
        // Patched:
        //      this.ItemsToGrabMenu = new InventoryMenu(base.xPositionOnScreen + 32, base.yPositionOnScreen, false, inventory, highlightFunction, ResizeChestMenu.GetMenuCapacity(), ResizeChestMenu.GetMenuRows(), 0, 0, true);
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
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuCapacity))));
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuRows))));
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
        Log.Trace($"Applying patches to {nameof(ItemGrabMenu)}.{nameof(ItemGrabMenu.draw)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Draw Backpack Patch
        // This adds ResizeChestMenu.GetMenuOffset() to the y-coordinate of the backpack sprite
        patcher.AddSeek(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))));
        patcher.AddPatch(
                   code =>
                   {
                       Log.Trace("Moving backpack icon down by expanded menu extra height.", true);
                       code.Add(new(OpCodes.Ldarg_0));
                       code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuOffset))));
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
        Log.Trace($"Applying patches to {nameof(MenuWithInventory)}.{nameof(MenuWithInventory.draw)}", true);
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Move Dialogue Patch
        // This adds ResizeChestMenu.GetMenuOffset() to the y-coordinate of the inventory dialogue
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Moving bottom dialogue box down by expanded menu height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuOffset))));
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
        // This subtracts ResizeChestMenu.GetMenuOffset() from the height of the inventory dialogue
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Shrinking bottom dialogue box height by expanded menu height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuOffset))));
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

    [SortedEventPriority(EventPriority.High)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu?.IsPlayerChestMenu(out _) == true
            ? e.ItemGrabMenu
            : null;

        if (this.Menu is null)
        {
            return;
        }

        if (e.IsNew && this.MenuOffset != 0)
        {
            Log.Trace($"Resizing Chest Menu for Chest {e.Chest.Name}");

            // Shift components down for increased ItemsToGrabMenu size
            this.Menu.height += this.MenuOffset;
            this.Menu.inventory.movePosition(0, this.MenuOffset);
            if (this.Menu.okButton is not null)
            {
                this.Menu.okButton.bounds.Y += this.MenuOffset;
            }

            if (this.Menu.trashCan is not null)
            {
                this.Menu.trashCan.bounds.Y += this.MenuOffset;
            }

            if (this.Menu.dropItemInvisibleButton is not null)
            {
                this.Menu.dropItemInvisibleButton.bounds.Y += this.MenuOffset;
            }

            // Set upNeighborId for first row of player inventory
            var slot = this.Menu.ItemsToGrabMenu.capacity - this.Menu.ItemsToGrabMenu.capacity / this.Menu.ItemsToGrabMenu.rows;
            for (var index = 0; index < 12; index++)
            {
                this.Menu.inventory.inventory[index].upNeighborID = this.Menu.ItemsToGrabMenu.inventory[slot + index].myID;
            }
        }
    }
}