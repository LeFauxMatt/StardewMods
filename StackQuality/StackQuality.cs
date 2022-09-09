namespace StardewMods.StackQuality;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewMods.Common.Helpers;
using StardewMods.StackQuality.Helpers;
using StardewMods.StackQuality.UI;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
public class StackQuality : Mod
{
    private static StackQuality? Instance;

    /// <summary>
    ///     Gets or sets the currently held item for the active menu.
    /// </summary>
    internal static Item? HeldItem
    {
        get => Game1.activeClickableMenu switch
        {
            ItemGrabMenu itemGrabMenu => itemGrabMenu.heldItem,
            GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage => Game1.player.CursorSlotItem,
            _ => null,
        };
        set
        {
            switch (Game1.activeClickableMenu)
            {
                case ItemGrabMenu itemGrabMenu:
                    itemGrabMenu.heldItem = value;
                    return;
                case GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage:
                    Game1.player.CursorSlotItem = value;
                    return;
            }
        }
    }

    private static Item? HoveredItem
    {
        set
        {
            switch (Game1.activeClickableMenu)
            {
                case ItemGrabMenu itemGrabMenu:
                    itemGrabMenu.hoveredItem = value;
                    return;
                case GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage inventoryPage:
                    StackQuality.Instance!.Helper.Reflection.GetField<Item?>(inventoryPage, "hoveredItem")
                                .SetValue(value);
                    return;
            }
        }
    }

    private static string? HoverText
    {
        set
        {
            switch (Game1.activeClickableMenu)
            {
                case ItemGrabMenu itemGrabMenu:
                    itemGrabMenu.hoverText = value;
                    return;
                case GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage inventoryPage:
                    StackQuality.Instance!.Helper.Reflection.GetField<string?>(inventoryPage, "hoverText")
                                .SetValue(value);
                    return;
            }
        }
    }

