namespace BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using BetterChests.Models;
using Common.Extensions;
using Common.Helpers;
using Common.Helpers.PatternPatcher;
using FuryCore.Attributes;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChestMenu : Feature
{
    private readonly PerScreen<MenuWithInventory> _menuWithInventory = new();
    private readonly PerScreen<int?> _menuCapacity = new();
    private readonly PerScreen<int?> _menuRows = new();
    private readonly PerScreen<int?> _menuOffset = new();
    private readonly PerScreen<MenuComponent> _downArrow = new();
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly Lazy<IMenuComponents> _menuComponents;
    private readonly Lazy<IMenuItems> _menuItems;
    private readonly PerScreen<MenuComponent> _upArrow = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResizeChestMenu" /> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public ResizeChestMenu(ModConfig config, IModHelper helper, ServiceCollection services)
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

        this._menuComponents = services.Lazy<IMenuComponents>();
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static ResizeChestMenu Instance { get; set; }

    private IHarmonyHelper HarmonyHelper
    {
        get => this._harmony.Value;
    }

    private MenuWithInventory Menu
    {
        get => this._menuWithInventory.Value;
        set
        {
            if (!ReferenceEquals(this._menuWithInventory.Value, value))
            {
                this._menuWithInventory.Value = value;
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

            if (!this.Menu.IsPlayerChestMenu(out var chest))
            {
                return this._menuCapacity.Value ??= -1; // Vanilla
            }

            var capacity = Math.Max(chest.items.Count(item => item is not null).RoundUp(12), chest.GetActualCapacity());
            return this._menuCapacity.Value ??= capacity switch
            {
                Chest.capacity => -1, // Vanilla
                < 72 => Math.Min(this.Config.MenuRows * 12, capacity.RoundUp(12)), // Variable
                _ => this.Config.MenuRows * 12, // Large
            };
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

            if (!this.Menu.IsPlayerChestMenu(out var chest))
            {
                return this._menuRows.Value ??= 3; // Vanilla
            }

            var capacity = Math.Max(chest.items.Count(item => item is not null).RoundUp(12), chest.GetActualCapacity());
            return this._menuRows.Value ??= capacity switch
            {
                < 72 => (int)Math.Min(this.Config.MenuRows, Math.Ceiling(capacity / 12f)), // Variable
                _ => this.Config.MenuRows, // Large
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

    private IMenuComponents MenuComponents
    {
        get => this._menuComponents.Value;
    }

    private IMenuItems MenuItems
    {
        get => this._menuItems.Value;
    }

    private MenuComponent UpArrow
    {
        get => this._upArrow.Value ??= new(new(new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new(421, 459, 11, 12), Game1.pixelZoom));
    }

    private MenuComponent DownArrow
    {
        get => this._downArrow.Value ??= new(new(new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new(421, 472, 11, 12), Game1.pixelZoom));
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.HarmonyHelper.ApplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.HarmonyHelper.UnapplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
    }

    /// <summary>Generate additional slots/rows for top inventory menu.</summary>
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
    private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(ItemGrabMenu)}.ctor");
        var patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

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
        var patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

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
        var patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

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

    private static int GetMenuCapacity(MenuWithInventory menu)
    {
        ResizeChestMenu.Instance.Menu = menu;
        return ResizeChestMenu.Instance.MenuCapacity;
    }

    private static int GetMenuRows(MenuWithInventory menu)
    {
        ResizeChestMenu.Instance.Menu = menu;
        return ResizeChestMenu.Instance.MenuRows;
    }

    private static int GetMenuOffset(MenuWithInventory menu)
    {
        ResizeChestMenu.Instance.Menu = menu;
        return ResizeChestMenu.Instance.MenuOffset;
    }

    [SortedEventPriority(EventPriority.High)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (e.ItemGrabMenu?.IsPlayerChestMenu(out var chest) != true)
        {
            return;
        }

        this.Menu = e.ItemGrabMenu;
        if (e.IsNew && this.MenuOffset != 0)
        {
            // Shift components down for increased ItemsToGrabMenu size
            e.ItemGrabMenu.height += this.MenuOffset;
            e.ItemGrabMenu.inventory.movePosition(0, this.MenuOffset);
            if (e.ItemGrabMenu.okButton is not null)
            {
                e.ItemGrabMenu.okButton.bounds.Y += this.MenuOffset;
            }

            if (e.ItemGrabMenu.trashCan is not null)
            {
                e.ItemGrabMenu.trashCan.bounds.Y += this.MenuOffset;
            }

            if (e.ItemGrabMenu.dropItemInvisibleButton is not null)
            {
                e.ItemGrabMenu.dropItemInvisibleButton.bounds.Y += this.MenuOffset;
            }
        }

        if (this.MenuComponents.Menu is not null)
        {
            // Add Up/Down Arrows
            this.MenuComponents.Components.Add(this.UpArrow);
            this.MenuComponents.Components.Add(this.DownArrow);

            // Initialize Arrow visibility
            this.UpArrow.Component.visible = this.MenuItems.Offset > 0;
            this.DownArrow.Component.visible = this.MenuItems.Offset < this.MenuItems.Rows;

            // Align to ItemsToGrabMenu top/bottom inventory slots
            var topSlot = e.ItemGrabMenu.GetColumnCount() - 1;
            var bottomSlot = e.ItemGrabMenu.ItemsToGrabMenu.capacity - 1;
            this.UpArrow.Component.bounds.X = e.ItemGrabMenu.ItemsToGrabMenu.xPositionOnScreen + e.ItemGrabMenu.ItemsToGrabMenu.width + 8;
            this.UpArrow.Component.bounds.Y = e.ItemGrabMenu.ItemsToGrabMenu.inventory[topSlot].bounds.Center.Y - (6 * Game1.pixelZoom);
            this.DownArrow.Component.bounds.X = e.ItemGrabMenu.ItemsToGrabMenu.xPositionOnScreen + e.ItemGrabMenu.ItemsToGrabMenu.width + 8;
            this.DownArrow.Component.bounds.Y = e.ItemGrabMenu.ItemsToGrabMenu.inventory[bottomSlot].bounds.Center.Y - (6 * Game1.pixelZoom);

            // Assign Neighbor IDs
            this.UpArrow.Component.leftNeighborID = e.ItemGrabMenu.ItemsToGrabMenu.inventory[topSlot].myID;
            e.ItemGrabMenu.ItemsToGrabMenu.inventory[topSlot].rightNeighborID = this.UpArrow.Id;
            this.DownArrow.Component.leftNeighborID = e.ItemGrabMenu.ItemsToGrabMenu.inventory[bottomSlot].myID;
            e.ItemGrabMenu.ItemsToGrabMenu.inventory[bottomSlot].rightNeighborID = this.DownArrow.Id;
            this.UpArrow.Component.downNeighborID = this.DownArrow.Id;
            this.DownArrow.Component.upNeighborID = this.UpArrow.Id;
        }
    }

    private void OnMenuComponentPressed(object sender, MenuComponentPressedEventArgs e)
    {
        if (e.Component == this.UpArrow)
        {
            this.MenuItems.Offset--;
        }
        else if (e.Component == this.DownArrow)
        {
            this.MenuItems.Offset++;
        }
        else
        {
            return;
        }

        this.UpArrow.Component.visible = this.MenuItems.Offset > 0;
        this.DownArrow.Component.visible = this.MenuItems.Offset < this.MenuItems.Rows;
    }

    private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
    {
        if (this.MenuItems.Menu is null)
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.MenuItems.Offset--;
                break;
            case < 0:
                this.MenuItems.Offset++;
                break;
            default:
                return;
        }

        this.UpArrow.Component.visible = this.MenuItems.Offset > 0;
        this.DownArrow.Component.visible = this.MenuItems.Offset < this.MenuItems.Rows;
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (this.MenuItems.Menu is null)
        {
            return;
        }

        if (this.Config.ScrollUp.JustPressed())
        {
            this.MenuItems.Offset--;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ScrollUp);
            return;
        }

        if (this.Config.ScrollDown.JustPressed())
        {
            this.MenuItems.Offset++;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ScrollDown);
        }
    }
}