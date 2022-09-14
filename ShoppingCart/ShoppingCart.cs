namespace StardewMods.ShoppingCart;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Helpers;
using StardewMods.ShoppingCart.Helpers;
using StardewMods.ShoppingCart.ShopHandlers;
using StardewValley.Menus;
using StardewValley.Tools;

/// <inheritdoc />
public class ShoppingCart : Mod
{
    private static ShoppingCart? Instance;

    private readonly PerScreen<ShopMenu?> _currentMenu = new();
    private readonly PerScreen<VirtualShop?> _currentShop = new();
    private readonly PerScreen<bool> _makePurchase = new();

    private IReflectedField<string?>? _boldTitleText;
    private IReflectedMethod? _getHoveredItemExtraItemAmount;
    private IReflectedMethod? _getHoveredItemExtraItemIndex;

    private IReflectedField<string?>? _hoverText;

    /// <summary>
    ///     Gets the current instance of VirtualShop.
    /// </summary>
    internal static VirtualShop? CurrentShop
    {
        get => ShoppingCart.Instance!._currentShop.Value;
        private set => ShoppingCart.Instance!._currentShop.Value = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to make a purchase (or add to cart).
    /// </summary>
    internal static bool MakePurchase
    {
        get => ShoppingCart.Instance!._makePurchase.Value;
        set => ShoppingCart.Instance!._makePurchase.Value = value;
    }

    private static string? BoldTitleText => ShoppingCart.Instance!._boldTitleText?.GetValue();

    private static ShopMenu? CurrentMenu
    {
        get => ShoppingCart.Instance!._currentMenu.Value;
        set => ShoppingCart.Instance!._currentMenu.Value = value;
    }

    private static string? HoverText => ShoppingCart.Instance!._hoverText?.GetValue();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ShoppingCart.Instance = this;
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);
        Integrations.Init(this.Helper);

