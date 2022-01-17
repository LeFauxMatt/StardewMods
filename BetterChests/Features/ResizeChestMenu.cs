namespace BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using Common.Extensions;
using Common.Helpers;
using CommonHarmony;
using Extensions;
using FuryCore.Attributes;
using FuryCore.Enums;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChestMenu : Feature
{
    private readonly PerScreen<Chest> _chest = new();
    private readonly PerScreen<ItemsDisplayedEventArgs> _displayedItems = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly Lazy<HarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeChestMenu"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public ResizeChestMenu(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        ResizeChestMenu.Instance = this;
        this._harmony = services.Lazy<HarmonyHelper>(ResizeChestMenu.AddPatches);
    }

    private static ResizeChestMenu Instance { get; set; }

    private HarmonyHelper HarmonyHelper
    {
        get => this._harmony.Value;
    }

    private Chest Chest
    {
        get => this._chest.Value;
        set => this._chest.Value = value;
    }

    private ItemsDisplayedEventArgs DisplayedItems
    {
        get => this._displayedItems.Value;
        set => this._displayedItems.Value = value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.HarmonyHelper.ApplyPatches(nameof(ResizeChestMenu));
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.ItemsDisplayed += this.OnItemsDisplayed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.HarmonyHelper.UnapplyPatches(nameof(ResizeChestMenu));
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.ItemsDisplayed -= this.OnItemsDisplayed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
    }

    private static void AddPatches(HarmonyHelper harmony)
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
            nameof(ResizeChestMenu),
            new SavedPatch[]
            {
                new(
                    AccessTools.Constructor(typeof(ItemGrabMenu), ctorItemGrabMenu),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.ItemGrabMenu_constructor_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
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

    /// <summary>Generate additional slots/rows for top inventory menu.</summary>
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
    private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace("Changing jump condition from Beq 36 to Bge 10.");
        var jumpPatch = new PatternPatch();
        jumpPatch
            .Find(
                new[]
                {
                    new CodeInstruction(OpCodes.Isinst, typeof(Chest)), new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))), new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)36), new CodeInstruction(OpCodes.Beq_S),
                })
            .Patch(
                delegate(LinkedList<CodeInstruction> list)
                {
                    var jumpCode = list.Last?.Value;
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)10));
                    list.AddLast(new CodeInstruction(OpCodes.Bge_S, jumpCode?.operand));
                });

        Log.Trace("Overriding default values for capacity and rows.");
        var capacityPatch = new PatternPatch();
        capacityPatch
            .Find(
                new[]
                {
                    new CodeInstruction(
                        OpCodes.Newobj,
                        AccessTools.Constructor(
                            typeof(InventoryMenu),
                            new[]
                            {
                                typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool),
                            })),
                    new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                })
            .Find(
                new[]
                {
                    new CodeInstruction(OpCodes.Ldc_I4_M1), new CodeInstruction(OpCodes.Ldc_I4_3),
                })
            .Patch(
                delegate(LinkedList<CodeInstruction> list)
                {
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuCapacity))));
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuRows))));
                });

        var patternPatches = new PatternPatches(instructions);
        patternPatches.AddPatch(jumpPatch);
        patternPatches.AddPatch(capacityPatch);

        foreach (var patternPatch in patternPatches)
        {
            yield return patternPatch;
        }

        if (!patternPatches.Done)
        {
            Log.Warn($"Failed to apply all patches in {typeof(ResizeChestMenu)}::{nameof(ResizeChestMenu.ItemGrabMenu_constructor_transpiler)}");
        }
    }

    /// <summary>Move/resize backpack by expanded menu height.</summary>
    private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace("Moving backpack icon down by expanded menu extra height.");
        var moveBackpackPatch = new PatternPatch();
        moveBackpackPatch
            .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
            .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
            .Patch(
                delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                })
            .Repeat(3);

        var patternPatches = new PatternPatches(instructions, moveBackpackPatch);

        foreach (var patternPatch in patternPatches)
        {
            yield return patternPatch;
        }

        if (!patternPatches.Done)
        {
            Log.Warn($"Failed to apply all patches in {typeof(ItemGrabMenu)}::{nameof(ItemGrabMenu.draw)}.");
        }
    }

    /// <summary>Move/resize bottom dialogue box by search bar height.</summary>
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
    private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace("Moving bottom dialogue box down by expanded menu height.");
        var moveDialogueBoxPatch = new PatternPatch();
        moveDialogueBoxPatch
            .Find(
                new[]
                {
                    new CodeInstruction(
                        OpCodes.Ldfld,
                        AccessTools.Field(
                            typeof(IClickableMenu),
                            nameof(IClickableMenu.yPositionOnScreen))),
                    new CodeInstruction(
                        OpCodes.Ldsfld,
                        AccessTools.Field(
                            typeof(IClickableMenu),
                            nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(
                        OpCodes.Ldsfld,
                        AccessTools.Field(
                            typeof(IClickableMenu),
                            nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(
                        OpCodes.Ldc_I4_S,
                        (sbyte)64),
                    new CodeInstruction(OpCodes.Add),
                })
            .Patch(
                list =>
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

        Log.Trace("Shrinking bottom dialogue box height by expanded menu height.");
        var resizeDialogueBoxPatch = new PatternPatch();
        resizeDialogueBoxPatch
            .Find(
                new[]
                {
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))), new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))), new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))), new CodeInstruction(OpCodes.Add), new CodeInstruction(OpCodes.Ldc_I4, 192), new CodeInstruction(OpCodes.Add),
                })
            .Patch(
                list =>
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenu), nameof(ResizeChestMenu.GetMenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

        var patternPatches = new PatternPatches(instructions);
        patternPatches.AddPatch(moveDialogueBoxPatch);
        patternPatches.AddPatch(resizeDialogueBoxPatch);

        foreach (var patternPatch in patternPatches)
        {
            yield return patternPatch;
        }

        if (!patternPatches.Done)
        {
            Log.Warn($"Failed to apply all patches in {typeof(MenuWithInventory)}::{nameof(MenuWithInventory.draw)}.");
        }
    }

    private static int GetMenuCapacity(MenuWithInventory menu)
    {
        return ResizeChestMenu.Instance.MenuCapacity(menu);
    }

    private static int GetMenuRows(MenuWithInventory menu)
    {
        return ResizeChestMenu.Instance.MenuRows(menu);
    }

    private static int GetMenuOffset(MenuWithInventory menu)
    {
        return ResizeChestMenu.Instance.MenuOffset(menu);
    }

    private int MenuCapacity(MenuWithInventory menu)
    {
        if (!menu.IsPlayerChestMenu(out var chest))
        {
            return -1; // Vanilla
        }

        if (!this.ManagedChests.FindChest(chest, out var managedChest) || managedChest.Config.Capacity == 0)
        {
            return -1;
        }

        return managedChest.Config.Capacity switch
        {
            < 72 => Math.Min(this.Config.MenuRows * 12, managedChest.Config.Capacity.RoundUp(12)), // Variable
            _ => this.Config.MenuRows * 12, // Large
        };
    }

    private int MenuRows(MenuWithInventory menu)
    {
        if (!menu.IsPlayerChestMenu(out var chest))
        {
            return 3; // Vanilla
        }

        if (!this.ManagedChests.FindChest(chest, out var managedChest) || managedChest.Config.Capacity == 0)
        {
            return 3;
        }

        return managedChest.Config.Capacity switch
        {
            < 72 => (int)Math.Min(this.Config.MenuRows, Math.Ceiling(managedChest.Config.Capacity / 12f)),
            _ => this.Config.MenuRows,
        };
    }

    private int MenuOffset(MenuWithInventory menu)
    {
        if (!menu.IsPlayerChestMenu(out var chest))
        {
            return 0; // Vanilla
        }

        if (!this.ManagedChests.FindChest(chest, out var managedChest) || managedChest.Config.Capacity == 0)
        {
            return 0;
        }

        var rows = this.MenuRows(menu);
        return Game1.tileSize * (rows - 3);
    }

    [SortedEventPriority(EventPriority.High)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu;
        this.Chest = e.Chest;
        if (this.Menu?.IsPlayerChestMenu(out _) != true || !e.IsNew || !this.ManagedChests.FindChest(this.Chest, out var managedChest) || managedChest.Config.Capacity == 0)
        {
            return;
        }

        var offset = this.MenuOffset(e.ItemGrabMenu);
        e.ItemGrabMenu.height += offset;
        e.ItemGrabMenu.inventory.movePosition(0, offset);
        if (e.ItemGrabMenu.okButton is not null)
        {
            e.ItemGrabMenu.okButton.bounds.Y += offset;
        }

        if (e.ItemGrabMenu.trashCan is not null)
        {
            e.ItemGrabMenu.trashCan.bounds.Y += offset;
        }

        if (e.ItemGrabMenu.dropItemInvisibleButton is not null)
        {
            e.ItemGrabMenu.dropItemInvisibleButton.bounds.Y += offset;
        }

        e.ItemGrabMenu.RepositionSideButtons();
    }

    private void OnItemsDisplayed(object sender, ItemsDisplayedEventArgs e)
    {
        this.DisplayedItems = e;
    }

    private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
    {
        if (this.DisplayedItems is null || !ReferenceEquals(this.Menu, this.DisplayedItems.Menu))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.DisplayedItems.Offset--;
                break;
            case < 0:
                this.DisplayedItems.Offset++;
                break;
            default:
                return;
        }
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (this.Menu is null || this.DisplayedItems is null)
        {
            return;
        }

        if (this.Config.ScrollUp.JustPressed())
        {
            this.DisplayedItems.Offset--;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ScrollUp);
            return;
        }

        if (this.Config.ScrollDown.JustPressed())
        {
            this.DisplayedItems.Offset++;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ScrollDown);
        }
    }
}