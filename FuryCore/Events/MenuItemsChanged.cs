namespace StardewMods.FuryCore.Events;

using System;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class MenuItemsChanged : SortedEventHandler<IMenuItemsChangedEventArgs>
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
        services.Lazy<ICustomEvents>(customEvents => customEvents.ClickableMenuChanged += this.OnClickableMenuChanged);
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

    [SortedEventPriority(EventPriority.High)]
    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        if (e.Menu is not ItemGrabMenu { context: { } context } itemGrabMenu || !this.GameObjects.TryGetGameObject(context, out var gameObject) || gameObject is not IStorageContainer storageContainer)
        {
            this.Reset();
            this.Menu = null;
            return;
        }

        if (!ReferenceEquals(this.Menu, itemGrabMenu))
        {
            this.Menu = itemGrabMenu;
            this.Reset();
        }

        if (this.Menu is null || this.HandlerCount == 0 || !ReferenceEquals(this.Menu, Game1.activeClickableMenu))
        {
            return;
        }

        this.InvokeAll(new MenuItemsChangedEventArgs(this.Menu, storageContainer, this.MenuItems));
        this.MenuItems.ForceRefresh();
    }

    private void Reset()
    {
        // Unsubscribe old filter events
        foreach (var itemMatcher in this.MenuItems.ItemFilters)
        {
            itemMatcher.CollectionChanged -= this.MenuItems.OnItemFilterChanged;
        }

        // Unsubscribe old highlight events
        foreach (var itemMatcher in this.MenuItems.ItemFilters)
        {
            itemMatcher.CollectionChanged -= this.MenuItems.OnItemHighlighterChanged;
        }

        // Clear old filters/highlighters
        this.MenuItems.ItemFilters.Clear();
        this.MenuItems.ItemHighlighters.Clear();
        this.MenuItems.SortMethod = null;
        this.MenuItems.ForceRefresh();
    }
}