namespace StardewMods.ShoppingCart;

using System;
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

    private ModConfig? _config;
    private bool _showMenuBackground;

    /// <summary>
    ///     Gets the current instance of VirtualShop.
    /// </summary>
    internal static VirtualShop? CurrentShop
    {
        get => ShoppingCart.Instance!._currentShop.Value;
        private set => ShoppingCart.Instance!._currentShop.Value = value;
    }

    private ModConfig Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            ModConfig? config = null;
            try
            {
                config = this.Helper.ReadConfig<ModConfig>();
            }
            catch (Exception)
            {
                // ignored
            }

            this._config = config ?? new ModConfig();
            Log.Trace(this._config.ToString());
            return this._config;
        }
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
        return !ShoppingCart.CurrentShop?.Scroll(direction) ?? true;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ShopMenu
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

        // Create new virtual shop
        var newShop = new VirtualShop(this.Helper, newMenu);

        // Migrate shopping cart
        ShoppingCart.CurrentShop?.MoveItems(newShop);
        ShoppingCart.CurrentShop = newShop;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ShopMenu)
        {
            return;
        }

        Game1.options.showMenuBackground = this._showMenuBackground;
    }

    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ShopMenu)
        {
            return;
        }

        ShoppingCart.CurrentShop?.Draw(e.SpriteBatch);
        this._showMenuBackground = Game1.options.showMenuBackground;
        Game1.options.showMenuBackground = true;
    }
}