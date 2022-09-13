namespace StardewMods.StackQuality;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.AtraBase.StringHandlers;
using StardewMods.StackQuality.Helpers;
using StardewMods.StackQuality.UI;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
public class StackQuality : Mod
{
    private static StackQuality? Instance;

    /// <summary>
    ///     Sets the currently held item for the active menu.
    /// </summary>
    internal static Item? HeldItem
    {
        set
        {
            switch (Game1.activeClickableMenu)
            {
                case GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage:
                    Game1.player.CursorSlotItem = value;
                    return;
                case JunimoNoteMenu junimoNoteMenu:
                    StackQuality.Instance!.Helper.Reflection.GetField<Item?>(junimoNoteMenu, "heldItem")
                                .SetValue(value);
                    return;
                case MenuWithInventory menuWithInventory:
                    menuWithInventory.heldItem = value;
                    return;
                case ShopMenu shopMenu:
                    shopMenu.heldItem = value;
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
                case GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage inventoryPage:
                    StackQuality.Instance!.Helper.Reflection.GetField<Item?>(inventoryPage, "hoveredItem")
                                .SetValue(value);
                    return;
                case JunimoNoteMenu junimoNoteMenu:
                    StackQuality.Instance!.Helper.Reflection.GetField<Item?>(junimoNoteMenu, "hoveredItem")
                                .SetValue(value);
                    return;
                case MenuWithInventory menuWithInventory:
                    menuWithInventory.hoveredItem = value;
                    return;
                case ShopMenu shopMenu:
                    shopMenu.hoveredItem = value;
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
                case GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage inventoryPage:
                    StackQuality.Instance!.Helper.Reflection.GetField<string?>(inventoryPage, "hoverText")
                                .SetValue(value);
                    return;
                case JunimoNoteMenu:
                    JunimoNoteMenu.hoverText = value;
                    return;
                case MenuWithInventory menuWithInventory:
                    menuWithInventory.hoverText = value;
                    return;
                case ShopMenu shopMenu:
                    StackQuality.Instance!.Helper.Reflection.GetField<string?>(shopMenu, "hoverText")
                                .SetValue(value ?? string.Empty);
                    return;
            }
        }
    }

    private static bool IsSupported =>
        Game1.activeClickableMenu is JunimoNoteMenu or MenuWithInventory or ShopMenu
     || (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.pages[gameMenu.currentTab] is InventoryPage);

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        StackQuality.Instance = this;
        Log.Monitor = this.Monitor;

