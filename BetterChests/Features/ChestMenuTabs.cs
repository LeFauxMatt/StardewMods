namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using FuryCore.Helpers;
using FuryCore.Interfaces;
using FuryCore.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class ChestMenuTabs : Feature
{
    private readonly PerScreen<Chest> _chest = new();
    private readonly PerScreen<ItemMatcher> _itemMatcher = new(() => new(true));
    private readonly PerScreen<int> _tabIndex = new(() => -1);
    private readonly PerScreen<IList<TabComponent>> _tabs = new(ChestMenuTabs.GetTabs);
    private readonly Lazy<IMenuComponents> _menuComponents;
    private readonly Lazy<IMenuItems> _menuItems;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChestMenuTabs"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ChestMenuTabs(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        ChestMenuTabs.Instance = this;
        this.Texture = this.Helper.Content.Load<Texture2D>("assets/tabs.png");
        this.TabData = this.Helper.Content.Load<List<TabData>>("assets/tabs.json");
        this._menuComponents = services.Lazy<IMenuComponents>();
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static ChestMenuTabs Instance { get; set; }

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

    private IList<TabData> TabData { get; }

    private IList<TabComponent> Tabs
    {
        get => this._tabs.Value;
    }

    private Texture2D Texture { get; }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
    }

    private static IList<TabComponent> GetTabs()
    {
        return ChestMenuTabs.Instance.TabData.Select(
            (tab, i) => new TabComponent(
                new(
                    new(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                    ChestMenuTabs.Instance.Texture,
                    new(16 * i, 0, 16, 16),
                    Game1.pixelZoom)
                {
                    hoverText = ChestMenuTabs.Instance.Helper.Translation.Get($"tabs.{tab.Name}.name"),
                    name = tab.Name,
                },
                tab.Tags)).ToList();
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (e.Chest is null || !this.ManagedChests.FindChest(e.Chest, out var managedChest) || managedChest.ChestMenuTabs == FeatureOption.Disabled)
        {
            return;
        }

        if (this.MenuItems.Menu is not null)
        {
            // Add filter to Menu Items
            this.MenuItems.AddFilter(this.ItemMatcher);
        }

        if (this.MenuComponents.Menu is not null)
        {
            this.MenuComponents.Components.AddRange(managedChest.ChestMenuTabSet.Any() ? this.Tabs.Where(tab => managedChest.ChestMenuTabSet.Contains(tab.Name)) : this.Tabs);

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

        if (this.Config.ControlScheme.NextTab.JustPressed())
        {
            this.SetTab(this.Index == this.Tabs.Count - 1 ? -1 : this.Index + 1);
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.NextTab);
            return;
        }

        if (this.Config.ControlScheme.PreviousTab.JustPressed())
        {
            this.SetTab(this.Index == -1 ? this.Tabs.Count - 1 : this.Index - 1);
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.PreviousTab);
        }
    }

    private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
    {
        if (this.MenuComponents.Menu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.Tabs.Any(tab => tab.Component.containsPoint(x, y)))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.SetTab(this.Index == -1 ? this.Tabs.Count - 1 : this.Index - 1);
                break;
            case < 0:
                this.SetTab(this.Index == this.Tabs.Count - 1 ? -1 : this.Index + 1);
                break;
            default:
                return;
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
            if (this.MenuComponents.Menu.currentlySnappedComponent is not null && Game1.options.SnappyMenus)
            {
                this.MenuComponents.Menu.setCurrentlySnappedComponentTo(this.Tabs[this.Index].Id);
                this.MenuComponents.Menu.snapCursorToCurrentSnappedComponent();
            }
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
}