namespace StardewMods.ShoppingCart;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Helpers;
using StardewMods.ShoppingCart.ShopHandlers;
using StardewValley.Menus;
using StardewValley.Tools;

/// <inheritdoc />
public class ShoppingCart : Mod
{
    private static ShoppingCart? Instance;

    private readonly PerScreen<VirtualShop?> _currentShop = new();
    private readonly PerScreen<bool> _makePurchase = new();

    private bool _showMenuBackground;

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

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ShoppingCart.Instance = this;
        I18n.Init(this.Helper.Translation);
        Log.Monitor = this.Monitor;

        // Events
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.Helper.Events.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;

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

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_leftClick_postfix(
        InventoryMenu __instance,
        ref Item? __result,
        int x,
        int y,
        Item? toPlace)
    {
        if (ShoppingCart.CurrentShop is null
         || toPlace is not null
         || __result is null
         || !ShoppingCart.CurrentShop.AddToCart(__result))
        {
            return;
        }

        // Return item to inventory
        var component = __instance.inventory.Single(cc => cc.containsPoint(x, y));
        var slotNumber = int.Parse(component.name);
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
        if (ShoppingCart.CurrentShop is null
         || toAddTo is not null
         || __result is null
         || !ShoppingCart.CurrentShop.AddToCart(__result))
        {
            return;
        }

        // Return item to inventory
        var component = __instance.inventory.Single(cc => cc.containsPoint(x, y));
        var slotNumber = int.Parse(component.name);
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
        return menu is ShopMenu { currency: 0 } shopMenu
            && shopMenu.forSale.OfType<Item>().Any()
            && !(shopMenu.portraitPerson?.Equals(Game1.getCharacterFromName("Clint")) == true
              && shopMenu.forSale.Any(forSale => forSale is Axe or WateringCan or Pickaxe or Hoe or GenericTool));
    }

    private static IEnumerable<CodeInstruction> ShopMenu_constructor_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.updatePosition))))
            {
                yield return new(OpCodes.Ldarg_3);
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

    private static void ShopMenu_updatePosition(ShopMenu shopMenu, string who)
    {
        shopMenu.updatePosition();

        switch (who)
        {
            case "ClintUpgrade":
                shopMenu.xPositionOnScreen += VirtualShop.MenuWidth / 2;
                shopMenu.upperRightCloseButton.bounds.X -= VirtualShop.MenuWidth / 2 + Game1.tileSize;
                break;
        }
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
        if (ShoppingCart.CurrentShop.LeftClick(x, y))
        {
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // Check for supported
        if (!ShoppingCart.IsSupported(e.NewMenu))
        {
            ShoppingCart.CurrentShop = null;
            return;
        }

        // Create new virtual shop
        ShoppingCart.MakePurchase = false;
        ShoppingCart.CurrentShop = new(this.Helper, (ShopMenu)e.NewMenu!);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (ShoppingCart.CurrentShop is null)
        {
            return;
        }

        Game1.options.showMenuBackground = this._showMenuBackground;
    }

    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (ShoppingCart.CurrentShop is null)
        {
            return;
        }

        ShoppingCart.CurrentShop.Draw(e.SpriteBatch);
        this._showMenuBackground = Game1.options.showMenuBackground;
        Game1.options.showMenuBackground = true;
    }
}