        // Events
        this.Helper.Events.Display.RenderedActiveMenu += StackQuality.OnRenderedActiveMenu;

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(
                typeof(Farmer),
                nameof(Farmer.addItemToInventory),
                new[]
                {
                    typeof(Item),
                    typeof(List<Item>),
                }),
            new(typeof(StackQuality), nameof(StackQuality.Farmer_addItemToInventory_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Farmer), nameof(Farmer.removeItemsFromInventory)),
            new(typeof(StackQuality), nameof(StackQuality.Farmer_removeItemsFromInventory_prefix)));
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
            AccessTools.Method(typeof(Item), nameof(Item.GetContextTags)),
            postfix: new(typeof(StackQuality), nameof(StackQuality.Item_GetContextTags_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.addToStack)),
            new(typeof(StackQuality), nameof(StackQuality.Object_addToStack_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.getOne)),
            postfix: new(typeof(StackQuality), nameof(StackQuality.Object_getOne_postfix)));
        harmony.Patch(
            AccessTools.PropertyGetter(typeof(SObject), nameof(SObject.Stack)),
            postfix: new(typeof(StackQuality), nameof(StackQuality.Object_StackGetter_postfix)));
        harmony.Patch(
            AccessTools.PropertySetter(typeof(SObject), nameof(SObject.Stack)),
            postfix: new(typeof(StackQuality), nameof(StackQuality.Object_StackSetter_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.addItemToInventory)),
            postfix: new(typeof(StackQuality), nameof(StackQuality.Utility_addItemToInventory_postfix)));
    }

    /// <inheritdoc/>
    public override object GetApi()
    {
        return new StackQualityApi();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Farmer_addItemToInventory_prefix(
        Farmer __instance,
        ref Item? __result,
        Item? item,
        List<Item>? affected_items_list)
    {
        if (item is not SObject obj || item.maximumStackSize() == 1)
        {
            return true;
        }

        // Stack to existing item slot(s)
        for (var i = 0; i < __instance.MaxItems; ++i)
        {
            var slot = __instance.Items.ElementAtOrDefault(i);
            if (slot is not SObject other || !other.canStackWith(obj))
            {
                continue;
            }

            var stack = other.addToStack(obj);
            affected_items_list?.Add(slot);
            if (stack <= 0)
            {
                return false;
            }
        }

        // Add to empty item slot
        for (var i = 0; i < __instance.MaxItems; ++i)
        {
            if (__instance.Items.ElementAtOrDefault(i) is not null)
            {
                continue;
            }

            __instance.Items[i] = item;
            affected_items_list?.Add(item);
            return false;
        }

        __result = item;
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Farmer_removeItemsFromInventory_prefix(
        Farmer __instance,
        ref bool __result,
        int index,
        int stack)
    {
        for (var i = 0; i < __instance.MaxItems; ++i)
        {
            var item = __instance.Items.ElementAtOrDefault(i);
            if (item is not SObject obj || obj.ParentSheetIndex != index)
            {
                continue;
            }

            var stacks = obj.GetStacks();
            for (var j = 0; j < 4; ++j)
            {
                if (stacks[j] > stack)
                {
                    stacks[j] -= stack;
                    obj.UpdateQuality(stacks);
                    if (obj.Stack == 0)
                    {
                        __instance.Items[i] = null;
                    }

                    __result = true;
                    return false;
                }

                stack -= stacks[j];
                stacks[j] = 0;

                if (stack != 0)
                {
                    continue;
                }

                obj.UpdateQuality(stacks);
                if (obj.Stack == 0)
                {
                    __instance.Items[i] = null;
                }

                __result = true;
                return false;
            }
        }

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool InventoryMenu_leftClick_prefix(
        InventoryMenu __instance,
        ref Item? __result,
        int x,
        int y,
        Item? toPlace,
        bool playSound)
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

        var slotNumber = int.Parse(component.name);
        var slot = __instance.actualInventory.ElementAtOrDefault(slotNumber);
        if (slot is not SObject obj)
        {
            return true;
        }

        if (obj.GetStacks().Any(stack => stack == obj.Stack))
        {
            return true;
        }

        if (StackQuality.Instance!.Helper.Input.IsDown(SButton.LeftShift))
        {
            if (playSound)
            {
                Game1.playSound(__instance.moveItemSound);
            }

            __result = Utility.removeItemFromInventory(slotNumber, __instance.actualInventory);
            return false;
        }

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
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool InventoryMenu_rightClick_prefix(
        InventoryMenu __instance,
        ref Item? __result,
        int x,
        int y,
        Item? toAddTo,
        bool playSound,
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

        var slotNumber = int.Parse(component.name);
        var slot = __instance.actualInventory.ElementAtOrDefault(slotNumber);
        if (slot is not SObject obj)
        {
            return true;
        }

        var take = 1;
        if (StackQuality.Instance!.Helper.Input.IsDown(SButton.LeftShift))
        {
            var stacks = obj.GetStacks();
            take = stacks.FirstOrDefault(stack => stack > 0);
        }

        if (!obj.SplitStacks(ref toAddTo, take))
        {
            return true;
        }

        __result = toAddTo;
        if (obj.Stack == 0)
        {
            __instance.actualInventory[slotNumber] = null;
        }

        if (playSound)
        {
            Game1.playSound(__instance.moveItemSound);
        }

        return false;
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
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_GetContextTags_postfix(Item __instance, ref HashSet<string> __result)
    {
        if (__instance is not SObject obj)
        {
            return;
        }

        var stacks = obj.GetStacks();
        for (var i = 0; i < 4; ++i)
        {
            var tag = Common.IndexToContextTag(i);
            if (stacks[i] == 0)
            {
                __result.Remove(tag);
                continue;
            }

            __result.Add(tag);
        }
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
        var stack = stacks.Sum();
        if (stack < maxStack)
        {
            for (var i = 0; i < 4; ++i)
            {
                var add = Math.Min(Math.Min(maxStack - stack, otherStacks[i]), maxStack - stacks[i]);
                stack += add;
                stacks[i] += add;
                otherStacks[i] -= add;
                if (stack >= maxStack)
                {
                    break;
                }
            }
        }

        __instance.UpdateQuality(stacks);
        other.UpdateQuality(otherStacks);
        __result = other.Stack;
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_getOne_postfix(ref Item __result)
    {
        if (__result is not SObject obj)
        {
            return;
        }

        var stacks = obj.GetStacks();
        var newStacks = new int[4];
        for (var i = 0; i < 4; ++i)
        {
            if (stacks[i] == 0)
            {
                continue;
            }

            newStacks[i] = 1;
            break;
        }

        obj.UpdateQuality(newStacks);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_StackGetter_postfix(SObject __instance, ref int __result)
    {
        if (!__instance.modData.TryGetValue("furyx639.StackQuality/qualities", out var qualities)
         || string.IsNullOrWhiteSpace(qualities))
        {
            return;
        }

        var qualitiesSpan = new StreamSplit(qualities);
        __result = 0;
        foreach (var quality in qualitiesSpan)
        {
            __result += int.Parse(quality);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_StackSetter_postfix(SObject __instance, int value)
    {
        if (!__instance.modData.ContainsKey("furyx639.StackQuality/qualities"))
        {
            return;
        }

        var stacks = __instance.GetStacks();
        var stack = stacks.Sum();
        var delta = value - stack;
        switch (delta)
        {
            case 0:
                break;

            case < 0:
                for (var i = 0; i < 4; ++i)
                {
                    if (stacks[i] > -delta)
                    {
                        stacks[i] += delta;
                        break;
                    }

                    delta += stacks[i];
                    stacks[i] = 0;

                    if (delta == 0)
                    {
                        break;
                    }
                }

                break;

            case > 0:
                var maxStack = __instance.maximumStackSize();
                for (var i = 0; i < 4; ++i)
                {
                    var add = Math.Min(Math.Min(maxStack - stack, delta), maxStack - stacks[i]);
                    stack += add;
                    stacks[i] += add;
                    delta -= add;
                    if (delta <= 0)
                    {
                        break;
                    }
                }

                break;
        }

        __instance.UpdateQuality(stacks, false);
    }

    [EventPriority(EventPriority.Low)]
    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!StackQuality.IsSupported
         || Game1.activeClickableMenu.GetChildMenu() is not ItemQualityMenu itemQualityMenu)
        {
            return;
        }

        itemQualityMenu.Draw(e.SpriteBatch);
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