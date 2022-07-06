namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Allows categories to be assigned to storages.
/// </summary>
internal class CategorizeChest : IFeature
{
    private readonly PerScreen<ClickableTextureComponent?> _configureButton = new();
    private readonly PerScreen<ItemSelectionMenu?> _currentMenu = new();
    private readonly PerScreen<ClickableTextureComponent?> _minusButton = new();
    private readonly PerScreen<ClickableTextureComponent?> _plusButton = new();

    private CategorizeChest(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static CategorizeChest? Instance { get; set; }

    private ClickableTextureComponent ConfigureButton
    {
        get => this._configureButton.Value ??= new(
            new(0, 0, Game1.tileSize, Game1.tileSize),
            this.Helper.GameContent.Load<Texture2D>("furyx639.BetterChests/Icons"),
            new(0, 0, 16, 16),
            Game1.pixelZoom)
        {
            name = "Configure",
            hoverText = I18n.Button_Configure_Name(),
        };
    }

    private ItemSelectionMenu? CurrentMenu
    {
        get => this._currentMenu.Value;
        set => this._currentMenu.Value = value;
    }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    private ClickableTextureComponent MinusButton
    {
        get => this._minusButton.Value ??=
            new(new(0, 0, 28, 32), Game1.mouseCursors, new(177, 345, 7, 8), Game1.pixelZoom)
            {
                hoverText = I18n.Button_MinusPriority_Name(),
                name = "Minus",
            };
    }

    private ClickableTextureComponent PlusButton
    {
        get => this._plusButton.Value ??=
            new(new(0, 0, 28, 32), Game1.mouseCursors, new(184, 345, 7, 8), Game1.pixelZoom)
            {
                hoverText = I18n.Button_PlusPriority_Name(),
                name = "Plus",
            };
    }

    /// <summary>
    ///     Initializes <see cref="CategorizeChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="CategorizeChest" /> class.</returns>
    public static CategorizeChest Init(IModHelper helper)
    {
        return CategorizeChest.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
            this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        switch (Game1.activeClickableMenu)
        {
            case ItemSelectionMenu { context: IStorageObject storage }:
                var (x, y) = Game1.getMousePosition(true);

                if (this.PlusButton.containsPoint(x, y))
                {
                    storage.StashToChestPriority++;
                    this.Helper.Input.Suppress(e.Button);
                    return;
                }

                if (this.MinusButton.containsPoint(x, y))
                {
                    storage.StashToChestPriority--;
                    this.Helper.Input.Suppress(e.Button);
                }

                return;

            case ItemGrabMenu { context: { } context, shippingBin: false } when StorageHelper.TryGetOne(context, out var storage):
                (x, y) = Game1.getMousePosition(true);
                if (this.ConfigureButton.containsPoint(x, y))
                {
                    Game1.activeClickableMenu = this.CurrentMenu = new(storage, storage.FilterMatcher, this.Helper.Translation);
                    this.Helper.Input.Suppress(e.Button);
                    return;
                }

                return;
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        switch (e.NewMenu)
        {
            case ItemSelectionMenu itemSelectionMenu:
                this.PlusButton.bounds.X = itemSelectionMenu.xPositionOnScreen - Game1.tileSize + 5 * Game1.pixelZoom;
                this.PlusButton.bounds.Y = itemSelectionMenu.yPositionOnScreen + 4 * Game1.pixelZoom;
                this.MinusButton.bounds.X = itemSelectionMenu.xPositionOnScreen - Game1.tileSize * 2 - 4 * Game1.pixelZoom;
                this.MinusButton.bounds.Y = itemSelectionMenu.yPositionOnScreen + 4 * Game1.pixelZoom;
                return;

            case ItemGrabMenu itemGrabMenu:
                var buttons = new List<ClickableComponent>(
                    new[]
                    {
                        itemGrabMenu.junimoNoteIcon,
                        itemGrabMenu.specialButton,
                        itemGrabMenu.colorPickerToggleButton,
                        itemGrabMenu.organizeButton,
                        itemGrabMenu.fillStacksButton,
                    }.OfType<ClickableComponent>().OrderByDescending(button => button.bounds.Y));

                if (!buttons.Any())
                {
                    return;
                }

                this.ConfigureButton.bounds.X = buttons[0].bounds.X;
                this.ConfigureButton.bounds.Y = buttons[0].bounds.Bottom;
                if (buttons.Count >= 2)
                {
                    this.ConfigureButton.bounds.Y += buttons[0].bounds.Top - buttons[1].bounds.Bottom;
                }

                return;

            case null when e.OldMenu is ItemSelectionMenu { context: IStorageObject storage } itemSelectionMenu && itemSelectionMenu == this.CurrentMenu:
                this.CurrentMenu = null;
                storage.ShowMenu();
                break;
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        switch (Game1.activeClickableMenu)
        {
            case ItemSelectionMenu { context: IStorageObject storage } itemSelectionMenu:
                this.PlusButton.draw(e.SpriteBatch);
                this.MinusButton.draw(e.SpriteBatch);

                IClickableMenu.drawTextureBox(
                    e.SpriteBatch,
                    Game1.menuTexture,
                    new(0, 256, 60, 60),
                    itemSelectionMenu.xPositionOnScreen - Game1.tileSize - 12 * Game1.pixelZoom,
                    itemSelectionMenu.yPositionOnScreen,
                    64,
                    34 + Game1.tileSize / 3 + Game1.tileSize / 16,
                    Color.White,
                    drawShadow: true);
                Utility.drawTextWithShadow(
                    e.SpriteBatch,
                    storage.StashToChestPriority.ToString(),
                    Game1.smallFont,
                    new(
                        itemSelectionMenu.xPositionOnScreen - Game1.tileSize - 8 * Game1.pixelZoom,
                        itemSelectionMenu.yPositionOnScreen + 4 * Game1.pixelZoom),
                    Game1.textColor);
                itemSelectionMenu.drawMouse(e.SpriteBatch);
                return;

            case ItemGrabMenu { context: { } context, shippingBin: false } itemGrabMenu when StorageHelper.TryGetOne(context, out _):
                var (x, y) = Game1.getMousePosition(true);
                this.ConfigureButton.tryHover(x, y);
                this.ConfigureButton.draw(e.SpriteBatch);
                if (this.ConfigureButton.containsPoint(x, y))
                {
                    itemGrabMenu.hoverText = this.ConfigureButton.hoverText;
                }

                return;
        }
    }
}