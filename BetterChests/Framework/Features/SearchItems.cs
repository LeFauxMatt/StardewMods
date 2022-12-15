namespace StardewMods.BetterChests.Framework.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewValley.Menus;

/// <summary>
///     Adds a search bar to the top of the <see cref="ItemGrabMenu" />.
/// </summary>
internal sealed class SearchItems : Feature
{
    private const int ExtraSpace = 24;
    private const int MaxTimeOut = 20;

#nullable disable
    private static Feature Instance;
#nullable enable

    private readonly ModConfig _config;
    private readonly PerScreen<ItemGrabMenu?> _currentMenu = new();
    private readonly IModHelper _helper;
    private readonly PerScreen<ItemMatcher> _itemMatcher;
    private readonly PerScreen<StorageNode?> _lastContext = new();
    private readonly PerScreen<ClickableComponent> _searchArea;
    private readonly PerScreen<TextBox> _searchField;
    private readonly PerScreen<ClickableTextureComponent> _searchIcon;
    private readonly PerScreen<string> _searchText = new(() => string.Empty);
    private readonly PerScreen<int> _timeOut = new();

    private SearchItems(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
        this._itemMatcher = new(() => new(false, config.SearchTagSymbol.ToString(), helper.Translation));
        this._searchArea = new(() => new(Rectangle.Empty, string.Empty));
        this._searchField = new(
            () => new(
                helper.GameContent.Load<Texture2D>("LooseSprites\\textBox"),
                null,
                Game1.smallFont,
                Game1.textColor));
        this._searchIcon = new(() => new(Rectangle.Empty, Game1.mouseCursors, new(80, 0, 13, 13), 2.5f));
    }

    private ItemGrabMenu? CurrentMenu
    {
        get => this._currentMenu.Value;
        set => this._currentMenu.Value = value;
    }

    private ItemMatcher ItemMatcher => this._itemMatcher.Value;

    private StorageNode? LastContext
    {
        get => this._lastContext.Value;
        set => this._lastContext.Value = value;
    }

    private ClickableComponent SearchArea => this._searchArea.Value;

    private TextBox SearchField => this._searchField.Value;

    private ClickableTextureComponent SearchIcon => this._searchIcon.Value;

    private string SearchText
    {
        get => this._searchText.Value;
        set => this._searchText.Value = value;
    }

    private int TimeOut
    {
        get => this._timeOut.Value;
        set => this._timeOut.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="SearchItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="SearchItems" /> class.</returns>
    public static Feature Init(IModHelper helper, ModConfig config)
    {
        return SearchItems.Instance ??= new SearchItems(helper, config);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        BetterItemGrabMenu.Constructing += SearchItems.OnConstructing;
        this._helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this._helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterItemGrabMenu.Constructing -= SearchItems.OnConstructing;
        this._helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this._helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static void OnConstructing(object? sender, ItemGrabMenu itemGrabMenu)
    {
        if (itemGrabMenu.shippingBin || BetterItemGrabMenu.TopPadding > 0)
        {
            return;
        }

        if (BetterItemGrabMenu.Context is null)
        {
            BetterItemGrabMenu.TopPadding = 0;
            return;
        }

        BetterItemGrabMenu.TopPadding = BetterItemGrabMenu.Context.SearchItems switch
        {
            FeatureOption.Enabled => SearchItems.ExtraSpace,
            _ => 0,
        };
    }

    private IEnumerable<Item> FilterBySearch(IEnumerable<Item> items)
    {
        if (this._config.HideItems is FeatureOption.Enabled)
        {
            return this.ItemMatcher.Any() ? items.Where(this.ItemMatcher.Matches) : items;
        }

        return this.ItemMatcher.Any() ? items.OrderBy(item => this.ItemMatcher.Matches(item) ? 0 : 1) : items;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null || !this.SearchArea.visible)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft when this.SearchArea.containsPoint(x, y):
                this.SearchField.Selected = true;
                break;
            case SButton.MouseRight when this.SearchArea.containsPoint(x, y):
                this.SearchField.Selected = true;
                this.SearchField.Text = string.Empty;
                break;
            case SButton.MouseLeft:
            case SButton.MouseRight:
                this.SearchField.Selected = false;
                break;
            case SButton.Escape when this.CurrentMenu.readyToClose():
                Game1.playSound("bigDeSelect");
                this.CurrentMenu.exitThisMenu();
                this._helper.Input.Suppress(e.Button);
                return;
            case SButton.Escape:
                return;
        }

        if (this.SearchField.Selected)
        {
            this._helper.Input.Suppress(e.Button);
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null || !this.SearchArea.visible)
        {
            return;
        }

        this.SearchField.Draw(e.SpriteBatch, false);
        this.SearchIcon.draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        var menu = Game1.activeClickableMenu switch
        {
            { } clickableMenu when clickableMenu.GetChildMenu() is ItemGrabMenu itemGrabMenu => itemGrabMenu,
            ItemGrabMenu itemGrabMenu => itemGrabMenu,
            _ => null,
        };

        if (menu is not null && ReferenceEquals(menu, this.CurrentMenu))
        {
            if (!this.SearchArea.visible)
            {
                return;
            }

            if (this.TimeOut > 0 && --this.TimeOut == 0)
            {
                Log.Trace($"SearchItems: {this.SearchText}");
                this.ItemMatcher.StringValue = this.SearchText;
                BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
            }

            if (this.SearchText.Equals(this.SearchField.Text, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            this.TimeOut = SearchItems.MaxTimeOut;
            this.SearchText = this.SearchField.Text;
            return;
        }

        this.CurrentMenu = menu;
        if (BetterItemGrabMenu.Context is not { Data: Storage storageObject }
         || this.CurrentMenu is null or { shippingBin: true })
        {
            this.SearchArea.visible = false;
            return;
        }

        if (this.LastContext is { Data: Storage lastStorage }
         && !ReferenceEquals(lastStorage.Context, storageObject.Context))
        {
            this.ItemMatcher.Clear();
            this.SearchField.Text = string.Empty;
        }

        this.LastContext = BetterItemGrabMenu.Context;
        this.SearchField.X = this.CurrentMenu.ItemsToGrabMenu.xPositionOnScreen;
        this.SearchField.Y = this.CurrentMenu.ItemsToGrabMenu.yPositionOnScreen - 14 * Game1.pixelZoom;
        this.SearchField.Width =
            this._config.TransferItems is FeatureOption.Enabled && this.CurrentMenu is not ItemSelectionMenu
                ? this.CurrentMenu.ItemsToGrabMenu.width - Game1.tileSize - 4
                : this.CurrentMenu.ItemsToGrabMenu.width;
        this.SearchField.Selected = false;
        this.SearchArea.visible = true;
        this.SearchArea.bounds = new(
            this.SearchField.X,
            this.SearchField.Y,
            this.SearchField.Width,
            this.SearchField.Height);
        this.SearchIcon.bounds = new(this.SearchField.X + this.SearchField.Width - 38, this.SearchField.Y + 6, 32, 32);

        BetterItemGrabMenu.ItemsToGrabMenu?.AddTransformer(this.FilterBySearch);
        BetterItemGrabMenu.ItemsToGrabMenu?.AddHighlighter(this.ItemMatcher);
    }
}