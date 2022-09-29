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
public sealed class ModEntry : Mod
{
    private static readonly FieldInfo MouseWheelScrolledEventArgsOldValueField =
        typeof(MouseWheelScrolledEventArgs).GetField(
            "<OldValue>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

#nullable disable
    private static ModEntry Instance;
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
        get => ModEntry.Instance._currentShop.Value;
        private set => ModEntry.Instance._currentShop.Value = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to make a purchase (or add to cart).
    /// </summary>
    internal static bool MakePurchase
    {
        get => ModEntry.Instance._makePurchase.Value;
        set => ModEntry.Instance._makePurchase.Value = value;
    }

    private static string? BoldTitleText => ModEntry.Instance._boldTitleText?.GetValue();

    private static ShopMenu? CurrentMenu
    {
        get => ModEntry.Instance._currentMenu.Value;
        set => ModEntry.Instance._currentMenu.Value = value;
    }

    private static string? HoverText => ModEntry.Instance._hoverText?.GetValue();

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
        ModEntry.Instance = this;
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);
        Integrations.Init(this.Helper);
        ModPatches.Init(this.Helper, this.ModManifest, this.Config);

        // Events
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Display.RenderedActiveMenu += ModEntry.OnRenderedActiveMenu;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.CursorMoved += ModEntry.OnCursorMoved;
        this.Helper.Events.Input.MouseWheelScrolled += ModEntry.OnMouseWheelScrolled;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new ShoppingCartApi();
    }

    private static int GetHoveredItemExtraItemAmount()
    {
        return ModEntry.Instance._getHoveredItemExtraItemAmount?.Invoke<int>() ?? -1;
    }

    private static int GetHoveredItemExtraItemIndex()
    {
        return ModEntry.Instance._getHoveredItemExtraItemIndex?.Invoke<int>() ?? -1;
    }

    private static void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (ModEntry.CurrentShop is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        ModEntry.CurrentShop.Hover(x, y);
    }

    [EventPriority(EventPriority.High)]
    private static void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (ModEntry.CurrentShop?.Scroll(e.Delta) != true)
        {
            return;
        }

        ModEntry.MouseWheelScrolledEventArgsOldValueField.SetValue(e, e.NewValue);
    }

    [EventPriority(EventPriority.Low)]
    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (ModEntry.CurrentShop is null || ModEntry.CurrentMenu is null)
        {
            return;
        }

        ModEntry.CurrentShop.Draw(e.SpriteBatch);

        // Redraw Hover Item
        if (!string.IsNullOrWhiteSpace(ModEntry.HoverText))
        {
            if (ModEntry.CurrentMenu.hoveredItem is SObject { IsRecipe: true })
            {
                IClickableMenu.drawToolTip(
                    e.SpriteBatch,
                    " ",
                    ModEntry.BoldTitleText,
                    ModEntry.CurrentMenu.hoveredItem as Item,
                    ModEntry.CurrentMenu.heldItem != null,
                    -1,
                    ModEntry.CurrentMenu.currency,
                    ModEntry.GetHoveredItemExtraItemIndex(),
                    ModEntry.GetHoveredItemExtraItemAmount(),
                    new(ModEntry.CurrentMenu.hoveredItem.Name.Replace(" Recipe", string.Empty)),
                    ModEntry.CurrentMenu.hoverPrice > 0 ? ModEntry.CurrentMenu.hoverPrice : -1);
            }
            else
            {
                IClickableMenu.drawToolTip(
                    e.SpriteBatch,
                    ModEntry.HoverText,
                    ModEntry.BoldTitleText,
                    ModEntry.CurrentMenu.hoveredItem as Item,
                    ModEntry.CurrentMenu.heldItem != null,
                    -1,
                    ModEntry.CurrentMenu.currency,
                    ModEntry.GetHoveredItemExtraItemIndex(),
                    ModEntry.GetHoveredItemExtraItemAmount(),
                    null,
                    ModEntry.CurrentMenu.hoverPrice > 0 ? ModEntry.CurrentMenu.hoverPrice : -1);
            }
        }

        // Redraw Mouse
        ModEntry.CurrentMenu.drawMouse(e.SpriteBatch);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (ModEntry.CurrentShop is null
         || (!e.Button.IsActionButton() && e.Button is not (SButton.MouseLeft or SButton.MouseRight)))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft when ModEntry.CurrentShop.LeftClick(x, y):
                break;
            case SButton.MouseRight when ModEntry.CurrentShop.RightClick(x, y):
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
        if (!ModEntry.IsSupported(e.NewMenu))
        {
            ModEntry.CurrentMenu = null;
            ModEntry.CurrentShop = null;
            return;
        }

        // Create new virtual shop
        ModEntry.MakePurchase = false;
        ModEntry.CurrentMenu = (ShopMenu)e.NewMenu!;
        ModEntry.CurrentShop = new(this.Helper, ModEntry.CurrentMenu);
        this._hoverText = this.Helper.Reflection.GetField<string?>(ModEntry.CurrentMenu, "hoverText");
        this._boldTitleText = this.Helper.Reflection.GetField<string?>(ModEntry.CurrentMenu, "boldTitleText");
        this._getHoveredItemExtraItemIndex = this.Helper.Reflection.GetMethod(
            ModEntry.CurrentMenu,
            "getHoveredItemExtraItemIndex");
        this._getHoveredItemExtraItemAmount = this.Helper.Reflection.GetMethod(
            ModEntry.CurrentMenu,
            "getHoveredItemExtraItemAmount");
    }
}