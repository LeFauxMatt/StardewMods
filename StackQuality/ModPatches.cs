namespace StardewMods.StackQuality;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewMods.Common.Helpers.AtraBase.StringHandlers;
using StardewMods.StackQuality.Helpers;
using StardewMods.StackQuality.UI;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Harmony Patches for ShoppingCart.
/// </summary>
internal sealed class ModPatches
{
    private static ModPatches? Instance;

    private readonly IModHelper _helper;

    private ModPatches(IModHelper helper, IManifest manifest)
    {
        this._helper = helper;
        var harmony = new Harmony(manifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(
                typeof(Farmer),
                nameof(Farmer.addItemToInventory),
                new[]
                {
                    typeof(Item),
                    typeof(List<Item>),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Farmer_addItemToInventory_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Farmer), nameof(Farmer.removeItemsFromInventory)),
            new(typeof(ModPatches), nameof(ModPatches.Farmer_removeItemsFromInventory_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.leftClick)),
            new(typeof(ModPatches), nameof(ModPatches.InventoryMenu_leftClick_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
            new(typeof(ModPatches), nameof(ModPatches.InventoryMenu_rightClick_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Item_canStackWith_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.GetContextTags)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Item_GetContextTags_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.addToStack)),
            new(typeof(ModPatches), nameof(ModPatches.Object_addToStack_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.getOne)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_getOne_postfix)));
        harmony.Patch(
            AccessTools.PropertyGetter(typeof(SObject), nameof(SObject.Stack)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_StackGetter_postfix)));
        harmony.Patch(
            AccessTools.PropertySetter(typeof(SObject), nameof(SObject.Stack)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_StackSetter_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.addItemToInventory)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_addItemToInventory_postfix)));
    }

    private static Item? HoveredItem
    {
        set
        {
            switch (Game1.activeClickableMenu)
            {
                case GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage inventoryPage:
                    ModPatches.Reflection.GetField<Item?>(inventoryPage, "hoveredItem").SetValue(value);
                    return;
                case JunimoNoteMenu junimoNoteMenu:
                    ModPatches.Reflection.GetField<Item?>(junimoNoteMenu, "hoveredItem").SetValue(value);
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
                    ModPatches.Reflection.GetField<string?>(inventoryPage, "hoverText").SetValue(value);
                    return;
                case JunimoNoteMenu:
                    JunimoNoteMenu.hoverText = value;
                    return;
                case MenuWithInventory menuWithInventory:
                    menuWithInventory.hoverText = value;
                    return;
                case ShopMenu shopMenu:
                    ModPatches.Reflection.GetField<string?>(shopMenu, "hoverText").SetValue(value ?? string.Empty);
                    return;
            }
        }
    }

    private static IInputHelper Input => ModPatches.Instance!._helper.Input;

    private static IReflectionHelper Reflection => ModPatches.Instance!._helper.Reflection;

    /// <summary>
    ///     Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IModHelper helper, IManifest manifest)
    {
        return ModPatches.Instance ??= new(helper, manifest);
    }

    private static IClickableMenu.onExit ExitFunction(IList<Item> inventory, int slotNumber)
    {
        return () =>
        {
            if (inventory.ElementAtOrDefault(slotNumber)?.Stack == 0)
            {
                inventory[slotNumber] = null!;
            }
        };
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
        if (slot is not SObject obj || obj.GetStacks().Any(stack => stack == obj.Stack))
        {
            return true;
        }

        if (ModPatches.Input.IsDown(SButton.LeftShift))
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
            exitFunction = ModPatches.ExitFunction(__instance.actualInventory, slotNumber),
        };

        ModPatches.HoveredItem = null;
        ModPatches.HoverText = null;
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
        if (slot is not SObject obj || (toAddTo is null && obj.GetStacks().Any(stack => stack == obj.Stack)))
        {
            return true;
        }

        var take = new int[4];
        var existingStacks = new int[4];
        var stacks = obj.GetStacks();
        if (Integrations.ShoppingCart.IsLoaded)
        {
            if (Integrations.ShoppingCart.API.CurrentShop is not null)
            {
                foreach (var cartItem in Integrations.ShoppingCart.API.CurrentShop.ToSell)
                {
                    if (cartItem.Item is not SObject cartObj)
                    {
                        continue;
                    }

                    var cartStacks = cartObj.GetStacks();
                    for (var i = 0; i < 4; ++i)
                    {
                        existingStacks[i] += cartStacks[i];
                    }
                }
            }
        }

        for (var i = 0; i < 4; ++i)
        {
            if (stacks[i] <= 0 || existingStacks[i] > 0)
            {
                continue;
            }

            take[i] += ModPatches.Input.IsDown(SButton.LeftShift) ? stacks[i] : 1;
            break;
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
        if (stack == 1)
        {
            for (var i = 0; i < 4; ++i)
            {
                if (stacks[i] == 0)
                {
                    continue;
                }

                stacks[i] = value;
                __instance.UpdateQuality(stacks, false);
                return;
            }
        }

        var delta = value - stack;
        if (delta == 0)
        {
            __instance.UpdateQuality(stacks, false);
            return;
        }

        switch (delta)
        {
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