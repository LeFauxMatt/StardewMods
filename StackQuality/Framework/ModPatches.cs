namespace StardewMods.StackQuality.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using StardewMods.Common.Integrations.StackQuality;
using StardewMods.StackQuality.UI;
using StardewValley.Menus;

/// <summary>
///     Harmony Patches for StackQuality.
/// </summary>
internal sealed class ModPatches
{
#nullable disable
    private static ModPatches Instance;
#nullable enable

    private readonly IStackQualityApi _api;
    private readonly IModHelper _helper;

    private ModPatches(IModHelper helper, IManifest manifest, IStackQualityApi api)
    {
        this._helper = helper;
        this._api = api;
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
            AccessTools.Method(typeof(SObject), nameof(SObject.sellToStorePrice)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_sellToStorePrice_postfix)));
        harmony.Patch(
            AccessTools.PropertySetter(typeof(SObject), nameof(SObject.Stack)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_StackSetter_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.addItemToInventory)),
            new(typeof(ModPatches), nameof(ModPatches.Utility_addItemToInventory_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.addItemToInventory)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_addItemToInventory_postfix)));
    }

    private static IStackQualityApi Api => ModPatches.Instance._api;

    private static Item? HoveredItem
    {
        set
        {
            switch (Game1.activeClickableMenu)
            {
                case GameMenu gameMenu when gameMenu.GetCurrentPage() is InventoryPage inventoryPage:
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
                case GameMenu gameMenu when gameMenu.GetCurrentPage() is InventoryPage inventoryPage:
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

    private static IInputHelper Input => ModPatches.Instance._helper.Input;

    private static IReflectionHelper Reflection => ModPatches.Instance._helper.Reflection;

    /// <summary>
    ///     Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="api">The StackQuality Api.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IModHelper helper, IManifest manifest, IStackQualityApi api)
    {
        return ModPatches.Instance ??= new(helper, manifest, api);
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
            if (slot is not SObject other
                || !ModPatches.Api.EquivalentObjects(other, obj)
                || !ModPatches.Api.AddToStacks(other, obj, out var remaining))
            {
                continue;
            }

            ModPatches.Api.UpdateStacks(obj, remaining);
            affected_items_list?.Add(slot);
            if (remaining.Sum() == 0)
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
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Farmer_removeItemsFromInventory_prefix(
        Farmer __instance,
        ref bool __result,
        int index,
        int stack)
    {
        for (var i = 0; i < Math.Min(__instance.MaxItems, __instance.Items.Count); ++i)
        {
            if (__instance.Items[i] is not SObject obj
                || obj.ParentSheetIndex != index
                || !ModPatches.Api.GetStacks(obj, out var stacks))
            {
                continue;
            }

            for (var j = 3; j >= 0; --j)
            {
                var toTake = Math.Min(stack, stacks[j]);
                stack -= toTake;
                stacks[j] -= toTake;
            }

            if (stacks.Sum() == 0)
            {
                __instance.Items[i] = null;
                continue;
            }

            ModPatches.Api.UpdateStacks(obj, stacks);
            if (stack == 0)
            {
                break;
            }
        }

        __result = stack == 0;
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
        if (!Helpers.IsSupported || toPlace is not null)
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
        if (slot is not SObject obj
            || !ModPatches.Api.GetStacks(obj, out var stacks)
            || stacks.Count(stack => stack > 0) <= 1)
        {
            return true;
        }

        // Pick up item directly
        if (ModPatches.Input.IsDown(SButton.LeftShift) || ModPatches.Input.IsDown(SButton.LeftControl))
        {
            if (playSound)
            {
                Game1.playSound(__instance.moveItemSound);
            }

            // Grab full stack
            __result = Utility.removeItemFromInventory(slotNumber, __instance.actualInventory);
            return false;
        }

        // Show stack overlay
        var overlay = new ItemQualityMenu(
            ModPatches.Api,
            obj,
            stacks,
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
        if (!Helpers.IsSupported || onlyCheckToolAttachments)
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
        if (slot is not SObject obj || !ModPatches.Api.GetStacks(obj, out var stacks))
        {
            return true;
        }

        // ShoppingCart Integration
        var availableStacks = new int[4];
        var existingStacks = new int[4];
        if (Integrations.ShoppingCart.IsLoaded)
        {
            if (Integrations.ShoppingCart.Api.CurrentShop is not null)
            {
                foreach (var cartItem in Integrations.ShoppingCart.Api.CurrentShop.ToSell)
                {
                    if (cartItem.Item is not SObject cartObj)
                    {
                        continue;
                    }

                    availableStacks[cartObj.Quality == 4 ? 3 : cartObj.Quality] += cartItem.Available;
                    existingStacks[cartObj.Quality == 4 ? 3 : cartObj.Quality] += cartItem.Quantity;
                }
            }
        }

        var amountToTake = new int[4];
        var limit = ModPatches.Input.IsDown(SButton.LeftShift) ? (int)Math.Ceiling(stacks.Sum() / 2f) : 1;
        for (var i = 3; i >= 0; --i)
        {
            if (stacks[i] <= 0 || (availableStacks[i] > 0 && existingStacks[i] >= availableStacks[i]))
            {
                continue;
            }

            if (ModPatches.Input.IsDown(SButton.LeftControl))
            {
                amountToTake[i] += stacks[i];
                break;
            }

            var toTake = Math.Min(limit, (int)Math.Ceiling(stacks[i] / 2f));
            amountToTake[i] += toTake;
            limit -= toTake;
            if (limit <= 0)
            {
                break;
            }
        }

        if (amountToTake.Sum() > 0 && !ModPatches.Api.MoveStacks(obj, ref toAddTo, amountToTake))
        {
            return true;
        }

        if (obj.Stack == 0)
        {
            __instance.actualInventory[slotNumber] = null;
        }

        if (playSound)
        {
            Game1.playSound(__instance.moveItemSound);
        }

        __result = toAddTo;
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyPriority(1_000)]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable? other)
    {
        if (__result || __instance.maximumStackSize() == 1 || !ModPatches.Api.EquivalentObjects(__instance, other))
        {
            return;
        }

        __result = true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_GetContextTags_postfix(Item __instance, ref HashSet<string> __result)
    {
        if (__instance is not SObject obj || !ModPatches.Api.GetStacks(obj, out var stacks))
        {
            return;
        }

        for (var i = 0; i < 4; ++i)
        {
            var tag = i switch
            {
                3 => "quality_iridium",
                2 => "quality_gold",
                1 => "quality_silver",
                0 or _ => "quality_none",
            };

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
        if (!ModPatches.Api.AddToStacks(__instance, otherStack, out var remaining))
        {
            return true;
        }

        __result = remaining.Sum();
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_sellToStorePrice_postfix(SObject __instance, ref int __result, long specificPlayerID)
    {
        if (!__instance.modData.ContainsKey("furyx639.StackQuality/qualities")
            || !ModPatches.Api.GetStacks(__instance, out var stacks)
            || stacks.Count(stack => stack > 0) <= 1)
        {
            return;
        }

        var totalSalePrice = 0f;
        for (var i = 0; i < 4; ++i)
        {
            var salePrice = __instance.Price * (1f + (i == 3 ? 4 : i) * 0.25f);
            var getPriceAfterMultipliers = ModPatches.Reflection.GetMethod(__instance, "getPriceAfterMultipliers");
            salePrice = getPriceAfterMultipliers.Invoke<float>(salePrice, specificPlayerID);
            if (__instance.ParentSheetIndex == 493)
            {
                salePrice /= 2f;
            }

            if (salePrice > 0)
            {
                salePrice = Math.Max(1f, salePrice * Game1.MasterPlayer.difficultyModifier);
            }

            totalSalePrice += salePrice;
        }

        totalSalePrice /= __instance.Stack;
        __result = (int)totalSalePrice;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_StackSetter_postfix(SObject __instance, int value)
    {
        if (!__instance.modData.ContainsKey("furyx639.StackQuality/qualities")
            || !ModPatches.Api.GetStacks(__instance, out var stacks))
        {
            return;
        }

        var maxStack = __instance.maximumStackSize();
        if (value > maxStack)
        {
            return;
        }

        var currentStacks = stacks.Sum();
        var delta = value - currentStacks;
        switch (delta)
        {
            case 0:
                return;

            // Reduce stacks to value
            case < 0:
                for (var i = 3; i >= 0; --i)
                {
                    var toTake = Math.Min(-delta, stacks[i]);
                    delta += toTake;
                    stacks[i] -= toTake;
                    if (delta == 0)
                    {
                        break;
                    }
                }

                break;

            // Increase stacks to value
            case > 0:
                for (var i = Math.Min(3, __instance.Quality); i >= 0; --i)
                {
                    var toAdd = Math.Min(delta, maxStack - stacks[i]);
                    stacks[i] += toAdd;
                    delta -= toAdd;
                    if (delta == 0)
                    {
                        break;
                    }
                }

                break;
        }

        // Update quality and mod data
        var quality = 0;
        var sb = new StringBuilder();
        for (var i = 0; i < 4; ++i)
        {
            sb.Append(stacks[i]);
            if (i < 3)
            {
                sb.Append(' ');
            }

            if (stacks[i] > 0)
            {
                quality = i;
            }
        }

        __instance.modData["furyx639.StackQuality/qualities"] = sb.ToString();
        __instance.Quality = quality == 3 ? 4 : quality;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Local", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Utility_addItemToInventory_postfix(
        ref Item? __result,
        ref bool __state,
        Item item,
        int position,
        IList<Item> items,
        ItemGrabMenu.behaviorOnItemSelect? onAddFunction)
    {
        // item is the originally held object (now in items[position])
        // __result is the item that was in items[position]
        if (ReferenceEquals(item, __result)
            || position >= items.Count
            || __result is not SObject obj
            || !ModPatches.Api.EquivalentObjects(obj, item))
        {
            return;
        }

        item.HasBeenInInventory = __state;
        Utility.checkItemFirstInventoryAdd(item);
        if (!ModPatches.Api.AddToStacks(obj, item, out var remaining))
        {
            return;
        }

        items[position] = obj;
        if (remaining.Sum() == 0)
        {
            __result = null;
            return;
        }

        ModPatches.Api.UpdateStacks((SObject)item, remaining);
        __result = item;
        onAddFunction?.Invoke(item, null);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Utility_addItemToInventory_prefix(ref bool __state, Item item)
    {
        __state = item.HasBeenInInventory;
    }
}