namespace StardewMods.BetterChests.Framework.Features;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Menus;

/// <summary>
///     Adds additional rows to the <see cref="ItemGrabMenu" />.
/// </summary>
internal sealed class ResizeChestMenu : IFeature
{
    private const string Id = "furyx639.BetterChests/ResizeChestMenu";

#nullable disable
    private static ResizeChestMenu Instance;
#nullable enable

    private readonly PerScreen<int> _extraSpace = new();
    private readonly IModHelper _helper;

    private bool _isActivated;

    private ResizeChestMenu(IModHelper helper)
    {
        this._helper = helper;
        HarmonyHelper.AddPatches(
            ResizeChestMenu.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(
                        typeof(ItemGrabMenu),
                        nameof(ItemGrabMenu.draw),
                        new[] { typeof(SpriteBatch) }),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.ItemGrabMenu_draw_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Constructor(
                        typeof(MenuWithInventory),
                        new[]
                        {
                            typeof(InventoryMenu.highlightThisItem),
                            typeof(bool),
                            typeof(bool),
                            typeof(int),
                            typeof(int),
                            typeof(int),
                        }),
                    typeof(ResizeChestMenu),
                    nameof(ResizeChestMenu.MenuWithInventory_constructor_postfix),
                    PatchType.Postfix),
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
        get => ResizeChestMenu.Instance._extraSpace.Value;
        set => ResizeChestMenu.Instance._extraSpace.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="ResizeChestMenu" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="ResizeChestMenu" /> class.</returns>
    public static IFeature Init(IModHelper helper)
    {
        return ResizeChestMenu.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void SetActivated(bool value)
    {
        if (this._isActivated == value)
        {
            return;
        }

        this._isActivated = value;
        if (this._isActivated)
        {
            HarmonyHelper.ApplyPatches(ResizeChestMenu.Id);
            BetterItemGrabMenu.Constructed += ResizeChestMenu.OnConstructed;
            this._helper.Events.Display.MenuChanged += ResizeChestMenu.OnMenuChanged;
            return;
        }

        HarmonyHelper.UnapplyPatches(ResizeChestMenu.Id);
        BetterItemGrabMenu.Constructed -= ResizeChestMenu.OnConstructed;
        this._helper.Events.Display.MenuChanged -= ResizeChestMenu.OnMenuChanged;
    }

    /// <summary>Move backpack down by expanded menu height.</summary>
    private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var patchCount = -1;

        foreach (var instruction in instructions)
        {
            yield return instruction;
            switch (patchCount)
            {
                case -1 when instruction.LoadsField(
                    AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))):
                    patchCount = 3;
                    break;
                case > 0 when instruction.LoadsField(
                    AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.yPositionOnScreen))):
                    --patchCount;
                    yield return new(
                        OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(ResizeChestMenu), nameof(ResizeChestMenu.ExtraSpace)));
                    yield return new(OpCodes.Add);
                    break;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void MenuWithInventory_constructor_postfix(MenuWithInventory __instance)
    {
        if (__instance is not ItemGrabMenu || BetterItemGrabMenu.Context is null)
        {
            ResizeChestMenu.ExtraSpace = 0;
        }
    }

    /// <summary>Move/resize bottom dialogue box by expanded menu height.</summary>
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

    private static void OnConstructed(object? sender, ItemGrabMenu itemGrabMenu)
    {
        if (BetterItemGrabMenu.Context is null)
        {
            ResizeChestMenu.ExtraSpace = 0;
            return;
        }

        ResizeChestMenu.ExtraSpace = (itemGrabMenu.ItemsToGrabMenu.rows - 3) * Game1.tileSize;
        itemGrabMenu.yPositionOnScreen -= ResizeChestMenu.ExtraSpace / 2;
        itemGrabMenu.height += ResizeChestMenu.ExtraSpace;
        itemGrabMenu.ItemsToGrabMenu.movePosition(0, -ResizeChestMenu.ExtraSpace / 2);
        itemGrabMenu.inventory.movePosition(0, ResizeChestMenu.ExtraSpace / 2);

        if (itemGrabMenu.chestColorPicker is not null)
        {
            itemGrabMenu.chestColorPicker.yPositionOnScreen -= ResizeChestMenu.ExtraSpace / 2;
        }

        if (itemGrabMenu.okButton is not null)
        {
            itemGrabMenu.okButton.bounds.Y += ResizeChestMenu.ExtraSpace / 2;
        }

        if (itemGrabMenu.trashCan is not null)
        {
            itemGrabMenu.trashCan.bounds.Y += ResizeChestMenu.ExtraSpace / 2;
        }

        if (itemGrabMenu.dropItemInvisibleButton is not null)
        {
            itemGrabMenu.dropItemInvisibleButton.bounds.Y += ResizeChestMenu.ExtraSpace / 2;
        }
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu
            {
                shippingBin: false, ItemsToGrabMenu.inventory: { } topRow, inventory.inventory: { } bottomRow,
            }
         || BetterItemGrabMenu.Context is null
         || BetterItemGrabMenu.Context.ResizeChestMenuRows == 3)
        {
            return;
        }

        // Set upNeighborId for first row of player inventory
        bottomRow = bottomRow.TakeLast(12).ToList();
        topRow = topRow.Take(12).ToList();
        for (var index = 0; index < 12; ++index)
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