namespace BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using BetterChests.Interfaces;
using FuryCore.Helpers;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using Microsoft.Xna.Framework.Graphics;
using BetterChests.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ChestMenuTabs : Feature
{
    // TODO: Add MouseScroll event for switching tags
    private readonly PerScreen<Chest> _chest = new();
    private readonly PerScreen<ItemMatcher> _itemMatcher = new(() => new(true));
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<int> _tabIndex = new(() => -1);
    private readonly Lazy<IMenuComponents> _menuComponents;
    private readonly Lazy<IMenuItems> _menuItems;
    private readonly Lazy<Texture2D> _texture;
    private readonly Lazy<IList<TabComponent>> _tabs;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChestMenuTabs"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    public ChestMenuTabs(IConfigModel config, IModHelper helper, IServiceLocator services)
        : base(config, helper, services)
    {
        this._texture = new(this.GetTexture);
        this._tabs = new(this.GetTabs);
        this._menuComponents = services.Lazy<IMenuComponents>();
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private Chest Chest
    {
        get => this._chest.Value;
        set => this._chest.Value = value;
    }

    private IMenuComponents MenuComponents
    {
        get => this._menuComponents.Value;
    }

    private IMenuItems MenuItems
    {
        get => this._menuItems.Value;
    }

    private int Index
    {
        get => this._tabIndex.Value;
        set => this._tabIndex.Value = value;
    }

    private ItemMatcher ItemMatcher
    {
        get => this._itemMatcher.Value;
    }

    private IList<TabComponent> Tabs
    {
        get => this._tabs.Value;
    }

    private Texture2D Texture
    {
        get => this._texture.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.MenuItems.Menu is not null)
        {
            // Add filter to Menu Items
            this.MenuItems.AddFilter(this.ItemMatcher);
        }

        if (this.MenuComponents.Menu is not null)
        {
            this.MenuComponents.Components.AddRange(this.Tabs);

            // Reposition tabs between inventory menus along a horizontal axis
            MenuComponent previousTab = null;
            var itemsToGrabMenu = this.MenuComponents.Menu.ItemsToGrabMenu;
            var inventoryMenu = this.MenuComponents.Menu.inventory.inventory;
            var slot = itemsToGrabMenu.capacity - (itemsToGrabMenu.capacity / itemsToGrabMenu.rows);
            foreach (var (tab, index) in this.Tabs.Select((tab, index) => (tab, index)))
            {
                tab.BaseY = itemsToGrabMenu.yPositionOnScreen + itemsToGrabMenu.height + Game1.pixelZoom;
                tab.Component.bounds.X = previousTab is not null
                    ? previousTab.Component.bounds.Right
                    : itemsToGrabMenu.xPositionOnScreen;

                tab.Component.upNeighborID = itemsToGrabMenu.inventory[slot + index].myID;
                tab.Component.downNeighborID = inventoryMenu[index].myID;
                itemsToGrabMenu.inventory[slot + index].downNeighborID = tab.Id;
                inventoryMenu[index].upNeighborID = tab.Id;

                if (previousTab is not null)
                {
                    previousTab.Component.rightNeighborID = tab.Id;
                    tab.Component.leftNeighborID = previousTab.Id;
                }

                previousTab = tab;
            }

            if (!ReferenceEquals(e.Chest, this.Chest))
            {
                this.Chest = e.Chest;
                this.SetTab(-1);
            }
        }
    }

    private void OnMenuComponentPressed(object sender, MenuComponentPressedEventArgs e)
    {
        if (e.Component is not TabComponent tab)
        {
            return;
        }

        var index = this.Tabs.IndexOf(tab);
        if (index == -1)
        {
            return;
        }

        this.SetTab(this.Index == index ? -1 : index);
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (this.MenuComponents.Menu is null)
        {
            return;
        }

        if (this.Config.NextTab.JustPressed())
        {
            this.SetTab(this.Index == this.Tabs.Count - 1 ? -1 : this.Index + 1);
            this.Helper.Input.SuppressActiveKeybinds(this.Config.NextTab);
            return;
        }

        if (this.Config.PreviousTab.JustPressed())
        {
            this.SetTab(this.Index == -1 ? this.Tabs.Count - 1 : this.Index - 1);
            this.Helper.Input.SuppressActiveKeybinds(this.Config.PreviousTab);
        }
    }

    private void SetTab(int index)
    {
        if (this.Index != -1)
        {
            this.Tabs[this.Index].Selected = false;
        }

        this.Index = index;
        if (this.Index != -1)
        {
            this.Tabs[this.Index].Selected = true;
            this.MenuComponents.Menu.setCurrentlySnappedComponentTo(this.Tabs[this.Index].Id);
            this.MenuComponents.Menu.snapCursorToCurrentSnappedComponent();
        }

        this.ItemMatcher.Clear();
        if (index != -1)
        {
            foreach (var tag in this.Tabs[this.Index].Tags)
            {
                this.ItemMatcher.Add(tag);
            }
        }
    }

    private Texture2D GetTexture()
    {
        return this.Helper.Content.Load<Texture2D>("assets/tabs.png");
    }

    private IList<TabComponent> GetTabs()
    {
        return this.Helper.Content.Load<List<TabData>>("assets/tabs.json").Select(
            (tab, i) => new TabComponent(
                new(
                    new(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                    this.Texture,
                    new(16 * i, 0, 16, 16),
                    Game1.pixelZoom)
                {
                    hoverText = this.Helper.Translation.Get($"tabs.{tab.Name}.name"),
                    name = tab.Name,
                },
                tab.Tags)).ToList();
    }
}