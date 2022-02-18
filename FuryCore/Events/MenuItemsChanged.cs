namespace StardewMods.FuryCore.Events;

using System;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley.Menus;

/// <inheritdoc />
internal class MenuItemsChanged : SortedEventHandler<MenuItemsChangedEventArgs>
{
    private readonly Lazy<IGameObjects> _gameObjects;
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly Lazy<MenuItems> _menuItems;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuItemsChanged" /> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public MenuItemsChanged(IModServices services)
    {
        this._gameObjects = services.Lazy<IGameObjects>();
        this._menuItems = services.Lazy<MenuItems>();
        services.Lazy<ICustomEvents>(
            customEvents => { customEvents.ClickableMenuChanged += this.OnClickableMenuChanged; });
    }

    private IGameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private MenuItems MenuItems
    {
        get => this._menuItems.Value;
    }

    private void OnClickableMenuChanged(object sender, ClickableMenuChangedEventArgs e)
    {
        if (e.Menu is not ItemGrabMenu { context: { } context } itemGrabMenu || !this.GameObjects.TryGetGameObject(context, out var gameObject) || gameObject is not IStorageContainer storageContainer)
        {
            this.Menu = null;
            return;
        }

        this.Menu = itemGrabMenu;
        this.MenuItems.ItemFilters.Clear();
        this.MenuItems.ItemHighlighters.Clear();
        this.MenuItems.SortMethod = null;

        this.InvokeAll(new(this.Menu, storageContainer, this.MenuItems.ItemFilters, this.MenuItems.ItemHighlighters, this.MenuItems.ItemHighlightCache, this.MenuItems.ForceRefresh));
        this.MenuItems.ForceRefresh();
    }
}