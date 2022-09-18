namespace StardewMods.ShoppingCart;

using System.Linq;
using System.Reflection;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Helpers;
using StardewMods.ShoppingCart.Framework;
using StardewValley.Menus;
using StardewValley.Tools;

/// <inheritdoc />
public sealed class ShoppingCart : Mod
{
#nullable disable
    private static ShoppingCart Instance;
#nullable enable

    private readonly PerScreen<ShopMenu?> _currentMenu = new();
    private readonly PerScreen<Shop?> _currentShop = new();
    private readonly PerScreen<bool> _makePurchase = new();

    private IReflectedField<string?>? _boldTitleText;
    private ModConfig? _config;
    private IReflectedMethod? _getHoveredItemExtraItemAmount;
    private IReflectedMethod? _getHoveredItemExtraItemIndex;

    private IReflectedField<string?>? _hoverText;

    /// <summary>
    ///     Gets the current instance of VirtualShop.
    /// </summary>
    internal static Shop? CurrentShop
    {
        get => ShoppingCart.Instance._currentShop.Value;
        private set => ShoppingCart.Instance._currentShop.Value = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to make a purchase (or add to cart).
    /// </summary>
    internal static bool MakePurchase
    {
        get => ShoppingCart.Instance._makePurchase.Value;
        set => ShoppingCart.Instance._makePurchase.Value = value;
    }

    private static string? BoldTitleText => ShoppingCart.Instance._boldTitleText?.GetValue();

    private static ShopMenu? CurrentMenu
    {
        get => ShoppingCart.Instance._currentMenu.Value;
        set => ShoppingCart.Instance._currentMenu.Value = value;
    }

    private static string? HoverText => ShoppingCart.Instance._hoverText?.GetValue();

    private ModConfig Config => this._config ??= CommonHelpers.GetConfig<ModConfig>(this.Helper);

    /// <summary>
    ///     Check if current menu supports ShoppingCart.
    /// </summary>
    /// <param name="menu">The menu to check.</param>
    /// <returns>Returns true if menu is supported.</returns>
    public static bool IsSupported(IClickableMenu? menu)
    {
        return menu is ShopMenu { currency: 0, storeContext: not ("Dresser" or "FishTank") } shopMenu
            && shopMenu.forSale.OfType<Item>().Any()
            && !(shopMenu.portraitPerson?.Equals(Game1.getCharacterFromName("Clint")) == true
              && shopMenu.forSale.Any(forSale => forSale is Axe or WateringCan or Pickaxe or Hoe or GenericTool));
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ShoppingCart.Instance = this;
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);
        Integrations.Init(this.Helper);
        ModPatches.Init(this.Helper, this.ModManifest, this.Config);

        // Events
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Display.RenderedActiveMenu += ShoppingCart.OnRenderedActiveMenu;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.CursorMoved += ShoppingCart.OnCursorMoved;
        this.Helper.Events.Input.MouseWheelScrolled += ShoppingCart.OnMouseWheelScrolled;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new ShoppingCartApi();
    }

    private static int GetHoveredItemExtraItemAmount()
    {
        return ShoppingCart.Instance._getHoveredItemExtraItemAmount?.Invoke<int>() ?? -1;
    }

    private static int GetHoveredItemExtraItemIndex()
    {
        return ShoppingCart.Instance._getHoveredItemExtraItemIndex?.Invoke<int>() ?? -1;
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

    [EventPriority(EventPriority.High)]
    private static void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (ShoppingCart.CurrentShop?.Scroll(e.Delta) != true)
        {
            return;
        }

        typeof(MouseWheelScrolledEventArgs).GetField(
                                               $"<{nameof(e.OldValue)}>k__BackingField",
                                               BindingFlags.Instance | BindingFlags.NonPublic)
                                           ?.SetValue(e, e.NewValue);
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

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (!Integrations.GMCM.IsLoaded)
        {
            return;
        }

        Integrations.GMCM.Register(
            this.ModManifest,
            () => this._config = new(),
            () => this.Helper.WriteConfig(this.Config));

        Integrations.GMCM.API.AddNumberOption(
            this.ModManifest,
            () => this.Config.ShiftClickQuantity,
            value => this.Config.ShiftClickQuantity = value,
            I18n.Config_ShiftClickQuantity_Name,
            I18n.Config_ShiftClickQuantity_Tooltip);
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