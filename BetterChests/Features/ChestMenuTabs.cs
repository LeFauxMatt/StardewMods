namespace BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers.ItemMatcher;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using Microsoft.Xna.Framework.Graphics;
using Models;
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
    private readonly PerScreen<ItemsDisplayedEventArgs> _displayedItems = new();
    private readonly PerScreen<ItemMatcher> _itemMatcher = new(() => new(true));
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<int> _tabIndex = new(() => -1);
    private readonly Lazy<IFuryMenu> _customMenuComponents;
    private readonly Lazy<Texture2D> _texture;
    private readonly Lazy<IList<Tab>> _tabs;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChestMenuTabs"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public ChestMenuTabs(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        this._texture = new(this.GetTexture);
        this._tabs = new(this.GetTabs);
        this._customMenuComponents = services.Lazy<IFuryMenu>();
    }

    private Chest Chest
    {
        get => this._chest.Value;
        set => this._chest.Value = value;
    }

    private ItemsDisplayedEventArgs DisplayedItems
    {
        get => this._displayedItems.Value;
        set => this._displayedItems.Value = value;
    }

    private IFuryMenu FuryMenu
    {
        get => this._customMenuComponents.Value;
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

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private IList<Tab> Tabs
    {
        get => this._tabs.Value;
    }

    private Texture2D Texture
    {
        get => this._texture.Value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.ItemsDisplayed += this.OnItemsDisplayed;
        this.FuryEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.ItemsDisplayed -= this.OnItemsDisplayed;
        this.FuryEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu;
        if (this.Menu is null)
        {
            return;
        }

        if (!ReferenceEquals(e.Chest, this.Chest))
        {
            this.SetTab(-1);
        }

        this.FuryMenu.BehindComponents.AddRange(this.Tabs);
        this.Chest = e.Chest;

        // Reposition tabs between inventory menus along a horizontal axis
        var x = this.Menu.ItemsToGrabMenu.xPositionOnScreen;
        var y = this.Menu.ItemsToGrabMenu.yPositionOnScreen + this.Menu.ItemsToGrabMenu.height + Game1.pixelZoom;
        foreach (var tab in this.Tabs)
        {
            tab.Component.bounds.X = x;
            tab.BaseY = this.Menu.ItemsToGrabMenu.yPositionOnScreen + this.Menu.ItemsToGrabMenu.height + Game1.pixelZoom;
            x = tab.Component.bounds.Right;
        }
    }

    private void OnItemsDisplayed(object sender, ItemsDisplayedEventArgs e)
    {
        this.DisplayedItems = e;
        e.AddFilter(this.ItemMatcher.Matches);
    }

    private void OnMenuComponentPressed(object sender, MenuComponentPressedEventArgs e)
    {
        if (this.Menu is null || e.Component is not Tab tab)
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
        if (this.Menu is null)
        {
            return;
        }

        if (this.Config.NextTab.JustPressed())
        {
            this.SetTab(this.Index == this.Tabs.Count ? -1 : this.Index + 1);
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
        }

        this.ItemMatcher.Clear();
        if (index != -1)
        {
            this.ItemMatcher.UnionWith(this.Tabs.ElementAt(this.Index).Tags);
        }

        this.DisplayedItems?.ForceRefresh();
    }

    private Texture2D GetTexture()
    {
        return this.Helper.Content.Load<Texture2D>("assets/tabs.png");
    }

    private IList<Tab> GetTabs()
    {
        return this.Helper.Content.Load<List<TabData>>("assets/tabs.json").Select(
            (tab, i) => new Tab(
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