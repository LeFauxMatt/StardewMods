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
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Adds additional rows to the <see cref="ItemGrabMenu" />.
/// </summary>
internal class ResizeChestMenu : IFeature
{
    private const string Id = "furyx639.BetterChests/ResizeChestMenu";

    private static ResizeChestMenu? Instance;

    private readonly PerScreen<object?> _context = new();
    private readonly PerScreen<int> _extraSpace = new();
    private readonly IModHelper _helper;
    private readonly PerScreen<IStorageObject?> _storage = new();

    private bool _isActivated;

    private ResizeChestMenu(IModHelper helper)
    {
        this._helper = helper;
        HarmonyHelper.AddPatches(
            ResizeChestMenu.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Constructor(
                        typeof(ItemGrabMenu),
                        new[]
                        {
                            typeof(IList<Item>),
                            typeof(bool),
                            typeof(bool),
                            typeof(InventoryMenu.highlightThisItem),
                            typeof(ItemGrabMenu.behaviorOnItemSelect),
                            typeof(string),
                            typeof(ItemGrabMenu.behaviorOnItemSelect),
                            typeof(bool),
                            typeof(bool),
                            typeof(bool),
                            typeof(bool),
                            typeof(bool),
                            typeof(int),
                            typeof(Item),
                            typeof(int),
                            typeof(object),
                        }),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.ItemGrabMenu_constructor_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.ItemGrabMenu_draw_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(
                        typeof(MenuWithInventory),
                        nameof(MenuWithInventory.draw),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(bool),
                            typeof(bool),
                            typeof(int),
                            typeof(int),
                            typeof(int),
                        }),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.MenuWithInventory_draw_transpiler),
                    PatchType.Transpiler),
            });
    }

    private static int ExtraSpace
    {
        get => ResizeChestMenu.Instance!._extraSpace.Value;
        set => ResizeChestMenu.Instance!._extraSpace.Value = value;
    }

    private object? Context
    {
        get => this._context.Value;
        set => this._context.Value = value;
    }

    private IStorageObject? Storage
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
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(ResizeChestMenu.Id);
        this._helper.Events.Display.MenuChanged += ResizeChestMenu.OnMenuChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(ResizeChestMenu.Id);
        this._helper.Events.Display.MenuChanged -= ResizeChestMenu.OnMenuChanged;
    }

    private static int GetExtraSpace(MenuWithInventory menu)
    {
        switch (menu)
        {
            case ItemGrabMenu { context: null } or not ItemGrabMenu:
                ResizeChestMenu.Instance!.Context = null;
                ResizeChestMenu.Instance.Storage = null;
                return 0;
            case ItemGrabMenu { context: { } context }
                when !ReferenceEquals(ResizeChestMenu.Instance!.Context, context):
                ResizeChestMenu.Instance.Context = context;
                ResizeChestMenu.Instance.Storage = StorageHelper.TryGetOne(context, out var storage) ? storage : null;
                break;
        }

        return ResizeChestMenu.Instance.Storage?.MenuExtraSpace ?? 0;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
    {
        ResizeChestMenu.ExtraSpace =
            __instance.context is not null && StorageHelper.TryGetOne(__instance.context, out var storage)
                ? storage.MenuExtraSpace
                : 0;

        if (ResizeChestMenu.ExtraSpace == 0)
        {
            return;
        }

        __instance.height += ResizeChestMenu.ExtraSpace;
        __instance.inventory.movePosition(0, ResizeChestMenu.ExtraSpace);
        if (__instance.okButton is not null)
        {
            __instance.okButton.bounds.Y += ResizeChestMenu.ExtraSpace;
        }

        if (__instance.trashCan is not null)
        {
            __instance.trashCan.bounds.Y += ResizeChestMenu.ExtraSpace;
        }

        if (__instance.dropItemInvisibleButton is not null)
        {
            __instance.dropItemInvisibleButton.bounds.Y += ResizeChestMenu.ExtraSpace;
        }
    }

    /// <summary>Move backpack down by expanded menu height.</summary>
    private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var patchCount = 0;

        foreach (var instruction in instructions)
        {
            if (instruction.LoadsField(AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
            {
                patchCount = 3;
                yield return instruction;
            }
            else if (patchCount > 0
                  && instruction.LoadsField(
                         AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.yPositionOnScreen))))
            {
                patchCount--;
                yield return instruction;
                yield return new(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(ResizeChestMenu), nameof(ResizeChestMenu.ExtraSpace)));
                yield return new(OpCodes.Add);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    /// <summary>Move/resize bottom dialogue box by search bar height.</summary>
    [SuppressMessage(
        "ReSharper",
        "HeapView.BoxingAllocation",
        Justification = "Boxing allocation is required for Harmony.")]
    private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsField(
                    AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))))
            {
                yield return instruction;
                yield return new(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(ResizeChestMenu), nameof(ResizeChestMenu.ExtraSpace)));
                yield return new(OpCodes.Add);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu
            {
                ItemsToGrabMenu.inventory: { } topRow, inventory.inventory: { } bottomRow,
            })
        {
            return;
        }

        // Set upNeighborId for first row of player inventory
        bottomRow = bottomRow.TakeLast(12).ToList();
        topRow = topRow.Take(12).ToList();
        for (var index = 0; index < 12; index++)
        {
            var bottomSlot = bottomRow.ElementAtOrDefault(index);
            var topSlot = topRow.ElementAtOrDefault(index);
            if (topSlot is null || bottomSlot is null)
            {
                continue;
            }

            bottomSlot.downNeighborID = topSlot.myID;
            topSlot.upNeighborID = bottomSlot.myID;
        }
    }
}