    private static bool IsSupported => Game1.activeClickableMenu is ItemGrabMenu
                                    || (Game1.activeClickableMenu is GameMenu gameMenu
                                     && gameMenu.pages[gameMenu.currentTab] is InventoryPage);

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        StackQuality.Instance = this;
        Log.Monitor = this.Monitor;

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.leftClick)),
            new(typeof(StackQuality), nameof(StackQuality.InventoryMenu_leftClick_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
            new(typeof(StackQuality), nameof(StackQuality.InventoryMenu_rightClick_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
            postfix: new(typeof(StackQuality), nameof(StackQuality.Item_canStackWith_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.addToStack)),
            new(typeof(StackQuality), nameof(StackQuality.Object_addToStack_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.addItemToInventory)),
            postfix: new(typeof(StackQuality), nameof(StackQuality.Utility_addItemToInventory_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool InventoryMenu_leftClick_prefix(InventoryMenu __instance, int x, int y, Item? toPlace)
    {
        if (!StackQuality.IsSupported || toPlace is not null)
        {
            return true;
        }

        var component = __instance.inventory.FirstOrDefault(cc => cc.containsPoint(x, y));
        if (component is null)
        {
            return true;
        }

        var slotNumber = Convert.ToInt32(component.name);
        var slot = __instance.actualInventory.ElementAtOrDefault(slotNumber);
        if (slot is not SObject obj)
        {
            return true;
        }

        if (obj.GetStacks().Any(stack => stack == obj.Stack))
        {
            return true;
        }

        // TODO: Test if object supports quality
        var overlay = new ItemQualityMenu(
            obj,
            component.bounds.X - Game1.tileSize / 2,
            component.bounds.Y - Game1.tileSize / 2)
        {
            exitFunction = () =>
            {
                if (slot.Stack == 0)
                {
                    __instance.actualInventory[slotNumber] = null;
                }
            },
        };

        StackQuality.HoveredItem = null;
        StackQuality.HoverText = null;
        Game1.activeClickableMenu.SetChildMenu(overlay);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool InventoryMenu_rightClick_prefix(
        InventoryMenu __instance,
        ref Item __result,
        int x,
        int y,
        Item? toAddTo,
        bool onlyCheckToolAttachments)
    {
        if (!StackQuality.IsSupported || onlyCheckToolAttachments)
        {
            return true;
        }

        var component = __instance.inventory.FirstOrDefault(cc => cc.containsPoint(x, y));
        if (component is null)
        {
            return true;
        }

        var slotNumber = Convert.ToInt32(component.name);
        var slot = __instance.actualInventory.ElementAtOrDefault(slotNumber);
        if (slot is not SObject obj || toAddTo?.canStackWith(obj) == false)
        {
            return true;
        }

        var take = StackQuality.Instance!.Helper.Input.IsDown(SButton.LeftShift) ? obj.Stack / 2 : 1;
        var stacks = obj.GetStacks();

        switch (toAddTo)
        {
            case SObject other:
                var otherStacks = other.GetStacks();
                if (take == 1)
                {
                    for (var i = 0; i < 4; ++i)
                    {
                        if (stacks[i] == 0)
                        {
                            continue;
                        }

                        stacks[i]--;
                        otherStacks[i]++;
                        break;
                    }
                }
                else
                {
                    var stack = 0;
                    for (var i = 0; i < 4; ++i)
                    {
                        stack += stacks[i];
                        if (stack > take)
                        {
                            var over = stack - take;
                            otherStacks[i] = stacks[i] - over;
                            stacks[i] = over;
                            break;
                        }

                        otherStacks[i] = stacks[i];
                        stacks[i] = 0;
                    }
                }

                obj.UpdateQuality(stacks);
                other.UpdateQuality(otherStacks);
                if (obj.Stack == 0)
                {
                    __instance.actualInventory[slotNumber] = null;
                }

                __result = other.getOne();
                __result.Stack = other.Stack;
                return false;

            case null:
                var newStacks = new int[4];
                var item = (SObject)obj.getOne();
                if (take == 1)
                {
                    for (var i = 0; i < 4; ++i)
                    {
                        if (stacks[i] == 0)
                        {
                            continue;
                        }

                        stacks[i]--;
                        newStacks[i]++;
                        break;
                    }
                }
                else
                {
                    var stack = 0;
                    for (var i = 0; i < 4; ++i)
                    {
                        stack += stacks[i];
                        if (stack > take)
                        {
                            var over = stack - take;
                            newStacks[i] = stacks[i] - over;
                            stacks[i] = over;
                            break;
                        }

                        newStacks[i] = stacks[i];
                        stacks[i] = 0;
                    }
                }

                obj.UpdateQuality(stacks);
                item.UpdateQuality(newStacks);
                if (obj.Stack == 0)
                {
                    __instance.actualInventory[slotNumber] = null;
                }

                __result = item.getOne();
                __result.Stack = item.Stack;
                return false;

            default:
                return true;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable? other)
    {
        if (__result
         || __instance is not SObject obj
         || other is not SObject otherObj
         || !__instance.Name.Equals(other.Name)
         || __instance.maximumStackSize() == 1
         || other.maximumStackSize() == 1
         || obj.orderData.Value != otherObj.orderData.Value
         || obj.ParentSheetIndex != otherObj.ParentSheetIndex
         || obj.bigCraftable.Value != otherObj.bigCraftable.Value
         || (__instance is ColoredObject && other is not ColoredObject)
         || (__instance is not ColoredObject && other is ColoredObject)
         || (__instance is ColoredObject c1 && other is ColoredObject c2 && !c1.color.Value.Equals(c2.color.Value)))
        {
            return;
        }

        __result = true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_addToStack_prefix(SObject __instance, ref int __result, Item otherStack)
    {
        if (otherStack is not SObject other)
        {
            return true;
        }

        var maxStack = __instance.maximumStackSize();
        if (maxStack == 1)
        {
            return true;
        }

        if (__instance.IsSpawnedObject && !other.IsSpawnedObject)
        {
            __instance.IsSpawnedObject = false;
        }

        var stacks = __instance.GetStacks();
        var otherStacks = other.GetStacks();
        for (var i = 0; i < 4; ++i)
        {
            __instance.Stack += otherStacks[i];
            if (__instance.Stack > maxStack)
            {
                __result = __instance.Stack - maxStack;
                stacks[i] += otherStacks[i] - __result;
                otherStacks[i] = __result;
                __instance.UpdateQuality(stacks);
                other.UpdateQuality(otherStacks);
                return false;
            }

            stacks[i] += otherStacks[i];
            otherStacks[i] = 0;
        }

        __instance.UpdateQuality(stacks);
        other.UpdateQuality(otherStacks);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Local", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Utility_addItemToInventory_postfix(
        ref Item? __result,
        Item item,
        int position,
        IList<Item> items,
        ItemGrabMenu.behaviorOnItemSelect? onAddFunction)
    {
        if (__result is not SObject obj
         || items.ElementAtOrDefault(position) is not SObject otherObj
         || !obj.canStackWith(otherObj))
        {
            return;
        }

        Utility.checkItemFirstInventoryAdd(item);
        var stackLeft = otherObj.addToStack(obj);
        if (stackLeft <= 0)
        {
            __result = null;
            return;
        }

        item.Stack = stackLeft;
        onAddFunction?.Invoke(item, null);
        __result = item;
    }
}