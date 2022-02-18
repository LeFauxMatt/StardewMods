namespace StardewMods.TooManyAnimals.Services;

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.TooManyAnimals.Interfaces;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class AnimalMenuHandler : IModService
{
    private readonly PerScreen<int> _currentPage = new();
    private readonly PerScreen<IClickableComponent> _nextPage = new();
    private readonly PerScreen<IClickableComponent> _previousPage = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnimalMenuHandler" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public AnimalMenuHandler(IConfigData config, IModHelper helper, IModServices services)
    {
        AnimalMenuHandler.Instance = this;
        this.Config = config;
        this.Helper = helper;

        services.Lazy<IHarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{TooManyAnimals.ModUniqueId}.{nameof(AnimalMenuHandler)}";
                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new[] { typeof(List<SObject>) }),
                    typeof(AnimalMenuHandler),
                    nameof(AnimalMenuHandler.PurchaseAnimalsMenu_constructor_prefix));
                harmonyHelper.ApplyPatches(id);
            });

        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
    }

    private static AnimalMenuHandler Instance { get; set; }

    private IConfigData Config { get; }

    private int CurrentPage
    {
        get => this._currentPage.Value;
        set
        {
            if (this._currentPage.Value == value)
            {
                return;
            }

            this._currentPage.Value = value;
            Game1.activeClickableMenu = new PurchaseAnimalsMenu(this.Stock);
        }
    }

    private IModHelper Helper { get; }

    private IClickableComponent NextPage
    {
        get => this._nextPage.Value ??= new CustomClickableComponent(new(new(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new(365, 495, 12, 11), Game1.pixelZoom));
    }

    private IClickableComponent PreviousPage
    {
        get => this._previousPage.Value ??= new CustomClickableComponent(new(new(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new(352, 495, 12, 11), Game1.pixelZoom));
    }

    private List<SObject> Stock { get; set; }

    private static void PurchaseAnimalsMenu_constructor_prefix(ref List<SObject> stock)
    {
        // Get actual stock
        AnimalMenuHandler.Instance.Stock ??= stock;

        // Limit stock
        stock = AnimalMenuHandler.Instance.Stock
                                 .Skip(AnimalMenuHandler.Instance.CurrentPage * AnimalMenuHandler.Instance.Config.AnimalShopLimit)
                                 .Take(AnimalMenuHandler.Instance.Config.AnimalShopLimit)
                                 .ToList();
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (Game1.activeClickableMenu is not PurchaseAnimalsMenu || this.Stock is null || this.Stock.Count <= this.Config.AnimalShopLimit || e.Button != SButton.MouseLeft)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.CurrentPage * this.Config.AnimalShopLimit < this.Stock.Count - 1 && this.NextPage.Component.containsPoint(x, y))
        {
            this.CurrentPage++;
            return;
        }

        if (this.CurrentPage > 0 && this.PreviousPage.Component.containsPoint(x, y))
        {
            this.CurrentPage--;
        }
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (Game1.activeClickableMenu is not PurchaseAnimalsMenu || this.Stock is null || this.Stock.Count <= this.Config.AnimalShopLimit)
        {
            return;
        }

        if (this.CurrentPage * this.Config.AnimalShopLimit < this.Stock.Count - 1 && this.Config.ControlScheme.NextPage.JustPressed())
        {
            this.CurrentPage++;
            return;
        }

        if (this.CurrentPage > 0 && this.Config.ControlScheme.PreviousPage.JustPressed())
        {
            this.CurrentPage--;
        }
    }

    private void OnCursorMoved(object sender, CursorMovedEventArgs e)
    {
        if (Game1.activeClickableMenu is not PurchaseAnimalsMenu || this.Stock is null || this.Stock.Count <= this.Config.AnimalShopLimit)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.NextPage.TryHover(x, y);
        this.PreviousPage.TryHover(x, y);
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        // Reset Stock/CurrentPage
        if (e.OldMenu is PurchaseAnimalsMenu && e.NewMenu is not PurchaseAnimalsMenu)
        {
            this.Stock = null;
            this._currentPage.Value = 0;
        }

        // Reposition Next/Previous Page Buttons
        if (e.NewMenu is PurchaseAnimalsMenu menu)
        {
            this.NextPage.X = menu.xPositionOnScreen + menu.width - this.NextPage.Component.bounds.Width;
            this.NextPage.Y = menu.yPositionOnScreen + menu.height;
            this.PreviousPage.X = menu.xPositionOnScreen;
            this.PreviousPage.Y = menu.yPositionOnScreen + menu.height;

            for (var index = 0; index < menu.animalsToPurchase.Count; index++)
            {
                var i = index + this.CurrentPage * this.Config.AnimalShopLimit;
                if (ReferenceEquals(menu.animalsToPurchase[index].texture, Game1.mouseCursors))
                {
                    menu.animalsToPurchase[index].sourceRect.X = i % 3 * 16 * 2;
                    menu.animalsToPurchase[index].sourceRect.Y = 448 + i / 3 * 16;
                }

                if (ReferenceEquals(menu.animalsToPurchase[index].texture, Game1.mouseCursors2))
                {
                    menu.animalsToPurchase[index].sourceRect.X = 128 + i % 3 * 16 * 2;
                    menu.animalsToPurchase[index].sourceRect.Y = i / 3 * 16;
                }
            }
        }
    }

    private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not PurchaseAnimalsMenu menu || this.Stock is null || this.Stock.Count <= this.Config.AnimalShopLimit)
        {
            return;
        }

        // Conditionally draw next page button
        if (this.CurrentPage * this.Config.AnimalShopLimit < this.Stock.Count - 1)
        {
            this.NextPage.Draw(e.SpriteBatch);
        }

        // Conditionally draw previous page button
        if (this.CurrentPage > 0)
        {
            this.PreviousPage.Draw(e.SpriteBatch);
        }

        // Redraw foreground components
        if (menu.hovered?.item is SObject obj)
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, Game1.parseText(obj.Type, Game1.dialogueFont, 320), Game1.dialogueFont);
        }

        Game1.mouseCursorTransparency = 1f;
        menu.drawMouse(e.SpriteBatch);
    }
}