        // Events
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Display.RenderedActiveMenu += ShoppingCart.OnRenderedActiveMenu;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.CursorMoved += ShoppingCart.OnCursorMoved;

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.leftClick)),
            postfix: new(typeof(ShoppingCart), nameof(ShoppingCart.InventoryMenu_leftClick_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
            postfix: new(typeof(ShoppingCart), nameof(ShoppingCart.InventoryMenu_rightClick_postfix)));
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
            transpiler: new(typeof(ShoppingCart), nameof(ShoppingCart.ShopMenu_constructor_transpiler)));
        harmony.Patch(
            AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveScrollWheelAction)),
            new(typeof(ShoppingCart), nameof(ShoppingCart.ShopMenu_receiveScrollWheelAction_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(ShopMenu), "tryToPurchaseItem"),
            new(typeof(ShoppingCart), nameof(ShoppingCart.ShopMenu_tryToPurchaseItem_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.updatePosition)),
            postfix: new(typeof(ShoppingCart), nameof(ShoppingCart.ShopMenu_updatePosition_postfix)));
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new ShoppingCartApi();
    }

    private static int GetHoveredItemExtraItemAmount()
    {
        return ShoppingCart.Instance!._getHoveredItemExtraItemAmount?.Invoke<int>() ?? -1;
    }

    private static int GetHoveredItemExtraItemIndex()
    {
        return ShoppingCart.Instance!._getHoveredItemExtraItemIndex?.Invoke<int>() ?? -1;
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

    private static bool IsSupported(IClickableMenu? menu)
    {
        return menu is ShopMenu { currency: 0, storeContext: not ("Dresser" or "FishTank") } shopMenu
            && shopMenu.forSale.OfType<Item>().Any()
            && !(shopMenu.portraitPerson?.Equals(Game1.getCharacterFromName("Clint")) == true
              && shopMenu.forSale.Any(forSale => forSale is Axe or WateringCan or Pickaxe or Hoe or GenericTool));
    }

    private static void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (ShoppingCart.CurrentShop is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        ShoppingCart.CurrentShop.Hover(x, y);
    }

    [EventPriority(EventPriority.Low)]
    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (ShoppingCart.CurrentShop is null || ShoppingCart.CurrentMenu is null)
        {
            return;
        }

        ShoppingCart.CurrentShop.Draw(e.SpriteBatch);

        // Redraw Hover Item
        if (!string.IsNullOrWhiteSpace(ShoppingCart.HoverText))
        {
            if (ShoppingCart.CurrentMenu.hoveredItem is SObject { IsRecipe: true })
            {
                IClickableMenu.drawToolTip(
                    e.SpriteBatch,
                    " ",
                    ShoppingCart.BoldTitleText,
                    ShoppingCart.CurrentMenu.hoveredItem as Item,
                    ShoppingCart.CurrentMenu.heldItem != null,
                    -1,
                    ShoppingCart.CurrentMenu.currency,
                    ShoppingCart.GetHoveredItemExtraItemIndex(),
                    ShoppingCart.GetHoveredItemExtraItemAmount(),
                    new(ShoppingCart.CurrentMenu.hoveredItem.Name.Replace(" Recipe", string.Empty)),
                    ShoppingCart.CurrentMenu.hoverPrice > 0 ? ShoppingCart.CurrentMenu.hoverPrice : -1);
            }
            else
            {
                IClickableMenu.drawToolTip(
                    e.SpriteBatch,
                    ShoppingCart.HoverText,
                    ShoppingCart.BoldTitleText,
                    ShoppingCart.CurrentMenu.hoveredItem as Item,
                    ShoppingCart.CurrentMenu.heldItem != null,
                    -1,
                    ShoppingCart.CurrentMenu.currency,
                    ShoppingCart.GetHoveredItemExtraItemIndex(),
                    ShoppingCart.GetHoveredItemExtraItemAmount(),
                    null,
                    ShoppingCart.CurrentMenu.hoverPrice > 0 ? ShoppingCart.CurrentMenu.hoverPrice : -1);
            }
        }

        // Redraw Mouse
        ShoppingCart.CurrentMenu.drawMouse(e.SpriteBatch);
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
                yield return CodeInstruction.Call(typeof(ShoppingCart), nameof(ShoppingCart.ShopMenu_updatePosition));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static bool ShopMenu_receiveScrollWheelAction_prefix(int direction)
    {
        return ShoppingCart.CurrentShop?.Scroll(direction) != true;
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
         || __instance.readOnly
         || !ShoppingCart.CurrentShop.AddToCart(item, numberToBuy))
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

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (ShoppingCart.CurrentShop is null
         || (!e.Button.IsActionButton() && e.Button is not (SButton.MouseLeft or SButton.MouseRight)))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft when ShoppingCart.CurrentShop.LeftClick(x, y):
                break;
            case SButton.MouseRight when ShoppingCart.CurrentShop.RightClick(x, y):
                break;
            default:
                return;
        }

        this.Helper.Input.Suppress(e.Button);
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // Check for supported
        if (!ShoppingCart.IsSupported(e.NewMenu))
        {
            ShoppingCart.CurrentMenu = null;
            ShoppingCart.CurrentShop = null;
            return;
        }

        // Create new virtual shop
        ShoppingCart.MakePurchase = false;
        ShoppingCart.CurrentMenu = (ShopMenu)e.NewMenu!;
        ShoppingCart.CurrentShop = new(this.Helper, ShoppingCart.CurrentMenu);
        this._hoverText = this.Helper.Reflection.GetField<string?>(ShoppingCart.CurrentMenu, "hoverText");
        this._boldTitleText = this.Helper.Reflection.GetField<string?>(ShoppingCart.CurrentMenu, "boldTitleText");
        this._getHoveredItemExtraItemIndex = this.Helper.Reflection.GetMethod(
            ShoppingCart.CurrentMenu,
            "getHoveredItemExtraItemIndex");
        this._getHoveredItemExtraItemAmount = this.Helper.Reflection.GetMethod(
            ShoppingCart.CurrentMenu,
            "getHoveredItemExtraItemAmount");
    }
}