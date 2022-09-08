namespace StardewMods.ShoppingCart;

using System.Linq;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Helpers;
using StardewMods.ShoppingCart.ShopHandlers;
using StardewValley.Menus;

/// <inheritdoc />
public class ShoppingCart : Mod
{
    private static ShoppingCart? Instance;

    private readonly PerScreen<VirtualShop?> _currentShop = new();
    private readonly PerScreen<bool> _isSupported = new();

    private bool _showMenuBackground;

    /// <summary>
    ///     Gets the current instance of VirtualShop.
    /// </summary>
    internal static VirtualShop? CurrentShop
    {
        get => ShoppingCart.Instance!._currentShop.Value;
        private set => ShoppingCart.Instance!._currentShop.Value = value;
    }

    private static bool IsSupported
    {
        get => ShoppingCart.Instance!._isSupported.Value;
        set => ShoppingCart.Instance!._isSupported.Value = value;
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
            AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveScrollWheelAction)),
            new(typeof(ShoppingCart), nameof(ShoppingCart.ShopMenu_receiveScrollWheelAction_prefix)));
    }

    private static bool ShopMenu_receiveScrollWheelAction_prefix(int direction)
    {
        if (!ShoppingCart.IsSupported)
        {
            return true;
        }

        return !ShoppingCart.CurrentShop?.Scroll(direction) ?? true;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ShopMenu
         || !ShoppingCart.IsSupported
         || ShoppingCart.CurrentShop is null
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
        if (e.NewMenu is not ShopMenu newMenu)
        {
            return;
        }

        // Check for supported
        ShoppingCart.IsSupported = newMenu.portraitPerson is not null
                                && newMenu.currency == 0
                                && newMenu.forSale.Any()
                                && newMenu.itemPriceAndStock.Any();
        if (!ShoppingCart.IsSupported)
        {
            return;
        }

        // Create new virtual shop
        ShoppingCart.CurrentShop = new(this.Helper, newMenu);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ShopMenu || !ShoppingCart.IsSupported)
        {
            return;
        }

        Game1.options.showMenuBackground = this._showMenuBackground;
    }

    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ShopMenu || !ShoppingCart.IsSupported)
        {
            return;
        }

        ShoppingCart.CurrentShop?.Draw(e.SpriteBatch);
        this._showMenuBackground = Game1.options.showMenuBackground;
        Game1.options.showMenuBackground = true;
    }
}