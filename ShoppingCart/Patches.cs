namespace StardewMods.ShoppingCart;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using StardewMods.ShoppingCart.Helpers;
using StardewMods.ShoppingCart.ShopHandlers;
using StardewValley.Menus;

/// <summary>
///     Harmony Patches for ShoppingCart.
/// </summary>
internal sealed class Patches
{
    private static Patches? Instance;
    private readonly ModConfig _config;

    private readonly IModHelper _helper;

    private Patches(IModHelper helper, IManifest manifest, ModConfig config)
    {
        this._helper = helper;
        this._config = config;

        var harmony = new Harmony(manifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.leftClick)),
            postfix: new(typeof(Patches), nameof(Patches.InventoryMenu_leftClick_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
            postfix: new(typeof(Patches), nameof(Patches.InventoryMenu_rightClick_postfix)));
        harmony.Patch(
            AccessTools.Constructor(
                typeof(ShopMenu),
                new[]
                {
                    typeof(List<ISalable>),
                    typeof(int),
                    typeof(string),
                    typeof(Func<ISalable, Farmer, int, bool>),
                    typeof(Func<ISalable, bool>),
                    typeof(string),
                }),
            transpiler: new(typeof(Patches), nameof(Patches.ShopMenu_constructor_transpiler)));
        harmony.Patch(
            AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveScrollWheelAction)),
            new(typeof(Patches), nameof(Patches.ShopMenu_receiveScrollWheelAction_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(ShopMenu), "tryToPurchaseItem"),
            new(typeof(Patches), nameof(Patches.ShopMenu_tryToPurchaseItem_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.updatePosition)),
            postfix: new(typeof(Patches), nameof(Patches.ShopMenu_updatePosition_postfix)));
    }

    private static ModConfig Config => Patches.Instance!._config;

    private static IInputHelper Input => Patches.Instance!._helper.Input;

    /// <summary>
    ///     Initializes <see cref="Patches" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="Patches" /> class.</returns>
    public static Patches Init(IModHelper helper, IManifest manifest, ModConfig config)
    {
        return Patches.Instance ??= new(helper, manifest, config);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_leftClick_postfix(
        InventoryMenu __instance,
        ref Item? __result,
        int x,
        int y,
        Item? toPlace)
    {
        if (ShoppingCart.CurrentShop is null || toPlace is not null || __result is null)
        {
            return;
        }

        var component = __instance.inventory.Single(cc => cc.containsPoint(x, y));
        var slotNumber = int.Parse(component.name);

        if (Integrations.StackQuality.IsLoaded && __result is SObject obj)
        {
            var stacks = Integrations.StackQuality.API.GetStacks(obj);
            for (var i = 0; i < 4; ++i)
            {
                Item? split = null;
                var take = new int[4];
                take[i] = stacks[i];
                if (stacks[i] <= 0 || !Integrations.StackQuality.API.SplitStacks(obj, ref split, take))
                {
                    continue;
                }

                ShoppingCart.CurrentShop.AddToCart(split);
            }

            // Return item to inventory
            Integrations.StackQuality.API.UpdateQuality(obj, stacks);
            if (__instance.actualInventory[slotNumber] is SObject otherObj)
            {
                otherObj.addToStack(obj);
                __result = null;
                return;
            }

            __instance.actualInventory[slotNumber] = obj;
            __result = null;
            return;
        }

        if (!ShoppingCart.CurrentShop.AddToCart(__result))
        {
            return;
        }

        // Return item to inventory
        __instance.actualInventory[slotNumber] = __result;
        __result = null;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_rightClick_postfix(
        InventoryMenu __instance,
        ref Item? __result,
        int x,
        int y,
        Item? toAddTo)
    {
        if (ShoppingCart.CurrentShop is null || toAddTo is not null || __result is null)
        {
            return;
        }

        var component = __instance.inventory.Single(cc => cc.containsPoint(x, y));
        var slotNumber = int.Parse(component.name);

        if (Integrations.StackQuality.IsLoaded && __result is SObject obj)
        {
            var stacks = Integrations.StackQuality.API.GetStacks(obj);
            for (var i = 0; i < 4; ++i)
            {
                Item? split = null;
                var take = new int[4];
                take[i] = stacks[i];
                if (stacks[i] <= 0 || !Integrations.StackQuality.API.SplitStacks(obj, ref split, take))
                {
                    continue;
                }

                ShoppingCart.CurrentShop.AddToCart(split);
            }

            // Return item to inventory
            Integrations.StackQuality.API.UpdateQuality(obj, stacks);
            if (__instance.actualInventory[slotNumber] is SObject otherObj)
            {
                otherObj.addToStack(obj);
                __result = null;
                return;
            }

            __instance.actualInventory[slotNumber] = obj;
            __result = null;
            return;
        }

        if (!ShoppingCart.CurrentShop.AddToCart(__result))
        {
            return;
        }

        // Return item to inventory
        var slot = __instance.actualInventory.ElementAtOrDefault(slotNumber);
        if (slot is not null)
        {
            slot.Stack += __result.Stack;
        }
        else
        {
            __instance.actualInventory[slotNumber] = __result;
        }

        __result = null;
    }

    private static IEnumerable<CodeInstruction> ShopMenu_constructor_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.updatePosition))))
            {
                yield return new(OpCodes.Ldarg_3);
                yield return new(OpCodes.Ldarg_S, (short)6);
                yield return CodeInstruction.Call(typeof(Patches), nameof(Patches.ShopMenu_updatePosition));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static bool ShopMenu_receiveScrollWheelAction_prefix()
    {
        var (x, y) = Game1.getMousePosition(true);
        return ShoppingCart.CurrentShop?.Bounds.Contains(x, y) != true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool ShopMenu_tryToPurchaseItem_prefix(
        ShopMenu __instance,
        ref bool __result,
        ISalable item,
        ISalable? held_item,
        int numberToBuy)
    {
        if (ShoppingCart.CurrentShop is null
         || ShoppingCart.MakePurchase
         || held_item is not null
         || __instance.readOnly)
        {
            return true;
        }

        if (numberToBuy == 5 && Patches.Input.IsDown(SButton.LeftShift))
        {
            numberToBuy = Patches.Config.ShiftClickQuantity;
        }

        if (!ShoppingCart.CurrentShop.AddToCart(item, numberToBuy))
        {
            return true;
        }

        __result = false;
        return false;
    }

    private static void ShopMenu_updatePosition(ShopMenu shopMenu, string who, string context)
    {
        shopMenu.updatePosition();

        if (who is not "ClintUpgrade" && context is not ("Dresser" or "FishTank"))
        {
            return;
        }

        shopMenu.xPositionOnScreen += VirtualShop.MenuWidth / 2;
        shopMenu.upperRightCloseButton.bounds.X -= VirtualShop.MenuWidth / 2 + Game1.tileSize;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ShopMenu_updatePosition_postfix(ShopMenu __instance)
    {
        if (!ShoppingCart.IsSupported(__instance))
        {
            return;
        }

        __instance.xPositionOnScreen -= VirtualShop.MenuWidth / 2;
        __instance.upperRightCloseButton.bounds.X += VirtualShop.MenuWidth / 2 + Game1.tileSize;
    }
}